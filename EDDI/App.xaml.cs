using System;
using System.Diagnostics;
using System.Globalization;
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

        [STAThread]
        public static void Main()
        {
            if (!FromVA && AlreadyRunning()) { return; }

            App app = new App();
            app.Exit += OnExit;

            // Start the application
            Logging.incrementLogs(); // Increment to a new log file.
            StartRollbar(); // do immediately to initialize error reporting
            ApplyAnyOverrideCulture(); // this must be done before any UI is generated

            if (FromVA)
            {
                // Start with the MainWindow hidden
                app.MainWindow = new MainWindow();
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
#pragma warning disable IDE0067 // Dispose objects before losing scope
            eddiMutex = new Mutex(true, Constants.EDDI_SYSTEM_MUTEX_NAME, out bool firstOwner);
#pragma warning restore IDE0067 // Dispose objects before losing scope

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

        public static void StartRollbar()
        {
            // Configure Rollbar error reporting

            // Generate or retrieve an id unique to this configuration for bug tracking
            if (string.IsNullOrEmpty(Eddi.Properties.Settings.Default.uniqueID))
            {
                Eddi.Properties.Settings.Default.uniqueID = Guid.NewGuid().ToString();
            }
            _Rollbar.configureRollbar(Eddi.Properties.Settings.Default.uniqueID);

            // Catch and send unhandled exceptions from Windows forms
            System.Windows.Forms.Application.ThreadException += (sender, args) =>
            {
                Exception exception = args.Exception as Exception;
                _Rollbar.ExceptionHandler(exception);
                ReloadAndRecover(exception);
            };
            // Catch and send unhandled exceptions from non-UI threads
            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
            {
                Exception exception = args.ExceptionObject as Exception;
                _Rollbar.ExceptionHandler(exception);
                ReloadAndRecover(exception);
            };
            // Catch and send unhandled exceptions from the task scheduler
            TaskScheduler.UnobservedTaskException += (sender, args) =>
            {
                Exception exception = args.Exception as Exception;
                _Rollbar.ExceptionHandler(exception);
                ReloadAndRecover(exception);
            };
            // Catch and write managed exceptions to the local debug console (but do not send)
            AppDomain.CurrentDomain.FirstChanceException += (sender, eventArgs) =>
            {
                Debug.WriteLine(eventArgs.Exception.ToString());
            };
        }

        public static void ApplyAnyOverrideCulture()
        {
            string overrideCultureName = null;
            try
            {
                // Use Eddi.Properties.Settings if an override culture isn't set in our configuration
                EDDIConfiguration configuration = EDDIConfiguration.FromFile();
                if (configuration.OverrideCulture is null && !string.IsNullOrEmpty(Eddi.Properties.Settings.Default.OverrideCulture))
                {
                    configuration.OverrideCulture = Eddi.Properties.Settings.Default.OverrideCulture;
                    configuration.ToFile();
                }

                overrideCultureName = configuration.OverrideCulture;

                // we are using the InvariantCulture name "" to mean user's culture
                CultureInfo overrideCulture = string.IsNullOrEmpty(overrideCultureName) ? null : new CultureInfo(overrideCultureName);
                ApplyCulture(overrideCulture);
            }
            catch
            {
                ApplyCulture(null);
                Debug.WriteLine("Culture [{0}] not available", overrideCultureName);
            }
        }

        private static void ApplyCulture(CultureInfo ci)
        {
            CultureInfo.DefaultThreadCurrentCulture = ci;
            CultureInfo.DefaultThreadCurrentUICulture = ci;
            if (ci != null)
            {
                Thread.CurrentThread.CurrentCulture = ci;
                Thread.CurrentThread.CurrentUICulture = ci;
            }
        }

        private static void ReloadAndRecover(Exception exception)
        {
#if DEBUG
#else
            Logging.Debug("Reloading after unhandled exception: " + exception.ToString());
            EDDI.Instance.Stop();
            EDDI.Instance.Start();
#endif
        }
    }
}
