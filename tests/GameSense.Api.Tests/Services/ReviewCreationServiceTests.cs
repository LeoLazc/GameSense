using GameSense.Api.Services;
using GameSense.Core.Exceptions;
using GameSense.Core.Repositories;
using GameSense.Core.Services;
using GameSense.Core.Models;
using Moq;
using NUnit.Framework;

namespace GameSense.Api.Tests.Services;

[TestFixture]
public sealed class ReviewCreationServiceTests
{
    private static CatalogGame Catalog(string id = "4", string name = "Game", string? image = "image") =>
        new("rawg", id, "game", name, "summary", DateTime.UtcNow, DateTime.UtcNow.Year, image, "background", null, null, null, DateTime.UtcNow);

    [Test]
    public async Task CreateAsync_trims_fields_and_persists_game_metadata()
    {
        var users = new Mock<IUserRepository>();
        users.Setup(x => x.GetByIdAsync(7, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User { Id = 7, Username = "expert", ExpertiseScore = 60m });
        var catalog = new Mock<IGameCatalogProvider>();
        catalog.Setup(x => x.GetGameAsync("4", It.IsAny<CancellationToken>())).ReturnsAsync(Catalog());
        var reviews = new Mock<IReviewRepository>();

        var result = await new ReviewCreationService(users.Object, catalog.Object, reviews.Object, new ReviewEligibilityPolicy())
            .CreateAsync(7, "4", " Title ", " Content ", 90);

        Assert.Multiple(() =>
        {
            Assert.That(result.RawgGameId, Is.EqualTo("4"));
            Assert.That(result.GameTitle, Is.EqualTo("Game"));
            Assert.That(result.GameImageUrl, Is.EqualTo("image"));
            Assert.That(result.Title, Is.EqualTo("Title"));
            Assert.That(result.Content, Is.EqualTo("Content"));
        });
        reviews.Verify(x => x.AddAsync(result, It.IsAny<CancellationToken>()), Times.Once);
        reviews.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public void CreateAsync_rejects_expertise_score_below_60()
    {
        var users = new Mock<IUserRepository>();
        users.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User { Id = 1, ExpertiseScore = 59.99m });

        Assert.ThrowsAsync<ExpertiseEligibilityException>(() => new ReviewCreationService(
                users.Object,
                Mock.Of<IGameCatalogProvider>(),
                Mock.Of<IReviewRepository>(),
                new ReviewEligibilityPolicy())
            .CreateAsync(1, "2", "Title", "Content", 90));
    }

    [Test]
    public void CreateAsync_rejects_duplicate_before_persistence()
    {
        var users = new Mock<IUserRepository>();
        users.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(new User { Id = 1, ExpertiseScore = 95m });
        var catalog = new Mock<IGameCatalogProvider>();
        catalog.Setup(x => x.GetGameAsync("2", It.IsAny<CancellationToken>())).ReturnsAsync(Catalog("2"));
        var reviews = new Mock<IReviewRepository>();
        reviews.Setup(x => x.ExistsForUserAndGameAsync(1, "2", It.IsAny<CancellationToken>())).ReturnsAsync(true);

        Assert.ThrowsAsync<ReviewConflictException>(() => new ReviewCreationService(users.Object, catalog.Object, reviews.Object, new ReviewEligibilityPolicy())
            .CreateAsync(1, "2", "Title", "Content", 90));
        reviews.Verify(x => x.AddAsync(It.IsAny<Review>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public void CreateAsync_rejects_missing_game()
    {
        var users = new Mock<IUserRepository>();
        users.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(new User { Id = 1, ExpertiseScore = 95m });
        var catalog = new Mock<IGameCatalogProvider>();
        catalog.Setup(x => x.GetGameAsync("2", It.IsAny<CancellationToken>())).ReturnsAsync((CatalogGame?)null);

        Assert.ThrowsAsync<KeyNotFoundException>(() => new ReviewCreationService(
                users.Object, catalog.Object, Mock.Of<IReviewRepository>(), new ReviewEligibilityPolicy())
            .CreateAsync(1, "2", "Title", "Content", 90));
    }
}
