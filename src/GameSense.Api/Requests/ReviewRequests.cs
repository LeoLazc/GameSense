using GameSense.Api.DTOs;
using MediatR;

namespace GameSense.Api.Requests;

public sealed record GetGameQuery(string GameId, int Page = 1, int? ViewerId = null) : IRequest<GameResponseDto?>;
public sealed record CreateReviewCommand(int UserId, string GameId, CreateReviewRequest Request) : IRequest<ReviewResponseDto>;
public sealed record SaveGotyPredictionCommand(int UserId, SaveGotyPredictionRequest Request) : IRequest<GotyPredictionResponseDto>;
public sealed record GetGotyPredictionQuery(int UserId) : IRequest<GotyPredictionResponseDto?>;
