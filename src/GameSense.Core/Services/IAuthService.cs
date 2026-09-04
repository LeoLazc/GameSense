namespace GameSense.Core.Services
{
    public interface IAuthService
    {
        Task<AuthResult> RegisterAsync(string username, string email, string password, CancellationToken cancellationToken = default);
        Task<AuthResult?> LoginAsync(string identifier, string password, CancellationToken cancellationToken = default);
    }

    public sealed record AuthResult(int UserId, string Username, string Email, string AccessToken, DateTime ExpiresAt);
}
