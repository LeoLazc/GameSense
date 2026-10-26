using FluentValidation;
using GameSense.Api.Services;
using GameSense.Core.Exceptions;
using GameSense.Core.Models;
using GameSense.Core.Repositories;
using GameSense.Core.Services;
using Moq;
using NUnit.Framework;

namespace GameSense.Api.Tests.Services;

[TestFixture]
public sealed class GotyPredictionServiceTests
{
    private static CatalogGame Catalog(string id, int year) =>
        new("rawg", id, id, $"Game {id}", null, new DateTime(year, 1, 1), year, null, null, null, null, null, DateTime.UtcNow);

    [Test]
    public void CreateAsync_requires_six_distinct_nominees_and_includes_goty()
    {
        var service = new GotyPredictionService(Mock.Of<IGameCatalogProvider>(), Mock.Of<IGotyPredictionRepository>());
        Assert.ThrowsAsync<ValidationException>(() => service.CreateAsync(1, ["1", "2", "3", "4", "5", "5"], "1"));
        Assert.ThrowsAsync<ValidationException>(() => service.CreateAsync(1, ["1", "2", "3", "4", "5", "6"], "7"));
    }

    [Test]
    public void CreateAsync_rejects_existing_current_year_prediction()
    {
        var predictions = new Mock<IGotyPredictionRepository>();
        predictions.Setup(x => x.GetAsync(1, DateTime.UtcNow.Year, It.IsAny<CancellationToken>())).ReturnsAsync(new GotyPrediction());
        Assert.ThrowsAsync<PredictionConflictException>(() => new GotyPredictionService(Mock.Of<IGameCatalogProvider>(), predictions.Object)
            .CreateAsync(1, ["1", "2", "3", "4", "5", "6"], "1"));
    }

    [Test]
    public void CreateAsync_requires_all_nominees_to_exist_in_current_year()
    {
        var catalog = new Mock<IGameCatalogProvider>();
        catalog.Setup(x => x.GetGameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string id, CancellationToken _) => Catalog(id, DateTime.UtcNow.Year - 1));
        Assert.ThrowsAsync<ValidationException>(() => new GotyPredictionService(catalog.Object, Mock.Of<IGotyPredictionRepository>())
            .CreateAsync(1, ["1", "2", "3", "4", "5", "6"], "1"));
    }

    [Test]
    public async Task CreateAsync_persists_one_prediction_with_ordered_nominees()
    {
        var catalog = new Mock<IGameCatalogProvider>();
        catalog.Setup(x => x.GetGameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string id, CancellationToken _) => Catalog(id, DateTime.UtcNow.Year));
        var predictions = new Mock<IGotyPredictionRepository>();

        var result = await new GotyPredictionService(catalog.Object, predictions.Object)
            .CreateAsync(9, ["1", "2", "3", "4", "5", "6"], "4");

        Assert.That(result.GotyRawgGameId, Is.EqualTo("4"));
        Assert.That(result.Nominees.Select(x => x.RawgGameId), Is.EqualTo(new[] { "1", "2", "3", "4", "5", "6" }));
        predictions.Verify(x => x.AddAsync(result, It.IsAny<CancellationToken>()), Times.Once);
        predictions.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
