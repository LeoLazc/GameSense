using GameSense.Core.Models;
using GameSense.Core.Services;
using GameSense.Infrastructure.Data;
using GameSense.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace GameSense.Infrastructure.Tests.Repositories;

[TestFixture]
public sealed class GameRepositoryTests
{
    [Test]
    public async Task Upsert_is_duplicate_safe_for_the_same_provider_and_external_id()
    {
        await using var db = new GameSenseDbContext(new DbContextOptionsBuilder<GameSenseDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        var repository = new GameRepository(db);
        var first = Catalog("Game", "10");
        var updated = Catalog("Updated Game", "10");

        var firstResult = await repository.UpsertCatalogGamesAsync([first]);
        var secondResult = await repository.UpsertCatalogGamesAsync([updated]);

        Assert.Multiple(() =>
        {
            Assert.That(secondResult, Has.Count.EqualTo(1));
            Assert.That(secondResult[0].Id, Is.EqualTo(firstResult[0].Id));
            Assert.That(secondResult[0].Name, Is.EqualTo("Updated Game"));
            Assert.That(db.Games.Count(), Is.EqualTo(1));
        });
    }

    [Test]
    public async Task Upsert_deduplicates_repeated_catalog_records_in_one_sync()
    {
        await using var db = new GameSenseDbContext(new DbContextOptionsBuilder<GameSenseDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        var repository = new GameRepository(db);

        var result = await repository.UpsertCatalogGamesAsync([Catalog("Game", "10"), Catalog("Updated Game", "10")]);

        Assert.Multiple(() =>
        {
            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result[0].Name, Is.EqualTo("Updated Game"));
            Assert.That(db.Games.Count(), Is.EqualTo(1));
        });
    }

    [Test]
    public async Task GetRecent_excludes_future_games_and_limits_by_release_date()
    {
        await using var db = new GameSenseDbContext(new DbContextOptionsBuilder<GameSenseDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        db.Games.AddRange(
            new Game { Name = "Future", ReleaseYear = 2027, ReleasedAt = new DateTime(2027, 1, 1) },
            new Game { Name = "Latest", ReleaseYear = 2026, ReleasedAt = new DateTime(2026, 6, 1) },
            new Game { Name = "Earlier", ReleaseYear = 2026, ReleasedAt = new DateTime(2026, 1, 1) });
        await db.SaveChangesAsync();

        var result = await new GameRepository(db).GetRecentAsync(1, new DateTime(2026, 9, 1));

        Assert.That(result.Select(summary => summary.Game.Name), Is.EqualTo(["Latest"]));
    }

    [Test]
    public async Task GetRecent_returns_score_summary_without_loading_review_entities()
    {
        await using var db = new GameSenseDbContext(new DbContextOptionsBuilder<GameSenseDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        var game = new Game { Name = "Reviewed", ReleaseYear = 2026, ReleasedAt = new DateTime(2026, 6, 1) };
        db.Games.Add(game);
        db.Reviews.AddRange(
            new Review { Game = game, User = new User { Username = "one", Email = "one@test.local", PasswordHash = "hash" }, Title = "One", Content = "Content", Rating = 80 },
            new Review { Game = game, User = new User { Username = "two", Email = "two@test.local", PasswordHash = "hash" }, Title = "Two", Content = "Content", Rating = 81 });
        await db.SaveChangesAsync();

        var result = await new GameRepository(db).GetRecentAsync(1, new DateTime(2026, 9, 1));

        Assert.Multiple(() =>
        {
            Assert.That(result[0].AverageScore, Is.EqualTo(81));
            Assert.That(result[0].VoteCount, Is.EqualTo(2));
            Assert.That(result[0].Game.Reviews, Is.Empty);
        });
    }

    [Test]
    public async Task GameExists_returns_presence_without_loading_reviews()
    {
        await using var db = new GameSenseDbContext(new DbContextOptionsBuilder<GameSenseDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        db.Games.Add(new Game { Name = "Game", ReleaseYear = 2026 });
        await db.SaveChangesAsync();

        var repository = new GameRepository(db);

        Assert.That(await repository.GameExistsAsync(1), Is.True);
        Assert.That(await repository.GameExistsAsync(999), Is.False);
    }

    [Test]
    public async Task GetReviewPage_returns_ten_reviews_and_rounds_average_away_from_zero()
    {
        await using var db = new GameSenseDbContext(new DbContextOptionsBuilder<GameSenseDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        var game = new Game { Name = "Reviewed", ReleaseYear = 2026 };
        db.Games.Add(game);
        for (var index = 0; index < 11; index++)
            db.Reviews.Add(new Review { Game = game, User = new User { Username = $"user{index}", Email = $"{index}@test.local", PasswordHash = "hash" }, Title = "Review", Content = "Content", Rating = index == 0 ? 80 : 81, CreatedAt = DateTime.UtcNow.AddMinutes(-index) });
        await db.SaveChangesAsync();

        var result = await new GameRepository(db).GetReviewPageAsync(game.Id, 1, 10);

        Assert.That(result!.TotalReviews, Is.EqualTo(11));
        Assert.That(result.AverageScore, Is.EqualTo(81));
        Assert.That(result.Game.Reviews, Has.Count.EqualTo(10));
        var secondPage = await new GameRepository(db).GetReviewPageAsync(game.Id, 2, 10);
        Assert.That(secondPage!.Game.Reviews, Has.Count.EqualTo(1));
    }

    private static CatalogGame Catalog(string name, string id) => new("rawg", id, "slug", name, "description", new DateTime(2026, 1, 1), 2026, null, null, null, null, null, DateTime.UtcNow);
}
