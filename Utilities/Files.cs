using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Utilities
{
    public static class Files
    {
        private const int readAttempts = 10;
        private const int readIODelayMilliseconds = 25;
        private const int writeAttempts = 20;
        private const int writeIODelayMilliseconds = 25;

        /// <summary> If true, skips writing to permanent storage </summary>
        public static bool unitTesting { get; set; }

        /// <summary>Attempt to read a file, handling exceptions, and bailing if too many attempts fail</summary>
        /// <param name="fileName">the file to read</param>
        /// <returns>the contents of the file</returns>
        public static string Read(string fileName)
        {
            string result = null;
            var attempts = readAttempts;
            if (fileName != null)
            {
                while (attempts > 0 && TryRead(fileName, attempts, out result))
                {
                    attempts--;
                    Thread.Sleep(readIODelayMilliseconds);
                }
                if (attempts == 0)
                {
                    throw new IOException($"IO read failed for {fileName}, too many attempts.");
                }
            }
            return result;
        }

        /// <summary>Attempt to read a file asynchronously, handling exceptions, and bailing if too many attempts fail</summary>
        /// <param name="fileName">the file to read</param>
        /// <param name="ct">the cancellation token, if any</param>
        /// <returns>the contents of the file</returns>
        public static async Task<string> ReadAsync ( string fileName, CancellationToken ct = default )
        {
            if ( fileName == null )
            {
                return null;
            }

            var attempts = readAttempts;

            while ( attempts > 0 )
            {
                ct.ThrowIfCancellationRequested();

                var (success, result) = await TryReadAsync( fileName, attempts, ct ).ConfigureAwait( false );

                if ( !success )
                {
                    // Either success OR a non-retryable failure
                    return result;
                }

                // Sharing violation → retry
                attempts--;
                await Task.Delay( readIODelayMilliseconds, ct ).ConfigureAwait( false );
            }

            throw new IOException( $"IO read failed for {fileName}, too many attempts." );
        }

        /// <summary>
        /// Read the contents of a file, handling exceptions, and returning a boolean to indicate whether the read was successful
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="attempts"></param>
        /// <param name="result"></param>
        /// <param name="ignoreMissing">Set to true if no exception should be thrown for missing files</param>
        /// <returns></returns>
        private static bool TryRead(string fileName, int attempts, out string result, bool ignoreMissing = false )
        {
            result = null;
            try
            {
                result = File.ReadAllText(fileName, Encoding.UTF8);
            }
            catch (ArgumentException ex)
            {
                Logging.Error("Failed to read from " + fileName, ex);
            }
            catch (PathTooLongException ex)
            {
                Logging.Error("Path " + fileName + " too long", ex);
            }
            catch (DirectoryNotFoundException ex)
            {
                Logging.Error("Directory for " + fileName + " not found", ex);
            }
            catch (FileNotFoundException ex)
            {
                if (fileName.Contains(@"\EDDI\personalities\"))
                {
                    Logging.Warn("Personality " + fileName + " not found", ex);
                }
                if (!ignoreMissing)
                {
                    Logging.Error("File " + fileName + " not found", ex);
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                Logging.Error("Not allowed to read from " + fileName, ex);
            }
            catch (NotSupportedException ex)
            {
                Logging.Error("Not supported reading from " + fileName, ex);
            }
            catch (SecurityException ex)
            {
                Logging.Error("Security exception reading from " + fileName, ex);
            }
            catch (IOException ex) when ((ex.HResult & 0x0000FFFF) == 32) // Sharing violation
            {
                if (!ignoreMissing)
                {
                    Logging.Debug($"IO read exception for {fileName}, {attempts} attempts left", ex);
                    return true; // We have failed to read the file and will need to make another attempt
                }
            }
            catch (IOException ex) // Other IO issue 
            {
                Logging.Error($"IO write exception for {fileName}, {ex.Message}", ex);
            }
            // We have either successfully read the file or encountered an exception that would not benefit from another attempt
            return false;
        }

        private static async Task<(bool retry, string result)> TryReadAsync (
            string fileName,
            int attempts,
            CancellationToken ct,
            bool ignoreMissing = false )
        {
            try
            {
                var text = await Task.Run(() => File.ReadAllText(fileName, Encoding.UTF8), ct).ConfigureAwait(false);
                return (retry: false, result: text);
            }
            catch ( ArgumentException ex )
            {
                Logging.Error( "Failed to read from " + fileName, ex );
            }
            catch ( PathTooLongException ex )
            {
                Logging.Error( "Path " + fileName + " too long", ex );
            }
            catch ( DirectoryNotFoundException ex )
            {
                Logging.Error( "Directory for " + fileName + " not found", ex );
            }
            catch ( FileNotFoundException ex )
            {
                if ( fileName.Contains( @"\EDDI\personalities\" ) )
                {
                    Logging.Warn( "Personality " + fileName + " not found", ex );
                }
                if ( !ignoreMissing )
                {
                    Logging.Error( "File " + fileName + " not found", ex );
                }
            }
            catch ( UnauthorizedAccessException ex )
            {
                Logging.Error( "Not allowed to read from " + fileName, ex );
            }
            catch ( NotSupportedException ex )
            {
                Logging.Error( "Not supported reading from " + fileName, ex );
            }
            catch ( SecurityException ex )
            {
                Logging.Error( "Security exception reading from " + fileName, ex );
            }
            catch ( IOException ex ) when ( ( ex.HResult & 0x0000FFFF ) == 32 ) // Sharing violation
            {
                if ( !ignoreMissing )
                {
                    Logging.Debug( $"IO read exception for {fileName}, {attempts} attempts left", ex );
                    return (retry: true, result: null);
                }
            }
            catch ( IOException ex )
            {
                Logging.Error( $"IO read exception for {fileName}, {ex.Message}", ex );
            }

            // Non-retryable failure
            return (retry: false, result: null);
        }

        /// <summary>
        /// Write a file, handling exceptions
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="content"></param>
        public static void Write(string fileName, string content)
        {
            if (fileName != null && content != null)
            {
                // Skip writing to storage if we're unit testing
                if (unitTesting)
                {
                    Logging.Debug("Skipping write to " + fileName + " during unit test");
                    return;
                }

                var attempts = writeAttempts;
                while (attempts > 0 && TryWrite(fileName, attempts, content))
                {
                    attempts--;
                    Thread.Sleep(writeIODelayMilliseconds);
                }
                if (attempts == 0)
                {
                    throw new IOException("IO write failed for " + fileName + ", too many attempts.");
                }
            }
        }

        private static bool TryWrite(string fileName, int attempts, string content)
        {
            // Attempt to write the file
            try
            {
                LockManager.GetLock(fileName, () => File.WriteAllText(fileName, content, Encoding.UTF8));
            }
            catch (ArgumentException ex)
            {
                Logging.Error("Failed to write to " + fileName, ex);
            }
            catch (PathTooLongException ex)
            {
                Logging.Error("Path " + fileName + " too long", ex);
            }
            catch (DirectoryNotFoundException ex)
            {
                Logging.Error("Directory for " + fileName + " not found", ex);
            }
            catch (UnauthorizedAccessException ex)
            {
                Logging.Warn("Not allowed to write to " + fileName, ex);
            }
            catch (NotSupportedException ex)
            {
                Logging.Error("Not supported writing to " + fileName, ex);
            }
            catch (SecurityException ex)
            {
                Logging.Error("Security exception writing to " + fileName, ex);
            }
            catch (IOException ex) when ((ex.HResult & 0x0000FFFF) == 32) // Sharing violation
            {
                Logging.Debug($"IO write exception for {fileName}, {attempts} attempts left", ex);
                return true; // We have failed to write the file and will need to make another attempt
            }
            catch (IOException ex) when (ex.HResult == unchecked((int)0x80070027) || ex.HResult == unchecked((int)0x80070070)) // Not enough disk space
            {
                Logging.Warn($"IO write exception for {fileName}, {ex.Message}", ex);
            }
            catch (IOException ex) // Other IO issue 
            {
                Logging.Error($"IO write exception for {fileName}, {ex.Message}", ex);
            }
            // We have either successfully written to the file or encountered an exception that would not benefit from another attempt
            return false;
        }

        public static string FromSavedGames(string filename)
        {
            string data = null;
            var directory = GetSavedGamesDir();
            if (directory == null || directory.Trim() == "")
            {
                return null;
            }

            FileInfo fileInfo = null;
            try
            {
                fileInfo = FileInfo(directory, filename);
            }
            catch (NotSupportedException nsex)
            {
                Logging.Error($"Directory '{directory}' not supported: ", nsex);
            }

            if (fileInfo != null)
            {
                var maxTries = 6;
                while (IsFileLocked(fileInfo))
                {
                    Thread.Sleep(100);
                    maxTries--;
                    if (maxTries == 0)
                    {
                        Logging.Info($"Unable to open Elite Dangerous '{filename}' file");
                        return null;
                    }
                }

                using (var fs = new FileStream(fileInfo.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var reader = new StreamReader(fs, Encoding.UTF8))
                {
                    fs.Seek(0, SeekOrigin.Begin);
                    data = reader.ReadToEnd();
                }
            }
            return data;
        }

        public static async Task<string> FromSavedGamesAsync ( string filename )
        {
            string data = null;
            var directory = GetSavedGamesDir();
            if ( directory == null || directory.Trim() == "" )
            {
                return null;
            }

            FileInfo fileInfo = null;
            try
            {
                fileInfo = FileInfo( directory, filename );
            }
            catch ( NotSupportedException nsex )
            {
                Logging.Error( $"Directory '{directory}' not supported: ", nsex );
            }

            if ( fileInfo != null )
            {
                var maxTries = 6;
                while ( IsFileLocked( fileInfo ) )
                {
                    Thread.Sleep( 100 );
                    maxTries--;
                    if ( maxTries == 0 )
                    {
                        Logging.Info( $"Unable to open Elite Dangerous '{filename}' file" );
                        return null;
                    }
                }

                using ( var fs = new FileStream( fileInfo.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite ) )
                using ( var reader = new StreamReader( fs, Encoding.UTF8 ) )
                {
                    fs.Seek( 0, SeekOrigin.Begin );
                    data = await reader.ReadToEndAsync();
                }
            }
            return data;
        }

        // Obtain file info for a file name and path, or null if the file is not available
        public static FileInfo FileInfo(string path, string file)
        {
            if (path == null)
            {
                // Configuration can be changed underneath us so we do have to check each time...
                return null;
            }

            try
            {
                var info = new FileInfo(path + @"\" + file);
                if (info.Exists)
                {
                    // This info can be cached so force a refresh
                    info.Refresh();
                }
                return info;
            }
            catch { return null; }
        }

        private static string GetSavedGamesDir()
        {
            var result = NativeMethods.SHGetKnownFolderPath(new Guid("4C5C32FF-BB9D-43B0-B5B4-2D72E54EAAA4"), 0, new IntPtr(0), out var path);
            if (result >= 0)
            {
                return Marshal.PtrToStringUni(path) + @"\Frontier Developments\Elite Dangerous";
            }
            else
            {
                throw new ExternalException("Failed to find the saved games directory.", result);
            }
        }

        private abstract class NativeMethods
        {
            [DllImport("Shell32.dll")]
            internal static extern int SHGetKnownFolderPath([MarshalAs(UnmanagedType.LPStruct)] Guid rfid, uint dwFlags, IntPtr hToken, out IntPtr ppszPath);
        }

        public static bool IsFileLocked(FileInfo file)
        {
            FileStream stream = null;

            try
            {
                stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.None);
            }
            catch (IOException)
            {
                //the file is unavailable because it is:
                //still being written to
                //or being processed by another thread
                //or does not exist (has already been processed)
                return true;
            }
            finally
            {
                stream?.Close();
            }

            //file is not locked
            return false;
        }

        public static string GetAbsoluteFilePath ( string basePath, string path )
        {
            if ( string.IsNullOrEmpty( path ) || string.IsNullOrEmpty( basePath ) ) { return null; }

            string finalPath;
            var pathRoot = Path.GetPathRoot( path );
            if ( !Path.IsPathRooted( path ) || "\\".Equals( pathRoot ) )
            {
                finalPath = path.StartsWith( Path.DirectorySeparatorChar.ToString() ) && !string.IsNullOrEmpty( pathRoot )
                    ? Path.Combine( pathRoot, path.TrimStart( Path.DirectorySeparatorChar ) )
                    : Path.Combine( basePath, path );
            }
            else
            {
                finalPath = path;
            }

            // resolves any internal "..\" to get the true full path.
            return Path.GetFullPath( finalPath );
        }
    }
}
