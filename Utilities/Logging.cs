using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Rollbar;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace Utilities
{
    public partial class Logging // convenience methods
    {
        // For Logging.Error, do not convert data to strings until we've prepared it first.
        public static void Warn(string message, Exception ex, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "") => Warn(message, ex.ToString(), memberName, filePath);
        public static void Info(string message, Exception ex, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "") => Info(message, ex.ToString(), memberName, filePath);
        public static void Debug(string message, Exception ex, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "") => Debug(message, ex.ToString(), memberName, filePath);
    }

    public partial class Logging : _Rollbar
    {
        private static readonly Regex JsonRegex = new Regex(@"^{.*}$", RegexOptions.Singleline);

        public static readonly string LogFile = Constants.DATA_DIR + @"\eddi.log";
        public static bool Verbose { get; set; }

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

        private static async void Report(ErrorLevel errorLevel, string message, object originalData = null, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "")
        {
            message = Redaction.RedactEnvironmentVariables(message);
            Dictionary<string, object> preppedData = PrepRollbarData(ref originalData);
            if (originalData is null || preppedData != null)
            {
                await System.Threading.Tasks.Task.Run(() => SendToRollbar(errorLevel, message, preppedData, memberName, filePath)).ConfigureAwait(false); 
            }
        }

        private static Dictionary<string, object> PrepRollbarData(ref object data)
        {
            if (data is null) { return null; }
            try
            {
                if (data is string str && !JsonRegex.IsMatch(str))
                {
                    return Wrap("message", Redaction.RedactEnvironmentVariables(str));
                }
                else
                {
                    // Serialize the data to a string 
                    string serialized = JsonConvert.SerializeObject(data);
                    serialized = FilterPropertiesFromJsonString(serialized);
                    serialized = Redaction.RedactEnvironmentVariables(serialized);
                    if (data is Exception)
                    {
                        return JsonConvert.DeserializeObject<Dictionary<string, object>>(serialized);
                    }
                    else
                    {
                        var jToken = JToken.Parse(serialized);
                        if (jToken is JArray jArray)
                        {
                            return Wrap("data", jArray);
                        }
                        if (jToken is JObject jObject)
                        {
                            return jObject.ToObject<Dictionary<string, object>>();
                        }
                    }
                }
            }
            catch (Exception)
            {
                // Something went wrong. Return null and don't send data to Rollbar
            }
            return null;
        }

        private static Dictionary<string, object> Wrap(string key, object data)
        {
            var wrappedData = new Dictionary<string, object>()
            {
                {key, data}
            };
            return wrappedData;
        }

        private static string FilterPropertiesFromJsonString(string json)
        {
            if (string.IsNullOrEmpty(json)) { return null; }

            try
            {
                var jToken = JToken.Parse(json);
                if (jToken is JObject data)
                {
                    // Repeated data should be matched even if timestamps differ, so remove journal event timestamps here.
                    // Strip module data that is not useful to report for more consistent matching
                    // Strip commodity data that is not useful to report for more consistent matching
                    // Strip other sensitive data like "apiKey" or "frontierID"
                    string[] filterProperties =
                    {
                        "name",
                        "on",
                        "priority",
                        "health",
                        "buyprice",
                        "stock",
                        "stockbracket",
                        "sellprice",
                        "demand",
                        "demandbracket",
                        "StatusFlags",
                        "apiKey",
                        "frontierID",
                        "FID"
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
                // Not parseable json.
            }
            return json;
        }

        private static void SendToRollbar(ErrorLevel errorLevel, string message, Dictionary<string, object> preppedData, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "")
        {
            if (RollbarLocator.RollbarInstance.Config.Enabled == false) { return; }
            string personID = RollbarLocator.RollbarInstance.Config.Person?.Id;
            if (!string.IsNullOrEmpty(personID))
            {
                try
                {
                    switch (errorLevel)
                    {
                        case ErrorLevel.Error:
                            RollbarLocator.RollbarInstance.Error(message, preppedData);
                            log(errorLevel, $"Reporting error, anonymous ID {personID}: {message} {JsonConvert.SerializeObject(preppedData)}", memberName, filePath);
                            break;
                        default:
                            // If this is an Info Report, report only unique messages and data
                            RollbarLocator.RollbarInstance.Log(errorLevel, message, preppedData);
                            log(errorLevel, $"Reporting unique data, anonymous ID {personID}: {message} {JsonConvert.SerializeObject(preppedData)}", memberName, filePath);
                            break;
                    }
                }
                catch
                {
                    // Nothing to do here. Just continue gracefully.
                }
            }
        }
    }

    public class _Rollbar
    {
        // Exception handling (configuration instructions are at https://github.com/rollbar/Rollbar.NET)
        // The Rollbar API test console is available at https://docs.rollbar.com/reference.

        const string rollbarWriteToken = "debe6e50f82d4e8c955d5efafa79c789";
        public static bool TelemetryEnabled {
            get => RollbarLocator.RollbarInstance.Config.Enabled;
            set => RollbarLocator.RollbarInstance.Config.Enabled = value;
        }

        public static void configureRollbar(string uniqueId, bool fromVA = false)
        {
            var config = new RollbarConfig(rollbarWriteToken)
            {
                Environment = Constants.EDDI_VERSION + (fromVA ? " VA" : ""),
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
                IpAddressCollectionPolicy = IpAddressCollectionPolicy.DoNotCollect,
                PayloadPostTimeout = TimeSpan.FromSeconds(10), 
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

            Logging.Info("Reporting unhandled exception, anonymous ID " + RollbarLocator.RollbarInstance.Config.Person.Id + ":" + exception);
            RollbarLocator.RollbarInstance.Error(exception, trace);
        }
    }
}
