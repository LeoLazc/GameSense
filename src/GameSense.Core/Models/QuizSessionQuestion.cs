namespace GameSense.Core.Models;

public class QuizSessionQuestion
{
    public int QuizSessionId { get; set; }
    public int QuestionId { get; set; }
    public int Order { get; set; }

    public QuizSession? QuizSession { get; set; }
    public Question? Question { get; set; }
}
