namespace GameSense.Core.Services;

public interface IAiQuizProvider
{
    Task<QuizBatchEvaluation> EvaluateBatchAsync(
        IReadOnlyList<QuizBatchAnswer> answers,
        CancellationToken cancellationToken = default);
}

public sealed record QuizBatchAnswer(
    int QuestionId,
    string QuestionText,
    string ExpectedAnswer,
    string EvaluationCriteria,
    string UserAnswer);

public sealed record QuizBatchEvaluation(decimal Score, decimal Confidence, string Evaluation);
