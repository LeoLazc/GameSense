namespace GameSense.Infrastructure.Options;

public sealed class GameCatalogOptions
{
    //ToDo: get this from the configuration file
    public string ApiKey { get; set; } = string.Empty;
    public string ApiBaseUrl { get; set; } = "https://api.rawg.io/api/";
    public bool EnableDevelopmentStartupSync { get; set; }
}
