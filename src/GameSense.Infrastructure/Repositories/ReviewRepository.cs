using GameSense.Core.Models;
using GameSense.Core.Repositories;
using GameSense.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GameSense.Infrastructure.Repositories;

public sealed class ReviewRepository(GameSenseDbContext db) : IReviewRepository
{
    public Task<bool> ExistsForUserAndGameAsync(int userId, string rawgGameId, CancellationToken cancellationToken = default) =>
        db.Reviews.AnyAsync(review => review.UserId == userId && review.RawgGameId == rawgGameId, cancellationToken);

    public Task AddAsync(Review review, CancellationToken cancellationToken = default) => db.Reviews.AddAsync(review, cancellationToken).AsTask();
    public Task SaveChangesAsync(CancellationToken cancellationToken = default) => db.SaveChangesAsync(cancellationToken);
}
