using RestSharp;
using Rollbar;
using System;
using System.Runtime.CompilerServices;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Linq;

namespace Utilities
{
    public class Logging: _Rollbar
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
                if (!(data is Dictionary<string, object>))
                {
                    data = JsonConvert.DeserializeObject<Dictionary<string, object>>(data.ToString());
                }
                Dictionary<string, object> thisData = (Dictionary<string, object>)data;
                if (isUniqueMessage(message, thisData))
                {
                    RollbarLocator.RollbarInstance.Info(message, thisData);
                    Info("Reporting unique data, anonymous ID " + RollbarLocator.RollbarInstance.Config.Person.Id + ": " + message + thisData);
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

    public class _Rollbar
    {
        // Exception handling (configuration instructions are at https://github.com/rollbar/Rollbar.NET)
        // We have a limited data plan, so before sending exceptions and other reports, we shall use the API to check that the item is unique
        // The Rollbar API test console is available at https://rollbar.com/docs/api/test_console/.

        const string rollbarReadToken = "66e63ff290854a75b8b4c3263f084db6";
        const string rollbarWriteToken = "debe6e50f82d4e8c955d5efafa79c789";
        private static bool filterMessages = true; // We are rate limited, so keep this set to true unless we have a good reason to do otherwise.

        public static void configureRollbarExceptionHandling(bool beta, string uniqueId)
        {
            var config = new RollbarConfig(rollbarWriteToken)
            {
                Environment = beta ? "development" : "production",
                ScrubFields = new string[] // Scrub these fields from the reported data
                {
                    "Commander", "apiKey", "commanderName"
                },
                // Identify each EDDI configuration by a unique ID, or by "Commander" if a unique ID isn't available.
                Person = new Rollbar.DTOs.Person(uniqueId),
                // Set server info
                Server = new Rollbar.DTOs.Server
                {
                    CodeVersion = Constants.EDDI_VERSION,
                    Root = "/"
                },
            };
            RollbarLocator.RollbarInstance.Configure(config);
            // Send unhandled exceptions
            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
            {
                Exception exception = args.ExceptionObject as Exception;

                Dictionary<string, object> trace = new Dictionary<string, object>();
                trace.Add("StackTrace", exception.StackTrace ?? "StackTrace not available");

                if (isUniqueMessage(exception.GetType() + ": " + exception.Message, trace))
                {
                    Logging.Error("Reporting unhandled exception, anonymous ID " + RollbarLocator.RollbarInstance.Config.Person.Id);
                    RollbarLocator.RollbarInstance.Error(exception, trace);
                }
            };
        }

        public static bool isUniqueMessage(string message, Dictionary<string, object> thisData = null)
        {
            if (!filterMessages)
            {
                return true;
            }

            var client = new RestClient("https://api.rollbar.com/api/1");
            var request = new RestRequest("/items/", Method.GET);
            request.AddParameter("access_token", rollbarReadToken);
            var clientResponse = client.Execute<Dictionary<string, object>>(request);
            Dictionary<string, object> response = clientResponse.Data;

            object val;
            response.TryGetValue("err", out val); // Check for errors before we proceed
            if ((long)val == 0)
            {
                response.TryGetValue("result", out val);
                Dictionary<string, object> result = (Dictionary<string, object>)val;

                result.TryGetValue("items", out val);
                SimpleJson.JsonArray jsonArray = (SimpleJson.JsonArray)val;
                object[] items = jsonArray.ToArray();

                foreach (object Item in items)
                {
                    Dictionary<string, object> item = (Dictionary<string, object>)Item;
                    string itemMessage = JsonParsing.getString(item, "title");
                    string itemStatus = JsonParsing.getString(item, "status");
                    string itemResolvedVersion = JsonParsing.getString(item, "resolved_in_version");
                    long itemId = JsonParsing.getLong(item, "id");
                    bool uniqueData = isUniqueData(itemId, thisData);

                    if (itemMessage == message)
                    {
                        // Filter messages & data so that we send only reports which are unique
                        if (itemStatus == "active" && !uniqueData)
                        {
                            return false; // Note that if an item reoccurs after being marked as "resolved" it is automatically reactivated.
                        }
                        else if (itemResolvedVersion != null)
                        {
                            if (Versioning.Compare(itemResolvedVersion, Constants.EDDI_VERSION) > 0)
                            {
                                return false; // This has been marked as resolved in a more current client version.
                            }
                        }
                    }
                }
            }
            return true;
        }

        public static bool isUniqueData(long itemId, Dictionary<string, object> thisData = null)
        {
            if (thisData is null)
            {
                return true;
            }

            var client = new RestClient("https://api.rollbar.com/api/1");
            var request = new RestRequest("/item/" + itemId + "/instances/", Method.GET);
            request.AddParameter("access_token", rollbarReadToken);
            var clientResponse = client.Execute<Dictionary<string, object>>(request);
            Dictionary<string, object> response = clientResponse.Data;

            object val;
            response.TryGetValue("err", out val); // Check for errors before we proceed
            if ((long)val == 0)
            {
                response.TryGetValue("result", out val);
                Dictionary<string, object> result = (Dictionary<string, object>)val;

                result.TryGetValue("instances", out val);
                SimpleJson.JsonArray jsonArray = (SimpleJson.JsonArray)val;
                object[] instances = jsonArray.ToArray();

                foreach (object Instance in instances)
                {
                    Dictionary<string, object> instance = (Dictionary<string, object>)Instance;

                    instance.TryGetValue("data", out val);
                    Dictionary<string, object> instanceData = (Dictionary<string, object>)val;

                    instanceData.TryGetValue("custom", out val);
                    Dictionary<string, object> customData = (Dictionary<string, object>)val;

                    if (customData != null)
                    {
                        if (customData.Keys.Count == thisData.Keys.Count &&
                            customData.Keys.All(k => thisData.ContainsKey(k) && Equals(thisData[k], customData[k])))
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }
    }
}
