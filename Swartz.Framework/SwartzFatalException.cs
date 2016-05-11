using System;
using System.Runtime.Serialization;

namespace Swartz
{
    [Serializable]
    public class SwartzFatalException : Exception
    {
        public SwartzFatalException(string message) : base(message)
        {
        }

        public SwartzFatalException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected SwartzFatalException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}