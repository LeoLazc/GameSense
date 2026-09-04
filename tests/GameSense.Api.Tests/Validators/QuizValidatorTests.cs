using GameSense.Api.DTOs;
using GameSense.Api.Validators;
using NUnit.Framework;

namespace GameSense.Api.Tests.Validators;

[TestFixture]
public sealed class QuizValidatorTests
{
    [Test]
    public void Submit_answer_rejects_missing_question_and_overlong_answer()
    {
        var validator = new SubmitQuizAnswerDtoValidator();

        var result = validator.Validate(new SubmitQuizAnswerDto(0, new string('x', 4001)));

        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors.Select(x => x.PropertyName), Does.Contain(nameof(SubmitQuizAnswerDto.QuestionId)));
        Assert.That(result.Errors.Select(x => x.PropertyName), Does.Contain(nameof(SubmitQuizAnswerDto.AnswerText)));
    }
}
