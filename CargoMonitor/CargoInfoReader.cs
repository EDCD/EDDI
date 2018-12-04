using EddiDataDefinitions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Utilities;



namespace EddiCargoMonitor
{
    public class CargoInfoReader
    {
        public int Count { get; set; }
        public List<CargoInfo> Inventory { get; set; }

        public CargoInfoReader()
        {
            Inventory = new List<CargoInfo>();
        }

        public static CargoInfoReader FromFile(string filename = null)
        {
            CargoInfoReader info = new CargoInfoReader();
            Regex Filter = new Regex(@"^Cargo\.json$");
            string directory = GetSavedGamesDir();
            if (directory == null || directory.Trim() == "")
            {
                return null;
            }

            FileInfo fileInfo = null;
            try
            {
                fileInfo = FindCargoInfoFile(directory, Filter);
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
                        Logging.Info("Unable to open Elite Dangerous Cargo.json file");
                        return null;
                    }
                }

                using (FileStream fs = new FileStream(fileInfo.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (StreamReader reader = new StreamReader(fs, Encoding.UTF8))
                {
                    string data = string.Empty;
                    fs.Seek(0, SeekOrigin.Begin);
                    data = reader.ReadToEnd() ?? string.Empty;
                    info = JsonConvert.DeserializeObject<CargoInfoReader>(data);
                }
            }
            return info;
        }

        private static string GetSavedGamesDir()
        {
            IntPtr path;
            int result = NativeMethods.SHGetKnownFolderPath(new Guid("4C5C32FF-BB9D-43B0-B5B4-2D72E54EAAA4"), 0, new IntPtr(0), out path);
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

        /// <summary>Find the latest file in a given directory matching a given expression, or null if no such file exists</summary>
        private static FileInfo FindCargoInfoFile(string path, Regex filter = null)
        {
            if (path == null)
            {
                // Configuration can be changed underneath us so we do have to check each time...
                return null;
            }

            var directory = new DirectoryInfo(path);
            if (directory != null)
            {
                try
                {
                    FileInfo info = directory.GetFiles().Where(f => filter == null || filter.IsMatch(f.Name)).FirstOrDefault();
                    if (info != null)
                    {
                        // This info can be cached so force a refresh
                        info.Refresh();
                    }
                    return info;
                }
                catch { }
            }
            return null;
        }

        private static bool IsFileLocked(FileInfo file)
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
                    stream.Close();
            }

            //file is not locked
            return false;
        }
    }
}
