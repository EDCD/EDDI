using System;

namespace Utilities.TelemetryService
{
    /// <summary>Base class for exceptions thrown by the telemetry services</summary>
    internal class TelemetryException : Exception
    {
        public TelemetryException () { }

        public TelemetryException ( string message ) : base( message ) { }

        public TelemetryException ( string message, Exception innerException ) : base( message, innerException )
        { }
    }
}
