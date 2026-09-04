using GameSense.Core.Models;
using NUnit.Framework;

namespace GameSense.Core.Tests.Models;

[TestFixture]
public sealed class QuizAnswerTests
{
    [Test]
    public void Initializes_timestamp_and_preserves_answer_fields()
    {
        var answer = new QuizAnswer { AnswerText = "  answer  ", AiScore = 80m, AiConfidence = 0.9m };

        Assert.That(answer.AnswerText, Is.EqualTo("  answer  "));
        Assert.That(answer.AiScore, Is.EqualTo(80m));
        Assert.That(answer.AiConfidence, Is.EqualTo(0.9m));
        Assert.That(answer.AnsweredAt, Is.LessThanOrEqualTo(DateTime.UtcNow));
    }
}
