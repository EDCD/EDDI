using System;
using System.Globalization;
using System.Threading;
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
                string localisedMultipleInstanceAlertTitle = Eddi.Properties.Resources.already_running_alert_title;
                string localisedMultipleInstanceAlertText = Eddi.Properties.Resources.already_running_alert_body_text;
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
    }
}
