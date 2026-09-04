using AutoMapper;
using GameSense.Api.DTOs;
using GameSense.Api.Requests;
using GameSense.Core.Exceptions;
using GameSense.Core.Models;
using GameSense.Core.Repositories;
using GameSense.Core.Services;
using MediatR;

namespace GameSense.Api.Handlers;

internal static class QuizMapping
{
    public static QuizQuestionDto Question(IMapper mapper, Question question) => mapper.Map<QuizQuestionDto>(question);

    public static QuizSessionDto Session(IMapper mapper, QuizSession session, IReadOnlyList<Question> questions) =>
        new(session.Id, session.StartedAt, session.CompletedAt, session.Status, session.FinalScore,
            questions.Select(question => Question(mapper, question)).ToList(), new QuizProgressDto(session.QuizAnswers.Count, questions.Count));

    public static QuizResultDto Result(IMapper mapper, QuizSession session) => mapper.Map<QuizResultDto>(session);
}

internal static class QuizOrdering
{
    public static IReadOnlyList<Question> ForSession(IReadOnlyList<Question> questions, int sessionId)
    {
        var random = new Random(sessionId);
        var buckets = questions.GroupBy(q => q.Difficulty).Select(group => group.OrderBy(_ => random.Next()).ToList()).ToList();
        buckets = buckets.OrderBy(_ => random.Next()).ToList();

        var ordered = new List<Question>(questions.Count);
        for (var index = 0; ordered.Count < questions.Count; index++)
            foreach (var bucket in buckets)
                if (index < bucket.Count) ordered.Add(bucket[index]);

        return ordered;
    }
}

public sealed class StartQuizHandler(IQuizRepository quizzes, IMapper mapper) : IRequestHandler<StartQuizCommand, QuizSessionDto>
{
    public async Task<QuizSessionDto> Handle(StartQuizCommand request, CancellationToken ct)
    {
        if (await quizzes.HasAnySessionAsync(request.UserId, ct))
            throw new QuizConflictException("This user has already used their one quiz attempt.");

        var questions = await quizzes.GetActiveQuestionsAsync(ct);
        if (questions.Count == 0) throw new QuizConflictException("No active quiz questions are available.");

        var orderedQuestions = QuizOrdering.ForSession(questions, request.UserId);
        var session = await quizzes.AddSessionWithQuestionsAsync(new QuizSession { UserId = request.UserId, Status = QuizSessionStatuses.InProgress }, orderedQuestions, ct);
        var persistedSession = await quizzes.GetSessionAsync(session.Id, request.UserId, ct)
            ?? throw new QuizConflictException("The quiz session could not be reloaded after creating its question snapshot.");
        var persistedQuestions = persistedSession.QuizSessionQuestions.OrderBy(sq => sq.Order).Select(sq => sq.Question!).ToList();
        return QuizMapping.Session(mapper, persistedSession, persistedQuestions);
    }
}

public sealed class SubmitQuizAnswerHandler(IQuizRepository quizzes, IQuizAnswerEvaluator evaluator, IMapper mapper) : IRequestHandler<SubmitQuizAnswerCommand, QuizAnswerResponseDto?>
{
    public async Task<QuizAnswerResponseDto?> Handle(SubmitQuizAnswerCommand request, CancellationToken ct)
    {
        var session = await quizzes.GetSessionAsync(request.SessionId, request.UserId, ct);
        if (session == null) return null;
        if (session.Status != QuizSessionStatuses.InProgress) throw new QuizConflictException("The quiz session is not in progress.");
        var snapshotQuestions = session.QuizSessionQuestions.OrderBy(sq => sq.Order).Select(sq => sq.Question!).ToList();
        if (await quizzes.HasAnswerAsync(request.SessionId, request.QuestionId, ct)) throw new QuizConflictException("This question has already been answered.");

        var question = snapshotQuestions.SingleOrDefault(q => q.Id == request.QuestionId);
        if (question == null) throw new QuizConflictException("The question is not part of this quiz session.");

        var evaluation = await evaluator.EvaluateAsync(question, request.AnswerText, ct);
        await quizzes.AddAnswerAsync(new QuizAnswer
        {
            QuizSessionId = session.Id,
            QuestionId = question.Id,
            AnswerText = request.AnswerText.Trim(),
            AiScore = evaluation.Score,
            AiConfidence = evaluation.Confidence,
            AiEvaluation = evaluation.Evaluation
        }, ct);

        session = await quizzes.GetSessionAsync(request.SessionId, request.UserId, ct)
            ?? throw new QuizConflictException("The quiz session could not be reloaded after recording the answer.");
        var answered = session.QuizAnswers.Count;
        var progress = new QuizProgressDto(answered, snapshotQuestions.Count);
        if (answered < snapshotQuestions.Count)
        {
            var next = snapshotQuestions.FirstOrDefault(q => session.QuizAnswers.All(a => a.QuestionId != q.Id));
            return new QuizAnswerResponseDto(false, progress, next == null ? null : QuizMapping.Question(mapper, next), null);
        }

        var totalWeight = snapshotQuestions.Sum(q => q.QuestionWeight);
        var scores = session.QuizAnswers.ToDictionary(a => a.QuestionId, a => a.AiScore);
        var expertiseScore = totalWeight == 0
            ? 0m
            : Math.Round(snapshotQuestions.Sum(q => q.QuestionWeight * scores.GetValueOrDefault(q.Id)) / totalWeight, 2, MidpointRounding.AwayFromZero);
        session.FinalScore = expertiseScore;
        if (session.User != null)
            session.User.ExpertiseScore = expertiseScore;
        session.Status = QuizSessionStatuses.Completed;
        session.CompletedAt = DateTime.UtcNow;
        await quizzes.SaveChangesAsync(ct);

        return new QuizAnswerResponseDto(true, progress, null, QuizMapping.Result(mapper, session));
    }
}

public sealed class GetQuizResultHandler(IQuizRepository quizzes, IMapper mapper) : IRequestHandler<GetQuizResultQuery, QuizResultDto?>
{
    public async Task<QuizResultDto?> Handle(GetQuizResultQuery request, CancellationToken ct)
    {
        var session = await quizzes.GetSessionAsync(request.SessionId, request.UserId, ct);
        return session == null ? null : QuizMapping.Result(mapper, session);
    }
}
