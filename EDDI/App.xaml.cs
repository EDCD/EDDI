using System;
using System.Threading;
using System.Windows;

namespace Eddi
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static Mutex eddiMutex = null;

        protected override void OnStartup(StartupEventArgs e)
        {
            const string appName = "EDCD/EDDI";
            bool createdNew;

            eddiMutex = new Mutex(true, appName, out createdNew);

            if (!createdNew)
            {
                // EDDI already running, so exit now.
                Application.Current.Shutdown();
            }

            base.OnStartup(e);
        }
    }
}
