using System;
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
    }
}
