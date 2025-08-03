using System;

namespace EddiCompanionAppService.Exceptions
{
    /// <summary>Base class for exceptions thrown by the Elite:Dangerous companion app API</summary>
    internal class EliteDangerousCompanionAppException : Exception
    {
        internal EliteDangerousCompanionAppException () { }

        internal EliteDangerousCompanionAppException (string message) : base(message) { }

        internal EliteDangerousCompanionAppException (string message, Exception innerException) : base(message, innerException) { }
    }
}
