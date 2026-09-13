using GameSense.Core.Exceptions;
using GameSense.Core.Models;
using GameSense.Core.Repositories;
using GameSense.Core.Services;

namespace GameSense.Api.Services;

public sealed class QuizSessionService(
    IQuizRepository quizzes,
    IAiQuizProvider batchEvaluator) : IQuizSessionService
{
    public async Task<QuizAnswerSubmissionResult?> SubmitAnswersAsync(
        int userId,
        int sessionId,
        IReadOnlyList<QuizAnswerSubmission> answers,
        CancellationToken cancellationToken = default)
    {
        var session = await quizzes.GetSessionAsync(sessionId, userId, cancellationToken);
        if (session == null) return null;
        if (session.Status != QuizSessionStatuses.InProgress)
            throw new QuizConflictException("The quiz session is not in progress.");
        var questions = session.QuizSessionQuestions
            .OrderBy(snapshot => snapshot.Order)
            .Select(snapshot => snapshot.Question!)
            .ToList();
        var questionIds = questions.Select(question => question.Id).ToHashSet();
        if (answers.Count != questions.Count || answers.Select(answer => answer.QuestionId).Distinct().Count() != answers.Count ||
            answers.Any(answer => !questionIds.Contains(answer.QuestionId)))
            throw new QuizConflictException("The submitted answers must contain each quiz question exactly once.");

        if (session.QuizAnswers.Count > 0)
            throw new QuizConflictException("This quiz session already contains submitted answers.");

        var answerByQuestionId = answers.ToDictionary(answer => answer.QuestionId);
        var evaluation = await batchEvaluator.EvaluateBatchAsync(
            questions.Select(question => new QuizBatchAnswer(
                question.Id,
                question.QuestionText,
                question.ExpectedAnswer,
                question.EvaluationCriteria,
                answerByQuestionId[question.Id].AnswerText)).ToList(),
            cancellationToken);

        foreach (var question in questions)
        {
            await quizzes.AddAnswerAsync(new QuizAnswer
            {
                QuizSessionId = session.Id,
                QuestionId = question.Id,
                AnswerText = answerByQuestionId[question.Id].AnswerText.Trim(),
                AiScore = evaluation.Score,
                AiConfidence = evaluation.Confidence,
                AiEvaluation = evaluation.Evaluation
            }, cancellationToken);
        }

        session = await quizzes.GetSessionAsync(sessionId, userId, cancellationToken)
            ?? throw new QuizConflictException("The quiz session could not be reloaded after recording the answers.");
        session.FinalScore = evaluation.Score;
        if (session.User != null) session.User.ExpertiseScore = evaluation.Score;
        session.Status = QuizSessionStatuses.Completed;
        session.CompletedAt = DateTime.UtcNow;
        await quizzes.SaveChangesAsync(cancellationToken);

        return new QuizAnswerSubmissionResult(session, null, questions.Count, questions.Count, true);
    }
}
