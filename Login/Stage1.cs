using EliteDangerousCompanionAppService;
using System;
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
            try
            {
                Credentials credentials = CompanionAppService.Login(username, password);
                Stage2 stage2 = new Stage2(credentials);
                stage2.Show();
                this.Hide();
            }
            catch (EliteDangerousCompanionAppAuthenticationException ex)
            {
                errorLabel.Text = ex.Message;
            }
            catch (EliteDangerousCompanionAppErrorException ex)
            {
                errorLabel.Text = ex.Message;
            }
            catch (Exception ex)
            {
                errorLabel.Text = "Unexpected problem\r\nPlease report this at http://github.com/CmdrMcDonald/EliteDangerousDataProvider/issues\r\n" + ex;
            }
        }
    }
}
