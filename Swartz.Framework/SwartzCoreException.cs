using System;
using System.Runtime.Serialization;

namespace Swartz
{
    [Serializable]
    public class SwartzCoreException : Exception
    {
        public SwartzCoreException(string message) : base(message)
        {
        }

        public SwartzCoreException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected SwartzCoreException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}