using System.ComponentModel.DataAnnotations;

namespace GameSense.Api.DTOs
{
    public sealed class RegisterRequest
    {
        [Required, StringLength(100, MinimumLength = 3)]
        public string Username { get; set; } = null!;

        [Required, EmailAddress, StringLength(320)]
        public string Email { get; set; } = null!;

        [Required, StringLength(100, MinimumLength = 8)]
        public string Password { get; set; } = null!;
    }

    public sealed class LoginRequest
    {
        [Required]
        public string Identifier { get; set; } = null!;

        [Required]
        public string Password { get; set; } = null!;
    }

    public sealed record AuthResponse(int UserId, string Username, string Email, string AccessToken, DateTime ExpiresAt);
}
