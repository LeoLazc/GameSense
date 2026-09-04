namespace GameSense.Core.Models
{
    public class QuizAnswer
    {
        public int Id { get; set; }
        public int QuizSessionId { get; set; }
        public int QuestionId { get; set; }
        public string AnswerText { get; set; } = null!;
        public decimal AiScore { get; set; }
        public decimal AiConfidence { get; set; }
        public string AiEvaluation { get; set; } = null!;
        public DateTime AnsweredAt { get; set; } = DateTime.UtcNow;

        public QuizSession? QuizSession { get; set; }
        public Question? Question { get; set; }
    }
}
