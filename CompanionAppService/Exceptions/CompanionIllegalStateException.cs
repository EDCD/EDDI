using JetBrains.Annotations;
using System;

namespace EddiCompanionAppService.Exceptions
{
    /// <summary>Exceptions thrown due to illegal service state</summary>
    internal class EliteDangerousCompanionAppIllegalStateException : EliteDangerousCompanionAppException
    {
        [UsedImplicitly] internal EliteDangerousCompanionAppIllegalStateException () { }

        [UsedImplicitly] internal EliteDangerousCompanionAppIllegalStateException (string message) : base(message) { }

        [UsedImplicitly] internal EliteDangerousCompanionAppIllegalStateException (string message, Exception innerException) : base(message, innerException) { }
    }
}
