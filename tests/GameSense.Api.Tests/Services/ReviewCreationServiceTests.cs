using GameSense.Api.Services;
using GameSense.Core.Exceptions;
using GameSense.Core.Models;
using GameSense.Core.Repositories;
using Moq;
using NUnit.Framework;

namespace GameSense.Api.Tests.Services;

[TestFixture]
public sealed class ReviewCreationServiceTests
{
    [Test]
    public async Task CreateAsync_trims_fields_and_persists_authenticated_user()
    {
        var user = new User { Id = 7, Username = "expert", ExpertiseScore = 90m };
        var users = new Mock<IUserRepository>();
        users.Setup(x => x.GetByIdAsync(7, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        var games = new Mock<IGameRepository>();
        games.Setup(x => x.GetWithReviewsAsync(4, It.IsAny<CancellationToken>())).ReturnsAsync(new Game { Id = 4 });
        var reviews = new Mock<IReviewRepository>();
        reviews.Setup(x => x.ExistsForUserAndGameAsync(7, 4, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var result = await new ReviewCreationService(users.Object, games.Object, reviews.Object, new ReviewEligibilityPolicy())
            .CreateAsync(7, 4, " Title ", " Content ", 90);

        Assert.That(result.UserId, Is.EqualTo(7));
        Assert.That(result.Title, Is.EqualTo("Title"));
        Assert.That(result.Content, Is.EqualTo("Content"));
        reviews.Verify(x => x.AddAsync(result, It.IsAny<CancellationToken>()), Times.Once);
        reviews.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public void CreateAsync_rejects_duplicate_before_persistence()
    {
        var users = new Mock<IUserRepository>();
        users.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(new User { Id = 1, ExpertiseScore = 95m });
        var games = new Mock<IGameRepository>();
        games.Setup(x => x.GetWithReviewsAsync(2, It.IsAny<CancellationToken>())).ReturnsAsync(new Game { Id = 2 });
        var reviews = new Mock<IReviewRepository>();
        reviews.Setup(x => x.ExistsForUserAndGameAsync(1, 2, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        Assert.ThrowsAsync<ReviewConflictException>(() => new ReviewCreationService(users.Object, games.Object, reviews.Object, new ReviewEligibilityPolicy())
            .CreateAsync(1, 2, "Title", "Content", 90));
        reviews.Verify(x => x.AddAsync(It.IsAny<Review>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public void CreateAsync_rejects_missing_game()
    {
        var users = new Mock<IUserRepository>();
        users.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(new User { Id = 1, ExpertiseScore = 95m });
        var games = new Mock<IGameRepository>();
        games.Setup(x => x.GetWithReviewsAsync(2, It.IsAny<CancellationToken>())).ReturnsAsync((Game?)null);

        Assert.ThrowsAsync<KeyNotFoundException>(() => new ReviewCreationService(users.Object, games.Object, Mock.Of<IReviewRepository>(), new ReviewEligibilityPolicy())
            .CreateAsync(1, 2, "Title", "Content", 90));
    }
}
