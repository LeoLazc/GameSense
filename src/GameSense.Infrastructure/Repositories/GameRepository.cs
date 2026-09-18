using GameSense.Core.Models;
using GameSense.Core.Repositories;
using GameSense.Core.Services;
using GameSense.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GameSense.Infrastructure.Repositories;

public sealed class GameRepository(GameSenseDbContext db) : IGameRepository
{
    public Task<Game?> GetWithReviewsAsync(int id, CancellationToken cancellationToken = default) =>
        db.Games.AsNoTracking()
        .Include(g => g.Franchise)
        .Include(g => g.Reviews)
         .ThenInclude(r => r.User)
          .SingleOrDefaultAsync(g => g.Id == id, cancellationToken);

    public Task<bool> GameExistsAsync(int id, CancellationToken cancellationToken = default) =>
        db.Games.AsNoTracking().AnyAsync(g => g.Id == id, cancellationToken);

    public async Task<GameReviewPage?> GetReviewPageAsync(int id, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var game = await db.Games.AsNoTracking().Include(g => g.Franchise)
            .SingleOrDefaultAsync(g => g.Id == id, cancellationToken);
        if (game is null) return null;

        var ratings = db.Reviews.Where(r => r.GameId == id).Select(r => r.Rating);
        var total = await ratings.CountAsync(cancellationToken);
        // Public scores use nearest-integer rounding, with .5 rounded away from zero.
        var average = total == 0 ? null : (int?)Math.Round(await ratings.AverageAsync(cancellationToken), MidpointRounding.AwayFromZero);
        game.Reviews = await db.Reviews.AsNoTracking().Where(r => r.GameId == id)
            .Include(r => r.User).OrderByDescending(r => r.CreatedAt).ThenByDescending(r => r.Id)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        return new GameReviewPage(game, total, average);
    }

    public async Task<IReadOnlyList<GameCatalogSummary>> GetRecentAsync(int limit, DateTime releasedBefore, CancellationToken cancellationToken = default)
    {
        var recentGames = await db.Games.AsNoTracking()
            .Include(g => g.Franchise)
            .Where(g => g.ReleasedAt != null && g.ReleasedAt <= releasedBefore)
            .OrderByDescending(g => g.ReleasedAt)
            .ThenByDescending(g => g.Id)
            .Take(limit)
            .Select(g => new
            {
                Game = g,
                VoteCount = g.Reviews.Count(),
                AverageScore = g.Reviews.Select(review => (double?)review.Rating).Average()
            })
            .ToListAsync(cancellationToken);

        return recentGames
            .Select(item => new GameCatalogSummary(
                item.Game,
                item.AverageScore is null ? null : (int?)Math.Round(item.AverageScore.Value, MidpointRounding.AwayFromZero),
                item.VoteCount))
            .ToArray();
    }

    public async Task<IReadOnlyList<Game>> GetByIdsAsync(IEnumerable<int> ids, CancellationToken cancellationToken = default) =>
        await db.Games.AsNoTracking()
        .Where(g => ids.Contains(g.Id))
        .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Game>> UpsertCatalogGamesAsync(IEnumerable<CatalogGame> catalogGames, CancellationToken cancellationToken = default)
    {
        var records = catalogGames.ToList();
        var externalIds = records.Select(g => g.ExternalId).Distinct().ToArray();
        var providers = records.Select(g => g.Provider).Distinct().ToArray();
        var existing = await db.Games.Where(g => g.CatalogProvider != null && providers.Contains(g.CatalogProvider)
                && g.ExternalId != null && externalIds.Contains(g.ExternalId))
            .ToDictionaryAsync(g => (g.CatalogProvider!, g.ExternalId!), cancellationToken);

        foreach (var record in records)
        {
            if (!existing.TryGetValue((record.Provider, record.ExternalId), out var game))
            {
                game = new Game { ExternalId = record.ExternalId };
                db.Games.Add(game);
                existing[(record.Provider, record.ExternalId)] = game;
            }

            game.Name = record.Name;
            game.ReleaseYear = record.ReleaseYear ?? record.ReleasedAt?.Year ?? 0;
            game.Slug = record.Slug;
            game.Description = record.Summary;
            game.ReleasedAt = record.ReleasedAt;
            game.CoverImageUrl = record.CoverImageUrl;
            game.BackgroundImageUrl = record.BackgroundImageUrl;
            game.WebsiteUrl = record.WebsiteUrl;
            game.FranchiseExternalId = record.FranchiseExternalId;
            game.FranchiseName = record.FranchiseName;
            game.CatalogProvider = record.Provider;
            game.LastSyncedAt = record.SyncedAt;
        }

        await db.SaveChangesAsync(cancellationToken);
        var ids = records.Select(r => existing[(r.Provider, r.ExternalId)].Id).Distinct().ToArray();
        return await db.Games.AsNoTracking().Include(g => g.Franchise).Where(g => ids.Contains(g.Id)).ToListAsync(cancellationToken);
    }
}
