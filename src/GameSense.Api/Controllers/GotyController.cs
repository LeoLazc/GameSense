using System.Security.Claims;
using GameSense.Api.DTOs;
using GameSense.Api.Requests;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GameSense.Api.Controllers;

[ApiController, Authorize, Route("api/goty")]
public sealed class GotyController(IMediator mediator) : ControllerBase
{
    [HttpPut("prediction")]
    public async Task<ActionResult<GotyPredictionResponseDto>> Save(SaveGotyPredictionRequest request, CancellationToken cancellationToken)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return Ok(await mediator.Send(new SaveGotyPredictionCommand(userId, request), cancellationToken));
    }

    [HttpGet("prediction")]
    public async Task<ActionResult<GotyPredictionResponseDto>> Get(CancellationToken cancellationToken)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await mediator.Send(new GetGotyPredictionQuery(userId), cancellationToken);
        return result == null ? NotFound() : Ok(result);
    }
}
