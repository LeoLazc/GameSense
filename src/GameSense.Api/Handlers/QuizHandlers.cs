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

public sealed class SubmitQuizAnswerHandler(IQuizSessionService quizSessions, IMapper mapper) : IRequestHandler<SubmitQuizAnswerCommand, QuizAnswerResponseDto?>
{
    public async Task<QuizAnswerResponseDto?> Handle(SubmitQuizAnswerCommand request, CancellationToken ct)
    {
        var result = await quizSessions.SubmitAnswerAsync(request.UserId, request.SessionId, request.QuestionId, request.AnswerText, ct);
        if (result == null) return null;

        var progress = new QuizProgressDto(result.Answered, result.Total);
        return new QuizAnswerResponseDto(
            result.Completed,
            progress,
            result.NextQuestion == null ? null : QuizMapping.Question(mapper, result.NextQuestion),
            result.Completed ? QuizMapping.Result(mapper, result.Session) : null);
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
