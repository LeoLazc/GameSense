using GameSense.Core.Models;

namespace GameSense.Core.Services;

public interface IQuizAnswerEvaluator
{
    Task<QuizEvaluation> EvaluateAsync(Question question, string answer, CancellationToken cancellationToken = default);
}

public sealed record QuizEvaluation(decimal Score, decimal Confidence, string Evaluation);

public interface IAiQuizProvider
{
    Task<QuizEvaluation> EvaluateAsync(
        string questionText,
        string expectedAnswer,
        string evaluationCriteria,
        string userAnswer,
        CancellationToken cancellationToken = default);
}
