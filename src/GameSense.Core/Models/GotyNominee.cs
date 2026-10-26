namespace GameSense.Core.Models;

public class GotyNominee
{
    public int Id { get; set; }
    public int PredictionId { get; set; }
    public string RawgGameId { get; set; } = null!;
    public int Order { get; set; }

    public GotyPrediction Prediction { get; set; } = null!;
}
