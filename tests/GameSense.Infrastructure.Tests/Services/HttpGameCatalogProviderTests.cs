using System.Net;
using System.Text;
using GameSense.Infrastructure.Options;
using GameSense.Infrastructure.Services;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;

namespace GameSense.Infrastructure.Tests.Services;

[TestFixture]
public sealed class HttpGameCatalogProviderTests
{
    [Test]
    public async Task Search_sends_rawg_key_and_maps_paginated_results()
    {
        var handler = new RecordingHandler(_ => JsonResponse("{\"count\":2,\"next\":null,\"previous\":null,\"results\":[{\"id\":42,\"slug\":\"game\",\"name\":\"Game\",\"released\":\"2025-01-02\",\"background_image\":\"https://image\"}]}"));
        var provider = Provider(handler);

        var games = await provider.SearchGamesAsync("zelda & mario");

        Assert.Multiple(() =>
        {
            Assert.That(handler.Requests, Has.Count.EqualTo(1));
            Assert.That(handler.Requests[0].RequestUri!.Query, Is.EqualTo("?search=zelda%20%26%20mario&page_size=50&key=test-key"));
            Assert.That(handler.Requests[0].Headers, Has.Count.EqualTo(0));
            Assert.That(games, Has.Count.EqualTo(1));
            Assert.That(games[0].Provider, Is.EqualTo("rawg"));
            Assert.That(games[0].ExternalId, Is.EqualTo("42"));
            Assert.That(games[0].ReleasedAt, Is.EqualTo(new DateTime(2025, 1, 2)));
            Assert.That(games[0].BackgroundImageUrl, Is.EqualTo("https://image"));
            Assert.That(games[0].CoverImageUrl, Is.Null);
            Assert.That(games[0].Summary, Is.Null);
            Assert.That(games[0].ToString(), Does.Not.Contain("MetacriticScore"));
        });
    }

    [Test]
    public async Task Current_year_fetches_single_continuous_inclusion_window()
    {
        var handler = new RecordingHandler(_ => JsonResponse("{\"count\":0,\"results\":[]}"));
        var provider = Provider(handler);

        await provider.GetCurrentYearGamesAsync(2024);

        Assert.Multiple(() =>
        {
            Assert.That(handler.Requests, Has.Count.EqualTo(1));
            Assert.That(handler.Requests[0].RequestUri!.Query,
                Is.EqualTo("?dates=2023-11-15,2024-12-31&page_size=50&key=test-key"));
        });
    }

    [Test]
    public async Task Current_year_deduplicates_games_by_provider_and_external_id()
    {
        var handler = new RecordingHandler(_ => JsonResponse(
            "{\"results\":[{\"id\":42,\"name\":\"Game\"},{\"id\":43,\"name\":\"Other\"},{\"id\":42,\"name\":\"Game duplicate\"},{\"id\":44,\"name\":\"Previous\"}]}"));
        var provider = Provider(handler);

        var games = await provider.GetCurrentYearGamesAsync(2024);

        Assert.Multiple(() =>
        {
            Assert.That(handler.Requests, Has.Count.EqualTo(1));
            Assert.That(games.Select(game => (game.Provider, game.ExternalId)), Is.EquivalentTo([
                ("rawg", "42"), ("rawg", "43"), ("rawg", "44")]));
        });
    }

    [Test]
    public async Task Current_year_follows_next_link_and_deduplicates_across_pages()
    {
        var handler = new RecordingHandler(request => request.RequestUri!.Query.Contains("page=2")
            ? JsonResponse("{\"results\":[{\"id\":42,\"name\":\"Duplicate\"},{\"id\":43,\"name\":\"Second\"}]}")
            : JsonResponse("{\"next\":\"games?page=2\",\"results\":[{\"id\":42,\"name\":\"First\"}] }"));
        var provider = Provider(handler);

        var games = await provider.GetCurrentYearGamesAsync(2024);

        Assert.Multiple(() =>
        {
            Assert.That(handler.Requests, Has.Count.EqualTo(2));
            Assert.That(games.Select(game => (game.Provider, game.ExternalId)),
                Is.EquivalentTo([("rawg", "42"), ("rawg", "43")]));
        });
    }

    [Test]
    public async Task Follows_absolute_next_link_and_replaces_any_key_without_exposing_it()
    {
        var handler = new RecordingHandler(request => handlerResponse(request));
        var provider = Provider(handler);

        var games = await provider.SearchGamesAsync("game");

        Assert.Multiple(() =>
        {
            Assert.That(handler.Requests, Has.Count.EqualTo(2));
            Assert.That(handler.Requests[1].RequestUri!.Query, Is.EqualTo("?page=2&key=test-key"));
            Assert.That(handler.Requests[1].RequestUri!.ToString(), Does.Not.Contain("old-key"));
            Assert.That(games.Select(game => game.ExternalId), Is.EquivalentTo(["1", "2"]));
        });

        HttpResponseMessage handlerResponse(HttpRequestMessage request)
            => request.RequestUri!.Query.Contains("page=2")
                ? JsonResponse("{\"results\":[{\"id\":2,\"name\":\"Second\"}]}")
                : JsonResponse("{\"next\":\"https://api.rawg.io/api/games?page=2&key=old-key\",\"results\":[{\"id\":1,\"name\":\"First\"}]}");
    }

