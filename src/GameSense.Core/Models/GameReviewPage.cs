namespace GameSense.Core.Models;

public sealed record GameReviewPage(Game Game, int TotalReviews, int? AverageScore);
