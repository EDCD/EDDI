using System;

namespace EddiCompanionAppService
{
    /// <summary>Exceptions thrown due to illegal service state</summary>
    [Serializable]
    public class EliteDangerousCompanionAppIllegalStateException : EliteDangerousCompanionAppException
    {
        public EliteDangerousCompanionAppIllegalStateException() : base() { }

        public EliteDangerousCompanionAppIllegalStateException(string message) : base(message) { }
    }
}
