using GameSense.Api.Controllers;
using GameSense.Core.Services;
using MediatR;
using Moq;
using NUnit.Framework;
using Microsoft.AspNetCore.Mvc;

namespace GameSense.Api.Tests.Controllers;

[TestFixture]
public sealed class GamesControllerTests
{
    private static CatalogGame Catalog(string id = "42") =>
        new("rawg", id, "game", "Game", "summary", new DateTime(2026, 1, 2), 2026, "cover", "background", "website", null, null, DateTime.UtcNow);

    [Test]
    public async Task Search_returns_rawg_games_without_persistence()
    {
        var provider = new Mock<IGameCatalogProvider>();
        provider.Setup(x => x.SearchGamesAsync("game", It.IsAny<CancellationToken>())).ReturnsAsync([Catalog()]);
        var controller = new GamesController(Mock.Of<IMediator>(), provider.Object);

        var result = await controller.Search("game", CancellationToken.None);

        var response = (OkObjectResult)result.Result!;
        var games = (IReadOnlyList<GameSense.Api.DTOs.GameCatalogResponseDto>)response.Value!;
        Assert.That(games.Single().Id, Is.EqualTo("42"));
    }

    [Test]
    public async Task Current_year_uses_utc_current_year()
    {
        var provider = new Mock<IGameCatalogProvider>();
        provider.Setup(x => x.GetCurrentYearGamesAsync(DateTime.UtcNow.Year, It.IsAny<CancellationToken>())).ReturnsAsync([]);
        var controller = new GamesController(Mock.Of<IMediator>(), provider.Object);

        await controller.CurrentYear(CancellationToken.None);

        provider.Verify(x => x.GetCurrentYearGamesAsync(DateTime.UtcNow.Year, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Recent_requests_at_most_ten_games()
    {
        var provider = new Mock<IGameCatalogProvider>();
        provider.Setup(x => x.GetRecentGamesAsync(10, It.IsAny<DateTime>(), It.IsAny<CancellationToken>())).ReturnsAsync([]);
        var controller = new GamesController(Mock.Of<IMediator>(), provider.Object);

        var result = await controller.Recent(20, CancellationToken.None);

        Assert.That(result.Result, Is.TypeOf<OkObjectResult>());
        provider.Verify(x => x.GetRecentGamesAsync(10, It.Is<DateTime>(date => date.Kind == DateTimeKind.Utc), It.IsAny<CancellationToken>()), Times.Once);
    }
}
