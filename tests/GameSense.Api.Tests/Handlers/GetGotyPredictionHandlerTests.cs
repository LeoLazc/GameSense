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
public sealed class GetGotyPredictionHandlerTests
{
    [Test]
    public async Task Retrieves_the_prediction_for_the_current_utc_year()
    {
        var prediction = new GotyPrediction { UserId = 7, Year = DateTime.UtcNow.Year };
        var predictions = new Mock<IGotyPredictionRepository>();
        predictions.Setup(x => x.GetAsync(7, DateTime.UtcNow.Year, It.IsAny<CancellationToken>())).ReturnsAsync(prediction);
        var dto = new GotyPredictionResponseDto(prediction.Year, 1, Array.Empty<GotyNomineeResponseDto>());
        var mapper = new Mock<IMapper>();
        mapper.Setup(x => x.Map<GotyPredictionResponseDto>(prediction)).Returns(dto);

        var result = await new GetGotyPredictionHandler(predictions.Object, mapper.Object)
            .Handle(new GetGotyPredictionQuery(7), CancellationToken.None);

        Assert.That(result, Is.SameAs(dto));
        predictions.Verify(x => x.GetAsync(7, DateTime.UtcNow.Year, It.IsAny<CancellationToken>()), Times.Once);
    }
}
