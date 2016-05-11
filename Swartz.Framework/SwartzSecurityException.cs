using System;
using System.Runtime.Serialization;

namespace Swartz
{
    [Serializable]
    public class SwartzSecurityException : SwartzCoreException
    {
        public SwartzSecurityException(string message) : base(message)
        {
        }

        public SwartzSecurityException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public SwartzSecurityException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}