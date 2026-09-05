using GameSense.Core.Models;

namespace GameSense.Core.Repositories;

public interface IReviewRepository
{
    Task<bool> ExistsForUserAndGameAsync(int userId, int gameId, CancellationToken cancellationToken = default);
    Task AddAsync(Review review, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
