namespace GameSense.Api.DTOs;

public sealed record CreateReviewRequest(string Title, string Content, int Rating);
public sealed record ReviewResponseDto(
    int Id,
    string Title,
    string Content,
    int Rating,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    string Username,
    string? GameTitle = null,
    string? GameImageUrl = null);
public sealed record GameResponseDto(
    string Id,
    string Name,
    int ReleaseYear,
    string? FranchiseName,
    IReadOnlyList<ReviewResponseDto> Reviews,
    string? Description = null,
    DateTime? ReleasedAt = null,
    string? CoverImageUrl = null,
    string? BackgroundImageUrl = null,
    string? WebsiteUrl = null,
    int? AverageScore = null,
    int VoteCount = 0,
    int CurrentPage = 1,
    int PageSize = 10,
    int TotalPages = 0,
    bool IsViewerEligible = false,
    bool HasViewerReviewed = false);
public sealed record GotyNomineeResponseDto(string GameId, string Name, int Order);
public sealed record GotyPredictionResponseDto(int Year, string GotyGameId, IReadOnlyList<GotyNomineeResponseDto> Nominees);
public sealed record SaveGotyPredictionRequest(IReadOnlyList<string> GameIds, string GotyGameId);
