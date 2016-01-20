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
    public partial class Stage2 : Form
    {
        Credentials credentials;
        public Stage2(Credentials credentials)
        {
            InitializeComponent();
            this.credentials = credentials;
        }

        private void confirmButton_Click(object sender, EventArgs e)
        {
            string code = codeText.Text;
            credentials = CompanionApp.Confirm(credentials, code);
            if (credentials != null && credentials.appId != null && credentials.machineId != null && credentials.machineToken != null)
            {
                credentials.ToFile();

                Stage3 stage3 = new Stage3();
                stage3.Show();
                this.Hide();
            }
        }
    }
}
