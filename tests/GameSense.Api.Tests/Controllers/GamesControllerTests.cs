using GameSense.Api.Controllers;
using GameSense.Core.Models;
using GameSense.Core.Repositories;
using GameSense.Core.Services;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

namespace GameSense.Api.Tests.Controllers;

[TestFixture]
public sealed class GamesControllerTests
{
    [Test]
    public async Task Search_synchronizes_catalog_games_and_returns_local_ids()
    {
        var mediator = new Mock<IMediator>();
        var provider = new Mock<IGameCatalogProvider>();
        var repository = new Mock<IGameRepository>();
        var catalog = new CatalogGame("igdb", "42", "game", "Game", "summary", null, 2026, "cover", "background", "website", null, null, DateTime.UtcNow);
        provider.Setup(x => x.SearchGamesAsync("game", It.IsAny<CancellationToken>())).ReturnsAsync([catalog]);
        repository.Setup(x => x.UpsertCatalogGamesAsync(It.Is<IEnumerable<CatalogGame>>(items => items.Count() == 1), It.IsAny<CancellationToken>()))
            .ReturnsAsync([new Game { Id = 7, Name = "Game", ReleaseYear = 2026, ExternalId = "42", CoverImageUrl = "cover" }]);
        var controller = new GamesController(mediator.Object, provider.Object, repository.Object);

        var result = await controller.Search("game", CancellationToken.None);

        var response = (OkObjectResult)result.Result!;
        var games = (IReadOnlyList<GameSense.Api.DTOs.GameCatalogResponseDto>)response.Value!;
        Assert.That(games.Single().Id, Is.EqualTo(7));
        Assert.That(games.Single().ExternalId, Is.EqualTo("42"));
        repository.Verify(x => x.UpsertCatalogGamesAsync(It.IsAny<IEnumerable<CatalogGame>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Current_year_uses_utc_current_year()
    {
        var provider = new Mock<IGameCatalogProvider>();
        var repository = new Mock<IGameRepository>();
        provider.Setup(x => x.GetCurrentYearGamesAsync(DateTime.UtcNow.Year, It.IsAny<CancellationToken>())).ReturnsAsync([]);
        repository.Setup(x => x.UpsertCatalogGamesAsync(It.IsAny<IEnumerable<CatalogGame>>(), It.IsAny<CancellationToken>())).ReturnsAsync([]);
        var controller = new GamesController(new Mock<IMediator>().Object, provider.Object, repository.Object);

        await controller.CurrentYear(CancellationToken.None);

        provider.Verify(x => x.GetCurrentYearGamesAsync(DateTime.UtcNow.Year, It.IsAny<CancellationToken>()), Times.Once);
    }
}
