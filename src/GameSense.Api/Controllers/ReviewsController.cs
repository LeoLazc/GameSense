using System.Threading.Tasks;
using GameSense.Api.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using GameSense.Api.Requests;

namespace GameSense.Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class ReviewsController : ControllerBase
    {
        private readonly MediatR.IMediator _mediator;

        public ReviewsController(MediatR.IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("generate")]
        public async Task<IActionResult> Generate([FromBody] ReviewRequestDto request)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var results = await _mediator.Send(new GenerateReviewsCommand(userId, request));
            return Ok(results);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> Get(int id)
        {
            var result = await _mediator.Send(new Requests.GetReviewQuery { Id = id });
            if (result == null) return NotFound();
            return Ok(result);
        }
    }
}
