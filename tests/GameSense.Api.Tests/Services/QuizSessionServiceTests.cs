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
    public async Task SubmitAnswersAsync_returns_null_for_another_users_session()
    {
        var quizzes = new Mock<IQuizRepository>();
        quizzes.Setup(x => x.GetSessionAsync(4, 7, It.IsAny<CancellationToken>())).ReturnsAsync((QuizSession?)null);

        Assert.That(await Create(quizzes).SubmitAnswersAsync(7, 4, [new QuizAnswerSubmission(2, "answer")]), Is.Null);
    }

    [Test]
    public void SubmitAnswersAsync_rejects_incomplete_or_duplicate_submissions()
    {
        var session = Session(QuizSessionStatuses.InProgress, Question(2), Question(3));
        var quizzes = new Mock<IQuizRepository>();
        quizzes.Setup(x => x.GetSessionAsync(4, 7, It.IsAny<CancellationToken>())).ReturnsAsync(session);

        Assert.ThrowsAsync<QuizConflictException>(() => Create(quizzes).SubmitAnswersAsync(7, 4, [new QuizAnswerSubmission(2, "answer")]));
        Assert.ThrowsAsync<QuizConflictException>(() => Create(quizzes).SubmitAnswersAsync(7, 4, [new QuizAnswerSubmission(2, "one"), new QuizAnswerSubmission(2, "two")]));
    }

    [Test]
    public async Task SubmitAnswersAsync_evaluates_the_complete_quiz_once_and_completes_session()
    {
        var first = Question(2);
        var second = Question(3);
        var session = Session(QuizSessionStatuses.InProgress, first, second);
        var quizzes = new Mock<IQuizRepository>();
        quizzes.Setup(x => x.GetSessionAsync(4, 7, It.IsAny<CancellationToken>())).ReturnsAsync(session);
        quizzes.Setup(x => x.AddAnswerAsync(It.IsAny<QuizAnswer>(), It.IsAny<CancellationToken>()))
            .Callback<QuizAnswer, CancellationToken>((answer, _) => session.QuizAnswers.Add(answer))
            .ReturnsAsync((QuizAnswer answer, CancellationToken _) => answer);

        var provider = new Mock<IAiQuizProvider>();
        provider.Setup(x => x.EvaluateBatchAsync(It.Is<IReadOnlyList<QuizBatchAnswer>>(answers => answers.Count == 2), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new QuizBatchEvaluation(87m, .95m, "Evaluación completa"));

        var result = await Create(quizzes, provider).SubmitAnswersAsync(
            7,
            4,
            [new QuizAnswerSubmission(2, "first"), new QuizAnswerSubmission(3, "second")]);

        Assert.That(result!.Completed, Is.True);
        Assert.That(result.Answered, Is.EqualTo(2));
        Assert.That(session.FinalScore, Is.EqualTo(87m));
        Assert.That(session.Status, Is.EqualTo(QuizSessionStatuses.Completed));
        Assert.That(session.QuizAnswers, Has.Count.EqualTo(2));
        provider.VerifyAll();
    }

    private static QuizSessionService Create(Mock<IQuizRepository> quizzes, Mock<IAiQuizProvider>? provider = null) =>
        new(quizzes.Object, (provider ?? new Mock<IAiQuizProvider>()).Object);

    private static QuizSession Session(string status, params Question[] questions) => new()
    {
        Id = 4,
        UserId = 7,
        Status = status,
        QuizSessionQuestions = questions.Select((q, i) => new QuizSessionQuestion { QuestionId = q.Id, Order = i, Question = q }).ToList()
    };

    private static Question Question(int id) => new() { Id = id, QuestionText = $"Q{id}" };
}
