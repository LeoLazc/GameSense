using GameSense.Api.Services;
using GameSense.Core.Exceptions;
using GameSense.Core.Models;
using GameSense.Core.Repositories;
using GameSense.Core.Services;
using Moq;
using NUnit.Framework;

namespace GameSense.Api.Tests.Services;

[TestFixture]
public sealed class QuizSessionServiceTests
{
    [Test]
    public async Task SubmitAnswerAsync_returns_null_for_another_users_session()
    {
        var quizzes = new Mock<IQuizRepository>();
        quizzes.Setup(x => x.GetSessionAsync(4, 7, It.IsAny<CancellationToken>())).ReturnsAsync((QuizSession?)null);

        Assert.That(await Create(quizzes).SubmitAnswerAsync(7, 4, 2, "answer"), Is.Null);
    }

    [Test]
    public void SubmitAnswerAsync_rejects_completed_session_and_duplicate_answer()
    {
        var session = Session(QuizSessionStatuses.Completed, Question(2));
        var quizzes = new Mock<IQuizRepository>();
        quizzes.Setup(x => x.GetSessionAsync(4, 7, It.IsAny<CancellationToken>())).ReturnsAsync(session);
        Assert.ThrowsAsync<QuizConflictException>(() => Create(quizzes).SubmitAnswerAsync(7, 4, 2, "answer"));

        session.Status = QuizSessionStatuses.InProgress;
        quizzes.Setup(x => x.HasAnswerAsync(4, 2, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        Assert.ThrowsAsync<QuizConflictException>(() => Create(quizzes).SubmitAnswerAsync(7, 4, 2, "answer"));
    }

    [Test]
    public async Task SubmitAnswerAsync_persists_answer_and_returns_next_question()
    {
        var first = Question(2);
        var next = Question(3);
        var session = Session(QuizSessionStatuses.InProgress, first, next);
        var quizzes = new Mock<IQuizRepository>();
        quizzes.Setup(x => x.GetSessionAsync(4, 7, It.IsAny<CancellationToken>())).ReturnsAsync(session);
        quizzes.Setup(x => x.HasAnswerAsync(4, 2, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        quizzes.Setup(x => x.AddAnswerAsync(It.IsAny<QuizAnswer>(), It.IsAny<CancellationToken>()))
            .Callback<QuizAnswer, CancellationToken>((answer, _) => session.QuizAnswers.Add(answer)).ReturnsAsync((QuizAnswer answer, CancellationToken _) => answer);
        var evaluator = new Mock<IQuizAnswerEvaluator>();
        evaluator.Setup(x => x.EvaluateAsync(first, " answer ", It.IsAny<CancellationToken>())).ReturnsAsync(new QuizEvaluation(80m, .9m, "Good"));

        var result = await Create(quizzes, evaluator).SubmitAnswerAsync(7, 4, 2, " answer ");

        Assert.That(result!.NextQuestion, Is.SameAs(next));
        Assert.That(result.Answered, Is.EqualTo(1));
        Assert.That(session.QuizAnswers.Single().AnswerText, Is.EqualTo("answer"));
    }

    [Test]
    public async Task SubmitAnswerAsync_completes_session_and_uses_weighted_score()
    {
        var first = Question(2, 1m);
        var second = Question(3, 2m);
        var session = Session(QuizSessionStatuses.InProgress, first, second);
        session.QuizAnswers.Add(new QuizAnswer { QuestionId = 2, AiScore = 80m });
        var quizzes = new Mock<IQuizRepository>();
        quizzes.Setup(x => x.GetSessionAsync(4, 7, It.IsAny<CancellationToken>())).ReturnsAsync(session);
        quizzes.Setup(x => x.HasAnswerAsync(4, 3, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        quizzes.Setup(x => x.AddAnswerAsync(It.IsAny<QuizAnswer>(), It.IsAny<CancellationToken>()))
            .Callback<QuizAnswer, CancellationToken>((answer, _) => session.QuizAnswers.Add(answer)).ReturnsAsync((QuizAnswer answer, CancellationToken _) => answer);
        var evaluator = new Mock<IQuizAnswerEvaluator>();
        evaluator.Setup(x => x.EvaluateAsync(second, "answer", It.IsAny<CancellationToken>())).ReturnsAsync(new QuizEvaluation(100m, 1m, "Correct"));

        var result = await Create(quizzes, evaluator).SubmitAnswerAsync(7, 4, 3, "answer");

        Assert.That(result!.Completed, Is.True);
        Assert.That(session.FinalScore, Is.EqualTo(93.33m));
        Assert.That(session.Status, Is.EqualTo(QuizSessionStatuses.Completed));
        quizzes.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    private static QuizSessionService Create(Mock<IQuizRepository> quizzes, Mock<IQuizAnswerEvaluator>? evaluator = null) =>
        new(quizzes.Object, (evaluator ?? new Mock<IQuizAnswerEvaluator>()).Object, new QuizScoringPolicy());

    private static QuizSession Session(string status, params Question[] questions) => new() { Id = 4, UserId = 7, Status = status, QuizSessionQuestions = questions.Select((q, i) => new QuizSessionQuestion { QuestionId = q.Id, Order = i, Question = q }).ToList() };
    private static Question Question(int id, decimal weight = 1m) => new() { Id = id, QuestionText = $"Q{id}", QuestionWeight = weight };
}
