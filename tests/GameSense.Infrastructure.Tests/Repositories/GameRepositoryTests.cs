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

    private static CatalogGame Catalog(string name, string id) => new("rawg", id, "slug", name, "description", new DateTime(2026, 1, 1), 2026, null, null, null, null, null, DateTime.UtcNow);
}
