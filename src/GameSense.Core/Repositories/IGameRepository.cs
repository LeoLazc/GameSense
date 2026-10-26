using GameSense.Core.Models;
using GameSense.Core.Services;

namespace GameSense.Core.Repositories;

public interface IGameRepository
{
    Task<GameReviewPage> GetReviewPageAsync(CatalogGame game, int page, int pageSize, CancellationToken cancellationToken = default);
}
