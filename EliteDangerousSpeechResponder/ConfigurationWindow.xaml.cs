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
        List<Personality> personalities;
        Personality personality;

        public ConfigurationWindow()
        {
            InitializeComponent();
            personalities = Personality.AllFromDirectory();
            foreach (Personality personality in personalities)
            {
                Logging.Info("Found personality " + personality.Name);
            }

            SpeechResponderConfiguration configuration = SpeechResponderConfiguration.FromFile();

            foreach (Personality personality in personalities)
            {
                if (personality.Name == configuration.Personality)
                {
                    this.personality = personality;
                    scriptsData.ItemsSource = personality.Scripts;
                }
            }

            personalityComboBox.ItemsSource = personalities;

            Logging.Debug("Configuration is " + JsonConvert.SerializeObject(configuration));
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
            EditScriptWindow editScriptWindow = new EditScriptWindow(personality.Scripts, script.Name);
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
            personality.ToFile();
        }
    }
}
