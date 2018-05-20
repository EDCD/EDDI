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
        [STAThread]
        static void Main()
        {
            // Configure Rollbar error reporting

            // Generate or retrieve an id unique to this configuration for bug tracking
            if (Eddi.Properties.Settings.Default.uniqueID == null ||
                Eddi.Properties.Settings.Default.uniqueID == "")
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

            // Start the application

            ApplyAnyOverrideCulture(); // this must be done before any UI is generated

            MainWindow mainWindow = null;
            bool firstOwner = false;
            Mutex eddiMutex = new Mutex(true, Constants.EDDI_SYSTEM_MUTEX_NAME, out firstOwner);

            if (firstOwner)
            {
                App app = new App();
                mainWindow = new MainWindow();
                app.Run(mainWindow);
                eddiMutex.ReleaseMutex();
            }
            else
            {
                string localisedMultipleInstanceAlertTitle = Eddi.Properties.EddiResources.already_running_alert_title;
                string localisedMultipleInstanceAlertText = Eddi.Properties.EddiResources.already_running_alert_body_text;
                MessageBox.Show(localisedMultipleInstanceAlertText,
                                localisedMultipleInstanceAlertTitle,
                                MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private static void ApplyAnyOverrideCulture()
        {
            try
            {
                string overrideCultureName = Eddi.Properties.Settings.Default.OverrideCulture;
                // we are using the InvariantCulture name "" to mean user's culture
                CultureInfo overrideCulture = String.IsNullOrEmpty(overrideCultureName) ? null : new CultureInfo(overrideCultureName);
                ApplyCulture(overrideCulture);
            }
            catch
            {
                ApplyCulture(null);
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
