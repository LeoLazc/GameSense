namespace GameSense.Infrastructure.Options;

public sealed class GameCatalogOptions
{
    public string ApiKey { get; set; } = string.Empty;
    public string ApiBaseUrl { get; set; } = "https://api.rawg.io/api/";
    public bool EnableDevelopmentStartupSync { get; set; }
}
