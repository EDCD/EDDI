#nullable enable

using System;
using System.IO;
using System.Text;

namespace EddiVoiceAttackAdapter.Logging
{
    internal static class AdapterLogger
    {
        private const long MaxLogFileBytes = 1024 * 1024;

        public static string DataDirectory => Path.Combine(
            Environment.GetFolderPath( Environment.SpecialFolder.ApplicationData ),
            "EDDI" );

        private static readonly object FileLock = new();

        public static void Debug ( string message ) => Write( "DEBUG", message, writeToFile: IsDebugFileLoggingEnabled );

        public static void Info ( string message ) => Write( "INFO", message );

        public static void Warn ( string message ) => Write( "WARN", message );

        public static void Warn ( string message, Exception exception ) => Write( "WARN", message, exception );

        public static void Error ( string message ) => Write( "ERROR", message );

        public static void Error ( string message, Exception exception ) => Write( "ERROR", message, exception );

        private static bool IsDebugFileLoggingEnabled =>
            System.Diagnostics.Debugger.IsAttached ||
            string.Equals(
                Environment.GetEnvironmentVariable( "EDDI_VA_ADAPTER_DEBUG_LOGGING" ),
                "1",
                StringComparison.OrdinalIgnoreCase );

        private static void Write ( string level, string message, Exception? exception = null, bool writeToFile = true )
        {
            var line = $"{DateTimeOffset.Now:O} [{level}] {message}";
            DebuggerLog( line, exception );

            if ( !writeToFile )
            {
                return;
            }

            try
            {
                Directory.CreateDirectory( DataDirectory );
                var logPath = Path.Combine( DataDirectory, "eddi-va-plugin.log" );
                var logText = exception == null
                    ? line + Environment.NewLine
                    : line + Environment.NewLine + exception + Environment.NewLine;

                lock ( FileLock )
                {
                    RotateIfNeeded( logPath, Encoding.UTF8.GetByteCount( logText ) );
                    File.AppendAllText( logPath, logText, Encoding.UTF8 );
                }
            }
            catch
            {
                // VoiceAttack must not fail plugin startup because shim logging is unavailable.
            }
        }

        private static void RotateIfNeeded ( string logPath, int incomingByteCount )
        {
            if ( !File.Exists( logPath ) )
            {
                return;
            }

            var fileInfo = new FileInfo( logPath );
            if ( fileInfo.Length + incomingByteCount <= MaxLogFileBytes )
            {
                return;
            }

            var archivePath = logPath + ".1";
            if ( File.Exists( archivePath ) )
            {
                File.Delete( archivePath );
            }

            File.Move( logPath, archivePath );
        }

        private static void DebuggerLog ( string line, Exception? exception )
        {
            try
            {
                System.Diagnostics.Debug.WriteLine( line );
                if ( exception != null )
                {
                    System.Diagnostics.Debug.WriteLine( exception );
                }
            }
            catch
            {
                // Ignore debugger logging failures.
            }
        }
    }
}
