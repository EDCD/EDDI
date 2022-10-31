﻿using System;
using System.Runtime.Serialization;

namespace EddiCompanionAppService.Exceptions
{
    /// <summary>Base class for exceptions thrown by the Elite:Dangerous companion app API</summary>
    [Serializable]
    public class EliteDangerousCompanionAppException : Exception
    {
        public EliteDangerousCompanionAppException() { }

        public EliteDangerousCompanionAppException(string message) : base(message) { }

        public EliteDangerousCompanionAppException(string message, Exception innerException) : base(message, innerException)
        { }

        protected EliteDangerousCompanionAppException(SerializationInfo info, StreamingContext context) : base(info, context)
        { }
    }
}
