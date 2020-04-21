using System;
using System.Runtime.Serialization;

namespace EddiCompanionAppService
{
    /// <summary>Exceptions thrown due to authentication errors</summary>
    [Serializable]
    public class EliteDangerousCompanionAppAuthenticationException : EliteDangerousCompanionAppException
    {
        public EliteDangerousCompanionAppAuthenticationException() : base() { }

        public EliteDangerousCompanionAppAuthenticationException(string message) : base(message) { }

        public EliteDangerousCompanionAppAuthenticationException(string message, Exception innerException) : base(message, innerException)
        { }

        protected EliteDangerousCompanionAppAuthenticationException(SerializationInfo info, StreamingContext context) : base(info, context)
        { }
    }
}
