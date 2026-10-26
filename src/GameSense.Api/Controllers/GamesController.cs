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
public sealed class GamesController(IMediator mediator, IGameCatalogProvider catalogProvider) : ControllerBase
{
    [AllowAnonymous, HttpGet("search")]
    public async Task<ActionResult<IReadOnlyList<GameCatalogResponseDto>>> Search([FromQuery] string query, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(query)) return BadRequest("Query is required.");
        var catalogGames = await catalogProvider.SearchGamesAsync(query, cancellationToken);
        return Ok(catalogGames.Select(ToCatalogDto).ToArray());
    }

    [AllowAnonymous, HttpGet("current-year")]
    public async Task<ActionResult<IReadOnlyList<GameCatalogResponseDto>>> CurrentYear(CancellationToken cancellationToken)
    {
        var catalogGames = await catalogProvider.GetCurrentYearGamesAsync(DateTime.UtcNow.Year, cancellationToken);
        return Ok(catalogGames.Select(ToCatalogDto).ToArray());
    }

    [AllowAnonymous, HttpGet("recent")]
    public async Task<ActionResult<IReadOnlyList<GameCatalogResponseDto>>> Recent([FromQuery] int limit = 10, CancellationToken cancellationToken = default)
    {
        if (limit <= 0) return BadRequest("Limit must be greater than zero.");

        var recentGames = await catalogProvider.GetRecentGamesAsync(Math.Min(limit, 10), DateTime.UtcNow, cancellationToken);
        return Ok(recentGames.Select(ToCatalogDto).ToArray());
    }

    [AllowAnonymous, HttpGet("{id}")]
    public async Task<ActionResult<GameResponseDto>> Get(string id, [FromQuery] int page = 1, CancellationToken cancellationToken = default)
    {
        var viewerId = int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var parsedId) ? parsedId : (int?)null;
        var result = await mediator.Send(new GetGameQuery(id, page, viewerId), cancellationToken);
        return result == null ? NotFound() : Ok(result);
    }

    [Authorize, HttpPost("{gameId}/reviews")]
    public async Task<ActionResult<ReviewResponseDto>> CreateReview(string gameId, CreateReviewRequest request, CancellationToken cancellationToken)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await mediator.Send(new CreateReviewCommand(userId, gameId, request), cancellationToken);
        return Created($"/api/reviews/{result.Id}", result);
    }

    private static GameCatalogResponseDto ToCatalogDto(CatalogGame game) => new(
        game.ExternalId, game.Name, game.ReleaseYear ?? 0, game.ExternalId, game.Slug, game.Summary, game.ReleasedAt,
        game.CoverImageUrl, game.BackgroundImageUrl, game.WebsiteUrl, game.FranchiseName, game.SyncedAt);
}
