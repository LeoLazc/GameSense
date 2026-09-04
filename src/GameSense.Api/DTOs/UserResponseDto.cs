namespace GameSense.Api.DTOs
{
    public sealed record UserResponseDto(int Id, string Username, string Email, DateTime CreatedAt, decimal? ExpertiseScore);
}
