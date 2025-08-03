using JetBrains.Annotations;
using System;

namespace EddiCompanionAppService.Exceptions
{
    /// <summary>Exceptions thrown due to API errors</summary>
    internal class EliteDangerousCompanionAppErrorException : EliteDangerousCompanionAppException
    {
        [UsedImplicitly] internal EliteDangerousCompanionAppErrorException () { }

        [UsedImplicitly] internal EliteDangerousCompanionAppErrorException (string message) : base(message) { }

        [UsedImplicitly] internal EliteDangerousCompanionAppErrorException (string message, Exception innerException) : base(message, innerException) { }
    }
}
