using System;
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
            // Generate or retrieve an id unique to this configuration for bug tracking
            string uniqueId = Eddi.Properties.Settings.Default.uniqueID ?? Guid.NewGuid().ToString();
            if (Eddi.Properties.Settings.Default.uniqueID == null)
            {
                Eddi.Properties.Settings.Default.uniqueID = uniqueId;
            }

            // Configure Rollbar error reporting
            _Rollbar.configureRollbar(uniqueId);

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

            MainWindow mainWindow = null;
            bool firstOwner = false;
            Mutex eddiMutex = new Mutex(true, Constants.EDDI_SYSTEM_MUTEX_NAME, out firstOwner);
            const string localisedMultipleInstanceAlertTitle = "EDDI is already running";
            const string localisedMultipleInstanceAlertText = "Only one instance of EDDI can run at at time.\r\n\r\nPlease close the other instance and try again.";

            if (firstOwner)
            {
                App app = new App();
                mainWindow = new MainWindow();
                app.Run(mainWindow);
                eddiMutex.ReleaseMutex();
            }
            else
            {
                MessageBox.Show(localisedMultipleInstanceAlertText,
                                localisedMultipleInstanceAlertTitle,
                                MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private static void ReloadAndRecover(Exception exception)
        {
            Logging.Debug("Reloading after unhandled exception: " + exception.ToString());
            EDDI.Instance.Stop();
            EDDI.Instance.Start();
        }
    }
}
