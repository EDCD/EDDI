using System;
using System.Runtime.Serialization;

namespace EddiInaraService
{
    /// <summary>Exceptions thrown due to API errors</summary>
    [Serializable]
    public class InaraErrorException : InaraException
    {
        public InaraErrorException() : base() { }

        public InaraErrorException(string message) : base(message) { }

        public InaraErrorException(string message, Exception innerException) : base(message, innerException)
        { }

        protected InaraErrorException(SerializationInfo info, StreamingContext context) : base(info, context)
        { }
    }
}
