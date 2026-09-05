using AutoMapper;
using GameSense.Api.DTOs;
using GameSense.Api.Requests;
using GameSense.Core.Repositories;
using GameSense.Core.Services;
using MediatR;

namespace GameSense.Api.Handlers;

public sealed class SaveGotyPredictionHandler(IGotyPredictionService gotyPredictions, IMapper mapper) : IRequestHandler<SaveGotyPredictionCommand, GotyPredictionResponseDto>
{
    public async Task<GotyPredictionResponseDto> Handle(SaveGotyPredictionCommand request, CancellationToken cancellationToken)
    {
        var prediction = await gotyPredictions.CreateAsync(request.UserId, request.Request.GameIds, request.Request.GotyGameId, cancellationToken);
        return mapper.Map<GotyPredictionResponseDto>(prediction);
    }
}

public sealed class GetGotyPredictionHandler(IGotyPredictionRepository predictions, IMapper mapper) : IRequestHandler<GetGotyPredictionQuery, GotyPredictionResponseDto?>
{
    public async Task<GotyPredictionResponseDto?> Handle(GetGotyPredictionQuery request, CancellationToken cancellationToken)
    {
        var prediction = await predictions.GetAsync(request.UserId, DateTime.UtcNow.Year, cancellationToken);
        return prediction == null ? null : mapper.Map<GotyPredictionResponseDto>(prediction);
    }
}
