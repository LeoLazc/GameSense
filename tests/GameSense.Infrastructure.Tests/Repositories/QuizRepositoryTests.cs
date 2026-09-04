using GameSense.Core.Models;
using GameSense.Infrastructure.Data;
using GameSense.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace GameSense.Infrastructure.Tests.Repositories;

[TestFixture]
public sealed class QuizRepositoryTests
{
    [Test]
    public async Task Returns_only_active_questions_and_enforces_session_ownership()
    {
        await using var db = CreateDb();
        db.Questions.AddRange(TestQuestion(1, true), TestQuestion(2, false));
        db.Users.Add(new User { Id = 3, Username = "owner", Email = "owner@example.com", PasswordHash = "hash" });
        db.QuizSessions.Add(new QuizSession { Id = 10, UserId = 3, Status = "InProgress" });
        await db.SaveChangesAsync();
        var repository = new QuizRepository(db);

        var questions = await repository.GetActiveQuestionsAsync();
        var owned = await repository.GetSessionAsync(10, 3);
        var otherUser = await repository.GetSessionAsync(10, 4);

        Assert.That(questions.Select(q => q.Id), Is.EqualTo(new[] { 1 }));
        Assert.That(owned, Is.Not.Null);
        Assert.That(otherUser, Is.Null);
    }

    [Test]
    public async Task Persists_ordered_question_snapshot_and_reports_existing_answers()
    {
        await using var db = CreateDb();
        db.Questions.AddRange(TestQuestion(1), TestQuestion(2));
        await db.SaveChangesAsync();
        var repository = new QuizRepository(db);
        var session = await repository.AddSessionWithQuestionsAsync(
            new QuizSession { UserId = 8, Status = "InProgress" },
            new[] { await db.Questions.FindAsync(2), await db.Questions.FindAsync(1) }!);

        var snapshots = await db.QuizSessionQuestions.Where(x => x.QuizSessionId == session.Id).OrderBy(x => x.Order).ToListAsync();
        await repository.AddAnswerAsync(new QuizAnswer { QuizSessionId = session.Id, QuestionId = 2, AnswerText = "x", AiEvaluation = "ok" });
        var answerExists = await repository.HasAnswerAsync(session.Id, 2);

        Assert.Multiple(() =>
        {
            Assert.That(snapshots.Select(x => x.QuestionId), Is.EqualTo(new[] { 2, 1 }));
            Assert.That(snapshots.Select(x => x.Order), Is.EqualTo(new[] { 0, 1 }));
            Assert.That(answerExists, Is.True);
        });
    }

    private static GameSenseDbContext CreateDb() => new(new DbContextOptionsBuilder<GameSenseDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString())
        .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
        .Options);

    private static Question TestQuestion(int id, bool active = true) => new()
    {
        Id = id, IsActive = active, QuestionText = $"Question {id}", ExpectedAnswer = "answer",
        EvaluationCriteria = "exact", CategoryId = 1
    };
}
