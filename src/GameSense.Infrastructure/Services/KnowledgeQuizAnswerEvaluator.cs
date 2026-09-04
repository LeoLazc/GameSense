using GameSense.Core.Models;
using GameSense.Core.Services;

namespace GameSense.Infrastructure.Services;

public sealed class KnowledgeQuizAnswerEvaluator : IQuizAnswerEvaluator
{
    private readonly IAiQuizProvider _provider;

    public KnowledgeQuizAnswerEvaluator(IAiQuizProvider provider)
    {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
    }

    public Task<QuizEvaluation> EvaluateAsync(Question question, string answer, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(question);
        return _provider.EvaluateAsync(question.QuestionText, question.ExpectedAnswer, question.EvaluationCriteria, answer, cancellationToken);
    }
}
