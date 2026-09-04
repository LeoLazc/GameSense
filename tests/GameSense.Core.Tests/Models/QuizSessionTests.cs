using GameSense.Core.Models;
using NUnit.Framework;

namespace GameSense.Core.Tests.Models;

[TestFixture]
public sealed class QuizSessionTests
{
    [Test]
    public void Defaults_to_empty_relationships_and_uncompleted_state()
    {
        var session = new QuizSession();

        Assert.That(session.CompletedAt, Is.Null);
        Assert.That(session.FinalScore, Is.Null);
        Assert.That(session.QuizAnswers, Is.Empty);
        Assert.That(session.QuizSessionQuestions, Is.Empty);
        Assert.That(session.StartedAt, Is.LessThanOrEqualTo(DateTime.UtcNow));
    }
}
