using GameSense.Core.Models;
using GameSense.Core.Repositories;
using GameSense.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GameSense.Infrastructure.Repositories;

public sealed class GotyPredictionRepository(GameSenseDbContext db) : IGotyPredictionRepository
{
    public Task<GotyPrediction?> GetAsync(int userId, int year, CancellationToken cancellationToken = default) =>
        db.GotyPredictions.AsNoTracking().Include(p => p.Nominees).ThenInclude(n => n.Game)
            .Include(p => p.GotyGame).SingleOrDefaultAsync(p => p.UserId == userId && p.Year == year, cancellationToken);
    public Task AddAsync(GotyPrediction prediction, CancellationToken cancellationToken = default) => db.GotyPredictions.AddAsync(prediction, cancellationToken).AsTask();
    public Task SaveChangesAsync(CancellationToken cancellationToken = default) => db.SaveChangesAsync(cancellationToken);
}
