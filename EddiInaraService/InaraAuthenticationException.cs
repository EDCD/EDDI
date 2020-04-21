using System;
using System.Runtime.Serialization;

namespace EddiInaraService  
{
    /// <summary>Exceptions thrown due to authentication errors</summary>
    [Serializable]
    public class InaraAuthenticationException : InaraException
    {
        public InaraAuthenticationException() : base() { }

        public InaraAuthenticationException(string message) : base(message) { }

        public InaraAuthenticationException(string message, Exception innerException) : base(message, innerException)
        { }

        protected InaraAuthenticationException(SerializationInfo info, StreamingContext context) : base(info, context)
        { }
    }
}
