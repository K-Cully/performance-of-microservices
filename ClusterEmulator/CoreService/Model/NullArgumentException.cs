using System;
using System.Runtime.Serialization;

namespace CoreService.Model
{
    [Serializable]
    internal class NullArgumentException : Exception
    {
        public NullArgumentException()
        {
        }

        public NullArgumentException(string message) : base(message)
        {
        }

        public NullArgumentException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected NullArgumentException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}