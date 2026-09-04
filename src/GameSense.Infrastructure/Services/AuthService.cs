using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using GameSense.Core.Models;
using GameSense.Core.Services;
using GameSense.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace GameSense.Infrastructure.Services
{
    public sealed class AuthService : IAuthService
    {
        private readonly GameSenseDbContext _db;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly IConfiguration _configuration;

        public AuthService(GameSenseDbContext db, IPasswordHasher<User> passwordHasher, IConfiguration configuration)
        {
            _db = db;
            _passwordHasher = passwordHasher;
            _configuration = configuration;
        }

        public async Task<AuthResult> RegisterAsync(string username, string email, string password, CancellationToken cancellationToken = default)
        {
            username = username.Trim();
            email = email.Trim().ToLowerInvariant();

            if (await _db.Users.AnyAsync(u => u.Username == username || u.Email == email, cancellationToken))
                throw new InvalidOperationException("Username or email is already registered.");

            var user = new User { Username = username, Email = email };
            user.PasswordHash = _passwordHasher.HashPassword(user, password);
            _db.Users.Add(user);
            await _db.SaveChangesAsync(cancellationToken);

            return CreateAuthResult(user);
        }

        public async Task<AuthResult?> LoginAsync(string identifier, string password, CancellationToken cancellationToken = default)
        {
            var normalizedIdentifier = identifier.Trim();
            var email = normalizedIdentifier.ToLowerInvariant();
            var user = await _db.Users.FirstOrDefaultAsync(
                u => u.Username == normalizedIdentifier || u.Email == email,
                cancellationToken);

            if (user == null || _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password) == PasswordVerificationResult.Failed)
                return null;

            return CreateAuthResult(user);
        }

        private AuthResult CreateAuthResult(User user)
        {
            var section = _configuration.GetSection("Jwt");
            var key = section["Key"] ?? throw new InvalidOperationException("Jwt:Key is not configured.");
            var issuer = section["Issuer"] ?? throw new InvalidOperationException("Jwt:Issuer is not configured.");
            var audience = section["Audience"] ?? throw new InvalidOperationException("Jwt:Audience is not configured.");
            var expiresAt = DateTime.UtcNow.AddMinutes(section.GetValue("ExpirationMinutes", 60));

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.Username),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username)
            };
            var credentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(issuer, audience, claims, DateTime.UtcNow, expiresAt, credentials);

            return new AuthResult(user.Id, user.Username, user.Email, new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
        }
    }
}
