namespace BuildSecrets
{
    public static partial class BuildInjectedSecrets
    {
        public static string CompanionAppClientId
        {
            get
            {
                var value = "";
                SetCompanionAppClientId( ref value );
                return value;
            }
        }

        public static string TelemetryApiKey
        {
            get
            {
                var value = "";
                SetTelemetryApiKey( ref value );
                return value;
            }
        }

        static partial void SetCompanionAppClientId ( ref string value );
        static partial void SetTelemetryApiKey ( ref string value );
    }
}