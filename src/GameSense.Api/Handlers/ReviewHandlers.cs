using AutoMapper;
using GameSense.Api.DTOs;
using GameSense.Api.Requests;
using GameSense.Core.Repositories;
using GameSense.Core.Services;
using MediatR;

namespace GameSense.Api.Handlers;

public sealed class GetGameHandler(
    IGameRepository games,
    IGameCatalogProvider catalog,
    IMapper mapper,
    IUserRepository? users = null,
    IReviewRepository? reviews = null) : IRequestHandler<GetGameQuery, GameResponseDto?>
{
    public async Task<GameResponseDto?> Handle(GetGameQuery request, CancellationToken cancellationToken)
    {
        var game = await catalog.GetGameAsync(request.GameId, cancellationToken);
        if (game is null)
            return null;

        var page = Math.Max(1, request.Page);
        var data = await games.GetReviewPageAsync(game, page, 10, cancellationToken);
        var eligible = false;
        var hasReviewed = false;
        if (request.ViewerId is int viewerId && users is not null && reviews is not null)
        {
            var user = await users.GetByIdAsync(viewerId, cancellationToken);
            eligible = user?.ExpertiseScore >= 60m;
            hasReviewed = await reviews.ExistsForUserAndGameAsync(viewerId, request.GameId, cancellationToken);
        }

        return new GameResponseDto(
            game.ExternalId,
            game.Name,
            game.ReleaseYear ?? 0,
            game.FranchiseName,
            mapper.Map<IReadOnlyList<ReviewResponseDto>>(data.Reviews),
            game.Summary,
            game.ReleasedAt,
            game.CoverImageUrl,
            game.BackgroundImageUrl,
            game.WebsiteUrl,
            data.AverageScore,
            data.TotalReviews,
            page,
            10,
            data.TotalReviews == 0 ? 0 : (int)Math.Ceiling(data.TotalReviews / 10d),
            eligible,
            hasReviewed);
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
