using GameSense.Core.Models;
using NUnit.Framework;

namespace GameSense.Core.Tests.Models;

[TestFixture]
public sealed class UserTests
{
    [Test]
    public void Initializes_collection_and_timestamp_without_exposing_a_score()
    {
        var before = DateTime.UtcNow;
        var user = new User();

        Assert.That(user.QuizSessions, Is.Empty);
        Assert.That(user.ExpertiseScore, Is.Null);
        Assert.That(user.CreatedAt, Is.InRange(before, DateTime.UtcNow));
    }
}
