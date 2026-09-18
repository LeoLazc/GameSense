using AutoMapper;
using GameSense.Api.DTOs;
using GameSense.Api.Requests;
using GameSense.Core.Exceptions;
using GameSense.Core.Models;
using GameSense.Core.Repositories;
using GameSense.Core.Services;
using MediatR;

namespace GameSense.Api.Handlers;

public sealed class GetGameHandler(IGameRepository games, IMapper mapper, IUserRepository? users = null, IReviewRepository? reviews = null) : IRequestHandler<GetGameQuery, GameResponseDto?>
{
    public async Task<GameResponseDto?> Handle(GetGameQuery request, CancellationToken cancellationToken)
    {
        var page = Math.Max(1, request.Page);
        var data = await games.GetReviewPageAsync(request.GameId, page, 10, cancellationToken);
        if (data is null)
        {
            var game = await games.GetWithReviewsAsync(request.GameId, cancellationToken);
            return game == null ? null : mapper.Map<GameResponseDto>(game);
        }
        var dto = mapper.Map<GameResponseDto>(data.Game);
        var eligible = false;
        var hasReviewed = false;
        if (request.ViewerId is int viewerId && users is not null && reviews is not null)
        {
            var user = await users.GetByIdAsync(viewerId, cancellationToken);
            eligible = user?.ExpertiseScore >= 60m;
            hasReviewed = await reviews.ExistsForUserAndGameAsync(viewerId, request.GameId, cancellationToken);
        }
        return dto with
        {
            AverageScore = data.AverageScore,
            VoteCount = data.TotalReviews,
            CurrentPage = page,
            PageSize = 10,
            TotalPages = data.TotalReviews == 0 ? 0 : (int)Math.Ceiling(data.TotalReviews / 10d),
            IsViewerEligible = eligible,
            HasViewerReviewed = hasReviewed
        };
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
