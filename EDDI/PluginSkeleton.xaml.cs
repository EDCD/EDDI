using EddiConfigService;
using EddiConfigService.Configurations;
using EddiCore;
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

        public PluginSkeleton(string pluginName, bool isDisableable = true)
        {
            InitializeComponent();
            this.pluginName = pluginName;
            PluginSkeletonCheckbox.Visibility = isDisableable ? Visibility.Visible : Visibility.Collapsed;
        }

        private void pluginenabled_Checked(object sender, RoutedEventArgs e)
        {
            EDDIConfiguration configuration = ConfigService.Instance.eddiConfiguration;
            configuration.Plugins[pluginName] = true;
            ConfigService.Instance.eddiConfiguration = configuration;
            EDDI.Instance.EnableResponder(pluginName);
            EDDI.Instance.EnableMonitor(pluginName);
        }

        private void pluginenabled_Unchecked(object sender, RoutedEventArgs e)
        {
            EDDIConfiguration configuration = ConfigService.Instance.eddiConfiguration;
            configuration.Plugins[pluginName] = false;
            ConfigService.Instance.eddiConfiguration = configuration;
            EDDI.Instance.DisableResponder(pluginName);
            EDDI.Instance.DisableMonitor(pluginName);
        }
    }
}
