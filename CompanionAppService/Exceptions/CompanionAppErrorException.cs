using System;
using System.Runtime.Serialization;

namespace EddiCompanionAppService.Exceptions
{
    /// <summary>Exceptions thrown due to API errors</summary>
    [Serializable]
    public class EliteDangerousCompanionAppErrorException : EliteDangerousCompanionAppException
    {
        public EliteDangerousCompanionAppErrorException() { }

        public EliteDangerousCompanionAppErrorException(string message) : base(message) { }

        public EliteDangerousCompanionAppErrorException(string message, Exception innerException) : base(message, innerException)
        { }

        protected EliteDangerousCompanionAppErrorException(SerializationInfo info, StreamingContext context) : base(info, context)
        { }
    }
}
