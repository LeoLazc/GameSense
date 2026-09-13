using GameSense.Core.Models;

namespace GameSense.Core.Services;

public interface IQuizSessionService
{
    Task<QuizAnswerSubmissionResult?> SubmitAnswersAsync(
        int userId,
        int sessionId,
        IReadOnlyList<QuizAnswerSubmission> answers,
        CancellationToken cancellationToken = default);

}

public sealed record QuizAnswerSubmissionResult(
    QuizSession Session,
    Question? NextQuestion,
    int Answered,
    int Total,
    bool Completed);

public sealed record QuizAnswerSubmission(int QuestionId, string AnswerText);
