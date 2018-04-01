using RestSharp;
using Rollbar;
using System;
using System.Runtime.CompilerServices;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Utilities
{
    public class Logging
    {
        const string rollbarReadAccessToken = "ffec003259a14847bbf588ffec517d66";
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
                    if (isUniqueMessage(message, (string)data))
                    {
                        Dictionary<string, object> json = new Dictionary<string, object>();
                        json.Add("raw", json);
                        RollbarLocator.RollbarInstance.Configure("b16e82cc9116430eb05d901cd9ed5a25");
                        RollbarLocator.RollbarInstance.Info(message, json);
                    }
                    else
                    {
                        Warn(@"Unable to report message """ + message + @""". Invalid data type " + data.GetType());
                    }
                }
            }
            catch (Exception)
            {
                // Nothing to do
            }
        }

        public static bool isUniqueMessage(string message, string data = null)
        {
            // We have a limited data plan, so before sending exceptions and other reports, check to make sure the item is unique
            // The Rollbar API test console is available at https://rollbar.com/docs/api/test_console/.
            var client = new RestClient("https://api.rollbar.com/api/1");
            var request = new RestRequest("/items/", Method.GET);
            request.AddParameter("access_token", rollbarReadAccessToken);
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

                    // Filter for messages & data, send only reports which are not unique
                    if (itemMessage == message && isUniqueData(itemId, data))
                    {
                        // Any item that is already active can be safely ignored. If the item reoccurs after being marked as "resolved" it shall be reactivated.
                        if (itemStatus == "active")
                        {
                            return false;
                        }
                        else if (itemResolvedVersion != null)
                        {
                            if (Versioning.Compare(itemResolvedVersion, Constants.EDDI_VERSION) > 0)
                            {
                                // This has been marked as resolved in a more current client version.
                                return false;
                            }
                        }
                    }
                }
            }
            else
            {
                Debug("Connection to Rollbar API failed, code " + (int)val + @". Unclear whether reporting item is unique. Message: " + message + ". Data: " + data);
            }
            // This item appears to be unique.
            return true;
        }

        public static bool isUniqueData(long itemId, string data = null)
        {
            var client = new RestClient("https://api.rollbar.com/api/1");
            var request = new RestRequest("/item/" + itemId + "/instances/", Method.GET);
            request.AddParameter("access_token", rollbarReadAccessToken);
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
                    foreach (KeyValuePair<string, object> json in customData)
                    {
                        string escapedData = System.Text.RegularExpressions.Regex.Escape(data);
                        string escapedJson = System.Text.RegularExpressions.Regex.Escape((string)json.Value);
                        if ((string)json.Value == escapedData)
                        {
                            return false;
                        }
                    }
                }
            }
            // This item appears to be unique.
            return true;
        }
    }
}
