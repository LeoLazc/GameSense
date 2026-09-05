using GameSense.Api.Services;
using GameSense.Core.Models;
using NUnit.Framework;

namespace GameSense.Api.Tests.Services;

[TestFixture]
public sealed class QuizScoringPolicyTests
{
    [Test]
    public void Calculate_uses_question_weights_and_rounds_away_from_zero()
    {
        var questions = new[] { new Question { Id = 1, QuestionWeight = 1m }, new Question { Id = 2, QuestionWeight = 2m } };
        var answers = new[] { new QuizAnswer { QuestionId = 1, AiScore = 80.01m }, new QuizAnswer { QuestionId = 2, AiScore = 90.01m } };

        Assert.That(new QuizScoringPolicy().Calculate(questions, answers), Is.EqualTo(86.68m));
    }

    [Test]
    public void Calculate_returns_zero_when_questions_have_no_weight()
    {
        Assert.That(new QuizScoringPolicy().Calculate([new Question { Id = 1 }], []), Is.Zero);
    }
}
