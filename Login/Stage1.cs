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
            Credentials credentials = CompanionApp.Login(username, password);
            System.Console.WriteLine("Credentials are " + credentials);
        }
    }
}
