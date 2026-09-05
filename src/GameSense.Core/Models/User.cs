namespace GameSense.Core.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public decimal? ExpertiseScore { get; set; }

        public ICollection<QuizSession> QuizSessions { get; set; } = new List<QuizSession>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
        public ICollection<GotyPrediction> GotyPredictions { get; set; } = new List<GotyPrediction>();
    }
}
