using System;
using System.Runtime.Serialization;

namespace EddiInaraService
{
    /// <summary>Base class for exceptions thrown by Inara API</summary>
    [Serializable]
    public class InaraException : Exception
    {
        public InaraException() : base() { }

        public InaraException(string message) : base(message) { }

        public InaraException(string message, Exception innerException) : base(message, innerException)
        { }

        protected InaraException(SerializationInfo info, StreamingContext context) : base(info, context)
        { }
    }
}
