using EddiConfigService.Configurations;
using System;
using System.Globalization;
using System.Threading;
using Utilities;

namespace EddiConfigService
{
    public static class ApplicationCulture
    {
        public static void ApplyAnyOverrideCulture( EDDIConfiguration configuration = null )
        {
            configuration ??= ConfigService.Instance.eddiConfiguration;

            try
            {
                // we are using the InvariantCulture name "" to mean user's culture
                var overrideCulture = string.IsNullOrEmpty( configuration.OverrideCulture )
                    ? null
                    : new CultureInfo( configuration.OverrideCulture );
                ApplyCulture( overrideCulture );
            }
            catch ( Exception ex )
            {
                Logging.Warn( $"Culture [{configuration.OverrideCulture}] could not be applied; falling back to system culture.", ex );
                ApplyCulture( null );
            }
        }

        private static void ApplyCulture( CultureInfo ci )
        {
            CultureInfo.DefaultThreadCurrentCulture = ci;
            CultureInfo.DefaultThreadCurrentUICulture = ci;
            OverrideThreadCulture( ci );
        }

        private static void OverrideThreadCulture( CultureInfo ci )
        {
            if ( ci == null ) { return; }
            Thread.CurrentThread.CurrentCulture = ci;
            Thread.CurrentThread.CurrentUICulture = ci;
        }
    }
}
