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
    public async Task Awaits_evaluation_before_persisting_the_answer()
    {
        var session = new QuizSession { Id = 4, UserId = 7, Status = QuizSessionStatuses.InProgress };
        var question = new Question { Id = 2, QuestionText = "Q", ExpectedAnswer = "E", EvaluationCriteria = "C", QuestionWeight = 1m };
        session.QuizSessionQuestions.Add(new QuizSessionQuestion { QuizSessionId = 4, QuestionId = 2, Order = 0, Question = question });
        var quizzes = new Mock<IQuizRepository>();
        quizzes.Setup(x => x.GetSessionAsync(4, 7, It.IsAny<CancellationToken>())).ReturnsAsync(session);
        quizzes.Setup(x => x.HasAnswerAsync(4, 2, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        quizzes.Setup(x => x.AddAnswerAsync(It.IsAny<QuizAnswer>(), It.IsAny<CancellationToken>())).ReturnsAsync(
            (QuizAnswer answer, CancellationToken _) => answer);
        var evaluationReady = new TaskCompletionSource<QuizEvaluation>(TaskCreationOptions.RunContinuationsAsynchronously);
        var evaluationStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var evaluator = new Mock<IQuizAnswerEvaluator>();
        evaluator.Setup(x => x.EvaluateAsync(question, "answer", It.IsAny<CancellationToken>()))
            .Callback(() => evaluationStarted.SetResult())
            .Returns(evaluationReady.Task);
        var handler = new SubmitQuizAnswerHandler(quizzes.Object, evaluator.Object, new Mock<IMapper>().Object);

        var handling = handler.Handle(new SubmitQuizAnswerCommand(7, 4, 2, "answer"), CancellationToken.None);
        await evaluationStarted.Task;
        quizzes.Verify(x => x.AddAnswerAsync(It.IsAny<QuizAnswer>(), It.IsAny<CancellationToken>()), Times.Never);

        evaluationReady.SetResult(new QuizEvaluation(100m, 1m, "Correct"));
        await handling;
        quizzes.Verify(x => x.AddAnswerAsync(It.IsAny<QuizAnswer>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
