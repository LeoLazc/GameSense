using GameSense.Core.Models;
using GameSense.Core.Exceptions;
using GameSense.Core.Repositories;
using GameSense.Core.Services;
using GameSense.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;

namespace GameSense.Infrastructure.Repositories;

public sealed class QuizRepository : IQuizRepository
{
    private readonly GameSenseDbContext _db;

    public QuizRepository(GameSenseDbContext db) => _db = db;

    public async Task<IReadOnlyList<Question>> GetActiveQuestionsAsync(CancellationToken cancellationToken = default) =>
        await _db.Questions.AsNoTracking().Where(q => q.IsActive).OrderBy(q => q.Id).ToListAsync(cancellationToken);

    public Task<QuizSession?> GetSessionAsync(int sessionId, int userId, CancellationToken cancellationToken = default) =>
        _db.QuizSessions.Include(s => s.User).Include(s => s.QuizAnswers).ThenInclude(a => a.Question)
            .Include(s => s.QuizSessionQuestions).ThenInclude(sq => sq.Question)
            .SingleOrDefaultAsync(s => s.Id == sessionId && s.UserId == userId, cancellationToken);

    public Task<bool> HasAnySessionAsync(int userId, CancellationToken cancellationToken = default) =>
        _db.QuizSessions.AnyAsync(s => s.UserId == userId, cancellationToken);

    public async Task<QuizSession> AddSessionAsync(QuizSession session, CancellationToken cancellationToken = default)
    {
        _db.QuizSessions.Add(session);
        try
        {
            await _db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (ex.GetBaseException() is SqlException { Number: 2601 or 2627 })
        {
            throw new QuizConflictException("This user has already used their one quiz attempt.");
        }
        return session;
    }

    public async Task<QuizSession> AddSessionWithQuestionsAsync(QuizSession session, IReadOnlyList<Question> orderedQuestions, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);
        _db.QuizSessions.Add(session);
        await _db.SaveChangesAsync(cancellationToken);
        var snapshot = orderedQuestions.Select((question, index) => new QuizSessionQuestion
        {
            QuizSessionId = session.Id,
            QuestionId = question.Id,
            Order = index
        }).ToList();
        session.QuizSessionQuestions = snapshot;
        _db.QuizSessionQuestions.AddRange(snapshot);
        try
        {
            await _db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (ex.GetBaseException() is SqlException { Number: 2601 or 2627 })
        {
            await transaction.RollbackAsync(cancellationToken);
            throw new QuizConflictException("This user has already used their one quiz attempt.");
        }
        return session;
    }

    public Task<bool> HasAnswerAsync(int sessionId, int questionId, CancellationToken cancellationToken = default) =>
        _db.QuizAnswers.AnyAsync(a => a.QuizSessionId == sessionId && a.QuestionId == questionId, cancellationToken);

    public async Task<QuizAnswer> AddAnswerAsync(QuizAnswer answer, CancellationToken cancellationToken = default)
    {
        _db.QuizAnswers.Add(answer);
        await _db.SaveChangesAsync(cancellationToken);
        return answer;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) => _db.SaveChangesAsync(cancellationToken);
}
