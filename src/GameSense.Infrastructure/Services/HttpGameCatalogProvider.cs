using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using GameSense.Core.Services;
using GameSense.Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace GameSense.Infrastructure.Services;

public sealed class HttpGameCatalogProvider(
    IHttpClientFactory httpClientFactory,
    IOptions<GameCatalogOptions> options) : IGameCatalogProvider
{
    private const string Provider = "rawg";
    // Protect synchronization from malformed provider responses without imposing a normal catalog limit.
    private const int MaximumPages = 100;
    private readonly GameCatalogOptions settings = options.Value;

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

    private async Task<IReadOnlyList<CatalogGame>> QueryAsync(string path, CancellationToken cancellationToken)
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
                nextUrl = payload.Next;
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
