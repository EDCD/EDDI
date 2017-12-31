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
        private static Mutex eddiMutex = new Mutex(true, "{F1F85B96-14B8-45E4-AD19-0B2FCD6F6CF8}");
        private static MainWindow mainWindow = null;

        [STAThread]
        static void Main()
        {
            if (eddiMutex.WaitOne(TimeSpan.Zero, true))
            {
                App app = new App();
                mainWindow = new MainWindow();
                app.Run(mainWindow);
                eddiMutex.ReleaseMutex();
            }
        }

        //private static Mutex eddiMutex = null;

        //[STAThread]
        //static void Main()
        //{
        //    MainWindow mainWindow = null;
        //    bool createdNew;

        //    eddiMutex = new Mutex(true, "{F1F85B96-14B8-45E4-AD19-0B2FCD6F6CF8}", out createdNew);

        //    if (createdNew)
        //    {
        //        App app = new App();
        //        mainWindow = new MainWindow();
        //        app.Run(mainWindow);
        //        eddiMutex.ReleaseMutex();
        //    }
        //}

        //private static Mutex eddiMutex = null;

        //protected override void OnStartup(StartupEventArgs e)
        //{
        //    const string appName = "EDCD/EDDI";
        //    bool createdNew;

        //    eddiMutex = new Mutex(true, appName, out createdNew);

        //    if (!createdNew)
        //    {
        //        // EDDI already running, so exit now.
        //        Application.Current.Shutdown();
        //    }

        //    base.OnStartup(e);
        //}

        //public partial class App : Application
        //{
        //    private void Application_Startup(object sender, StartupEventArgs e)
        //    {
        //    }
        //}
    }
}
