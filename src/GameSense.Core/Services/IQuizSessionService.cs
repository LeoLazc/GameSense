using GameSense.Core.Models;

namespace GameSense.Core.Services;

public interface IQuizSessionService
{
    Task<QuizAnswerSubmissionResult?> SubmitAnswerAsync(
        int userId,
        int sessionId,
        int questionId,
        string answerText,
        CancellationToken cancellationToken = default);
}

public sealed record QuizAnswerSubmissionResult(
    QuizSession Session,
    Question? NextQuestion,
    int Answered,
    int Total,
    bool Completed);
