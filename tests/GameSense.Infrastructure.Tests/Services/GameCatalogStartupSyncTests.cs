using GameSense.Core.Repositories;
using GameSense.Core.Services;
using GameSense.Infrastructure.Options;
using GameSense.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;

namespace GameSense.Infrastructure.Tests.Services;

[TestFixture]
public sealed class GameCatalogStartupSyncTests
{
    [Test]
    public async Task Development_startup_sync_uses_provider_and_duplicate_safe_repository_path()
    {
        var provider = new Mock<IGameCatalogProvider>();
        var catalogGames = new[] { new CatalogGame("fake", "1", "slug", "Game", null, null, 2026, null, null, null, null, null, DateTime.UtcNow) };
        provider.Setup(x => x.GetCurrentYearGamesAsync(DateTime.UtcNow.Year, It.IsAny<CancellationToken>())).ReturnsAsync(catalogGames);
        var repository = new Mock<IGameRepository>();
        repository.Setup(x => x.UpsertCatalogGamesAsync(catalogGames, It.IsAny<CancellationToken>())).ReturnsAsync([]);
        var services = new ServiceCollection().AddScoped(_ => provider.Object).AddScoped(_ => repository.Object).BuildServiceProvider();
        var service = new GameCatalogStartupSync(services.GetRequiredService<IServiceScopeFactory>(),
            new TestHostEnvironment { EnvironmentName = Environments.Development },
            Microsoft.Extensions.Options.Options.Create(new GameCatalogOptions { EnableDevelopmentStartupSync = true }),
            NullLogger<GameCatalogStartupSync>.Instance);

        await service.StartAsync(CancellationToken.None);

        provider.Verify(x => x.GetCurrentYearGamesAsync(DateTime.UtcNow.Year, It.IsAny<CancellationToken>()), Times.Once);
        repository.Verify(x => x.UpsertCatalogGamesAsync(catalogGames, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Provider_failure_is_logged_and_does_not_escape_startup()
    {
        var provider = new Mock<IGameCatalogProvider>();
        provider.Setup(x => x.GetCurrentYearGamesAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("missing API key"));
        var repository = new Mock<IGameRepository>();
        var services = new ServiceCollection().AddScoped(_ => provider.Object).AddScoped(_ => repository.Object).BuildServiceProvider();
        var service = new GameCatalogStartupSync(services.GetRequiredService<IServiceScopeFactory>(),
            new TestHostEnvironment { EnvironmentName = Environments.Development },
            Microsoft.Extensions.Options.Options.Create(new GameCatalogOptions { EnableDevelopmentStartupSync = true }),
            NullLogger<GameCatalogStartupSync>.Instance);

        Assert.DoesNotThrowAsync(() => service.StartAsync(CancellationToken.None));
        repository.Verify(x => x.UpsertCatalogGamesAsync(It.IsAny<IEnumerable<CatalogGame>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task Non_development_does_not_call_provider()
    {
        var provider = new Mock<IGameCatalogProvider>();
        var repository = new Mock<IGameRepository>();
        var services = new ServiceCollection().AddScoped(_ => provider.Object).AddScoped(_ => repository.Object).BuildServiceProvider();
        var service = new GameCatalogStartupSync(services.GetRequiredService<IServiceScopeFactory>(),
            new TestHostEnvironment { EnvironmentName = Environments.Production },
            Microsoft.Extensions.Options.Options.Create(new GameCatalogOptions { EnableDevelopmentStartupSync = true }),
            NullLogger<GameCatalogStartupSync>.Instance);

        await service.StartAsync(CancellationToken.None);

        provider.Verify(x => x.GetCurrentYearGamesAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task Disabled_development_sync_does_not_call_provider()
    {
        var provider = new Mock<IGameCatalogProvider>();
        var repository = new Mock<IGameRepository>();
        var services = new ServiceCollection().AddScoped(_ => provider.Object).AddScoped(_ => repository.Object).BuildServiceProvider();
        var service = new GameCatalogStartupSync(services.GetRequiredService<IServiceScopeFactory>(),
            new TestHostEnvironment { EnvironmentName = Environments.Development },
            Microsoft.Extensions.Options.Options.Create(new GameCatalogOptions { EnableDevelopmentStartupSync = false }),
            NullLogger<GameCatalogStartupSync>.Instance);

        await service.StartAsync(CancellationToken.None);

        provider.Verify(x => x.GetCurrentYearGamesAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    private sealed class TestHostEnvironment : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = Environments.Development;
        public string ApplicationName { get; set; } = "Tests";
        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;
        public Microsoft.Extensions.FileProviders.IFileProvider ContentRootFileProvider { get; set; } = new Microsoft.Extensions.FileProviders.NullFileProvider();
    }
}
