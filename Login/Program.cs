using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

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
                Application.Run(new Stage3());
            }

        }
    }
}
