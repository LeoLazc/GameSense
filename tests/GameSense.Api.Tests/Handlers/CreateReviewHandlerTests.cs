using AutoMapper;
using GameSense.Api.DTOs;
using GameSense.Api.Handlers;
using GameSense.Api.Requests;
using GameSense.Core.Models;
using GameSense.Core.Services;
using Moq;
using NUnit.Framework;

namespace GameSense.Api.Tests.Handlers;

[TestFixture]
public sealed class CreateReviewHandlerTests
{
    [Test]
    public async Task Delegates_creation_and_maps_the_review()
    {
        var review = new Review { UserId = 7, GameId = 4, Title = "Title", Content = "Content", Rating = 90 };
        var service = new Mock<IReviewCreationService>();
        service.Setup(x => x.CreateAsync(7, 4, "Title", "Content", 90, It.IsAny<CancellationToken>())).ReturnsAsync(review);
        var dto = new ReviewResponseDto(1, "Title", "Content", 90, review.CreatedAt, review.UpdatedAt, "expert");
        var mapper = new Mock<IMapper>();
        mapper.Setup(x => x.Map<ReviewResponseDto>(review)).Returns(dto);

        var result = await new CreateReviewHandler(service.Object, mapper.Object)
            .Handle(new CreateReviewCommand(7, 4, new("Title", "Content", 90)), CancellationToken.None);

        Assert.That(result, Is.SameAs(dto));
    }
}
