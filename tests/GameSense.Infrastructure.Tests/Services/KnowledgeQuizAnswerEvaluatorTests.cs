using GameSense.Core.Models;
using GameSense.Core.Services;
using GameSense.Infrastructure.Services;
using Moq;
using NUnit.Framework;

namespace GameSense.Infrastructure.Tests.Services;

[TestFixture]
public sealed class KnowledgeQuizAnswerEvaluatorTests
{
    [Test]
    public async Task Delegates_the_complete_question_context_to_the_configured_provider()
    {
        var provider = new Mock<IAiQuizProvider>();
        var expected = new QuizEvaluation(73.5m, 0.82m, "Partially correct.");
        provider.Setup(x => x.EvaluateAsync("What is the game?", "A game", "Must mention the genre.", "My answer", It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);
        var evaluator = new KnowledgeQuizAnswerEvaluator(provider.Object);
        var question = new Question
        {
            QuestionText = "What is the game?", ExpectedAnswer = "A game", EvaluationCriteria = "Must mention the genre."
        };

        var result = await evaluator.EvaluateAsync(question, "My answer", CancellationToken.None);

        Assert.That(result, Is.EqualTo(expected));
        provider.Verify(x => x.EvaluateAsync(question.QuestionText, question.ExpectedAnswer, question.EvaluationCriteria, "My answer", It.IsAny<CancellationToken>()), Times.Once);
    }
}
