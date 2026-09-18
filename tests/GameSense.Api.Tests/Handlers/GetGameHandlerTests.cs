using AutoMapper;
using GameSense.Api.DTOs;
using GameSense.Api.Handlers;
using GameSense.Api.Requests;
using GameSense.Core.Models;
using GameSense.Core.Repositories;
using Moq;
using NUnit.Framework;

namespace GameSense.Api.Tests.Handlers;

[TestFixture]
public sealed class GetGameHandlerTests
{
    [Test]
    public async Task Maps_found_game_and_returns_null_when_missing()
    {
        var game = new Game { Id = 3, Name = "Game" };
        var games = new Mock<IGameRepository>();
        games.Setup(x => x.GetWithReviewsAsync(3, It.IsAny<CancellationToken>())).ReturnsAsync(game);
        games.Setup(x => x.GetWithReviewsAsync(4, It.IsAny<CancellationToken>())).ReturnsAsync((Game?)null);
        var dto = new GameResponseDto(3, "Game", 2026, null, Array.Empty<ReviewResponseDto>());
        var mapper = new Mock<IMapper>();
        mapper.Setup(x => x.Map<GameResponseDto>(game)).Returns(dto);
        var handler = new GetGameHandler(games.Object, mapper.Object);

        Assert.That(await handler.Handle(new GetGameQuery(3), CancellationToken.None), Is.SameAs(dto));
        Assert.That(await handler.Handle(new GetGameQuery(4), CancellationToken.None), Is.Null);
    }

    [Test]
    public async Task Returns_paginated_score_and_viewer_state()
    {
        var game = new Game { Id = 3, Name = "Game" };
        var games = new Mock<IGameRepository>();
        games.Setup(x => x.GetReviewPageAsync(3, 2, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GameReviewPage(game, 11, 81));
        var mapper = new Mock<IMapper>();
        mapper.Setup(x => x.Map<GameResponseDto>(game)).Returns(new GameResponseDto(3, "Game", 2026, null, []));
        var users = new Mock<IUserRepository>();
        users.Setup(x => x.GetByIdAsync(7, It.IsAny<CancellationToken>())).ReturnsAsync(new User { Id = 7, ExpertiseScore = 60m });
        var reviews = new Mock<IReviewRepository>();
        reviews.Setup(x => x.ExistsForUserAndGameAsync(7, 3, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var result = await new GetGameHandler(games.Object, mapper.Object, users.Object, reviews.Object)
            .Handle(new GetGameQuery(3, 2, 7), CancellationToken.None);

        Assert.That(result!.AverageScore, Is.EqualTo(81));
        Assert.That(result.VoteCount, Is.EqualTo(11));
        Assert.That(result.CurrentPage, Is.EqualTo(2));
        Assert.That(result.PageSize, Is.EqualTo(10));
        Assert.That(result.TotalPages, Is.EqualTo(2));
        Assert.That(result.IsViewerEligible, Is.True);
        Assert.That(result.HasViewerReviewed, Is.True);
    }
}
