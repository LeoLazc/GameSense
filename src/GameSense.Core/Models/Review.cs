namespace GameSense.Core.Models
{
    public class Review
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string RawgGameId { get; set; } = null!;
        public string GameTitle { get; set; } = null!;
        public string? GameImageUrl { get; set; }
        public string Title { get; set; } = null!;
        public string Content { get; set; } = null!;
        public int Rating { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public User User { get; set; } = null!;
    }
}
