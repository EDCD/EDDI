#nullable enable

using Microsoft.Win32;
using System;
using System.IO;

namespace Eddi
{
    internal enum EddiInstallLocatorWriterHive
    {
        CurrentUser
    }

    internal interface IEddiInstallLocatorWriterStore
    {
        bool TryWriteValues (
            EddiInstallLocatorWriterHive hive,
            string executablePath,
            string installDirectory,
            string version );
    }

    internal static class EddiInstallLocatorWriter
    {
        public const string RegistrySubKey = @"Software\EDCD\EDDI";
        public const string ExecutablePathValueName = "ExecutablePath";
        public const string InstallDirectoryValueName = "InstallDirectory";
        public const string VersionValueName = "Version";
        public const string MarkerFileName = "eddi_app_path.txt";

        public static bool TryWriteCurrentUserInstallLocation (
            string? executablePath,
            string version,
            IEddiInstallLocatorWriterStore? store = null )
        {
            if ( string.IsNullOrWhiteSpace( executablePath ) || !IsEddiExecutable( executablePath ) )
            {
                return false;
            }

            var fullExecutablePath = Path.GetFullPath( executablePath );
            var installDirectory = Path.GetDirectoryName( fullExecutablePath );
            if ( string.IsNullOrWhiteSpace( installDirectory ) )
            {
                return false;
            }

            store ??= new RegistryEddiInstallLocatorWriterStore();
            return store.TryWriteValues(
                EddiInstallLocatorWriterHive.CurrentUser,
                fullExecutablePath,
                installDirectory,
                version );
        }

        private static bool IsEddiExecutable ( string executablePath )
        {
            return string.Equals( Path.GetFileName( executablePath ), "EDDI.exe", StringComparison.OrdinalIgnoreCase ) &&
                   File.Exists( executablePath );
        }

        private sealed class RegistryEddiInstallLocatorWriterStore : IEddiInstallLocatorWriterStore
        {
            public bool TryWriteValues (
                EddiInstallLocatorWriterHive hive,
                string executablePath,
                string installDirectory,
                string version )
            {
                try
                {
                    using var key = GetRegistryKey( hive )?.CreateSubKey( RegistrySubKey, writable: true );
                    if ( key == null )
                    {
                        return false;
                    }

                    key.SetValue( ExecutablePathValueName, executablePath, RegistryValueKind.String );
                    key.SetValue( InstallDirectoryValueName, installDirectory, RegistryValueKind.String );
                    key.SetValue( VersionValueName, version, RegistryValueKind.String );
                    return true;
                }
                catch
                {
                    return false;
                }
            }

            private static RegistryKey? GetRegistryKey ( EddiInstallLocatorWriterHive hive )
            {
                return hive switch
                {
                    EddiInstallLocatorWriterHive.CurrentUser => Registry.CurrentUser,
                    _ => null
                };
            }
        }
    }
}
