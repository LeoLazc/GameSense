namespace GameSense.Core.Services;

public interface IGameCatalogProvider
{
    Task<IReadOnlyList<CatalogGame>> SearchGamesAsync(string query, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CatalogGame>> GetCurrentYearGamesAsync(int year, CancellationToken cancellationToken = default);
}

public sealed record CatalogGame(
    string Provider,
    string ExternalId,
    string? Slug,
    string Name,
    string? Summary,
    DateTime? ReleasedAt,
    int? ReleaseYear,
    string? CoverImageUrl,
    string? BackgroundImageUrl,
    string? WebsiteUrl,
    string? FranchiseExternalId,
    string? FranchiseName,
    DateTime SyncedAt);
