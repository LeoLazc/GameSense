using FluentValidation;
using GameSense.Api.Services;
using GameSense.Core.Exceptions;
using GameSense.Core.Models;
using GameSense.Core.Repositories;
using Moq;
using NUnit.Framework;

namespace GameSense.Api.Tests.Services;

[TestFixture]
public sealed class GotyPredictionServiceTests
{
    [Test]
    public void CreateAsync_requires_six_distinct_nominees_and_includes_goty()
    {
        var service = new GotyPredictionService(Mock.Of<IGameRepository>(), Mock.Of<IGotyPredictionRepository>());
        Assert.ThrowsAsync<ValidationException>(() => service.CreateAsync(1, [1, 2, 3, 4, 5, 5], 1));
        Assert.ThrowsAsync<ValidationException>(() => service.CreateAsync(1, [1, 2, 3, 4, 5, 6], 7));
    }

    [Test]
    public void CreateAsync_rejects_existing_current_year_prediction()
    {
        var predictions = new Mock<IGotyPredictionRepository>();
        predictions.Setup(x => x.GetAsync(1, DateTime.UtcNow.Year, It.IsAny<CancellationToken>())).ReturnsAsync(new GotyPrediction());
        Assert.ThrowsAsync<PredictionConflictException>(() => new GotyPredictionService(Mock.Of<IGameRepository>(), predictions.Object)
            .CreateAsync(1, [1, 2, 3, 4, 5, 6], 1));
    }

    [Test]
    public void CreateAsync_requires_all_nominees_to_exist_in_current_year()
    {
        var games = new Mock<IGameRepository>();
        games.Setup(x => x.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Range(1, 6).Select(id => new Game { Id = id, ReleaseYear = DateTime.UtcNow.Year - 1 }).ToList());
        Assert.ThrowsAsync<ValidationException>(() => new GotyPredictionService(games.Object, Mock.Of<IGotyPredictionRepository>())
            .CreateAsync(1, [1, 2, 3, 4, 5, 6], 1));
    }

    [Test]
    public async Task CreateAsync_persists_one_prediction_with_ordered_nominees()
    {
        var games = new Mock<IGameRepository>();
        var found = Enumerable.Range(1, 6).Select(id => new Game { Id = id, ReleaseYear = DateTime.UtcNow.Year }).ToList();
        games.Setup(x => x.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>())).ReturnsAsync(found);
        var predictions = new Mock<IGotyPredictionRepository>();

        var result = await new GotyPredictionService(games.Object, predictions.Object).CreateAsync(9, [1, 2, 3, 4, 5, 6], 4);

        Assert.That(result.UserId, Is.EqualTo(9));
        Assert.That(result.Year, Is.EqualTo(DateTime.UtcNow.Year));
        Assert.That(result.Nominees.Select(x => x.GameId), Is.EqualTo(new[] { 1, 2, 3, 4, 5, 6 }));
        Assert.That(result.Nominees.Select(x => x.Order), Is.EqualTo(new[] { 1, 2, 3, 4, 5, 6 }));
        predictions.Verify(x => x.AddAsync(result, It.IsAny<CancellationToken>()), Times.Once);
        predictions.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
