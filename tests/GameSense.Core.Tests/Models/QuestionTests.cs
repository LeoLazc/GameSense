using GameSense.Core.Models;
using NUnit.Framework;

namespace GameSense.Core.Tests.Models;

[TestFixture]
public sealed class QuestionTests
{
    [Test]
    public void Defaults_to_active_with_empty_answers_and_snapshots()
    {
        var question = new Question();

        Assert.That(question.IsActive, Is.True);
        Assert.That(question.QuizAnswers, Is.Empty);
        Assert.That(question.QuizSessionQuestions, Is.Empty);
        Assert.That(question.QuestionWeight, Is.Zero);
    }
}
