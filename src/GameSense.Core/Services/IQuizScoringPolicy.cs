using GameSense.Core.Models;

namespace GameSense.Core.Services;

public interface IQuizScoringPolicy
{
    decimal Calculate(IReadOnlyList<Question> questions, IReadOnlyCollection<QuizAnswer> answers);
}
