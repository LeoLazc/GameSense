using AutoMapper;
using GameSense.Api.DTOs;
using GameSense.Api.Requests;
using GameSense.Core.Exceptions;
using GameSense.Core.Models;
using GameSense.Core.Repositories;
using GameSense.Core.Services;
using MediatR;

namespace GameSense.Api.Handlers;

public sealed class GetGameHandler(IGameRepository games, IMapper mapper) : IRequestHandler<GetGameQuery, GameResponseDto?>
{
    public async Task<GameResponseDto?> Handle(GetGameQuery request, CancellationToken cancellationToken)
    {
        var game = await games.GetWithReviewsAsync(request.GameId, cancellationToken);
        return game == null ? null : mapper.Map<GameResponseDto>(game);
    }
}

public sealed class CreateReviewHandler(IReviewCreationService reviewCreation, IMapper mapper) : IRequestHandler<CreateReviewCommand, ReviewResponseDto>
{
    public async Task<ReviewResponseDto> Handle(CreateReviewCommand request, CancellationToken cancellationToken)
    {
        var review = await reviewCreation.CreateAsync(request.UserId, request.GameId, request.Request.Title, request.Request.Content, request.Request.Rating, cancellationToken);
        return mapper.Map<ReviewResponseDto>(review);
    }
}
