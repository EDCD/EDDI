#nullable enable

using System;

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
            shimDirectory ??= AppContext.BaseDirectory;
            baseDirectory ??= AppContext.BaseDirectory;

            var executablePath = EddiInstallLocator.ResolveExecutablePath(
                shimDirectory,
                store,
                markerFilePath,
                baseDirectory );

            return EddiInstallLocator.ResolveVersion( store, executablePath ) ??
                   typeof( AdapterVersionProvider ).Assembly.GetName().Version?.ToString() ??
                   "unknown";
        }
    }
}
