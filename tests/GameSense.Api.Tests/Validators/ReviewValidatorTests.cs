using GameSense.Api.DTOs;
using GameSense.Api.Validators;
using NUnit.Framework;

namespace GameSense.Api.Tests.Validators;

[TestFixture]
public sealed class ReviewValidatorTests
{
    [Test]
    public void Create_review_accepts_ratings_from_0_to_100()
    {
        var validator = new CreateReviewRequestValidator();

        Assert.That(validator.Validate(new CreateReviewRequest("Title", "Content", 0)).IsValid, Is.True);
        Assert.That(validator.Validate(new CreateReviewRequest("Title", "Content", 101)).IsValid, Is.False);
    }

    [Test]
    public void Create_review_rejects_content_longer_than_500_characters()
    {
        var content = new string('x', 501);

        Assert.That(new CreateReviewRequestValidator().Validate(new CreateReviewRequest("Title", content, 50)).IsValid, Is.False);
    }

    [Test]
    public void Save_goty_rejects_fewer_than_six_nominees()
    {
        Assert.That(new SaveGotyPredictionRequestValidator().Validate(new SaveGotyPredictionRequest([1, 2, 3], 1)).IsValid, Is.False);
    }
}
