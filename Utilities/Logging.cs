using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.IO;

namespace Utilities
{

    public class Logging
    {
        public static readonly string LogFile = Environment.GetEnvironmentVariable("AppData") + @"\EDDI\eddi.log";
        public static Boolean Verbose { get; set; } = false;

        public static void Error(Exception ex, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "")
        {
            Error(ex.ToString(), memberName, filePath);
        }

        public static void Error(string data, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "")
        {
            log(filePath, memberName, "E", data);
        }

        public static void Warn(string data, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "")
        {
            log(filePath, memberName, "W", data);
        }

        public static void Info(string data, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "")
        {
            log(filePath, memberName, "I", data);
        }

        public static void Debug(string data, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "")
        {
            if (Verbose)
            {
                log(filePath, memberName, "D", data);
            }
        }

        private static void log(string path, string method, string level, string data)
        {
            lock(LogFile)
            {
                using (System.IO.StreamWriter file = new System.IO.StreamWriter(LogFile, true))
                {
                    file.WriteLine(DateTime.Now.ToString() + " " + Path.GetFileNameWithoutExtension(path) + ":" + method + " [" + level + "] " + data);
                }
            }
        }
    }
}
