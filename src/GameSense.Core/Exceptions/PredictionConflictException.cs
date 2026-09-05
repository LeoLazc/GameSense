namespace GameSense.Core.Exceptions;

public sealed class PredictionConflictException : Exception
{
    public PredictionConflictException(string message) : base(message) { }
}
