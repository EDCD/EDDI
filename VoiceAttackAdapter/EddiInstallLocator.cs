#nullable enable

using Microsoft.Win32;
using System;
using System.IO;

namespace EddiVoiceAttackAdapter
{
    internal enum EddiInstallLocatorHive
    {
        CurrentUser,
        LocalMachine
    }

    internal interface IEddiInstallLocatorStore
    {
        string? ReadValue ( EddiInstallLocatorHive hive, string valueName );
    }

    internal static class EddiInstallLocator
    {
        public const string RegistrySubKey = @"Software\EDCD\EDDI";
        public const string ExecutablePathValueName = "ExecutablePath";
        public const string InstallDirectoryValueName = "InstallDirectory";
        public const string VersionValueName = "Version";
        public const string MarkerFileName = "eddi_app_path.txt";

        public static string? ResolveExecutablePath (
            string? shimDirectory,
            IEddiInstallLocatorStore? store = null,
            string? markerFilePath = null,
            string? baseDirectory = null )
        {
            store ??= new RegistryEddiInstallLocatorStore();

            return ResolveRegistryCandidate( store, EddiInstallLocatorHive.CurrentUser, shimDirectory ) ??
                   ResolveRegistryCandidate( store, EddiInstallLocatorHive.LocalMachine, shimDirectory ) ??
                   ResolveMarkerCandidate( markerFilePath ?? GetDefaultMarkerPath( shimDirectory ), shimDirectory ) ??
                   ResolveFallbackCandidate( shimDirectory ) ??
                   ResolveFallbackCandidate( baseDirectory );
        }

        public static string? ResolveVersion ( IEddiInstallLocatorStore? store = null )
        {
            store ??= new RegistryEddiInstallLocatorStore();

            return NormalizeVersion( store.ReadValue( EddiInstallLocatorHive.CurrentUser, VersionValueName ) ) ??
                   NormalizeVersion( store.ReadValue( EddiInstallLocatorHive.LocalMachine, VersionValueName ) );
        }

        private static string? ResolveRegistryCandidate (
            IEddiInstallLocatorStore store,
            EddiInstallLocatorHive hive,
            string? shimDirectory )
        {
            return NormalizeExternalCandidate( store.ReadValue( hive, ExecutablePathValueName ), shimDirectory );
        }

        private static string? ResolveMarkerCandidate ( string? markerFilePath, string? shimDirectory )
        {
            if ( string.IsNullOrWhiteSpace( markerFilePath ) || !File.Exists( markerFilePath ) )
            {
                return null;
            }

            try
            {
                return NormalizeExternalCandidate( File.ReadAllText( markerFilePath ).Trim(), shimDirectory );
            }
            catch
            {
                return null;
            }
        }

        private static string? ResolveFallbackCandidate ( string? directory )
        {
            if ( string.IsNullOrWhiteSpace( directory ) )
            {
                return null;
            }

            return NormalizeExistingEddiExecutable( Path.Combine( directory, "EDDI.exe" ) );
        }

        private static string? NormalizeExternalCandidate ( string? executablePath, string? shimDirectory )
        {
            var candidate = NormalizeExistingEddiExecutable( executablePath );
            if ( candidate == null )
            {
                return null;
            }

            return IsUnderDirectory( candidate, shimDirectory )
                ? null
                : candidate;
        }

        private static string? NormalizeExistingEddiExecutable ( string? executablePath )
        {
            if ( string.IsNullOrWhiteSpace( executablePath ) )
            {
                return null;
            }

            try
            {
                var fullPath = Path.GetFullPath( executablePath.Trim() );
                return IsEddiExecutable( fullPath ) ? fullPath : null;
            }
            catch
            {
                return null;
            }
        }

        private static bool IsEddiExecutable ( string executablePath )
        {
            return string.Equals( Path.GetFileName( executablePath ), "EDDI.exe", StringComparison.OrdinalIgnoreCase ) &&
                   File.Exists( executablePath );
        }

        private static string? NormalizeVersion ( string? version )
        {
            return string.IsNullOrWhiteSpace( version )
                ? null
                : version.Trim();
        }

        private static string? GetDefaultMarkerPath ( string? shimDirectory )
        {
            return string.IsNullOrWhiteSpace( shimDirectory )
                ? null
                : Path.Combine( shimDirectory, MarkerFileName );
        }

        private static bool IsUnderDirectory ( string candidatePath, string? directory )
        {
            if ( string.IsNullOrWhiteSpace( directory ) )
            {
                return false;
            }

            try
            {
                var candidate = Path.GetFullPath( candidatePath );
                var root = Path.GetFullPath( directory );
                root = root.TrimEnd( Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar ) +
                       Path.DirectorySeparatorChar;

                return candidate.StartsWith( root, StringComparison.OrdinalIgnoreCase );
            }
            catch
            {
                return false;
            }
        }

        private sealed class RegistryEddiInstallLocatorStore : IEddiInstallLocatorStore
        {
            public string? ReadValue ( EddiInstallLocatorHive hive, string valueName )
            {
                try
                {
                    using var key = GetRegistryKey( hive )?.OpenSubKey( RegistrySubKey, writable: false );
                    return key?.GetValue( valueName ) as string;
                }
                catch
                {
                    return null;
                }
            }

            private static RegistryKey? GetRegistryKey ( EddiInstallLocatorHive hive )
            {
                return hive switch
                {
                    EddiInstallLocatorHive.CurrentUser => Registry.CurrentUser,
                    EddiInstallLocatorHive.LocalMachine => Registry.LocalMachine,
                    _ => null
                };
            }
        }
    }
}
