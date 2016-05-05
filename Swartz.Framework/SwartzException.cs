using System;
using System.Runtime.Serialization;

namespace Swartz
{
    [Serializable]
    public class SwartzException : ApplicationException
    {
        public SwartzException(string message) : base(message)
        {
        }

        public SwartzException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected SwartzException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}