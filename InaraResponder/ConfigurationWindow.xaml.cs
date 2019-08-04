using Eddi;
using EddiInaraService;
using System.Windows;
using System.Windows.Controls;

namespace EddiInaraResponder
{
    /// <summary>
    /// Interaction logic for ConfigurationWindow.xaml
    /// </summary>
    public partial class ConfigurationWindow : UserControl
    {
        public ConfigurationWindow()
        {
            InitializeComponent();

            InaraConfiguration inaraConfiguration = InaraConfiguration.FromFile();
            inaraApiKeyTextBox.Text = inaraConfiguration.apiKey;
        }

        private void inaraApiKeyChanged(object sender, TextChangedEventArgs e)
        {
            updateInaraConfiguration();
        }

        private void updateInaraConfiguration()
        {
            InaraConfiguration inaraConfiguration = InaraConfiguration.FromFile();
            if (!string.IsNullOrWhiteSpace(inaraApiKeyTextBox.Text))
            {
                inaraConfiguration.apiKey = inaraApiKeyTextBox.Text.Trim();
            }
            inaraConfiguration.ToFile();
            EDDI.Instance.Reload("Inara responder");
        }
    }
}
