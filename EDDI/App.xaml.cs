using EddiConfigService;
using EddiConfigService.Configurations;
using EddiCore;
using EddiCore.Upgrader;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Utilities;

namespace Eddi
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static Mutex eddiMutex { get; private set; }

        // True if we have been started by VoiceAttack and the vaProxy object has been set
        public static bool FromVA => vaProxy != null;
        public static dynamic vaProxy;
        public static Action vaStartup;

        [STAThread]
        public static void Main()
        {
            if (!FromVA && AlreadyRunning()) { return; }

            App app = new App();
            app.Exit += OnExit;

            // Prepare to start the application
            Logging.IncrementLogs(); // Increment to a new log file.
            EDDIConfiguration configuration = ConfigService.Instance.eddiConfiguration;
            if (configuration != null && !configuration.DisableTelemetry)
            {
                StartTelemetryService(); // do immediately to initialize error reporting
            }
            ApplyAnyOverrideCulture(configuration); // this must be done before any UI is generated

            // Start by fetching information from the update server, and handling appropriately
            EddiUpgrader.CheckUpgrade();
            if ( EddiUpgrader.UpgradeRequired)
            {
                // We are too old to continue; initialize in a "safe mode". 
                EDDI.Init(true);
            }

            if (FromVA)
            {
                // Start with the MainWindow hidden
                app.MainWindow = new MainWindow();
                vaStartup?.Invoke();
                app.Run();
            }
            else
            {
                // Start by displaying the MainWindow
                app.Run(new MainWindow());
            }
        }

        private static void OnExit(object sender, ExitEventArgs e)
        {
            EDDI.Instance.Stop();

            if (!FromVA)
            {
                eddiMutex.ReleaseMutex();
            }
        }

        // We need to set and release our mutex from the same thread.
        // For VoiceAttack, this will be handled from the VoiceAttack plugin.
        // For standalone, this will be handled here.
        public static bool AlreadyRunning()
        {
            eddiMutex = new Mutex(true, Constants.EDDI_SYSTEM_MUTEX_NAME, out bool firstOwner);

            if (!firstOwner)
            {
                if (!FromVA)
                {
                    string localisedMultipleInstanceAlertTitle = Eddi.Properties.EddiResources.already_running_alert_title;
                    string localisedMultipleInstanceAlertText = Eddi.Properties.EddiResources.already_running_alert_body_text;
                    MessageBox.Show(localisedMultipleInstanceAlertText,
                                    localisedMultipleInstanceAlertTitle,
                                    MessageBoxButton.OK, MessageBoxImage.Information);
                    return true;
                }
                else
                {
                    vaProxy.WriteToLog("An instance of the EDDI application is already running.", "red");

                    MessageBoxResult result =
                        MessageBox.Show("An instance of EDDI is already running. Please close\r\n" +
                                        "the open EDDI application and click OK to continue. " +
                                        "If you click CANCEL, the EDDI VoiceAttack plugin will not be fully initialized.",
                                        "EDDI Instance Exists",
                                        MessageBoxButton.OKCancel, MessageBoxImage.Information);

                    // Any response will require the mutex to be reset
                    eddiMutex.Close();

                    if (MessageBoxResult.Cancel == result)
                    {
                        vaProxy.WriteToLog("EDDI initialization cancelled by user.", "red");
                        return true;
                    }
                }
            }
            return false;
        }

        private static void StartTelemetryService()
        {
            // Generate an id unique to this app run for bug tracking
            // and start the telemetry service
            var telemetryID = Guid.NewGuid().ToString();
            Utilities.TelemetryService.Telemetry.Start(telemetryID, FromVA);

            // Catch and send unhandled exceptions
            System.Windows.Forms.Application.ThreadException += (sender, args) =>
            {
                CrashLogger(args.Exception);
            };
            Current.DispatcherUnhandledException += (sender, args) =>
            {
                CrashLogger(args.Exception);
            };
            // Catch and send unhandled exceptions from non-UI threads
            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
            {
                CrashLogger(args.ExceptionObject as Exception);
            };
            // Catch and send unhandled exceptions from the task scheduler
            TaskScheduler.UnobservedTaskException += (sender, args) =>
            {
                CrashLogger(args.Exception);
            };
            // Catch and write managed exceptions to the local debug console (but do not send)
            AppDomain.CurrentDomain.FirstChanceException += (sender, args) =>
            {
                Debug.WriteLine(args.Exception.ToString());
            };
        }

        private static void CrashLogger(Exception ex)
        {
            // Suppress uncaught Rollbar internal HTTP exceptions
            if ( ex is AggregateException aex && 
                 aex.InnerException is HttpRequestException hre && 
                 hre.StackTrace.Contains("Rollbar") )
            {
                return;
            }

            Logging.Error($"Unhandled exception: {ex.Message}.", ex);
        }

        public static void ApplyAnyOverrideCulture(EDDIConfiguration configuration)
        {
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

        public static void OverrideThreadCulture(CultureInfo ci)
        {
            if (ci == null) { return; }
            Thread.CurrentThread.CurrentCulture = ci;
            Thread.CurrentThread.CurrentUICulture = ci;
        }
    }
}