    [Test]
    public async Task Stops_on_repeated_or_invalid_next_link_without_looping()
    {
        var repeatedHandler = new RecordingHandler(_ => JsonResponse(
            "{\"next\":\"games?search=game&page_size=50\",\"results\":[{\"id\":1,\"name\":\"First\"}]}"));
        var invalidHandler = new RecordingHandler(request => request.RequestUri!.Query.Contains("page=2")
            ? JsonResponse("{\"next\":\"ftp://invalid.example/games\",\"results\":[]}")
            : JsonResponse("{\"next\":\"https://api.rawg.io/api/games?page=2\",\"results\":[]}"));

        await Provider(repeatedHandler).SearchGamesAsync("game");
        await Provider(invalidHandler).SearchGamesAsync("game");

        Assert.Multiple(() =>
        {
            Assert.That(repeatedHandler.Requests, Has.Count.EqualTo(1));
            Assert.That(invalidHandler.Requests, Has.Count.EqualTo(2));
        });
    }

    [Test]
    public void Rejects_missing_key_or_invalid_base_url_before_http_call()
    {
        var handler = new RecordingHandler(_ => JsonResponse("{}"));
        var factory = Factory(handler);
        var missingKey = new HttpGameCatalogProvider(factory.Object, Options("", "https://api.rawg.io/api/"));
        var invalidUrl = new HttpGameCatalogProvider(factory.Object, Options("key", "not-a-url"));

        var keyError = Assert.ThrowsAsync<InvalidOperationException>(() => missingKey.SearchGamesAsync("game"));
        var urlError = Assert.ThrowsAsync<InvalidOperationException>(() => invalidUrl.SearchGamesAsync("game"));

        Assert.Multiple(() =>
        {
            Assert.That(keyError!.Message, Does.Contain("GameCatalog:ApiKey"));
            Assert.That(urlError!.Message, Does.Contain("GameCatalog:ApiBaseUrl"));
            Assert.That(handler.Requests, Is.Empty);
        });
    }

    [TestCase("not-json", "malformed game data")]
    [TestCase("{\"results\":null}", "without results")]
    public void Rejects_malformed_response(string body, string message)
    {
        var provider = Provider(new RecordingHandler(_ => JsonResponse(body)));

        var error = Assert.ThrowsAsync<InvalidOperationException>(() => provider.SearchGamesAsync("game"));

        Assert.That(error!.Message, Does.Contain(message));
    }

    [Test]
    public void Rejects_missing_game_identity()
    {
        var provider = Provider(new RecordingHandler(_ => JsonResponse("{\"results\":[{\"id\":0,\"name\":\"\"}]}")));

        var error = Assert.ThrowsAsync<InvalidOperationException>(() => provider.SearchGamesAsync("game"));

        Assert.That(error!.Message, Does.Contain("valid id or name"));
    }

    [Test]
    public void Propagates_http_errors()
    {
        var provider = Provider(new RecordingHandler(_ => new HttpResponseMessage(HttpStatusCode.Unauthorized)));

        var error = Assert.ThrowsAsync<HttpRequestException>(() => provider.SearchGamesAsync("game"));

        Assert.That(error!.Message, Does.Contain("401"));
    }

    private static HttpGameCatalogProvider Provider(RecordingHandler handler)
        => new(Factory(handler).Object, Options("test-key", "https://api.rawg.io/api/"));

    private static IOptions<GameCatalogOptions> Options(string key, string baseUrl)
        => Microsoft.Extensions.Options.Options.Create(new GameCatalogOptions { ApiKey = key, ApiBaseUrl = baseUrl });

    private static Mock<IHttpClientFactory> Factory(RecordingHandler handler)
    {
        var mock = new Mock<IHttpClientFactory>();
        mock.Setup(x => x.CreateClient("GameCatalogApi"))
            .Returns(new HttpClient(handler) { BaseAddress = new Uri("https://api.rawg.io/api/") });
        return mock;
    }

    private static HttpResponseMessage JsonResponse(string json)
        => new(HttpStatusCode.OK) { Content = new StringContent(json, Encoding.UTF8, "application/json") };

    private sealed class RecordingHandler(Func<HttpRequestMessage, HttpResponseMessage> response) : HttpMessageHandler
    {
        public List<HttpRequestMessage> Requests { get; } = [];
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Requests.Add(request);
            return Task.FromResult(response(request));
        }
    }
}
