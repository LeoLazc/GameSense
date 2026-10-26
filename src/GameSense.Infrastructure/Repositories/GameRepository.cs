using GameSense.Core.Models;
using GameSense.Core.Repositories;
using GameSense.Core.Services;
using GameSense.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GameSense.Infrastructure.Repositories;

public sealed class GameRepository(GameSenseDbContext db) : IGameRepository
{
    public async Task<GameReviewPage> GetReviewPageAsync(
        CatalogGame game,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var ratings = db.Reviews.Where(review => review.RawgGameId == game.ExternalId).Select(review => review.Rating);
        var total = await ratings.CountAsync(cancellationToken);
        var average = total == 0
            ? null
            : (int?)Math.Round(await ratings.AverageAsync(cancellationToken), MidpointRounding.AwayFromZero);
        var reviews = await db.Reviews
            .AsNoTracking()
            .Where(review => review.RawgGameId == game.ExternalId)
            .Include(review => review.User)
            .OrderByDescending(review => review.CreatedAt)
            .ThenByDescending(review => review.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new GameReviewPage(game, reviews, total, average);
    }
}
