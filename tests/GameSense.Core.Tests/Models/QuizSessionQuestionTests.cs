using GameSense.Core.Models;
using NUnit.Framework;

namespace GameSense.Core.Tests.Models;

[TestFixture]
public sealed class QuizSessionQuestionTests
{
    [Test]
    public void Represents_ordered_composite_membership()
    {
        var snapshot = new QuizSessionQuestion { QuizSessionId = 7, QuestionId = 11, Order = 2 };

        Assert.Multiple(() =>
        {
            Assert.That(snapshot.QuizSessionId, Is.EqualTo(7));
            Assert.That(snapshot.QuestionId, Is.EqualTo(11));
            Assert.That(snapshot.Order, Is.EqualTo(2));
        });
    }
}
