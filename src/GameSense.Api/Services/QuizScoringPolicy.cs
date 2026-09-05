using GameSense.Core.Models;
using GameSense.Core.Services;

namespace GameSense.Api.Services;

public sealed class QuizScoringPolicy : IQuizScoringPolicy
{
    public decimal Calculate(IReadOnlyList<Question> questions, IReadOnlyCollection<QuizAnswer> answers)
    {
        var totalWeight = questions.Sum(question => question.QuestionWeight);
        if (totalWeight == 0) return 0m;

        var scores = answers.ToDictionary(answer => answer.QuestionId, answer => answer.AiScore);
        return Math.Round(
            questions.Sum(question => question.QuestionWeight * scores.GetValueOrDefault(question.Id)) / totalWeight,
            2,
            MidpointRounding.AwayFromZero);
    }
}
