using System.Security.Claims;
using GameSense.Api.DTOs;
using GameSense.Api.Requests;
using GameSense.Core.Repositories;
using GameSense.Core.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GameSense.Api.Controllers;

[ApiController, Route("api/games")]
public sealed class GamesController(IMediator mediator, IGameCatalogProvider catalogProvider, IGameRepository games) : ControllerBase
{
    [AllowAnonymous, HttpGet("search")]
    public async Task<ActionResult<IReadOnlyList<GameCatalogResponseDto>>> Search([FromQuery] string query, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(query)) return BadRequest("Query is required.");
        var catalogGames = await catalogProvider.SearchGamesAsync(query, cancellationToken);
        var localGames = await games.UpsertCatalogGamesAsync(catalogGames, cancellationToken);
        return Ok(localGames.Select(ToCatalogDto).ToArray());
    }

    [AllowAnonymous, HttpGet("current-year")]
    public async Task<ActionResult<IReadOnlyList<GameCatalogResponseDto>>> CurrentYear(CancellationToken cancellationToken)
    {
        var catalogGames = await catalogProvider.GetCurrentYearGamesAsync(DateTime.UtcNow.Year, cancellationToken);
        var localGames = await games.UpsertCatalogGamesAsync(catalogGames, cancellationToken);
        return Ok(localGames.Select(ToCatalogDto).ToArray());
    }

    [AllowAnonymous, HttpGet("recent")]
    public async Task<ActionResult<IReadOnlyList<GameCatalogResponseDto>>> Recent([FromQuery] int limit = 10, CancellationToken cancellationToken = default)
    {
        if (limit <= 0) return BadRequest("Limit must be greater than zero.");

        var recentGames = await games.GetRecentAsync(Math.Min(limit, 10), DateTime.UtcNow, cancellationToken);
        return Ok(recentGames.Select(summary => ToCatalogDto(summary.Game, summary.AverageScore, summary.VoteCount)).ToArray());
    }

    [AllowAnonymous, HttpGet("{id:int}")]
    public async Task<ActionResult<GameResponseDto>> Get(int id, [FromQuery] int page = 1, CancellationToken cancellationToken = default)
    {
        var viewerId = int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var parsedId) ? parsedId : (int?)null;
        var result = await mediator.Send(new GetGameQuery(id, page, viewerId), cancellationToken);
        return result == null ? NotFound() : Ok(result);
    }

    [Authorize, HttpPost("{gameId:int}/reviews")]
    public async Task<ActionResult<ReviewResponseDto>> CreateReview(int gameId, CreateReviewRequest request, CancellationToken cancellationToken)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await mediator.Send(new CreateReviewCommand(userId, gameId, request), cancellationToken);
        return Created($"/api/reviews/{result.Id}", result);
    }

    private static GameCatalogResponseDto ToCatalogDto(GameSense.Core.Models.Game game) => new(
        game.Id, game.Name, game.ReleaseYear, game.ExternalId, game.Slug, game.Description, game.ReleasedAt,
        game.CoverImageUrl, game.BackgroundImageUrl, game.WebsiteUrl, game.FranchiseName, game.LastSyncedAt,
        game.Reviews.Count == 0 ? null : (int?)Math.Round(game.Reviews.Average(r => r.Rating), MidpointRounding.AwayFromZero),
        game.Reviews.Count);

    private static GameCatalogResponseDto ToCatalogDto(GameSense.Core.Models.Game game, int? averageScore, int voteCount) => new(
        game.Id, game.Name, game.ReleaseYear, game.ExternalId, game.Slug, game.Description, game.ReleasedAt,
        game.CoverImageUrl, game.BackgroundImageUrl, game.WebsiteUrl, game.FranchiseName, game.LastSyncedAt,
        averageScore, voteCount);
}
