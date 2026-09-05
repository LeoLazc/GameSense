using FluentValidation;
using GameSense.Core.Exceptions;
using GameSense.Core.Models;
using GameSense.Core.Repositories;
using GameSense.Core.Services;

namespace GameSense.Api.Services;

public sealed class GotyPredictionService(
    IGameRepository games,
    IGotyPredictionRepository predictions) : IGotyPredictionService
{
    public async Task<GotyPrediction> CreateAsync(
        int userId,
        IReadOnlyList<int> gameIds,
        int gotyGameId,
        CancellationToken cancellationToken = default)
    {
        var year = DateTime.UtcNow.Year;
        if (gameIds.Count != 6 || gameIds.Distinct().Count() != 6 || !gameIds.Contains(gotyGameId))
            throw new ValidationException("Exactly six distinct nominee games are required and the GOTY game must be one of them.");
        if (await predictions.GetAsync(userId, year, cancellationToken) != null)
            throw new PredictionConflictException("A GOTY prediction already exists for the current year.");

        var found = await games.GetByIdsAsync(gameIds, cancellationToken);
        if (found.Count != 6 || found.Any(game => game.ReleaseYear != year))
            throw new ValidationException("All nominee games must exist and be released in the current UTC year.");

        var prediction = new GotyPrediction
        {
            UserId = userId,
            Year = year,
            GotyGameId = gotyGameId,
            Nominees = gameIds.Select((id, index) => new GotyNominee { GameId = id, Order = index + 1 }).ToList()
        };
        await predictions.AddAsync(prediction, cancellationToken);
        await predictions.SaveChangesAsync(cancellationToken);
        prediction.Nominees = prediction.Nominees.Select(nominee =>
        {
            nominee.Game = found.Single(game => game.Id == nominee.GameId);
            return nominee;
        }).ToList();
        return prediction;
    }
}
