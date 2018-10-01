using EddiDataDefinitions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Utilities;



namespace EddiShipMonitor
{
    public class ModuleInfoReader
    {
        public List<ModuleInfo> Modules { get; set; }

        [JsonIgnore]
        private string dataPath;

        [JsonIgnore]
        static readonly object fileLock = new object();

        public ModuleInfoReader()
        {
            Modules = new List<ModuleInfo>();
        }

        /// <summary>
        /// Obtain ships configuration from a file.  If the file name is not supplied the the default
        /// path of Constants.Data_DIR\shipmonitor.json is used
        /// </summary>
        public static ModuleInfoReader FromFile(string filename = null)
        {
        Regex Filter = new Regex(@"^ModulesInfo\.json$");
        string directory = GetSavedGamesDir();
            if (directory == null || directory.Trim() == "")
            {
                return null;
            }


            if (filename == null)
            {
                filename = directory + @"\ModulesInfo.json";
            }

            ModuleInfoReader info = new ModuleInfoReader();
            if (File.Exists(filename))
            {
                try
                {
                    string data = Files.Read(filename);
                    if (data != null)
                    {
                        info = JsonConvert.DeserializeObject<ModuleInfoReader>(data);
                    }
                }
                catch (Exception ex)
                {
                    Logging.Debug("Failed to read modules info", ex);
                }
            }

            if (info == null)
            {
                info = new ModuleInfoReader();
            }

            info.dataPath = filename;
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
        private static FileInfo FindModuleInfoFile(string path, Regex filter = null)
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
    }
}
