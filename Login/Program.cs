using System;
using System.Windows.Forms;
using EliteDangerousCompanionAppService;
using EliteDangerousNetLogMonitor;
using EliteDangerousDataDefinitions;

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

            Credentials Credentials = Credentials.FromFile();
            if (Credentials == null || Credentials.appId == null || Credentials.machineId == null || Credentials.machineToken == null)
            {
                Application.Run(new Stage1());
            }
            else
            {
                CompanionAppService app = new CompanionAppService(Credentials);
                Commander Cmdr;
                try
                {
                    Cmdr = app.Profile();
                }
                catch (Exception e)
                {
                    Cmdr = null;
                }

                if (Cmdr == null)
                {
                    // Something wrong with the credentials
                    Credentials.Clear();
                    Credentials.ToFile();
                    Application.Run(new Stage1());
                }
                else
                {
                    // Credentials are good.  Skip straight to stage 3
                    Application.Run(new Stage3(Cmdr.Name));
                }
            }
        }
    }
}
