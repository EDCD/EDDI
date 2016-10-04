using Newtonsoft.Json;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Utilities;

namespace EddiNetLogMonitor
{
    /// <summary>
    /// Interaction logic for ConfigurationWindow.xaml
    /// </summary>
    public partial class ConfigurationWindow : UserControl
    {
        NetLogConfiguration configuration;

        public ConfigurationWindow()
        {
            InitializeComponent();

            // Configure the NetLog tab
            configuration = NetLogConfiguration.FromFile();
            netLogPathTextBox.Text = configuration.path;

            Logging.Debug("Configuration is " + JsonConvert.SerializeObject(configuration));
        }

        // Handle changes to NetLog tab
        private void netLogPathChanged(object sender, TextChangedEventArgs e)
        {
            updateNetLogConfiguration();
        }

        private void updateNetLogConfiguration()
        {
            NetLogConfiguration netLogConfiguration = new NetLogConfiguration();
            if (!string.IsNullOrWhiteSpace(netLogPathTextBox.Text))
            {
                netLogConfiguration.path = netLogPathTextBox.Text.Trim();
            }
            netLogConfiguration.ToFile();
        }

        private void netLogObtainClicked(object sender, RoutedEventArgs e)
        {
            List<string> processPaths = new Finder().GetPathFromProcess();
            if (processPaths.Count != 0)
            {
                netLogPathTextBox.Text = processPaths[0] + @"\Logs";
                updateNetLogConfiguration();
            }
            else
            {
                netLogText.Text = @"Unfortuantely we were unable to locate your product directory.  Please type in the location of the 'Logs' directory in your 'elite-dangerous-64' directory.  Possible locations include:";
                List<string> paths = new Finder().FindInstallationPaths();
                if (paths.Count == 0)
                {
                    paths.Add(Finder.DefProductsPath + @"\elite-dangerous-64");
                    paths.Add(Finder.DefLauncherPath + @"\elite-dangerous-64");
                    paths.Add(@"C:\Program Files (x86)\Steam\\SteamApps\common\Elite Dangerous\Products\elite-dangerous-64");
                }
                foreach (string path in paths)
                {
                    netLogText.Text += "\r\n\r\n" + path + @"\Logs";
                }
                netLogText.Text += "\r\n\r\nWhichever directory you select should contain a number of 'debugLog' files.";
            }
        }

    }
}
