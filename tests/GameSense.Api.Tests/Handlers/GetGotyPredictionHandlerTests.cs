using GameSense.Api.Handlers;
using GameSense.Api.DTOs;
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
        var prediction = new GotyPrediction { UserId = 7, Year = DateTime.UtcNow.Year, GotyRawgGameId = "1" };
        prediction.Nominees.Add(new GotyNominee { RawgGameId = "1", Order = 1 });
        var predictions = new Mock<IGotyPredictionRepository>();
        predictions.Setup(x => x.GetAsync(7, DateTime.UtcNow.Year, It.IsAny<CancellationToken>())).ReturnsAsync(prediction);

        var result = await new GetGotyPredictionHandler(predictions.Object)
            .Handle(new GetGotyPredictionQuery(7), CancellationToken.None);

        Assert.That(result!.GotyGameId, Is.EqualTo("1"));
        Assert.That(result.Nominees.Single().GameId, Is.EqualTo("1"));
        predictions.Verify(x => x.GetAsync(7, DateTime.UtcNow.Year, It.IsAny<CancellationToken>()), Times.Once);
    }
}
