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

            setTexts();
        }

        private void obtainButton_Click(object sender, EventArgs e)
        {
            setTexts();
        }

        private void setTexts()
        {
            List<string> paths = new Finder().GetPathFromProcess();
            if (paths.Count == 0)
            {
                label1.Text = "Thank you Commander " + name + ".  Please start the Elite: Dangerous game then come back to this screen and hit the \"Recheck\" button";
            }
            else
            {
                NetLogMonitor.WritePath(paths[0] + "\\Logs");
                label1.Text = "Thank you Commander " + name + ".  The required details for your game have been stored.  This completes the setup; you can now close this window.";
                obtainButton.Hide();
            }
        }
    }
}
