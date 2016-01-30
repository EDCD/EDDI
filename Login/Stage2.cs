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
            try
            {
                Credentials = CompanionAppService.Confirm(Credentials, code);
                Credentials.ToFile();
                CompanionAppService app = new CompanionAppService(Credentials);
                Commander Cmdr = app.Profile();
                Stage3 stage3 = new Stage3(Cmdr.Name);
                stage3.Show();
                this.Hide();

                }
                catch (EliteDangerousCompanionAppAuthenticationException ex)
            {
                Credentials.Clear();
                Credentials.ToFile();
                errorLabel.Text = ex.Message + "\r\nPlease restart this application to re-authenticate";
            }
            catch (EliteDangerousCompanionAppErrorException ex)
            {
                Credentials.Clear();
                Credentials.ToFile();
                errorLabel.Text = ex.Message + "\r\nPlease restart this application to re-authenticate";
            }
            catch (Exception ex)
            {
                Credentials.Clear();
                Credentials.ToFile();
                errorLabel.Text = "Unexpected problem\r\nPlease report this at http://github.com/CmdrMcDonald/EliteDangerousDataProvider/issues\r\n" + ex.Message + "\r\nPlease restart this application to re-authenticate";
            }
        }
    }
}
