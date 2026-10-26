using GameSense.Core.Models;

namespace GameSense.Core.Services;

public interface IGotyPredictionService
{
    Task<GotyPrediction> CreateAsync(
        int userId,
        IReadOnlyList<string> gameIds,
        string gotyGameId,
        CancellationToken cancellationToken = default);
}
