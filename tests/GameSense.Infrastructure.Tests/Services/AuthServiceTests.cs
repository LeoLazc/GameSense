using GameSense.Core.Models;
using GameSense.Infrastructure.Data;
using GameSense.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using NUnit.Framework;

namespace GameSense.Infrastructure.Tests.Services;

[TestFixture]
public sealed class AuthServiceTests
{
    [Test]
    public async Task Register_normalizes_credentials_and_hashes_password()
    {
        await using var db = CreateDb();
        var hasher = new PasswordHasher<User>();
        var service = new AuthService(db, hasher, CreateConfiguration());

        var result = await service.RegisterAsync(" alice ", " ALICE@EXAMPLE.COM ", "secret");
        var saved = await db.Users.SingleAsync();

        Assert.Multiple(() =>
        {
            Assert.That(result.Username, Is.EqualTo("alice"));
            Assert.That(saved.Email, Is.EqualTo("alice@example.com"));
            Assert.That(saved.PasswordHash, Is.Not.EqualTo("secret"));
            Assert.That(hasher.VerifyHashedPassword(saved, saved.PasswordHash, "secret"), Is.EqualTo(PasswordVerificationResult.Success));
        });
    }

    [Test]
    public async Task Register_rejects_duplicate_username_or_email()
    {
        await using var db = CreateDb();
        var service = new AuthService(db, new PasswordHasher<User>(), CreateConfiguration());
        await service.RegisterAsync("alice", "alice@example.com", "secret");

        var exception = Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.RegisterAsync(" alice ", "other@example.com", "secret"));

        Assert.That(exception!.Message, Does.Contain("already registered"));
    }

    [Test]
    public async Task Login_accepts_username_or_case_insensitive_email_and_rejects_bad_password()
    {
        await using var db = CreateDb();
        var service = new AuthService(db, new PasswordHasher<User>(), CreateConfiguration());
        await service.RegisterAsync("alice", "alice@example.com", "secret");

        var byEmail = await service.LoginAsync(" ALICE@EXAMPLE.COM ", "secret");
        var badPassword = await service.LoginAsync("alice", "wrong");

        Assert.That(byEmail, Is.Not.Null);
        Assert.That(byEmail!.AccessToken, Is.Not.Empty);
        Assert.That(badPassword, Is.Null);
    }

    private static GameSenseDbContext CreateDb() => new(new DbContextOptionsBuilder<GameSenseDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString())
        .Options);

    private static IConfiguration CreateConfiguration()
    {
        var section = new Mock<IConfigurationSection>();
        var expiration = new Mock<IConfigurationSection>();
        section.SetupGet(x => x["Key"]).Returns("a-long-test-signing-key-that-is-safe");
        section.SetupGet(x => x["Issuer"]).Returns("tests");
        section.SetupGet(x => x["Audience"]).Returns("tests");
        expiration.SetupGet(x => x.Value).Returns("60");
        section.Setup(x => x.GetSection("ExpirationMinutes")).Returns(expiration.Object);
        var configuration = new Mock<IConfiguration>();
        configuration.Setup(x => x.GetSection("Jwt")).Returns(section.Object);
        return configuration.Object;
    }
}
