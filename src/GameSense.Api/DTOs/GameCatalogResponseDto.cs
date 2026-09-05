namespace GameSense.Api.DTOs;

public sealed record GameCatalogResponseDto(
    int Id, string Name, int ReleaseYear, string? ExternalId, string? Slug, string? Description,
    DateTime? ReleasedAt, string? CoverImageUrl, string? BackgroundImageUrl, string? WebsiteUrl,
    string? FranchiseName, DateTime? LastSyncedAt);
