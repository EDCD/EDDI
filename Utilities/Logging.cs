using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;
using Utilities.TelemetryService;

namespace Utilities
{
    public class Logging : Telemetry
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

        private static void handleLogging(ErrorLevel errorlevel, string message, object data, string memberName,
            string filePath)
        {
            try
            {
                System.Threading.Tasks.Task.Run(() =>
                {
                    Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
                    string timestamp = DateTime.UtcNow.ToString("s", CultureInfo.InvariantCulture);
                    var shortPath = Redaction.RedactEnvironmentVariables(Path.GetFileNameWithoutExtension(filePath));
                    var method = Redaction.RedactEnvironmentVariables(memberName);
                    message = $"{shortPath}:{method} {Redaction.RedactEnvironmentVariables(message)}";
                    var preppedData = FilterAndRedactData(data);

                    void handleTelemetry (bool reportTelemetry = false)
                    {
                        if ( TelemetryEnabled )
                        {
                            try
                            {
                                if ( reportTelemetry )
                                {
                                    ReportTelemetryEvent( timestamp, errorlevel, message, preppedData );
                                }
                                else
                                {
                                    RecordTelemetryInfo( errorlevel, message, preppedData );
                                }
                            }
                            catch ( TelemetryException tex )
                            {
                                Warn( tex.Message, tex );
                            }
                            catch ( HttpRequestException httpEx )
                            {
                                Warn( httpEx.Message, httpEx );
                            }
                            catch ( Exception ex )
                            {
                                Warn( ex.Message, ex );
                            }

                            if ( !string.IsNullOrEmpty( anonymousTelemetryID ) )
                            {
                                log( timestamp, errorlevel, $"Reporting error to telemetry service, anonymous ID {anonymousTelemetryID}: {message}" );
                            }
                        }
                    }

                    switch (errorlevel)
                    {
                        case ErrorLevel.Debug:
                        {
                            if (Verbose)
                            {
                                log(timestamp, errorlevel, message, preppedData);
                            }
                            handleTelemetry();
                            break;
                        }
                        case ErrorLevel.Info:
                        case ErrorLevel.Warning:
                        {
                            log(timestamp, errorlevel, message, preppedData);
                            handleTelemetry();
                            break;
                        }
                        case ErrorLevel.Error:
                        case ErrorLevel.Critical:
                        {
                            log(timestamp, errorlevel, message, preppedData);
                            handleTelemetry( true );
                            break;
                        }
                    }
                }).ConfigureAwait(false);
            }
            catch
            {
                // Nothing to do here
            }
        }

        private static readonly object logLock = new object();
        private static void log(string timestamp, ErrorLevel errorlevel, string message, object data = null)
        {
            var str = $"{timestamp} [{errorlevel}] {message}" + (data != null
                            ? $": {Redaction.RedactEnvironmentVariables(JsonConvert.SerializeObject(data))}"
                            : null);
            lock (logLock)
            {
                try
                {
                    using (StreamWriter file = new StreamWriter(LogFile, true))
                    {
                        file.WriteLine(str);
                    }
                }
                catch (Exception)
                {
                    // Failed; can't do anything about it as we're in the logging code anyway
                }
            }
            if (errorlevel == ErrorLevel.Error || errorlevel == ErrorLevel.Critical)
            {
                Console.WriteLine(str);
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
                        "refresh_token",
                        "uploaderID",
                        "commanderName"
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
}
