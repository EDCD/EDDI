using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading;

namespace Utilities
{
    public class Files
    {
        public static bool ignoreMissing { get; set; } = false;

        /// <summary>
        /// Read a file, handling exceptions
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string Read(string name)
        {
            string result = null;
            if (name != null)
            {
                try
                {
                    result = File.ReadAllText(name, Encoding.UTF8);
                }
                catch (ArgumentException ex)
                {
                    Logging.Error("Failed to read from " + name, ex);
                }
                catch (PathTooLongException ex)
                {
                    Logging.Error("Path " + name + " too long", ex);
                }
                catch (DirectoryNotFoundException ex)
                {
                    Logging.Error("Directory for " + name + " not found", ex);
                }
                catch (FileNotFoundException ex)
                {
                    if (!ignoreMissing)
                    {
                        Logging.Error("File " + name + " not found", ex);
                    }
                }
                catch (IOException ex)
                {
                    if (!ignoreMissing)
                    {
                        Logging.Error("IO exception for " + name, ex);
                    }
                }
                catch (UnauthorizedAccessException ex)
                {
                    Logging.Error("Not allowed to read from " + name, ex);
                }
                catch (NotSupportedException ex)
                {
                    Logging.Error("Not supported reading from " + name, ex);
                }
                catch (SecurityException ex)
                {
                    Logging.Error("Security exception reading from " + name, ex);
                }

            }
            return result;
        }

        /// <summary>
        /// Write a file, handling exceptions
        /// </summary>
        /// <param name="name"></param>
        /// <param name="content"></param>
        public static void Write(string name, string content)
        {
            if (name != null && content != null)
            {
                // Attempt to write the file
                try
                {
                    File.WriteAllText(name, content, Encoding.UTF8);
                }
                catch (ArgumentException ex)
                {
                    Logging.Error("Failed to write to " + name, ex);
                }
                catch (PathTooLongException ex)
                {
                    Logging.Error("Path " + name + " too long", ex);
                }
                catch (DirectoryNotFoundException ex)
                {
                    Logging.Error("Directory for " + name + " not found", ex);
                }
                catch (IOException ex)
                {
                    Logging.Error("IO exception for " + name, ex);
                }
                catch (UnauthorizedAccessException ex)
                {
                    Logging.Error("Not allowed to write to " + name, ex);
                }
                catch (NotSupportedException ex)
                {
                    Logging.Error("Not supported writing to " + name, ex);
                }
                catch (SecurityException ex)
                {
                    Logging.Error("Security exception writing to " + name, ex);
                }
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")] // this usage is perfectly correct
        public static string FromSavedGames(string filename)
        {
            string data = null;
            string directory = GetSavedGamesDir();
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
                Logging.Error("Directory " + directory + " not supported: ", nsex);
            }

            if (fileInfo != null)
            {
                int maxTries = 6;
                while (IsFileLocked(fileInfo))
                {
                    Thread.Sleep(100);
                    maxTries--;
                    if (maxTries == 0)
                    {
                        Logging.Info("Unable to open Elite Dangerous" + filename + "file");
                        return null;
                    }
                }

                using (FileStream fs = new FileStream(fileInfo.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (StreamReader reader = new StreamReader(fs, Encoding.UTF8))
                {
                    fs.Seek(0, SeekOrigin.Begin);
                    data = reader.ReadToEnd();
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
                FileInfo info = new FileInfo(path + @"\" + file);
                if (info.Exists)
                {
                    // This info can be cached so force a refresh
                    info.Refresh();
                }
                return info;
            }
            catch { return null; }
        }

        public static string GetSavedGamesDir()
        {
            int result = NativeMethods.SHGetKnownFolderPath(new Guid("4C5C32FF-BB9D-43B0-B5B4-2D72E54EAAA4"), 0, new IntPtr(0), out IntPtr path);
            if (result >= 0)
            {
                return Marshal.PtrToStringUni(path) + @"\Frontier Developments\Elite Dangerous";
            }
            else
            {
                throw new ExternalException("Failed to find the saved games directory.", result);
            }
        }

        internal class NativeMethods
        {
            [DllImport("Shell32.dll")]
            internal static extern int SHGetKnownFolderPath([MarshalAs(UnmanagedType.LPStruct)]Guid rfid, uint dwFlags, IntPtr hToken, out IntPtr ppszPath);
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
                if (stream != null)
                {
                    stream.Close();
                }
            }

            //file is not locked
            return false;
        }
    }
}
