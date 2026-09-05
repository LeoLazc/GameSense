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
}
