using GameSense.Api.DTOs;
using GameSense.Api.Requests;
using GameSense.Core.Repositories;
using GameSense.Core.Services;
using MediatR;

namespace GameSense.Api.Handlers;

public sealed class SaveGotyPredictionHandler(IGotyPredictionService gotyPredictions) : IRequestHandler<SaveGotyPredictionCommand, GotyPredictionResponseDto>
{
    public async Task<GotyPredictionResponseDto> Handle(SaveGotyPredictionCommand request, CancellationToken cancellationToken)
    {
        var prediction = await gotyPredictions.CreateAsync(request.UserId, request.Request.GameIds, request.Request.GotyGameId, cancellationToken);
        return new GotyPredictionResponseDto(
            prediction.Year,
            prediction.GotyRawgGameId,
            prediction.Nominees.Select(nominee => new GotyNomineeResponseDto(nominee.RawgGameId, nominee.RawgGameId, nominee.Order)).ToArray());
    }
}

public sealed class GetGotyPredictionHandler(IGotyPredictionRepository predictions) : IRequestHandler<GetGotyPredictionQuery, GotyPredictionResponseDto?>
{
    public async Task<GotyPredictionResponseDto?> Handle(GetGotyPredictionQuery request, CancellationToken cancellationToken)
    {
        var prediction = await predictions.GetAsync(request.UserId, DateTime.UtcNow.Year, cancellationToken);
        return prediction == null
            ? null
            : new GotyPredictionResponseDto(
                prediction.Year,
                prediction.GotyRawgGameId,
                prediction.Nominees.Select(nominee => new GotyNomineeResponseDto(nominee.RawgGameId, nominee.RawgGameId, nominee.Order)).ToArray());
    }
}
