using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Rollbar;
using Rollbar.DTOs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Exception = System.Exception;

namespace Utilities
{
    public class Logging : _Rollbar
    {
        private static readonly Regex JsonRegex = new Regex(@"^{.*}$", RegexOptions.Singleline);

        public static readonly string LogFile = Constants.DATA_DIR + @"\eddi.log";
        public static bool Verbose { get; set; }

        public static void Error(string message, object data = null, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "")
        {
            handleLogging(ErrorLevel.Error, message, data, memberName, filePath);
        }

        public static void Warn(string message, object data = null, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "")
        {
            handleLogging(ErrorLevel.Warning, message, data, memberName, filePath);
        }

        public static void Info(string message, object data = null, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "")
        {
            handleLogging(ErrorLevel.Info, message, data, memberName, filePath);
        }

        public static void Debug(string message, object data = null, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "")
        {
            handleLogging(ErrorLevel.Debug, message, data, memberName, filePath);
        }

        private static void handleLogging(ErrorLevel errorlevel, string message, object data, string memberName, string filePath)
        {
            System.Threading.Tasks.Task.Run(() =>
            {
                string timestamp = DateTime.UtcNow.ToString("s", System.Globalization.CultureInfo.InvariantCulture);
                var shortPath = Redaction.RedactEnvironmentVariables(Path.GetFileNameWithoutExtension(filePath));
                var method = Redaction.RedactEnvironmentVariables(memberName);
                message = $"{shortPath}:{method} {Redaction.RedactEnvironmentVariables(message)}";
                var preppedData = FilterAndRedactData(data);

                switch (errorlevel)
                {
                    case ErrorLevel.Debug:
                        {
                            if (Verbose) { log(timestamp, errorlevel, message, preppedData); }
                            if (TelemetryEnabled) { RecordTelemetryInfo(errorlevel, message, preppedData); }
                        }
                        break;
                    case ErrorLevel.Info:
                    case ErrorLevel.Warning:
                        {
                            log(timestamp, errorlevel, message, preppedData);
                            if (TelemetryEnabled) { RecordTelemetryInfo(errorlevel, message, preppedData); }
                        }
                        break;
                    case ErrorLevel.Error:
                    case ErrorLevel.Critical:
                        {
                            log(timestamp, errorlevel, message, preppedData);
                            if (TelemetryEnabled) { ReportTelemetryEvent(timestamp, errorlevel, message, preppedData); }
                        }
                        break;
                }
            }).ConfigureAwait(false);
        }

        private static readonly object logLock = new object();
        private static void log(string timestamp, ErrorLevel errorlevel, string message, object data = null)
        {
            lock (logLock)
            {
                try
                {
                    using (StreamWriter file = new StreamWriter(LogFile, true))
                    {
                        file.WriteLine($"{timestamp} [{errorlevel}] {message}" + (data != null 
                            ? $": {Redaction.RedactEnvironmentVariables(JsonConvert.SerializeObject(data))}" 
                            : null));
                    }
                }
                catch (Exception)
                {
                    // Failed; can't do anything about it as we're in the logging code anyway
                }
            }
        }

        private static void RecordTelemetryInfo(ErrorLevel errorLevel, string message, Dictionary<string, object> preppedData = null)
        {
            if (Enum.TryParse(errorLevel.ToString(), out TelemetryLevel telemetryLevel))
            {
                try
                {
                    var telemetry = preppedData is null 
                        ? new Telemetry(TelemetrySource.Client, telemetryLevel, new LogTelemetry(message)) 
                        : new Telemetry(TelemetrySource.Client, telemetryLevel, new LogTelemetry(message, preppedData));
                    RollbarInfrastructure.Instance.TelemetryCollector?.Capture(telemetry);
                }
                catch (RollbarException rex)
                {
                    Warn(rex.Message, rex);
                }
            }
        }

        private static void ReportTelemetryEvent(string timestamp, ErrorLevel errorLevel, string message, Dictionary<string, object> preppedData = null)
        {
            try
            {
                RollbarLocator.RollbarInstance.Log(errorLevel, message, preppedData);
                string personID = RollbarLocator.RollbarInstance.Config.RollbarPayloadAdditionOptions.Person?.Id;
                if (!string.IsNullOrEmpty(personID))
                {
                    log(timestamp, errorLevel, $"Reporting error to Rollbar telemetry service, anonymous ID {personID}: {message}");
                }
            }
            catch (RollbarException rex)
            {
                Warn(rex.Message, rex);
            }
        }

        private static Dictionary<string, object> FilterAndRedactData(object data)
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
                    // Strip module data that is not useful to report for more consistent matching
                    // Strip commodity data that is not useful to report for more consistent matching
                    // Strip sensitive or personal data like "apiKey" or "frontierID"
                    string[] filterProperties =
                    {
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
                        "FID",
                        "ActiveFine",
                        "CockpitBreach",
                        "BoostUsed",
                        "FuelLevel",
                        "FuelUsed",
                        "JumpDist",
                        "Wanted",
                        "Latitude",
                        "Longitude",
                        "MyReputation",
                        "SquadronFaction",
                        "HappiestSystem",
                        "HomeSystem",
                        "access_token",
                        "refresh_token"
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
    }

    public class _Rollbar
    {
        // Exception handling (configuration instructions are at https://github.com/rollbar/Rollbar.NET)
        // The Rollbar API test console is available at https://docs.rollbar.com/reference.

        const string rollbarWriteToken = "26110ab001e24655a9a75e12e080f78c";

        public static bool TelemetryEnabled {
            get => RollbarLocator.RollbarInstance.Config.RollbarDeveloperOptions.Transmit;
            // ReSharper disable once ValueParameterNotUsed
            set => RollbarLocator.RollbarInstance.Config.RollbarDeveloperOptions.Transmit =
#if DEBUG
                false;
#else
                value;
#endif
        }

        public static void configureRollbar(string uniqueId, bool fromVA = false)
        {
            var config = new RollbarInfrastructureConfig(rollbarWriteToken, Constants.EDDI_VERSION.ToString());
            config.RollbarTelemetryOptions.Reconfigure(new RollbarTelemetryOptions(true, 250));
            config.RollbarInfrastructureOptions.Reconfigure(new RollbarInfrastructureOptions(1, TimeSpan.FromSeconds(10)));
            config.RollbarLoggerConfig.Reconfigure(new RollbarLoggerConfig(rollbarWriteToken, Constants.EDDI_VERSION.ToString()));
            config.RollbarLoggerConfig.RollbarDataSecurityOptions.Reconfigure(
                new RollbarDataSecurityOptions(PersonDataCollectionPolicies.None,
                    IpAddressCollectionPolicy.DoNotCollect,
                    new[] { "Commander", "apiKey", "commanderName", "access_token", "refresh_token" }));
            config.RollbarLoggerConfig.RollbarPayloadAdditionOptions.Reconfigure(
                new RollbarPayloadAdditionOptions(
                        new Person(uniqueId + (fromVA ? " VA" : "")),
                        new Server() { Root = "/" }
                    )
                    { CodeVersion = ThisAssembly.Git.Sha }
            );
            RollbarInfrastructure.Instance.Init(config);
            RollbarLocator.RollbarInstance.Configure(config.RollbarLoggerConfig);

            RollbarInfrastructure.Instance.Start();
        }
    }
}
