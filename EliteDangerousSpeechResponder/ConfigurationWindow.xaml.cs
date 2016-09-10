using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Utilities;

namespace EliteDangerousSpeechResponder
{
    /// <summary>
    /// Interaction logic for ConfigurationWindow.xaml
    /// </summary>
    public partial class ConfigurationWindow : UserControl
    {
        ScriptsConfiguration configuration;

        public ConfigurationWindow()
        {
            InitializeComponent();
            configuration = ScriptsConfiguration.FromFile();

            scriptsData.ItemsSource = configuration.Scripts;

            Logging.Info("Configuration is " + JsonConvert.SerializeObject(configuration));
        }

        private void eddiScriptsUpdated(object sender, RoutedEventArgs e)
        {
            updateScriptsConfiguration();
        }

        private void eddiScriptsUpdated(object sender, DataTransferEventArgs e)
        {
            updateScriptsConfiguration();
        }

        private void editScript(object sender, RoutedEventArgs e)
        {
            Script script = ((KeyValuePair<string, Script>)((Button)e.Source).DataContext).Value;
            EditScriptWindow editScriptWindow = new EditScriptWindow(configuration.Scripts, script.Name);
            editScriptWindow.ShowDialog();
            scriptsData.Items.Refresh();
        }

        private void resetScript(object sender, RoutedEventArgs e)
        {
            Script script = ((KeyValuePair<string, Script>)((Button)e.Source).DataContext).Value;
            script.Value = null;
            eddiScriptsUpdated(sender, e);
            scriptsData.Items.Refresh();
        }

        private void updateScriptsConfiguration()
        {
            configuration.ToFile();
        }
    }
}
