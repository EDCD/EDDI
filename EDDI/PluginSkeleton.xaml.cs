using System.Windows;
using System.Windows.Controls;
using Utilities;

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
            pluginenabledText.Text = I18N.GetString("plugin_enabled");
        }

        private void pluginenabled_Checked(object sender, RoutedEventArgs e)
        {
            EDDIConfiguration configuration = EDDIConfiguration.FromFile();
            configuration.Plugins[pluginName] = true;
            configuration.ToFile();
        }

        private void pluginenabled_Unchecked(object sender, RoutedEventArgs e)
        {
            EDDIConfiguration configuration = EDDIConfiguration.FromFile();
            configuration.Plugins[pluginName] = false;
            configuration.ToFile();
        }
    }
}
