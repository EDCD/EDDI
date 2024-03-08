using Rollbar.DTOs;
using Rollbar;
using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Collections.Generic;

namespace Utilities.TelemetryService
{
    public abstract class Telemetry
    {
        // Exception handling (configuration instructions are at https://github.com/rollbar/Rollbar.NET)
        // The Rollbar API test console is available at https://docs.rollbar.com/reference.

        public static bool TelemetryEnabled
        {
            get => RollbarLocator.RollbarInstance.Config.RollbarDeveloperOptions.Transmit;
            // ReSharper disable once ValueParameterNotUsed
            set => RollbarLocator.RollbarInstance.Config.RollbarDeveloperOptions.Transmit =
#if DEBUG
                false;
#else
                value;
#endif
        }

        public enum ErrorLevel
        {
            /// <summary>The debug log level.</summary>
            Debug,
            /// <summary>The informational log level.</summary>
            Info,
            /// <summary>The warning log level.</summary>
            Warning,
            /// <summary>The error log level.</summary>
            Error,
            /// <summary>The critical error/log level.</summary>
            Critical,
        }

        protected static string anonymousTelemetryID => RollbarLocator.RollbarInstance.Config.RollbarPayloadAdditionOptions.Person?.Id;

        public static void Start ( string uniqueId, bool fromVA = false )
        {
            try
            {
                TelemetryEnabled = true;

                var config = new RollbarInfrastructureConfig( TelemetryTokens.rollbarToken, Constants.EDDI_VERSION.ToString() );

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
                var loggerOptions = new RollbarLoggerConfig( TelemetryTokens.rollbarToken, Constants.EDDI_VERSION.ToString() );
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
            catch ( System.Exception e )
            {
                TelemetryEnabled = false;
                Logging.Warn( "Telemetry process has failed", e );
            }
        }

        internal static void RecordTelemetryInfo ( ErrorLevel errorLevel, string message, IDictionary<string, object> preppedData = null )
        {
            if ( Enum.TryParse( errorLevel.ToString(), out TelemetryLevel telemetryLevel ) )
            {
                try
                {
                    var telemetryBody = preppedData is null
                        ? new LogTelemetry(message)
                        : new LogTelemetry(message, preppedData);
                    var telemetry = new Rollbar.DTOs.Telemetry(TelemetrySource.Client, telemetryLevel, telemetryBody);
                    LockManager.GetLock( nameof( Telemetry ), () =>
                    {
                        RollbarInfrastructure.Instance.TelemetryCollector?.Capture( telemetry );
                    } );
                }
                catch ( RollbarException rex )
                {
                    throw new TelemetryException( rex.Message, rex );
                }
                catch ( System.Exception ex )
                {
                    if ( ex.Source != "Rollbar" )
                    {
                        throw;
                    }
                }
            }
        }

        internal static void ReportTelemetryEvent ( string timestamp, ErrorLevel errorLevel, string message, Dictionary<string, object> preppedData = null )
        {
            try
            {
                LockManager.GetLock( nameof( Telemetry ), () =>
                {
                    RollbarLocator.RollbarInstance.Log( errorLevel.ToRollbarErrorLevel(), message, preppedData );
                } );
            }
            catch ( RollbarException rex )
            {
                throw new TelemetryException( rex.Message, rex );
            }
            catch ( System.Exception ex )
            {
                if ( ex.Source != "Rollbar" )
                {
                    throw;
                }
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
                Logging.Warn( apiErrorEvent.ErrorDescription, apiErrorEvent );
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

            Logging.Warn( e.ToString() );
        }
    }
}
