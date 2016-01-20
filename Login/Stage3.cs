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
            CompanionApp app = new CompanionApp(Credentials.FromFile());
            dynamic json = app.Profile();
            MessageBox.Show(Convert.ToString(json), "Profile information", MessageBoxButtons.OKCancel, MessageBoxIcon.Asterisk);
        }
    }
}
