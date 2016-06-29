using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities
{

    public class Logging
    {
        public static readonly string LogFile = Environment.GetEnvironmentVariable("AppData") + @"\EDDI\eddi.log";
        public static Boolean Verbose { get; set; } = false;

        public static void Error(Exception ex)
        {
            Error(ex.ToString());
        }

        public static void Error(string data)
        {
            log(DateTime.Now.ToString() + " " + getCallingClass() + ":" + getCallingMethod() + " [E] " + data);
            Net.UploadString(@"http://api.eddp.co/error", data);
        }

        public static void Warn(string data)
        {
            log(DateTime.Now.ToString() + " " + getCallingClass() + ":" + getCallingMethod() + " [W] " + data);
        }

        public static void Info(string data)
        {
            log(DateTime.Now.ToString() + " " + getCallingClass() + ":" + getCallingMethod() + " [I] " + data);
        }

        public static void Debug(string data)
        {
            if (Verbose)
            {
                log(DateTime.Now.ToString() + " " + getCallingClass() + ":" + getCallingMethod() + " [D] " + data);
            }
        }

        private static void log(string data)
        {
            lock(LogFile)
            {
                using (System.IO.StreamWriter file = new System.IO.StreamWriter(LogFile, true))
                {
                    file.WriteLine(data);
                }
            }
        }

        private static string getCallingMethod()
        {
            return new StackFrame(2).GetMethod().Name;
        }

        private static string getCallingClass()
        {
            return new StackFrame(2).GetMethod().ReflectedType.Name;
        }
    }
}
