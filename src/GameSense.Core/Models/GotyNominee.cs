namespace GameSense.Core.Models;

public class GotyNominee
{
    public int Id { get; set; }
    public int PredictionId { get; set; }
    public int GameId { get; set; }
    public int Order { get; set; }

    public GotyPrediction Prediction { get; set; } = null!;
    public Game Game { get; set; } = null!;
}
