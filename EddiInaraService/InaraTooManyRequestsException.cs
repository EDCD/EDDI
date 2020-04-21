using System;
using System.Runtime.Serialization;

namespace EddiInaraService  
{
    /// <summary>Exceptions thrown due to authentication errors</summary>
    [Serializable]
    public class InaraTooManyRequestsException : InaraException
    {
        public InaraTooManyRequestsException() : base() { }

        public InaraTooManyRequestsException(string message) : base(message) { }

        public InaraTooManyRequestsException(string message, Exception innerException) : base(message, innerException)
        { }

        protected InaraTooManyRequestsException(SerializationInfo info, StreamingContext context) : base(info, context)
        { }
    }
}
