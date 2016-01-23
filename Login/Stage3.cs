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

namespace EliteDangerousDataProvider
{
    public partial class Stage3 : Form
    {
        public Stage3()
        {
            InitializeComponent();
        }

        private void profileButton_Click(object sender, EventArgs e)
        {
            Credentials Credentials = Credentials.FromFile();
            CompanionApp app = new CompanionApp(Credentials);
            Commander Cmdr = app.Profile();

            if (Cmdr == null)
            {
                MessageBox.Show("There was a problem.  You will need to close and restart this app and attempt to log in again", "Problem detected", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Credentials.Clear();
                Credentials.ToFile();
            }
            else
            {
                MessageBox.Show("You have successfully logged in Commander " + Cmdr.Name + ", you can now close this app and restart Voice Attack", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}
