using System;

namespace EddiCompanionAppService
{
    /// <summary>Exceptions thrown due to API errors</summary>
    [Serializable]
    public class EliteDangerousCompanionAppErrorException : EliteDangerousCompanionAppException
    {
        public EliteDangerousCompanionAppErrorException() : base() { }

        public EliteDangerousCompanionAppErrorException(string message) : base(message) { }
    }
}
