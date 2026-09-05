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
public sealed class SaveGotyPredictionHandlerTests
{
    [Test]
    public async Task Delegates_prediction_creation_and_maps_the_prediction()
    {
        var prediction = new GotyPrediction { UserId = 1, Year = DateTime.UtcNow.Year, GotyGameId = 1 };
        var service = new Mock<IGotyPredictionService>();
        var ids = new[] { 1, 2, 3, 4, 5, 6 };
        service.Setup(x => x.CreateAsync(1, It.Is<IReadOnlyList<int>>(values => values.SequenceEqual(ids)), 1, It.IsAny<CancellationToken>())).ReturnsAsync(prediction);
        var dto = new GotyPredictionResponseDto(prediction.Year, 1, Array.Empty<GotyNomineeResponseDto>());
        var mapper = new Mock<IMapper>();
        mapper.Setup(x => x.Map<GotyPredictionResponseDto>(prediction)).Returns(dto);

        var result = await new SaveGotyPredictionHandler(service.Object, mapper.Object)
            .Handle(new SaveGotyPredictionCommand(1, new([1, 2, 3, 4, 5, 6], 1)), CancellationToken.None);

        Assert.That(result, Is.SameAs(dto));
    }
}
