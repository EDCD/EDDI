using System.Windows;
using System.Windows.Controls;

namespace Eddi
{
    /// <summary>
    /// Interaction logic for PluginSkeleton.xaml
    /// </summary>
    public partial class PluginSkeleton : UserControl
    {
        string pluginName;

        public PluginSkeleton(string pluginName)
        {
            InitializeComponent();
            this.pluginName = pluginName;
        }

        private void pluginenabled_Checked(object sender, RoutedEventArgs e)
        {
            EDDIConfiguration configuration = EDDIConfiguration.FromFile();
            configuration.Plugins[pluginName] = true;
            configuration.ToFile();

            var responder = EDDI.Instance.ObtainResponder(pluginName);
            responder?.Start();
            responder?.Reload();

            var monitor = EDDI.Instance.ObtainMonitor(pluginName);
            monitor?.Start();
            monitor?.Reload();
        }

        private void pluginenabled_Unchecked(object sender, RoutedEventArgs e)
        {
            EDDIConfiguration configuration = EDDIConfiguration.FromFile();
            configuration.Plugins[pluginName] = false;
            configuration.ToFile();

            EDDI.Instance.ObtainResponder(pluginName)?.Stop();
            EDDI.Instance.ObtainMonitor(pluginName)?.Stop();
        }
    }
}
