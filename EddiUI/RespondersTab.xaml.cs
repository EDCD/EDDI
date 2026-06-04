using System;
using System.Linq;
using System.Windows.Controls;
using EddiConfigService;
using EddiCore;

namespace EddiUI
{
    /// <summary>
    /// Interaction logic for RespondersTab.xaml
    /// </summary>
    public partial class RespondersTab : UserControl
    {
        public RespondersTab()
        {
            InitializeComponent();
            LoadResponders();
        }

        private void LoadResponders()
        {
            var eddiConfiguration = ConfigService.Instance.eddiConfiguration;
            // The three responders we want to load:
            var targetResponderNames = new[] { "EDDN Responder", "EDSM Responder", "Inara Responder" };

            foreach (var responderName in targetResponderNames)
            {
                var responder = EDDI.Instance.responders.FirstOrDefault(r => string.Equals(r.ResponderName(), responderName, StringComparison.InvariantCultureIgnoreCase));
                if (responder == null) continue;

                var skeleton = new PluginSkeleton(responder.ResponderName());
                skeleton.plugindescription.Text = responder.ResponderDescription();

                if (eddiConfiguration.Plugins.TryGetValue(responder.ResponderName(), out var enabled))
                {
                    skeleton.pluginenabled.IsChecked = enabled;
                }
                else
                {
                    // Default to enabled
                    skeleton.pluginenabled.IsChecked = true;
                    ConfigService.Instance.eddiConfiguration = eddiConfiguration;
                }

                // Add responder-specific configuration items
                var monitorConfiguration = responder.ConfigurationTabItem();
                if (monitorConfiguration != null)
                {
                    monitorConfiguration.Margin = new System.Windows.Thickness(10);
                    skeleton.panel.Children.Add(monitorConfiguration);
                }

                var item = new TabItem { Header = responder.LocalizedResponderName(), Content = skeleton };
                tabControl.Items.Add(item);
            }
        }
    }
}
