using GameSense.Core.Models;

namespace GameSense.Core.Repositories;

public interface IGotyPredictionRepository
{
    Task<GotyPrediction?> GetAsync(int userId, int year, CancellationToken cancellationToken = default);
    Task AddAsync(GotyPrediction prediction, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
