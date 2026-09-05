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
public sealed class StartQuizHandlerTests
{
    [Test]
    public async Task Creates_a_seeded_snapshot_and_maps_the_persisted_session()
    {
        var questions = new List<Question> { new() { Id = 1, Difficulty = 1 }, new() { Id = 2, Difficulty = 2 } };
        var session = new QuizSession { Id = 8, UserId = 7, Status = QuizSessionStatuses.InProgress };
        session.QuizSessionQuestions.Add(new QuizSessionQuestion { Question = questions[0], Order = 0 });
        var quizzes = new Mock<IQuizRepository>();
        quizzes.Setup(x => x.HasAnySessionAsync(7, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        quizzes.Setup(x => x.GetActiveQuestionsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(questions);
        quizzes.Setup(x => x.AddSessionWithQuestionsAsync(It.IsAny<QuizSession>(), It.IsAny<IReadOnlyList<Question>>(), It.IsAny<CancellationToken>())).ReturnsAsync(session);
        quizzes.Setup(x => x.GetSessionAsync(8, 7, It.IsAny<CancellationToken>())).ReturnsAsync(session);
        var dto = new QuizSessionDto(8, session.StartedAt, null, session.Status, null, Array.Empty<QuizQuestionDto>(), new(0, 1));
        var mapper = new Mock<IMapper>();
        mapper.Setup(x => x.Map<QuizQuestionDto>(It.IsAny<Question>())).Returns(new QuizQuestionDto(1, 0, 1, "Q", 0m));

        var result = await new StartQuizHandler(quizzes.Object, mapper.Object).Handle(new StartQuizCommand(7), CancellationToken.None);

        Assert.That(result.Id, Is.EqualTo(8));
        quizzes.Verify(x => x.AddSessionWithQuestionsAsync(It.IsAny<QuizSession>(), It.IsAny<IReadOnlyList<Question>>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
