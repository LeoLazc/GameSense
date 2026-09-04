namespace GameSense.Core.Models
{
    public class QuizSession
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public DateTime StartedAt { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedAt { get; set; }
        public string Status { get; set; } = null!;
        public decimal? FinalScore { get; set; }

        public User? User { get; set; }
        public ICollection<QuizAnswer> QuizAnswers { get; set; } = new List<QuizAnswer>();
        public ICollection<QuizSessionQuestion> QuizSessionQuestions { get; set; } = new List<QuizSessionQuestion>();
    }
}
