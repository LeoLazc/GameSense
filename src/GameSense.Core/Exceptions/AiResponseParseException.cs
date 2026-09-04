using System;

namespace GameSense.Core.Exceptions
{
    public class AiResponseParseException : Exception
    {
        public AiResponseParseException()
        {
        }

        public AiResponseParseException(string message) : base(message)
        {
        }

        public AiResponseParseException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}
