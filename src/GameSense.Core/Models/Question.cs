namespace GameSense.Core.Models
{
    public class Question
    {
        public int Id { get; set; }
        public int CategoryId { get; set; }
        public int Difficulty { get; set; }
        public string QuestionText { get; set; } = null!;
        public string ExpectedAnswer { get; set; } = null!;
        public string EvaluationCriteria { get; set; } = null!;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public decimal QuestionWeight { get; set; }

        // Navigation
        public QuestionCategory? Category { get; set; }
        public ICollection<QuizAnswer> QuizAnswers { get; set; } = new List<QuizAnswer>();
        public ICollection<QuizSessionQuestion> QuizSessionQuestions { get; set; } = new List<QuizSessionQuestion>();
    }
}
