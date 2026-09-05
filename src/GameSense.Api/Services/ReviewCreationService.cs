using GameSense.Core.Exceptions;
using GameSense.Core.Models;
using GameSense.Core.Repositories;
using GameSense.Core.Services;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace GameSense.Api.Services;

public sealed class ReviewEligibilityPolicy : IReviewEligibilityPolicy
{
    public void EnsureEligible(User? user)
    {
        if (user?.ExpertiseScore is not decimal score || score < 90m)
            throw new ExpertiseEligibilityException("An ExpertiseScore of at least 90 is required to review videogames.");
    }
}

public sealed class ReviewCreationService(
    IUserRepository users,
    IGameRepository games,
    IReviewRepository reviews,
    IReviewEligibilityPolicy eligibilityPolicy) : IReviewCreationService
{
    public async Task<Review> CreateAsync(
        int userId,
        int gameId,
        string title,
        string content,
        int rating,
        CancellationToken cancellationToken = default)
    {
        var user = await users.GetByIdAsync(userId, cancellationToken);
        eligibilityPolicy.EnsureEligible(user);
        if (await games.GetWithReviewsAsync(gameId, cancellationToken) == null)
            throw new KeyNotFoundException("Game not found.");
        if (await reviews.ExistsForUserAndGameAsync(userId, gameId, cancellationToken))
            throw new ReviewConflictException("You have already reviewed this game.");

        var review = new Review
        {
            UserId = userId,
            GameId = gameId,
            Title = title.Trim(),
            Content = content.Trim(),
            Rating = rating,
            User = user!
        };
        try
        {
            await reviews.AddAsync(review, cancellationToken);
            await reviews.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (IsReviewUniqueConstraintViolation(exception))
        {
            throw new ReviewConflictException("You have already reviewed this game.");
        }

        return review;
    }

    private static bool IsReviewUniqueConstraintViolation(DbUpdateException exception)
    {
        for (var current = exception.InnerException; current is not null; current = current.InnerException)
            if (current is SqlException { Number: 2601 or 2627 }) return true;
        return false;
    }
}
