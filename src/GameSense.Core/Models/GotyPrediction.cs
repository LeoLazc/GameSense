namespace GameSense.Core.Models;

public class GotyPrediction
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int Year { get; set; }
    public int GotyGameId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
    public Game GotyGame { get; set; } = null!;
    public ICollection<GotyNominee> Nominees { get; set; } = new List<GotyNominee>();
}
