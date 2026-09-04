using AutoMapper;
using GameSense.Api.DTOs;
using GameSense.Api.Handlers;
using GameSense.Api.Requests;
using GameSense.Core.Exceptions;
using GameSense.Core.Models;
using GameSense.Core.Repositories;
using GameSense.Core.Services;
using Moq;
using NUnit.Framework;

namespace GameSense.Api.Tests.Handlers;

[TestFixture]
public sealed class GenerateReviewsFromRequestHandlerTests
{
    [TestCase(null)]
    [TestCase(79.99)]
    public void Requires_an_expertise_score_of_at_least_80(decimal? score)
    {
        var users = new Mock<IUserRepository>();
        users.Setup(x => x.GetByIdAsync(7, It.IsAny<CancellationToken>())).ReturnsAsync(new User { Id = 7, ExpertiseScore = score });
        var reviews = new Mock<IReviewService>();
        var mapper = new Mock<IMapper>();
        var handler = new GenerateReviewsFromRequestHandler(reviews.Object, mapper.Object, users.Object);

        Assert.ThrowsAsync<ExpertiseEligibilityException>(() => handler.Handle(
            new GenerateReviewsCommand(7, new ReviewRequestDto { FranchiseNames = new() { "Mario" } }), CancellationToken.None));
        reviews.Verify(x => x.GenerateAndSaveReviewsAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task At_80_calls_service_and_maps_results_without_password_hash()
    {
        var users = new Mock<IUserRepository>();
        users.Setup(x => x.GetByIdAsync(7, It.IsAny<CancellationToken>())).ReturnsAsync(new User { Id = 7, ExpertiseScore = 80m, PasswordHash = "secret" });
        var reviews = new Mock<IReviewService>();
        var mapper = new Mock<IMapper>();
        var entity = new Review { Id = 3, ContentJson = "{}", Franchise = new Franchise { Name = "Mario" } };
        var dto = new ReviewResponseDto { Id = 3, FranchiseName = "Mario", ReviewText = "review" };
        reviews.Setup(x => x.GenerateAndSaveReviewsAsync(It.IsAny<IEnumerable<string>>(), 5, null, It.IsAny<CancellationToken>())).ReturnsAsync(new List<Review> { entity });
        mapper.Setup(x => x.Map<ReviewResponseDto>(entity)).Returns(dto);
        var handler = new GenerateReviewsFromRequestHandler(reviews.Object, mapper.Object, users.Object);

        var result = await handler.Handle(new GenerateReviewsCommand(7, new ReviewRequestDto { FranchiseNames = new() { "Mario" } }), CancellationToken.None);

        Assert.That(result.Single(), Is.SameAs(dto));
        Assert.That(typeof(UserResponseDto).GetProperty(nameof(User.PasswordHash)), Is.Null);
        reviews.Verify(x => x.GenerateAndSaveReviewsAsync(It.Is<IEnumerable<string>>(n => n.Single() == "Mario"), 5, null, It.IsAny<CancellationToken>()), Times.Once);
    }
}
