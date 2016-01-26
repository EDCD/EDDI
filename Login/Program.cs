using System;
using System.Windows.Forms;
using EliteDangerousCompanionAppService;
using EliteDangerousNetLogMonitor;

namespace EliteDangerousDataProvider
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            //NetLogMonitor monitor = new NetLogMonitor("C:\\Program Files (x86)\\Elite\\Products\\elite-dangerous-64\\Logs", (result) => Console.WriteLine(result));
            //monitor.start();

            Credentials Credentials = Credentials.FromFile();
            if (Credentials == null || Credentials.appId == null || Credentials.machineId == null || Credentials.machineToken == null)
            {
                Application.Run(new Stage1());
            }
            else
            {
                Application.Run(new Stage3());
            }

        }
    }
}
