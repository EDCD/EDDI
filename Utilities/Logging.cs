using JetBrains.Annotations;
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

        private static readonly string LogFile = Constants.DATA_DIR + @"\eddi.log";

        public static bool Verbose { get; set; }

        public static void Error(string message, object data = null, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "")
        {
            Handle(ErrorLevel.Error, message, data is null ? null : JToken.FromObject(data), memberName, filePath);
        }

        public static void Warn(string message, object data = null, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "")
        {
            Handle(ErrorLevel.Warning, message, data is null ? null : JToken.FromObject( data ), memberName, filePath);
        }

        public static void Info(string message, object data = null, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "")
        {
            Handle(ErrorLevel.Info, message, data is null ? null : JToken.FromObject( data ), memberName, filePath);
        }

        public static void Debug(string message, object data = null, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "")
        {
            Handle(ErrorLevel.Debug, message, data is null ? null : JToken.FromObject( data ), memberName, filePath);
        }

        private static void Handle(ErrorLevel errorlevel, string message, [CanBeNull] JToken data, string memberName,
            string filePath)
        {
            try
            {
                System.Threading.Tasks.Task.Run(() =>
                {
                    Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
                    Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
                    var timestamp = DateTime.UtcNow.ToString("s", CultureInfo.InvariantCulture);
                    message = PrepareMessage( message, memberName, filePath );
                    var preppedData = PrepareData( data );

                    switch (errorlevel)
                    {
                        case ErrorLevel.Debug:
                        {
                            if (Verbose)
                            {
                                WriteToLog(timestamp, errorlevel, message, preppedData);
                            }
                            HandleTelemetry( errorlevel, message, timestamp, false, preppedData );
                            break;
                        }
                        case ErrorLevel.Info:
                        case ErrorLevel.Warning:
                        {
                            WriteToLog(timestamp, errorlevel, message, preppedData);
                            HandleTelemetry( errorlevel, message, timestamp, false, preppedData );
                            break;
                        }
                        case ErrorLevel.Error:
                        case ErrorLevel.Critical:
                        {
                            WriteToLog(timestamp, errorlevel, message, preppedData);
                            HandleTelemetry( errorlevel, message, timestamp, true, preppedData );
                            break;
                        }
                    }
                } ).ConfigureAwait(false);
            }
            catch
            {
                // Nothing to do here
            }
        }
        
        private static string PrepareMessage ( string message, string memberName, string filePath )
        {
            var shortPath = Redaction.RedactEnvironmentVariables( Path.GetFileNameWithoutExtension( filePath ) );
            var method = Redaction.RedactEnvironmentVariables( memberName );
            message = $"{shortPath}:{method} {Redaction.RedactEnvironmentVariables( message )}";
            return message;
        }

        private static Dictionary<string, object> PrepareData ( [CanBeNull] JToken data )
        {
            if ( data == null ) { return null; }
            if ( data.Type == JTokenType.String && !JsonRegex.IsMatch( data.ToString() ) )
            {
                return WrapData( "message", Redaction.RedactEnvironmentVariables( data.ToString() ) );
            }
            else
            {
                try
                {
                    data = Redaction.RedactEnvironmentVariables( data );
                    data = Redaction.RedactPersonalProperties( data );
                    if ( data is JObject )
                    {
                        return data.ToObject<Dictionary<string, object>>();
                    }
                    return WrapData( "data", data );
                }
                catch ( ObjectDisposedException )
                {
                    return null;
                }
            }
        }

        private static Dictionary<string, object> WrapData ( string key, object data )
        {
            var wrappedData = new Dictionary<string, object>()
            {
                {key, data}
            };
            return wrappedData;
        }

        private static readonly object logLock = new object();

        private static void WriteToLog ( string timestamp, ErrorLevel errorlevel, string message, object preppedData = null )
        {
            var str = $"{timestamp} [{errorlevel}] {message}" + ( preppedData != null
                ? $": {Redaction.RedactEnvironmentVariables( JsonConvert.SerializeObject( preppedData ) )}"
                : null );
            if ( string.IsNullOrEmpty( str ) ) { return; }

            lock ( logLock )
            {
                try
                {
                    // Log to console
                    Console.WriteLine( str );

                    // Log to file
                    using ( StreamWriter file = new StreamWriter( LogFile, true ) )
                    {
                        file.WriteLine( str );
                    }
                }
                catch ( Exception )
                {
                    // Failed; can't do anything about it as we're in the logging code anyway
                }
            }
        }

        private static void HandleTelemetry ( ErrorLevel errorlevel, string message, string timestamp, bool reportTelemetry,
            Dictionary<string, object> preppedData )
        {
            if ( TelemetryEnabled )
            {
                try
                {
                    if ( reportTelemetry && ( errorlevel == ErrorLevel.Error || errorlevel == ErrorLevel.Critical ) )
                    {
                        if ( !string.IsNullOrEmpty( anonymousTelemetryID ) )
                        {
                            WriteToLog( timestamp, errorlevel,
                                $"Reporting error to telemetry service, anonymous ID {anonymousTelemetryID}: {message}" );
                        }
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
            }
        }

        public static void IncrementLogs()
        {
            // Ensure dir exists
            DirectoryInfo directoryInfo = new DirectoryInfo(Constants.DATA_DIR);
            if (!directoryInfo.Exists)
            {
                Directory.CreateDirectory(Constants.DATA_DIR);
            }

            // Obtain files, sorting by last write time to ensure that older files are incremented prior to newer files
            foreach ( var file in directoryInfo.GetFiles().OrderBy( f => f.LastWriteTimeUtc ).ToList() )
            {
                var filePath = file.FullName;
                if ( !filePath.EndsWith( ".log" ) ) { continue; }

                try
                {
                    int.TryParse( filePath.Replace( Constants.DATA_DIR + @"\eddi", "" ).Replace( ".log", "" ),
                        out int i );
                    ++i; // Increment our index number

                    if ( i >= 10 )
                    {
                        File.Delete( filePath );
                    }
                    else
                    {
                        // This might be our primary log file, so we lock it prior to doing anything with it
                        lock ( logLock )
                        {
                            File.Move( filePath, Constants.DATA_DIR + @"\eddi" + i + ".log" );
                        }
                    }
                }
                catch ( Exception )
                {
                    // Someone may have had a log file open when this code executed? Nothing to do, we'll try again on the next run
                }
            }
        }
    }
}
