using AutoMapper;
using GameSense.Api.DTOs;
using GameSense.Api.Handlers;
using GameSense.Api.Requests;
using GameSense.Core.Models;
using GameSense.Core.Repositories;
using GameSense.Core.Services;
using Moq;
using NUnit.Framework;

namespace GameSense.Api.Tests.Handlers;

[TestFixture]
public sealed class GetGameHandlerTests
{
    private static CatalogGame Catalog(string id = "3") =>
        new("rawg", id, "game", "Game", "Description", new DateTime(2026, 1, 1), 2026, "cover", "background", null, null, null, DateTime.UtcNow);

    [Test]
    public async Task Returns_null_when_rawg_game_is_missing()
    {
        var catalog = new Mock<IGameCatalogProvider>();
        catalog.Setup(x => x.GetGameAsync("4", It.IsAny<CancellationToken>())).ReturnsAsync((CatalogGame?)null);
        var handler = new GetGameHandler(Mock.Of<IGameRepository>(), catalog.Object, Mock.Of<IMapper>());

        Assert.That(await handler.Handle(new GetGameQuery("4"), CancellationToken.None), Is.Null);
    }

    [Test]
    public async Task Returns_paginated_reviews_and_viewer_state()
    {
        var game = Catalog();
        var catalog = new Mock<IGameCatalogProvider>();
        catalog.Setup(x => x.GetGameAsync("3", It.IsAny<CancellationToken>())).ReturnsAsync(game);
        var review = new Review { RawgGameId = "3", GameTitle = "Game", User = new User { Username = "reviewer" } };
        var games = new Mock<IGameRepository>();
        games.Setup(x => x.GetReviewPageAsync(game, 2, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GameReviewPage(game, [review], 11, 81));
        var mapper = new Mock<IMapper>();
        mapper.Setup(x => x.Map<IReadOnlyList<ReviewResponseDto>>(It.IsAny<IReadOnlyList<Review>>()))
            .Returns([new ReviewResponseDto(1, "Title", "Content", 81, DateTime.UtcNow, DateTime.UtcNow, "reviewer", "Game", "cover")]);
        var users = new Mock<IUserRepository>();
        users.Setup(x => x.GetByIdAsync(7, It.IsAny<CancellationToken>())).ReturnsAsync(new User { Id = 7, ExpertiseScore = 60m });
        var reviews = new Mock<IReviewRepository>();
        reviews.Setup(x => x.ExistsForUserAndGameAsync(7, "3", It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var result = await new GetGameHandler(games.Object, catalog.Object, mapper.Object, users.Object, reviews.Object)
            .Handle(new GetGameQuery("3", 2, 7), CancellationToken.None);

        Assert.That(result!.Id, Is.EqualTo("3"));
        Assert.That(result.AverageScore, Is.EqualTo(81));
        Assert.That(result.VoteCount, Is.EqualTo(11));
        Assert.That(result.CurrentPage, Is.EqualTo(2));
        Assert.That(result.TotalPages, Is.EqualTo(2));
        Assert.That(result.IsViewerEligible, Is.True);
        Assert.That(result.HasViewerReviewed, Is.True);
    }
}
