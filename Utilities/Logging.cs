using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Rollbar;
using Rollbar.DTOs;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;
using Exception = System.Exception;

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

                    switch (errorlevel)
                    {
                        case ErrorLevel.Debug:
                        {
                            if (Verbose)
                            {
                                log(timestamp, errorlevel, message, preppedData);
                            }
                            if (TelemetryEnabled)
                            {
                                RecordTelemetryInfo(errorlevel, message, preppedData);
                            }
                            break;
                        }
                        case ErrorLevel.Info:
                        case ErrorLevel.Warning:
                        {
                            log(timestamp, errorlevel, message, preppedData);
                            if (TelemetryEnabled)
                            {
                                RecordTelemetryInfo(errorlevel, message, preppedData);
                            }
                            break;
                        }
                        case ErrorLevel.Error:
                        case ErrorLevel.Critical:
                        {
                            log(timestamp, errorlevel, message, preppedData);
                            if (TelemetryEnabled)
                            {
                                ReportTelemetryEvent(timestamp, errorlevel, message, preppedData);
                            }
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

        private static void RecordTelemetryInfo(ErrorLevel errorLevel, string message, IDictionary<string, object> preppedData = null)
        {
            if (Enum.TryParse(errorLevel.ToString(), out TelemetryLevel telemetryLevel))
            {
                try
                {
                    var telemetryBody = preppedData is null
                        ? new LogTelemetry(message)
                        : new LogTelemetry(message, preppedData);
                    var telemetry = new Rollbar.DTOs.Telemetry(TelemetrySource.Client, telemetryLevel, telemetryBody);
                    LockManager.GetLock(nameof(Telemetry), () =>
                    {
                        RollbarInfrastructure.Instance.TelemetryCollector?.Capture(telemetry);
                    });
                }
                catch (RollbarException rex)
                {
                    Warn(rex.Message, rex);
                }
                catch (HttpRequestException httpEx)
                {
                    Warn(httpEx.Message, httpEx);
                }
                catch (Exception ex)
                {
                    if (ex.Source != "Rollbar")
                    {
                        Warn(ex.Message, ex);
                    }
                }
            }
        }

        private static void ReportTelemetryEvent(string timestamp, ErrorLevel errorLevel, string message, Dictionary<string, object> preppedData = null)
        {
            try
            {
                LockManager.GetLock(nameof(Telemetry), () =>
                {
                    RollbarLocator.RollbarInstance.Log(errorLevel, message, preppedData);
                });
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
            catch (HttpRequestException httpEx)
            {
                Warn(httpEx.Message, httpEx);
            }
            catch (Exception ex)
            {
                Warn(ex.Message, ex);
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

    public class Telemetry
    {
        // Exception handling (configuration instructions are at https://github.com/rollbar/Rollbar.NET)
        // The Rollbar API test console is available at https://docs.rollbar.com/reference.

        const string rollbarWriteToken = "94b3c65b355448e6aa7040a5f6872a8a";

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

        public static void Start(string uniqueId, bool fromVA = false)
        {
            try
            {
                TelemetryEnabled = true;

                var config = new RollbarInfrastructureConfig( rollbarWriteToken, Constants.EDDI_VERSION.ToString() );

                // Configure telemetry
                var telemetryOptions = new RollbarTelemetryOptions( true, 250 );
                config.RollbarTelemetryOptions.Reconfigure( telemetryOptions );

                // Configure Infrastructure Options
                var infrastructureOptions = new RollbarInfrastructureOptions
                {
                    MaxReportsPerMinute = 1,
                    PayloadPostTimeout = TimeSpan.FromSeconds( 10 ),
                    CaptureUncaughtExceptions = false
                };
                config.RollbarInfrastructureOptions.Reconfigure( infrastructureOptions );

                // Configure Logger Options
                var loggerOptions = new RollbarLoggerConfig( rollbarWriteToken, Constants.EDDI_VERSION.ToString() );
                var loggerDataSecurityOptions = new RollbarDataSecurityOptions(
                    PersonDataCollectionPolicies.None,
                    IpAddressCollectionPolicy.DoNotCollect,
                    new[] { "Commander", "apiKey", "commanderName", "access_token", "refresh_token", "uploaderID" } );
                var assyMetadataAttributes = Assembly.GetExecutingAssembly()?.GetCustomAttributes<AssemblyMetadataAttribute>().ToList();
                var loggerPayloadOptions = new RollbarPayloadAdditionOptions()
                {
                    Person = new Person( uniqueId + ( fromVA ? " VA" : "" ) ),
                    Server = new Server
                    {
                        Root = "https://github.com/EDCD/EDDI",
                        Branch = assyMetadataAttributes.SingleOrDefault( a => a.Key == "SourceBranch")?.Value
                    },
                    CodeVersion = assyMetadataAttributes.SingleOrDefault( a => a.Key == "SourceRevisionId" )?.Value
                };
                loggerOptions.RollbarDataSecurityOptions.Reconfigure( loggerDataSecurityOptions );
                loggerOptions.RollbarPayloadAdditionOptions.Reconfigure( loggerPayloadOptions );
                config.RollbarLoggerConfig.Reconfigure( loggerOptions );

                // Initialize our configured client
                RollbarInfrastructure.Instance.Init( config );
                RollbarLocator.RollbarInstance.Configure( config.RollbarLoggerConfig );
                RollbarLocator.RollbarInstance.InternalEvent += OnRollbarInternalEvent;
                Thread.Sleep( 100 ); // Give some space for Rollbar to initialize before we begin sending data
            }
            catch ( Exception e )
            {
                TelemetryEnabled = false;
                Logging.Warn( "Telemetry process has failed", e );
            }
        }

        /// <summary>
        /// Called when rollbar internal event is detected.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RollbarEventArgs"/> instance containing the event data.</param>
        private static void OnRollbarInternalEvent ( object sender, RollbarEventArgs e )
        {
            if ( e is RollbarApiErrorEventArgs apiErrorEvent )
            {
                Logging.Warn(apiErrorEvent.ErrorDescription, apiErrorEvent);
                return;
            }

            if ( e is CommunicationEventArgs )
            {
                //TODO: handle/report Rollbar API communication event as needed...
                return;
            }

            if ( e is CommunicationErrorEventArgs commErrorEvent )
            {
                Logging.Warn( commErrorEvent.Error.Message, commErrorEvent );
                return;
            }

            if ( e is InternalErrorEventArgs internalErrorEvent )
            {
                Logging.Warn( internalErrorEvent.Details, internalErrorEvent );
                return;
            }
        }
    }
}
