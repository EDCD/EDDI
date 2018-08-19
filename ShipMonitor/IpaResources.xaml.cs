using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
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

namespace EddiShipMonitor
{
    /// <summary>
    /// Interaction logic for IpaResourcesWindow.xaml
    /// </summary>
    public partial class IpaResourcesWindow : Window
    {
        public IpaResourcesWindow()
        {
            InitializeComponent();
        }

        private void pageClicked(object sender, RoutedEventArgs e)
        {
            string url = EddiShipMonitor.Properties.ShipMonitor.ipa_page;
            Process.Start(url);
        }

        private void resource1Clicked(object sender, RoutedEventArgs e)
        {
            string url = EddiShipMonitor.Properties.ShipMonitor.ipa_resource1;
            Process.Start(url);
        }

        private void resource2Clicked(object sender, RoutedEventArgs e)
        {
            string url = EddiShipMonitor.Properties.ShipMonitor.ipa_resource2;
            Process.Start(url);
        }

        private void resource3Clicked(object sender, RoutedEventArgs e)
        {
            string url = EddiShipMonitor.Properties.ShipMonitor.ipa_resource3;
            Process.Start(url);
        }
    }
}
