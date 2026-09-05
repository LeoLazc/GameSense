using AutoMapper;
using GameSense.Api.DTOs;
using GameSense.Api.Handlers;
using GameSense.Api.Requests;
using GameSense.Core.Models;
using GameSense.Core.Repositories;
using GameSense.Core.Services;
using Moq;
using NUnit.Framework;

namespace GameSense.Api.Tests.Handlers;

[TestFixture]
public sealed class SubmitQuizAnswerHandlerTests
{
    [Test]
    public async Task Maps_service_result_to_response()
    {
        var question = new Question { Id = 2, QuestionText = "Q" };
        var mapper = new Mock<IMapper>();
        mapper.Setup(x => x.Map<QuizQuestionDto>(question)).Returns(new QuizQuestionDto(2, 0, 0, "Q", 0m));
        var service = new Mock<IQuizSessionService>();
        service.Setup(x => x.SubmitAnswerAsync(7, 4, 2, "answer", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new QuizAnswerSubmissionResult(new QuizSession { Id = 4 }, question, 1, 2, false));

        var result = await new SubmitQuizAnswerHandler(service.Object, mapper.Object)
            .Handle(new SubmitQuizAnswerCommand(7, 4, 2, "answer"), CancellationToken.None);

        Assert.That(result!.Completed, Is.False);
        Assert.That(result.Progress, Is.EqualTo(new QuizProgressDto(1, 2)));
        Assert.That(result.NextQuestion!.Id, Is.EqualTo(2));
    }
}
