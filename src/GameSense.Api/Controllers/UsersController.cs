using GameSense.Api.DTOs;
using GameSense.Api.Requests;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GameSense.Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public sealed class UsersController : ControllerBase
    {
        private readonly IMediator _mediator;

        public UsersController(IMediator mediator) => _mediator = mediator;

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<UserResponseDto>>> GetAll(CancellationToken cancellationToken)
        {
            var users = await _mediator.Send(new GetUsersQuery(), cancellationToken);
            return Ok(users);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<UserResponseDto>> GetById(int id, CancellationToken cancellationToken)
        {
            var user = await _mediator.Send(new GetUserByIdQuery(id), cancellationToken);
            return user == null ? NotFound() : Ok(user);
        }
    }
}
