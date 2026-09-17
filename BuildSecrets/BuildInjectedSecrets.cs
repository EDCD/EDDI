using System.Security.Cryptography;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo( "Tests" )]

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

        public static bool TrySignEddnPayload ( byte[] payload, out string signature )
        {
            return TrySignEddnPayload( payload, GetEddnSigningKey, out signature );
        }

        internal delegate void EddnKeyProvider ( ref byte[]? key );

        // Per-call injection keeps tests independent of private local secrets and shared state.
        internal static bool TrySignEddnPayload ( byte[] payload, EddnKeyProvider keyProvider, out string signature )
        {
            ArgumentNullException.ThrowIfNull( payload );
            signature = string.Empty;
            byte[]? key = null;
            try
            {
                keyProvider( ref key );
                if ( key is null ) { return false; }
                if ( key.Length == 0 ) { throw new InvalidOperationException( "EDDN signing key is empty." ); }
                signature = Convert.ToHexString( HMACSHA256.HashData( key, payload ) ).ToLowerInvariant();
                return true;
            }
            finally
            {
                if ( key is not null ) { CryptographicOperations.ZeroMemory( key ); }
            }
        }

        // Optional partial methods cannot be used directly as delegates.
        private static void GetEddnSigningKey ( ref byte[]? key ) => SetEddnSigningKey( ref key );

        static partial void SetCompanionAppClientId ( ref string value );
        // Supply a fresh buffer per call: TrySignEddnPayload clears it, including on failure.
        static partial void SetEddnSigningKey ( ref byte[]? key );
        static partial void SetTelemetryApiKey ( ref string value );
    }
}
