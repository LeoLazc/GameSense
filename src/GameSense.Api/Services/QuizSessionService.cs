using GameSense.Core.Exceptions;
using GameSense.Core.Models;
using GameSense.Core.Repositories;
using GameSense.Core.Services;

namespace GameSense.Api.Services;

public sealed class QuizSessionService(
    IQuizRepository quizzes,
    IQuizAnswerEvaluator evaluator,
    IQuizScoringPolicy scoringPolicy) : IQuizSessionService
{
    public async Task<QuizAnswerSubmissionResult?> SubmitAnswerAsync(
        int userId,
        int sessionId,
        int questionId,
        string answerText,
        CancellationToken cancellationToken = default)
    {
        var session = await quizzes.GetSessionAsync(sessionId, userId, cancellationToken);
        if (session == null) return null;
        if (session.Status != QuizSessionStatuses.InProgress)
            throw new QuizConflictException("The quiz session is not in progress.");

        var snapshotQuestions = session.QuizSessionQuestions
            .OrderBy(snapshot => snapshot.Order)
            .Select(snapshot => snapshot.Question!)
            .ToList();
        if (await quizzes.HasAnswerAsync(sessionId, questionId, cancellationToken))
            throw new QuizConflictException("This question has already been answered.");

        var question = snapshotQuestions.SingleOrDefault(candidate => candidate.Id == questionId);
        if (question == null)
            throw new QuizConflictException("The question is not part of this quiz session.");

        var evaluation = await evaluator.EvaluateAsync(question, answerText, cancellationToken);
        await quizzes.AddAnswerAsync(new QuizAnswer
        {
            QuizSessionId = session.Id,
            QuestionId = question.Id,
            AnswerText = answerText.Trim(),
            AiScore = evaluation.Score,
            AiConfidence = evaluation.Confidence,
            AiEvaluation = evaluation.Evaluation
        }, cancellationToken);

        session = await quizzes.GetSessionAsync(sessionId, userId, cancellationToken)
            ?? throw new QuizConflictException("The quiz session could not be reloaded after recording the answer.");
        var answered = session.QuizAnswers.Count;
        if (answered == snapshotQuestions.Count)
        {
            var expertiseScore = scoringPolicy.Calculate(snapshotQuestions, session.QuizAnswers.ToList());
            session.FinalScore = expertiseScore;
            if (session.User != null) session.User.ExpertiseScore = expertiseScore;
            session.Status = QuizSessionStatuses.Completed;
            session.CompletedAt = DateTime.UtcNow;
            await quizzes.SaveChangesAsync(cancellationToken);
            return new QuizAnswerSubmissionResult(session, null, answered, snapshotQuestions.Count, true);
        }

        var nextQuestion = snapshotQuestions.FirstOrDefault(candidate =>
            session.QuizAnswers.All(answer => answer.QuestionId != candidate.Id));
        return new QuizAnswerSubmissionResult(session, nextQuestion, answered, snapshotQuestions.Count, false);
    }
}
