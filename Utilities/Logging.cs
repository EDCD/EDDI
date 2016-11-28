using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.IO;
using System.Threading;
using System.Net;

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

        private static readonly object logLock = new object();
        private static void log(string path, string method, string level, string data)
        {
            lock (logLock)
            {
                using (StreamWriter file = new StreamWriter(LogFile, true))
                {
                    file.WriteLine(DateTime.UtcNow.ToString("s", System.Globalization.CultureInfo.InvariantCulture) + " " + Path.GetFileNameWithoutExtension(path) + ":" + method + " [" + level + "] " + data);
                }
            }
        }

        public static void Report(string message, string data = "{}", [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "")
        {
            try
            {
                if (data == null)
                {
                    data = "{}";
                }

                string body = @"{""message"":""" + message + @""", ""version"":""" + Constants.EDDI_VERSION + @""", ""json"":" + data + @"}";
                Thread thread = new Thread(() =>
                {
                    try
                    {
                        using (var client = new WebClient())
                        {
                            client.UploadString(@"http://api.eddp.co/error", body);
                        }
                    }
                    catch (ThreadAbortException)
                    {
                        Logging.Debug("Thread aborted");
                    }
                    catch (Exception ex)
                    {
                        Logging.Warn("Failed to send error to EDDP", ex);
                    }
                });
                thread.Name = "Reporter";
                thread.IsBackground = true;
                thread.Start();
            }
            catch { }
        }
    }
}
