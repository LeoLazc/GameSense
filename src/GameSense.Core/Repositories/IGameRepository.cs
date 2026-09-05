using GameSense.Core.Models;
using GameSense.Core.Services;

namespace GameSense.Core.Repositories;

public interface IGameRepository
{
    Task<Game?> GetWithReviewsAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Game>> GetByIdsAsync(IEnumerable<int> ids, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Game>> UpsertCatalogGamesAsync(IEnumerable<CatalogGame> games, CancellationToken cancellationToken = default);
}
