using GameSense.Core.Repositories;
using GameSense.Core.Services;
using GameSense.Infrastructure.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GameSense.Infrastructure.Services;

public sealed class GameCatalogStartupSync(
    IServiceScopeFactory scopeFactory,
    IHostEnvironment environment,
    IOptions<GameCatalogOptions> options,
    ILogger<GameCatalogStartupSync> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (!environment.IsDevelopment() || !options.Value.EnableDevelopmentStartupSync)
        {
            logger.LogDebug("Development game catalog startup sync is disabled for environment {EnvironmentName}.", environment.EnvironmentName);
            return;
        }

        try
        {
            using var scope = scopeFactory.CreateScope();
            var provider = scope.ServiceProvider.GetRequiredService<IGameCatalogProvider>();
            var repository = scope.ServiceProvider.GetRequiredService<IGameRepository>();
            var games = await provider.GetCurrentYearGamesAsync(DateTime.UtcNow.Year, cancellationToken);
            var persisted = await repository.UpsertCatalogGamesAsync(games, cancellationToken);
            logger.LogInformation("Development game catalog startup sync persisted {GameCount} games.", persisted.Count);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            logger.LogWarning("Development game catalog startup sync was canceled.");
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Development game catalog startup sync failed; the API will continue starting.");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
