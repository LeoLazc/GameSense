using System.Security.Claims;
using GameSense.Api.DTOs;
using GameSense.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GameSense.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public sealed class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService) => _authService = authService;

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _authService.RegisterAsync(request.Username, request.Email, request.Password, cancellationToken);
                return Created(string.Empty, ToResponse(result));
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<ActionResult<AuthResponse>> Login(LoginRequest request, CancellationToken cancellationToken)
        {
            var result = await _authService.LoginAsync(request.Identifier, request.Password, cancellationToken);
            return result == null ? Unauthorized(new { message = "Invalid credentials." }) : Ok(ToResponse(result));
        }

        [Authorize]
        [HttpPost("logout")]
        public IActionResult Logout() => NoContent();

        [Authorize]
        [HttpGet("me")]
        public IActionResult Me() => Ok(new
        {
            Id = User.FindFirstValue(ClaimTypes.NameIdentifier),
            Username = User.FindFirstValue(ClaimTypes.Name)
        });

        private static AuthResponse ToResponse(AuthResult result) =>
            new(result.UserId, result.Username, result.Email, result.AccessToken, result.ExpiresAt);
    }
}
