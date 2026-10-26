using FluentValidation;
using GameSense.Core.Exceptions;
using GameSense.Core.Models;
using GameSense.Core.Repositories;
using GameSense.Core.Services;

namespace GameSense.Api.Services;

public sealed class GotyPredictionService(
    IGameCatalogProvider catalog,
    IGotyPredictionRepository predictions) : IGotyPredictionService
{
    public async Task<GotyPrediction> CreateAsync(
        int userId,
        IReadOnlyList<string> gameIds,
        string gotyGameId,
        CancellationToken cancellationToken = default)
    {
        var year = DateTime.UtcNow.Year;
        if (gameIds.Count != 6 || gameIds.Distinct().Count() != 6 || !gameIds.Contains(gotyGameId))
            throw new ValidationException("Solo seis juegos pueden ser nominados y el GOTY debe ser uno de ellos.");
        if (await predictions.GetAsync(userId, year, cancellationToken) != null)
            throw new PredictionConflictException("La prediccioón ya existe para este año.");

        var found = (await Task.WhenAll(gameIds.Select(id => catalog.GetGameAsync(id, cancellationToken))))
            .Where(game => game is not null)
            .Cast<CatalogGame>()
            .ToList();
        if (found.Count != 6 || found.Any(game => game.ReleaseYear != year))
            throw new ValidationException("Todos los juegos nominados deben existir y ser lanzados en el año actual.");

        var prediction = new GotyPrediction
        {
            UserId = userId,
            Year = year,
            GotyRawgGameId = gotyGameId,
            Nominees = gameIds.Select((id, index) => new GotyNominee { RawgGameId = id, Order = index + 1 }).ToList()
        };
        await predictions.AddAsync(prediction, cancellationToken);
        await predictions.SaveChangesAsync(cancellationToken);
        return prediction;
    }
}
