namespace GameSense.Api.DTOs;

public sealed record CreateReviewRequest(string Title, string Content, int Rating);
public sealed record ReviewResponseDto(int Id, string Title, string Content, int Rating, DateTime CreatedAt, DateTime UpdatedAt, string Username);
public sealed record GameResponseDto(int Id, string Name, int ReleaseYear, string? FranchiseName, IReadOnlyList<ReviewResponseDto> Reviews);
public sealed record GotyNomineeResponseDto(int GameId, string Name, int Order);
public sealed record GotyPredictionResponseDto(int Year, int GotyGameId, IReadOnlyList<GotyNomineeResponseDto> Nominees);
public sealed record SaveGotyPredictionRequest(IReadOnlyList<int> GameIds, int GotyGameId);
