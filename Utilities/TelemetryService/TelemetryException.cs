using System;
using System.Runtime.Serialization;

namespace Utilities.TelemetryService
{
    /// <summary>Base class for exceptions thrown by the telemetry services</summary>
    [Serializable]
    internal class TelemetryException : Exception
    {
        public TelemetryException () { }

        public TelemetryException ( string message ) : base( message ) { }

        public TelemetryException ( string message, Exception innerException ) : base( message, innerException )
        { }

        protected TelemetryException ( SerializationInfo info, StreamingContext context ) : base( info, context )
        { }
    }
}
