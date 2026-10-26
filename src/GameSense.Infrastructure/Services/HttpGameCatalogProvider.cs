using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using GameSense.Core.Services;
using GameSense.Infrastructure.Options;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace GameSense.Infrastructure.Services;

public sealed class HttpGameCatalogProvider(
    IHttpClientFactory httpClientFactory,
    IOptions<GameCatalogOptions> options,
    IMemoryCache? cache = null) : IGameCatalogProvider
{
    //ToDo: find a way to improve call performance, maybe by using a single HttpClient instance and caching the results for a certain period of time.
    private const string Provider = "rawg";
    private const int MaximumPages = 100;
    private readonly GameCatalogOptions settings = options.Value;

    public async Task<CatalogGame?> GetGameAsync(string externalId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(externalId))
            throw new ArgumentException("A game external id is required.", nameof(externalId));

        var cacheKey = $"rawg-game:{externalId.Trim()}";
        if (cache is not null && cache.TryGetValue(cacheKey, out CatalogGame? cached))
            return cached;

        ValidateSettings();
        var client = httpClientFactory.CreateClient("GameCatalogApi");
        if (!TryBuildRequestUrl($"games/{Uri.EscapeDataString(externalId.Trim())}", out var requestUrl))
            return null;

        using var response = await client.GetAsync(requestUrl, cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;
        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException($"RAWG game request failed with HTTP {(int)response.StatusCode} ({response.ReasonPhrase}).");

        var game = await response.Content.ReadFromJsonAsync<RawgGame>(cancellationToken: cancellationToken);
        if (game is null)
            return null;

        var mapped = Map(game);
        cache?.Set(cacheKey, mapped, TimeSpan.FromMinutes(5));
        return mapped;
    }

    public Task<IReadOnlyList<CatalogGame>> SearchGamesAsync(string query, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query))
            throw new ArgumentException("A game search query is required.", nameof(query));

        return QueryAsync($"games?search={Uri.EscapeDataString(query.Trim())}&page_size=50", cancellationToken);
    }

    public async Task<IReadOnlyList<CatalogGame>> GetCurrentYearGamesAsync(int year, CancellationToken cancellationToken = default)
    {
        var previousYear = (year - 1).ToString(CultureInfo.InvariantCulture);
        var currentYear = year.ToString(CultureInfo.InvariantCulture);
        var games = await QueryAsync(
            $"games?dates={previousYear}-11-15,{currentYear}-12-31&page_size=50", cancellationToken);

        return games
            .GroupBy(game => (game.Provider, game.ExternalId))
            .Select(group => group.First())
            .ToArray();
    }

    public Task<IReadOnlyList<CatalogGame>> GetRecentGamesAsync(
        int limit,
        DateTime releasedBefore,
        CancellationToken cancellationToken = default)
    {
        if (limit <= 0)
            throw new ArgumentOutOfRangeException(nameof(limit));

        var date = releasedBefore.ToUniversalTime().ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        var pageSize = Math.Min(limit, 40);
        return QueryAsync(
            $"games?dates=1900-01-01,{date}&ordering=-released&page_size={pageSize}",
            cancellationToken,
            followNext: false,
            maximumResults: limit);
    }

    private async Task<IReadOnlyList<CatalogGame>> QueryAsync(
        string path,
        CancellationToken cancellationToken,
        bool followNext = true,
        int? maximumResults = null)
    {
        ValidateSettings();
        var client = httpClientFactory.CreateClient("GameCatalogApi");
        var nextUrl = path;
        var visitedUrls = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var games = new List<CatalogGame>();

        for (var page = 0; page < MaximumPages && nextUrl is not null; page++)
        {
            if (!TryBuildRequestUrl(nextUrl, out var requestUrl) || !visitedUrls.Add(RemoveApiKey(requestUrl)))
                break;

            using var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
            using var response = await client.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException($"RAWG games request failed with HTTP {(int)response.StatusCode} ({response.ReasonPhrase}).");

            try
            {
                var payload = await response.Content.ReadFromJsonAsync<RawgResponse>(cancellationToken: cancellationToken)
                    ?? throw new InvalidOperationException("RAWG returned an empty response.");
                if (payload.Results is null)
                    throw new InvalidOperationException("RAWG returned a response without results.");
                games.AddRange(payload.Results.Select(Map));
                if (maximumResults is int max && games.Count >= max)
                {
                    games = games.Take(max).ToList();
                    break;
                }

                nextUrl = followNext ? payload.Next : null;
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException("RAWG returned malformed game data.", ex);
            }
        }

        return games.ToArray();
    }

    private bool TryBuildRequestUrl(string url, out string requestUrl)
    {
        requestUrl = string.Empty;
        if (!Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out var uri) ||
            (uri.IsAbsoluteUri && uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
            return false;

        var withoutKey = RemoveApiKey(uri.IsAbsoluteUri ? uri.AbsoluteUri : url);
        var separator = withoutKey.Contains('?') ? '&' : '?';
        requestUrl = $"{withoutKey}{separator}key={Uri.EscapeDataString(settings.ApiKey)}";
        return true;
    }

    private static string RemoveApiKey(string url)
    {
        var fragmentIndex = url.IndexOf('#');
        var fragment = fragmentIndex >= 0 ? url[fragmentIndex..] : string.Empty;
        var withoutFragment = fragmentIndex >= 0 ? url[..fragmentIndex] : url;
        var queryIndex = withoutFragment.IndexOf('?');
        if (queryIndex < 0)
            return url;

        var path = withoutFragment[..queryIndex];
        var query = withoutFragment[(queryIndex + 1)..]
            .Split('&', StringSplitOptions.RemoveEmptyEntries)
            .Where(parameter => !parameter.Split('=', 2)[0].Equals("key", StringComparison.OrdinalIgnoreCase));
        var remainingQuery = string.Join('&', query);
        return $"{path}{(remainingQuery.Length > 0 ? $"?{remainingQuery}" : string.Empty)}{fragment}";
    }

    private void ValidateSettings()
    {
        if (string.IsNullOrWhiteSpace(settings.ApiKey))
            throw new InvalidOperationException("The game catalog is not configured. Set GameCatalog:ApiKey using backend-only configuration or user secrets.");
        if (!Uri.TryCreate(settings.ApiBaseUrl, UriKind.Absolute, out var baseUri) ||
            (baseUri.Scheme != Uri.UriSchemeHttp && baseUri.Scheme != Uri.UriSchemeHttps))
            throw new InvalidOperationException("GameCatalog:ApiBaseUrl must be an absolute HTTP or HTTPS URL.");
    }

    private static CatalogGame Map(RawgGame game)
    {
        if (game.Id <= 0 || string.IsNullOrWhiteSpace(game.Name))
            throw new InvalidOperationException("RAWG returned a game without a valid id or name.");

        DateTime? releasedAt = DateTime.TryParseExact(game.Released, "yyyy-MM-dd", CultureInfo.InvariantCulture,
            DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var released)
            ? released
            : null;

        return new CatalogGame(Provider, game.Id.ToString(CultureInfo.InvariantCulture), game.Slug, game.Name,
            null, releasedAt, releasedAt?.Year, null, game.BackgroundImage, null, null, null, DateTime.UtcNow);
    }

    private sealed class RawgResponse
    {
        [JsonPropertyName("results")]
        public List<RawgGame>? Results { get; set; }

        [JsonPropertyName("next")]
        public string? Next { get; set; }
    }

    private sealed class RawgGame
    {
        public int Id { get; set; }
        public string? Slug { get; set; }
        public string? Name { get; set; }
        public string? Released { get; set; }
        [JsonPropertyName("background_image")]
        public string? BackgroundImage { get; set; }
    }
}
