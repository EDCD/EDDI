﻿using System.Windows;
using System.Windows.Controls;
using EddiCore;

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
            EDDI.Instance.EnableResponder(pluginName);
            EDDI.Instance.EnableMonitor(pluginName);
        }

        private void pluginenabled_Unchecked(object sender, RoutedEventArgs e)
        {
            EDDIConfiguration configuration = EDDIConfiguration.FromFile();
            configuration.Plugins[pluginName] = false;
            configuration.ToFile();
            EDDI.Instance.DisableResponder(pluginName);
            EDDI.Instance.DisableMonitor(pluginName);
        }
    }
}
