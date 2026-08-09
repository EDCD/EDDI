#nullable enable

using System;
using System.Diagnostics;

namespace EddiVoiceAttackAdapter
{
    internal static class AdapterVersionProvider
    {
        public static string GetDisplayVersion (
            IEddiInstallLocatorStore? store = null,
            string? shimDirectory = null,
            string? markerFilePath = null,
            string? baseDirectory = null )
        {
            var adapterVersion = ResolveAdapterVisibleVersion();
            if ( adapterVersion != null )
            {
                return adapterVersion;
            }

            shimDirectory ??= AppContext.BaseDirectory;
            baseDirectory ??= AppContext.BaseDirectory;

            var executablePath = EddiInstallLocator.ResolveExecutablePath(
                shimDirectory,
                store,
                markerFilePath,
                baseDirectory );

            return EddiInstallLocator.ResolveExecutableVersion( executablePath ) ??
                   EddiInstallLocator.ResolveRegistryVersion( store ) ??
                   typeof( AdapterVersionProvider ).Assembly.GetName().Version?.ToString() ??
                   "unknown";
        }

        private static string? ResolveAdapterVisibleVersion ()
        {
            try
            {
                var versionInfo = FileVersionInfo.GetVersionInfo( typeof( AdapterVersionProvider ).Assembly.Location );
                return NormalizeVersion( versionInfo.ProductVersion ) ??
                       NormalizeVersion( versionInfo.FileVersion );
            }
            catch
            {
                return null;
            }
        }

        private static string? NormalizeVersion ( string? version )
        {
            return string.IsNullOrWhiteSpace( version )
                ? null
                : version.Trim();
        }
    }
}
