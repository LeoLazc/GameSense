using GameSense.Core.Models;

namespace GameSense.Core.Repositories;

public interface IQuizRepository
{
    Task<IReadOnlyList<Question>> GetActiveQuestionsAsync(CancellationToken cancellationToken = default);
    Task<QuizSession?> GetSessionAsync(int sessionId, int userId, CancellationToken cancellationToken = default);
    Task<bool> HasAnySessionAsync(int userId, CancellationToken cancellationToken = default);
    Task<QuizSession> AddSessionAsync(QuizSession session, CancellationToken cancellationToken = default);
    Task<QuizSession> AddSessionWithQuestionsAsync(QuizSession session, IReadOnlyList<Question> orderedQuestions, CancellationToken cancellationToken = default);
    Task<bool> HasAnswerAsync(int sessionId, int questionId, CancellationToken cancellationToken = default);
    Task<QuizAnswer> AddAnswerAsync(QuizAnswer answer, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
