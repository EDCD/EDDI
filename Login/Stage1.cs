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
    public partial class Stage1 : Form
    {
        public Stage1()
        {
            InitializeComponent();
        }

        private void loginButton_Click(object sender, EventArgs e)
        {
            string username = emailText.Text;
            string password = passwordText.Text;
            Credentials credentials = EliteDangerousCompanionAppService.CompanionAppService.Login(username, password);
            if (credentials != null && credentials.appId != null && credentials.machineId != null)
            {
                Stage2 stage2 = new Stage2(credentials);
                stage2.Show();
                this.Hide();
            }
            else
            {
                MessageBox.Show("There was a problem.  Please check your email address and password and try again", "Problem detected", MessageBoxButtons.OK, MessageBoxIcon.Error);

            }
        }
    }
}
