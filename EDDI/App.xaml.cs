using EddiConfigService;
using EddiConfigService.Configurations;
using EddiCompanionAppService;
using EddiCore;
using EddiCore.ApplicationContext;
using EddiCore.Upgrader;
using EddiSpeechService;
using EddiStatusService;
using EddiUI;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Utilities;

[assembly: InternalsVisibleTo( "Tests" )]
namespace Eddi
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static Mutex eddiMutex { get; internal set; }

        public static IApplicationContext ApplicationContext { get; private set; }

        // True if we have been started by VoiceAttack and the VaProxy object has been set
        public static System.Version VoiceAttackVersion { get; set; }
        public static bool FromVA => VoiceAttackVersion != null;
        public static Action vaStartup;

        [ STAThread ]
        public static void Main ()
        {
            if ( !FromVA && AlreadyRunning() )
            {
                var localisedMultipleInstanceAlertTitle = EddiCore.Properties.Resources.already_running_alert_title;
                var localisedMultipleInstanceAlertText = EddiCore.Properties.Resources.already_running_alert_body_text;
                MessageBox.Show( localisedMultipleInstanceAlertText,
                    localisedMultipleInstanceAlertTitle,
                    MessageBoxButton.OK, MessageBoxImage.Information );
                return;
            }

            var app = new App();
            app.Exit += OnExit;

            // Prepare to start the application
            ApplicationContext = new WpfApplicationContext();
            Logging.IncrementLogs(); // Increment to a new log file.
            var configuration = ConfigService.Instance.eddiConfiguration;
            if ( configuration != null && !configuration.DisableTelemetry )
            {
                StartTelemetryService(); // do immediately to initialize error reporting
            }

            ApplyAnyOverrideCulture( configuration ); // this must be done before any UI is generated

            // Start by fetching information from the update server, and handling appropriately.
            // This completes before showing any UI so that VoiceAttack can report the availability of the upgrade during its startup.
            EddiUpgrader.CheckUpgradeAsync().GetResultOrTimeout( TimeSpan.FromSeconds( 10 ) );

            // Initialize CompanionAppService DDE on UI thread BEFORE async preload
            // (must be done before MainWindow creation and Task.WaitAll to avoid deadlock)
            if ( ApplicationContext.HasUIDispatcher )
            {
                try
                {
                    var companionService = CompanionAppService.Instance;
                    companionService.InitializeOAuthCallback();
                    Logging.Debug( "CompanionAppService DDE initialized" );
                }
                catch ( Exception ex )
                {
                    Logging.Error( "Failed to initialize CompanionAppService DDE", ex );
                }
            }

            // Wait for preload to complete before MainWindow creation
            var preloadTasks = PreloadCriticalServicesAsync();
            Task.WaitAll( preloadTasks.ToArray() );

            try
            {
                if ( FromVA )
                {
                    // Create the MainWindow with visibility controlled by code-behind logic
                    // (hidden by default in VA mode, shown on demand via VA commands)
                    EDDI.FromVA = FromVA;
                    app.MainWindow = new MainWindow();
                    vaStartup?.Invoke();
                    app.Run();
                }
                else
                {
                    // Start by displaying the MainWindow
                    app.Run( new MainWindow() );
                }
            }
            catch ( Exception e )
            {
                // Catch exceptions from the main UI thread
                CrashLogger( e );
            }
        }

        // Parallel pre-load of service singletons
        private static Task[] PreloadCriticalServicesAsync ()
        {
            return
            [
                // Pre-warm ConfigService (file I/O)
                Task.Run(() => {
                    try {
                        _ = ConfigService.Instance;
                        Logging.Debug("ConfigService preloaded");
                    } catch (Exception ex) {
                        Logging.Error("Failed to preload ConfigService", ex);
                    }
                }),

                // Pre-load speech service asynchronously
                Task.Run(() => {
                    try
                    {
                        _ = SpeechService.Instance;
                        Logging.Debug( "SpeechService preloaded" );
                    }
                    catch ( Exception ex )
                    {
                        Logging.Error( "Failed to preload SpeechService", ex );
                    }
                }),

                // Pre-load status service asynchronously  
                Task.Run(() => {
                    try
                    {
                        _ = StatusService.Instance;
                        Logging.Debug( "StatusService preloaded" );
                    }
                    catch ( Exception ex )
                    {
                        Logging.Error( "Failed to preload StatusService", ex );
                    }
                })
            ];
        }

        private static void OnExit(object sender, ExitEventArgs e)
        {
            if ( !FromVA )
            {
                EDDI.Instance.Stop();
            }

            ApplicationContext?.InvokeOnUIThread( () =>
            {
                eddiMutex.ReleaseMutex();
            } );
        }

        // We need to set and release our mutex from the same thread.
        // For VoiceAttack, this will be handled from the VoiceAttack plugin.
        // For standalone, this will be handled here.
        public static bool AlreadyRunning()
        {
            eddiMutex = new Mutex(true, Constants.EDDI_SYSTEM_MUTEX_NAME, out bool firstOwner);
            return !firstOwner;
        }

        private static void StartTelemetryService()
        {
            // Generate an id unique to this app run for bug tracking
            // and start the telemetry service
            var telemetryID = Convert.ToBase64String( Guid.NewGuid().ToByteArray() ).Replace("=", "");
            Utilities.TelemetryService.Telemetry.Start( telemetryID, VoiceAttackVersion );

            // Catch and send unhandled exceptions
            System.Windows.Forms.Application.ThreadException += (_, args) =>
            {
                CrashLogger(args.Exception);
            };
            // Catch and send unhandled exceptions from non-UI threads
            AppDomain.CurrentDomain.UnhandledException += (_, args) =>
            {
                CrashLogger(args.ExceptionObject as Exception);
            };
            // Catch and send unhandled exceptions from the task scheduler
            TaskScheduler.UnobservedTaskException += (_, args) =>
            {
                CrashLogger(args.Exception);
            };
            // Catch and write managed exceptions to the local debug console (but do not send)
            AppDomain.CurrentDomain.FirstChanceException += (_, args) =>
            {
                Debug.WriteLine(args.Exception.ToString());
            };
        }

        private static void CrashLogger(Exception ex)
        {
            // Suppress uncaught Rollbar internal exceptions
            if ( ex.InnerException?.Source == "Rollbar" ||
                 ( ex is AggregateException aex &&
                   aex.InnerExceptions.Any( ie => ie.StackTrace != null && ie.StackTrace.Contains( "Rollbar" ) ) ) )
            {
                return;
            }

            Logging.Error($"Unhandled exception: {ex.Message}.", ex);
        }

        public static void ApplyAnyOverrideCulture(EDDIConfiguration configuration = null)
        {
            if ( configuration is null )
            {
                configuration = ConfigService.Instance.eddiConfiguration;
            }

            try
            {
                // we are using the InvariantCulture name "" to mean user's culture
                var overrideCulture = string.IsNullOrEmpty(configuration.OverrideCulture) ? null : new CultureInfo(configuration.OverrideCulture);
                ApplyCulture(overrideCulture);
            }
            catch
            {
                ApplyCulture(null);
                Debug.WriteLine("Culture [{0}] not available", configuration.OverrideCulture);
            }
        }

        private static void ApplyCulture(CultureInfo ci)
        {
            CultureInfo.DefaultThreadCurrentCulture = ci;
            CultureInfo.DefaultThreadCurrentUICulture = ci;
            OverrideThreadCulture(ci);
        }

        private static void OverrideThreadCulture(CultureInfo ci)
        {
            if (ci == null) { return; }
            Thread.CurrentThread.CurrentCulture = ci;
            Thread.CurrentThread.CurrentUICulture = ci;
        }
    }
}
