using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using Rollbar;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Utilities
{
    public partial class Logging // convenience methods
    {
        // For Logging.Error, do not convert data to strings until we've prepared it first.
        public static void Warn(string message, Exception ex, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "") => Warn(message, ex.ToString(), memberName, filePath);
        public static void Info(string message, Exception ex, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "") => Info(message, ex.ToString(), memberName, filePath);
        public static void Debug(string message, Exception ex, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "") => Debug(message, ex.ToString(), memberName, filePath);
    }

    public partial class Logging: _Rollbar
    {
        public static readonly string LogFile = Constants.DATA_DIR + @"\eddi.log";
        public static bool Verbose { get; set; } = false;

        public static void Error(string message, object data = null, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "")
        {
            log(ErrorLevel.Error, message + " " + JsonConvert.SerializeObject(data), memberName, filePath);
            Report(ErrorLevel.Error, message, data, memberName, filePath);
        }

        public static void Warn(string message, string data = "", [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "")
        {
            log(ErrorLevel.Warning, message + " " + data, memberName, filePath);
        }

        public static void Info(string message, string data = "", [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "")
        {
            log(ErrorLevel.Info, message + " " + data, memberName, filePath);
        }

        public static void Debug(string message, string data = "", [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "")
        {
            if (Verbose)
            {
                log(ErrorLevel.Debug, message + " " + data, memberName, filePath);
            }
        }

        private static readonly object logLock = new object();
        private static void log(ErrorLevel errorlevel, string data, string method, string path)
        {
            data = Redaction.RedactEnvironmentVariables(data);
            method = Redaction.RedactEnvironmentVariables(method);
            lock (logLock)
            {
                try
                {
                    using (StreamWriter file = new StreamWriter(LogFile, true))
                    {
                        string timestamp = DateTime.UtcNow.ToString("s", System.Globalization.CultureInfo.InvariantCulture);
                        string shortPath = Path.GetFileNameWithoutExtension(path);
                        shortPath = Redaction.RedactEnvironmentVariables(shortPath);
                        file.WriteLine($"{timestamp} [{errorlevel}] {shortPath}:{method} {data}");
                    }
                }
                catch (Exception)
                {
                    // Failed; can't do anything about it as we're in the logging code anyway
                }
            }
        }

        public static void incrementLogs()
        {
            // Ensure dir exists
            DirectoryInfo directoryInfo = new DirectoryInfo(Constants.DATA_DIR);
            if (!directoryInfo.Exists)
            {
                Directory.CreateDirectory(Constants.DATA_DIR);
            }

            // Obtain files, sorting by last write time to ensure that older files are incremented prior to newer files
            foreach (FileInfo file in directoryInfo.GetFiles().OrderBy(f => f.LastWriteTimeUtc).ToList())
            {
                string filePath = file.FullName;
                if (filePath.EndsWith(".log"))
                {
                    try
                    {
                        bool parsed = int.TryParse(filePath.Replace(Constants.DATA_DIR + @"\eddi", "").Replace(".log", ""), out int i);
                        ++i; // Increment our index number

                        if (i >= 10)
                        {
                            File.Delete(filePath);
                        }
                        else
                        {
                            // This might be our primary log file, so we lock it prior to doing anything with it
                            lock (logLock)
                            {
                                File.Move(filePath, Constants.DATA_DIR + @"\eddi" + i + ".log");
                            }
                        }
                    }
                    catch (Exception)
                    {
                        // Someone may have had a log file open when this code executed? Nothing to do, we'll try again on the next run
                    }
                }
            }
        }

        private static void Report(ErrorLevel errorLevel, string message, object data = null, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "")
        {
            message = Redaction.RedactEnvironmentVariables(message);
            Dictionary<string, object> thisData = PrepRollbarData(ref data);
            if (thisData != null)
            {
                var rollbarReport = System.Threading.Tasks.Task.Run(() => SendToRollbar(errorLevel, message, data, thisData, memberName, filePath));
            }
        }

        private static Dictionary<string, object> PrepRollbarData(ref object data)
        {
            try
            {
                // Serialize the data to a string 
                var serialized = JsonConvert.SerializeObject(data);
                // Redact all environment variables we find
                serialized = Redaction.RedactEnvironmentVariables(serialized);
                // Remove undesirable properties
                serialized = FilterProperties(serialized);

                try
                {
                    data = JsonConvert.DeserializeObject<Dictionary<string, object>>(serialized);
                }
                catch (Exception)
                {
                    // something went wrong with deserialization: we will need to redact manually
                    if (data is Exception ex)
                    {
                        data = new Dictionary<string, object>()
                        {
                            {"message", Redaction.RedactEnvironmentVariables(ex.Message)},
                            {"stacktrace", Redaction.RedactEnvironmentVariables(ex.StackTrace) }
                        };
                    }
                    else if (data is string)
                    {
                        data = new Dictionary<string, object>()
                        {
                            {"message", Redaction.RedactEnvironmentVariables((string)data)}
                        };
                    }
                    else if (!(data is Dictionary<string, object>))
                    {
                        var wrappedData = new Dictionary<string, object>()
                        {
                            {"data", Redaction.RedactEnvironmentVariables(data.ToString())}
                        };
                        data = wrappedData;
                    }
                }

                return (Dictionary<string, object>)data;
            }
            catch (Exception)
            {
                // Return null and don't send data to Rollbar
                return null;
            }
        }

        private static string FilterProperties(string json)
        {
            try
            {
                var data = JObject.Parse(json);

                if (data != null)
                {
                    // Repeated data should be matched even if timestamps differ, so remove journal event timestamps here.
                    // Strip module data that is not useful to report for more consistent matching
                    // Strip commodity data that is not useful to report for more consistent matching
                    string[] filterProperties = new string[]
                    {
                        "timestamp",
                        "on",
                        "priority",
                        "health",
                        "buyprice",
                        "stock",
                        "stockbracket",
                        "sellprice",
                        "demand",
                        "demandbracket",
                        "StatusFlags"
                    };

                    foreach (string property in filterProperties)
                    {
                        data.Descendants()
                            .OfType<JProperty>()
                            .Where(attr => attr.Name.StartsWith(property, StringComparison.OrdinalIgnoreCase))
                            .ToList()
                            .ForEach(attr => attr.Remove());
                    }
                    json = data.ToString();
                }
            }
            catch (Exception)
            {
                // Not parseable.
            }
            return json;
        }

        private static void SendToRollbar(ErrorLevel errorLevel, string message, object data, Dictionary<string, object> thisData, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "")
        {
            if (RollbarLocator.RollbarInstance.Config.Enabled != false)
            {
                string personID = RollbarLocator.RollbarInstance.Config.Person?.Id;
                if (personID.Length > 0)
                {
                    switch (errorLevel)
                    {
                        case ErrorLevel.Error:
                            RollbarLocator.RollbarInstance.Error(message, thisData);
                            log(errorLevel, $"{message} {data}", memberName, $"Reporting error, anonymous ID {personID}: {filePath}");
                            break;
                        default:
                            // If this is an Info Report, report only unique messages and data
                            if (isUniqueMessage(message, thisData))
                            {
                                RollbarLocator.RollbarInstance.Log(errorLevel, message, thisData);
                                log(errorLevel, $"{message} {data}", memberName, $"Reporting unique data, anonymous ID {personID}: {filePath}");
                            }
                            break;
                    }
                }
            }
        }
    }

    public class _Rollbar
    {
        // Exception handling (configuration instructions are at https://github.com/rollbar/Rollbar.NET)
        // We have a limited data plan, so before sending exceptions and other reports, we shall use the API to check that the item is unique
        // The Rollbar API test console is available at https://docs.rollbar.com/reference.

        const string rollbarReadToken = "66e63ff290854a75b8b4c3263f084db6";
        const string rollbarWriteToken = "debe6e50f82d4e8c955d5efafa79c789";
        private static bool filterMessages = true; // We are rate limited, so keep this set to true unless we have a good reason to do otherwise.

        public static void configureRollbar(string uniqueId)
        {
            var config = new RollbarConfig(rollbarWriteToken)
            {
                Environment = Constants.EDDI_VERSION.ToString(),
                ScrubFields = new string[] // Scrub these fields from the reported data
                {
                    "Commander", "apiKey", "commanderName", Constants.DATA_DIR
                },
                // Identify each EDDI configuration by a unique ID, or by "Commander" if a unique ID isn't available.
                Person = new Rollbar.DTOs.Person(uniqueId),
                // Set server info
                Server = new Rollbar.DTOs.Server
                {
                    CodeVersion = ThisAssembly.Git.Sha,
                    Root = "/"
                },
                MaxReportsPerMinute = 1,
#if DEBUG
                Enabled = false,
#else
                Enabled = true,
#endif
            };
            RollbarLocator.RollbarInstance.Configure(config);
        }

        public static void ExceptionHandler(Exception exception)
        {
            Dictionary<string, object> trace = new Dictionary<string, object>
            {
                { "StackTrace", exception.StackTrace ?? "StackTrace not available" }
            };

            if (isUniqueMessage(exception.GetType() + ": " + exception.Message, trace))
            {
                Logging.Info("Reporting unhandled exception, anonymous ID " + RollbarLocator.RollbarInstance.Config.Person.Id + ":" + exception);
                RollbarLocator.RollbarInstance.Error(exception, trace);
            }
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

            response.TryGetValue("err", out object val); // Check for errors before we proceed
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

                    if (itemMessage.ToLowerInvariant() == message.ToLowerInvariant())
                    {
                        string itemStatus = JsonParsing.getString(item, "status");
                        long itemId = JsonParsing.getLong(item, "id");
                        bool uniqueData = isUniqueData(itemId, thisData);
                        string itemResolvedVersion = JsonParsing.getString(item, "environment");

                        // Filter messages & data so that we send only reports which are unique
                        if (itemStatus.ToLowerInvariant() == "active" && !uniqueData)
                        {
                            return false; // Note that if an item reoccurs after being marked as "resolved" it is automatically reactivated.
                        }
                        else if (itemResolvedVersion != null)
                        {
                            try
                            {
                                Version resolvedVersion = new Version(itemResolvedVersion);
                                if (resolvedVersion > Constants.EDDI_VERSION)
                                {
                                    return false; // This has been marked as resolved in a more current client version.
                                }
                            }
                            catch (Exception)
                            {
                                // error parsing version string, ignore
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

            response.TryGetValue("err", out object val); // Check for errors before we proceed
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
                            customData.Keys.All(k => thisData.ContainsKey(k) && Equals(thisData[k]?.ToString()?.ToLowerInvariant(), customData[k]?.ToString()?.ToLowerInvariant())))
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
