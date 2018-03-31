using Rollbar;
using System;
using System.Runtime.CompilerServices;
using System.IO;
using System.Threading;
using System.Net;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Utilities
{
    public class Logging
    {
        public static readonly string LogFile = Constants.DATA_DIR + @"\eddi.log";
        public static bool Verbose { get; set; } = false;

        public static void Error(string message, Exception ex, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "")
        {
            Error(message, ex.ToString(), memberName, filePath);
        }

        public static void Error(Exception ex, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "")
        {
            Error(ex.ToString(), memberName, filePath);
        }

        public static void Error(string message, string data = null, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "")
        {
            log(filePath, memberName, "E", message + " " + data);
            Report(message, data, memberName, filePath);
        }

        public static void Warn(string message, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "")
        {
            Warn(message, (string)null, memberName, filePath);
        }

        public static void Warn(string message, Exception ex, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "")
        {
            Warn(message, ex.ToString(), memberName, filePath);
        }

        public static void Warn(Exception ex, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "")
        {
            Warn(ex.ToString(), memberName, filePath);
        }

        public static void Warn(string message, string data, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "")
        {
            log(filePath, memberName, "W", message + " " + data);
        }

        public static void Info(string message, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "")
        {
            Info(message, (string)null, memberName, filePath);
        }

        public static void Info(string message, Exception ex, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "")
        {
            Info(message, ex.ToString(), memberName, filePath);
        }

        public static void Info(Exception ex, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "")
        {
            Info(ex.ToString(), memberName, filePath);
        }

        public static void Info(string message, string data, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "")
        {
            log(filePath, memberName, "I", message + " " + data);
        }

        public static void Debug(string message, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "")
        {
            if (Verbose)
            {
                Debug(message, (string)null, memberName, filePath);
            }
        }

        public static void Debug(string message, Exception ex, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "")
        {
            if (Verbose)
            {
                Debug(message, ex.ToString(), memberName, filePath);
            }
        }

        public static void Debug(Exception ex, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "")
        {
            if (Verbose)
            {
                Debug(ex.ToString(), memberName, filePath);
            }
        }

        public static void Debug(string message, string data, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "")
        {
            if (Verbose)
            {
                log(filePath, memberName, "D", message + " " + data);
            }
        }
        private static readonly object logLock = new object();
        private static void log(string path, string method, string level, string data)
        {
            lock (logLock)
            {
                try
                {
                    using (StreamWriter file = new StreamWriter(LogFile, true))
                    {
                        file.WriteLine(DateTime.UtcNow.ToString("s", System.Globalization.CultureInfo.InvariantCulture) + " " + Path.GetFileNameWithoutExtension(path) + ":" + method + " [" + level + "] " + data);
                    }
                }
                catch (Exception)
                {
                    // Failed; can't do anything about it as we're in the logging code anyway
                }
            }
        }

        public static void Report(string message, object data = null, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "")
        {
            try
            {
                if (data is object)
                {
                    data = JsonConvert.SerializeObject(data);
                }
                if (data is string)
                {
                    Dictionary<string, object> json = new Dictionary<string, object>();
                    json.Add("raw", json);
                    RollbarLocator.RollbarInstance.Log(ErrorLevel.Info, message, json);
                }
                else
                {
                    Warn(@"Unable to report message """ + message + @""". Invalid data type " + data.GetType());
                }
            }
            catch (Exception)
            {
                // Nothing to do
            }
        }
    }
}
