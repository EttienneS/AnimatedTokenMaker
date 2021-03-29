using System;
using System.Runtime.Serialization;

namespace AnimatedTokenMaker
{
    [Serializable]
    internal class SourceNotReadyException : Exception
    {
        public SourceNotReadyException()
        {
        }

        public SourceNotReadyException(string message) : base(message)
        {
        }

        public SourceNotReadyException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected SourceNotReadyException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}