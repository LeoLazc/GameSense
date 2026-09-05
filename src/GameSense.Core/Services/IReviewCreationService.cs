using GameSense.Core.Models;

namespace GameSense.Core.Services;

public interface IReviewCreationService
{
    Task<Review> CreateAsync(
        int userId,
        int gameId,
        string title,
        string content,
        int rating,
        CancellationToken cancellationToken = default);
}

public interface IReviewEligibilityPolicy
{
    void EnsureEligible(User? user);
}
