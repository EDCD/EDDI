using JetBrains.Annotations;
using System;

namespace EddiCompanionAppService.Exceptions
{
    /// <summary>Exceptions thrown due to authentication errors</summary>
    internal class EliteDangerousCompanionAppAuthenticationException : EliteDangerousCompanionAppException
    {
        [UsedImplicitly] internal EliteDangerousCompanionAppAuthenticationException () { }

        [UsedImplicitly] internal EliteDangerousCompanionAppAuthenticationException (string message) : base(message) { }

        [UsedImplicitly] internal EliteDangerousCompanionAppAuthenticationException (string message, Exception innerException) : base(message, innerException) { }
    }
}
