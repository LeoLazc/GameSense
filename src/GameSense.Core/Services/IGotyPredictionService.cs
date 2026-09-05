using GameSense.Core.Models;

namespace GameSense.Core.Services;

public interface IGotyPredictionService
{
    Task<GotyPrediction> CreateAsync(
        int userId,
        IReadOnlyList<int> gameIds,
        int gotyGameId,
        CancellationToken cancellationToken = default);
}
