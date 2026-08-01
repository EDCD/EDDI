using BuildSecrets;

namespace Utilities.TelemetryService
{
    internal static class TelemetryTokens
    {
        // replace with official token for writing to the telemetry service
        internal static string rollbarToken =>
            string.IsNullOrWhiteSpace( BuildInjectedSecrets.TelemetryApiKey )
                ? null
                : BuildInjectedSecrets.TelemetryApiKey;
    }
}
