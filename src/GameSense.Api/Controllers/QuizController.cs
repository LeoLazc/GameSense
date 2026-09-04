using System.Security.Claims;
using GameSense.Api.DTOs;
using GameSense.Api.Requests;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GameSense.Api.Controllers;

[ApiController, Authorize, Route("api/quiz")]
public sealed class QuizController(IMediator mediator) : ControllerBase
{
    private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpPost("sessions")]
    public async Task<ActionResult<QuizSessionDto>> Start(CancellationToken ct) => Ok(await mediator.Send(new StartQuizCommand(UserId), ct));

    [HttpPost("sessions/{sessionId:int}/answers")]
    public async Task<ActionResult<QuizAnswerResponseDto>> Submit(int sessionId, SubmitQuizAnswerDto dto, CancellationToken ct)
    {
        var result = await mediator.Send(new SubmitQuizAnswerCommand(UserId, sessionId, dto.QuestionId, dto.AnswerText), ct);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpGet("sessions/{sessionId:int}/result")]
    public async Task<ActionResult<QuizResultDto>> Result(int sessionId, CancellationToken ct)
    {
        var result = await mediator.Send(new GetQuizResultQuery(UserId, sessionId), ct);
        return result == null ? NotFound() : Ok(result);
    }
}
