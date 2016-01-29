using EliteDangerousCompanionAppService;
using EliteDangerousDataDefinitions;
using System;
using System.Windows.Forms;

namespace EliteDangerousDataProvider
{
    public partial class Stage2 : Form
    {
        Credentials Credentials;
        public Stage2(Credentials Credentials)
        {
            InitializeComponent();
            this.Credentials = Credentials;
        }

        private void confirmButton_Click(object sender, EventArgs e)
        {
            string code = codeText.Text;
            Credentials = CompanionAppService.Confirm(Credentials, code);
            if (Credentials != null && Credentials.appId != null && Credentials.machineId != null && Credentials.machineToken != null)
            {
                Credentials.ToFile();

                CompanionAppService app = new CompanionAppService(Credentials);
                try
                {
                    Commander Cmdr = app.Profile();
                    if (Cmdr == null)
                    {
                        MessageBox.Show("There was a problem.  You will need to close and restart this app and attempt to log in again", "Problem detected", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        Credentials.Clear();
                        Credentials.ToFile();
                    }
                    else
                    {
                        Stage3 stage3 = new Stage3(Cmdr.Name);
                        stage3.Show();
                        this.Hide();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("There was a problem with your profile.  Please report the below error to github.com/cmdrmcdonald/EliteDangerousDataProvider/issues\r\n" + ex.StackTrace);
                }

            }
        }
    }
}
