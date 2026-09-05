using GameSense.Api.Services;
using GameSense.Core.Exceptions;
using GameSense.Core.Models;
using NUnit.Framework;

namespace GameSense.Api.Tests.Services;

[TestFixture]
public sealed class ReviewEligibilityPolicyTests
{
    [TestCase(null)]
    [TestCase(89.99)]
    public void EnsureEligible_rejects_missing_or_insufficient_expertise(decimal? score)
    {
        Assert.Throws<ExpertiseEligibilityException>(() => new ReviewEligibilityPolicy().EnsureEligible(
            score is null ? null : new User { ExpertiseScore = score }));
    }

    [Test]
    public void EnsureEligible_accepts_score_of_90()
    {
        Assert.DoesNotThrow(() => new ReviewEligibilityPolicy().EnsureEligible(new User { ExpertiseScore = 90m }));
    }
}
