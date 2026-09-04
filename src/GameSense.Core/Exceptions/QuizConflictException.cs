namespace GameSense.Core.Exceptions;

public sealed class QuizConflictException : InvalidOperationException
{
    public QuizConflictException(string message) : base(message) { }
}
