using EliteDangerousCompanionAppService;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using EliteDangerousDataDefinitions;
using System.IO;
using EliteDangerousNetLogMonitor;

namespace EliteDangerousDataProvider
{
    public partial class Stage3 : Form
    {
        string name;

        public Stage3(string name)
        {
            InitializeComponent();
            this.name = name;
            selectButton.Hide();
            label1.Text = "Thank you Commander " + name + ".  EDDI needs the path to your netlog files so that it knows when you enter and leave systems.  Please start the Elite: Dangerous game then come back to this screen and hit the \"Obtain\" button";
        }

        private void obtainButton_Click(object sender, EventArgs e)
        {
            List<string> processPaths = new Finder().GetPathFromProcess();
            if (processPaths.Count != 0)
            {
                NetLogMonitor.WritePath(processPaths[0] + @"\Logs");
                label1.Text = "Thank you Commander " + name + ".  The required details for your game have been stored.  This completes the setup; you can now close this window.";
                obtainButton.Hide();
            }
            else
            {
                label1.Text = @"Unfortuantely we were unable to locate your product directory.  Please select your 'elite-dangerous-64' directory.  Possible locations include:";
                List <string> paths = new Finder().FindInstallationPaths();
                if (paths.Count == 0)
                {
                    paths.Add(Finder.DefProductsPath + @"\elite-dangerous-64");
                    paths.Add(Finder.DefLauncherPath + @"\elite-dangerous-64");
                    paths.Add(@"C:\Program Files (x86)\Steam\\SteamApps\common\Elite Dangerous\Products\elite-dangerous-64 (for Steam)");
                }
                foreach (string path in paths)
                {
                    label1.Text += "\r\n\r\n" + path;
                }
                label1.Text += "\r\n\r\nWhichever directory you select should contain a Logs directory and inside that a number of 'debugLog' files.";
                obtainButton.Hide();
                selectButton.Show();
            }
        }

        private void selectButton_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            DialogResult result = fbd.ShowDialog();
            if (fbd.SelectedPath != null)
            {
                if (fbd.SelectedPath.EndsWith("ogs"))
                {
                    // User selected the logs subdirectory
                    NetLogMonitor.WritePath(fbd.SelectedPath);
                }
                else if (fbd.SelectedPath.EndsWith("oducts"))
                {
                    // User selected the products superdirectory
                    NetLogMonitor.WritePath(fbd.SelectedPath + @"\elite-dangerous-64\Logs");
                }
                else
                {
                    // Assume it's the correct path
                    NetLogMonitor.WritePath(fbd.SelectedPath + @"\Logs");
                }
                label1.Text = "Thank you Commander " + name + ".  EDDI Is now configured; you can close this window and fire up VoiceAttack.";
                selectButton.Hide();
            }
        }
    }
}
