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
    public async Task Delegates_prediction_creation_and_returns_the_prediction()
    {
        var prediction = new GotyPrediction { UserId = 1, Year = DateTime.UtcNow.Year, GotyRawgGameId = "1" };
        var service = new Mock<IGotyPredictionService>();
        var ids = new[] { "1", "2", "3", "4", "5", "6" };
        service.Setup(x => x.CreateAsync(1, It.Is<IReadOnlyList<string>>(values => values.SequenceEqual(ids)), "1", It.IsAny<CancellationToken>())).ReturnsAsync(prediction);

        var result = await new SaveGotyPredictionHandler(service.Object)
            .Handle(new SaveGotyPredictionCommand(1, new(ids, "1")), CancellationToken.None);

        Assert.That(result.GotyGameId, Is.EqualTo("1"));
    }
}
