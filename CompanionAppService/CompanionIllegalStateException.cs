using System;
using System.Runtime.Serialization;

namespace EddiCompanionAppService
{
    /// <summary>Exceptions thrown due to illegal service state</summary>
    [Serializable]
    public class EliteDangerousCompanionAppIllegalStateException : EliteDangerousCompanionAppException
    {
        public EliteDangerousCompanionAppIllegalStateException() : base() { }

        public EliteDangerousCompanionAppIllegalStateException(string message) : base(message) { }

        public EliteDangerousCompanionAppIllegalStateException(string message, Exception innerException) : base(message, innerException)
        { }

        protected EliteDangerousCompanionAppIllegalStateException(SerializationInfo info, StreamingContext context) : base(info, context)
        { }
    }
}
