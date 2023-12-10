using Rollbar;
using System;

namespace Utilities.TelemetryService
{

    internal static class TelemetryExtensions
    {
        public static ErrorLevel ToRollbarErrorLevel ( this Telemetry.ErrorLevel errorLevel )
        {
            if ( Enum.TryParse<ErrorLevel>( errorLevel.ToString(), out var rollbarErrorLevel ) )
            {
                return rollbarErrorLevel;
            }
            throw new ArgumentException( "Invalid telemetry error level:" + errorLevel );
        }
    }
}