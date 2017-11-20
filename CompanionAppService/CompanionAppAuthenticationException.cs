using System;

namespace EddiCompanionAppService
{
    /// <summary>Exceptions thrown due to authentication errors</summary>
    [Serializable]
    public class EliteDangerousCompanionAppAuthenticationException : EliteDangerousCompanionAppException
    {
        public EliteDangerousCompanionAppAuthenticationException() : base() { }

        public EliteDangerousCompanionAppAuthenticationException(string message) : base(message) { }
    }
}
