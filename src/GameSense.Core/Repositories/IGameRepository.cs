using GameSense.Core.Models;
using GameSense.Core.Services;

namespace GameSense.Core.Repositories;

public interface IGameRepository
{
    Task<Game?> GetWithReviewsAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> GameExistsAsync(int id, CancellationToken cancellationToken = default);
    Task<GameReviewPage?> GetReviewPageAsync(int id, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<GameCatalogSummary>> GetRecentAsync(int limit, DateTime releasedBefore, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Game>> GetByIdsAsync(IEnumerable<int> ids, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Game>> UpsertCatalogGamesAsync(IEnumerable<CatalogGame> games, CancellationToken cancellationToken = default);
}
