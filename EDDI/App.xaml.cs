using EddiCompanionAppService;
using EddiConfigService;
using EddiConfigService.Configurations;
using EddiCore;
using EddiCore.Upgrader;
using EddiStatusService;
using EddiUI;
using System;
using System.Diagnostics;
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

        // VoiceAttack host application version (bootstrap from process args; authoritative value can be overwritten via IPC handshake)
        public static System.Version VoiceAttackVersion { get; set; }

        [ STAThread ]
        public static void Main ( string[] args = null )
        {
            // Parse command-line arguments
            args ??= Environment.GetCommandLineArgs().Skip( 1 ).ToArray();
            var fromVoiceAttack = args.Any(arg =>
                arg.Equals("--voice-attack-plugin", StringComparison.OrdinalIgnoreCase));
            VoiceAttackVersion = ParseVoiceAttackVersion( args );

            Logging.IncrementLogs(); // Increment to a new log file.
            RefreshInstallLocator();
            var configuration = ConfigService.Instance.eddiConfiguration;

            // Must happen before any EDDI.Instance access, responder discovery,
            // SpeechResponder construction, or Personality.Default() call.
            ApplyAnyOverrideCulture( configuration );

            if ( VoiceAttackVersion != null )
            {
                Logging.Info( $"Parsed VoiceAttack version from process args: {VoiceAttackVersion}" );
            }

            if ( AlreadyRunning() )
            {
                var localisedMultipleInstanceAlertTitle = EddiCore.Properties.Resources.already_running_alert_title;
                var localisedMultipleInstanceAlertText = EddiCore.Properties.Resources.already_running_alert_body_text;
                MessageBox.Show( localisedMultipleInstanceAlertText,
                    localisedMultipleInstanceAlertTitle,
                    MessageBoxButton.OK, MessageBoxImage.Information );
                return;
            }

            var app = new App();
            LoadApplicationResources( app );
            app.Exit += OnExit;

            try
            {
                Initialize( app, fromVoiceAttack, VoiceAttackVersion, configuration );
            }
            catch ( Exception e )
            {
                CrashLogger( e );
            }
        }

        private static void LoadApplicationResources ( App app )
        {
            var resourceLocator = new Uri( "/Eddi;component/app.xaml", UriKind.Relative );
            Application.LoadComponent( app, resourceLocator );
        }

        internal static bool RefreshInstallLocator (
            IEddiInstallLocatorWriterStore store = null,
            string executablePath = null )
        {
            executablePath ??= Environment.ProcessPath ??
                               Process.GetCurrentProcess().MainModule?.FileName;

            return EddiInstallLocatorWriter.TryWriteCurrentUserInstallLocation(
                executablePath,
                Constants.EDDI_VERSION.ToString(),
                store );
        }

        /// <summary>
        /// Parse VoiceAttack host version from process args, if provided.
        /// Supports both '--voice-attack-version X.X.X' and '--voice-attack-version=X.X.X'.
        /// </summary>
        private static System.Version ParseVoiceAttackVersion( string[] args )
        {
            ArgumentNullException.ThrowIfNull( args );

            for ( var i = 0; i < args.Length; i++ )
            {
                var arg = args[ i ];

                if ( !arg.StartsWith( "--voice-attack-version", StringComparison.OrdinalIgnoreCase ) )
                {
                    continue;
                }

                if ( ( i + 1 ) < args.Length && System.Version.TryParse( args[ i + 1 ], out var versionFromNextArg ) )
                {
                    return versionFromNextArg;
                }

                var parts = arg.Split( '=', StringSplitOptions.RemoveEmptyEntries );
                if ( parts.Length == 2 && System.Version.TryParse( parts[ 1 ], out var versionFromEquals ) )
                {
                    return versionFromEquals;
                }
            }

            return null;
        }

        private static void Initialize ( App app, bool fromVA = false, System.Version vaVersion = null, EDDIConfiguration configuration = null )
        {
            // Initialize our dynamic theme management engine
            EddiUI.Themes.ThemeManager.Initialize();

            // Prepare to start the application
            if ( configuration != null && !configuration.DisableTelemetry )
            {
                StartTelemetryService( vaVersion ); // do immediately to initialize error reporting
            }

            // Start by fetching information from the update server, and handling appropriately.
            // This completes before showing any UI so that VoiceAttack can report the availability of the upgrade during its startup.
            EddiUpgrader.CheckUpgradeAsync().GetResultOrTimeout( TimeSpan.FromSeconds( 10 ) );

            // Initialize CompanionAppService DDE on UI thread BEFORE async preload
            // (must be done before MainWindow creation and Task.WaitAll to avoid deadlock)
            if ( Current != null )
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

            EDDI.Instance.FromVA = fromVA;

            // Wait for preload to complete before MainWindow creation
            var preloadTasks = PreloadCriticalServicesAsync();
            Task.WaitAll( preloadTasks.ToArray() );

            if ( fromVA )
            {
                // Create the MainWindow with visibility controlled by code-behind logic
                // (hidden by default in VA mode, shown on demand via VA commands)
                app.MainWindow = new MainWindow();
                app.Run();
            }
            else
            {
                // Start by displaying the MainWindow
                app.Run( new MainWindow() );
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
            // Always stop the EDDI instance so monitors and services are shut down
            // cleanly before the process exits.  
            EDDI.Instance.Stop();
            ConfigService.Instance.Dispose();

            Current?.Dispatcher?.InvokeAsync( () => {
                eddiMutex.ReleaseMutex();
            } );
        }

        // We need to set and release our mutex from the same thread.
        // For VoiceAttack, this will be handled from the VoiceAttack plugin.
        // For standalone, this will be handled here.
        public static bool AlreadyRunning()
        {
            eddiMutex = new Mutex(true, Constants.EDDI_SYSTEM_MUTEX_NAME, out var firstOwner);
            return !firstOwner;
        }

        private static void StartTelemetryService(System.Version voiceAttackVersion)
        {
            // Generate an id unique to this app run for bug tracking
            // and start the telemetry service
            var telemetryID = Convert.ToBase64String( Guid.NewGuid().ToByteArray() ).Replace("=", "");
            Utilities.TelemetryService.Telemetry.Start( telemetryID, voiceAttackVersion );

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
            ApplicationCulture.ApplyAnyOverrideCulture( configuration );
        }
    }
}
