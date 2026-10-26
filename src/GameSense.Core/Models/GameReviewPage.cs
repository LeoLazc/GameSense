using GameSense.Core.Services;

namespace GameSense.Core.Models;

public sealed record GameReviewPage(CatalogGame Game, IReadOnlyList<Review> Reviews, int TotalReviews, int? AverageScore);
