using Eddi;
using EddiNavigationService;
using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace EddiMaterialMonitor
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class ConfigurationWindow : UserControl
    {
        private MaterialMonitor materialMonitor()
        {
            return (MaterialMonitor)EDDI.Instance.ObtainMonitor("Material monitor");
        }

        public ConfigurationWindow()
        {
            InitializeComponent();
            materialsData.ItemsSource = materialMonitor()?.inventory;
        }

        private void findEncoded(object sender, RoutedEventArgs e)
        {
            Button encodedButton = (Button)sender;
            encodedButton.Foreground = Brushes.Red;
            encodedButton.FontWeight = FontWeights.Bold;

            findService(encodedButton, "encoded");
        }

        private void findGuardian(object sender, RoutedEventArgs e)
        {
            Button guardianButton = (Button)sender;
            guardianButton.Foreground = Brushes.Red;
            guardianButton.FontWeight = FontWeights.Bold;

            findService(guardianButton, "guardian");
        }

        private void findHuman(object sender, RoutedEventArgs e)
        {
            Button humanButton = (Button)sender;
            humanButton.Foreground = Brushes.Red;
            humanButton.FontWeight = FontWeights.Bold;

            findService(humanButton, "human");
        }

        private void findManufactured(object sender, RoutedEventArgs e)
        {
            Button manufacturedButton = (Button)sender;
            manufacturedButton.Foreground = Brushes.Red;
            manufacturedButton.FontWeight = FontWeights.Bold;

            findService(manufacturedButton, "manufactured");
        }

        private void findRaw(object sender, RoutedEventArgs e)
        {
            Button rawButton = (Button)sender;
            rawButton.Foreground = Brushes.Red;
            rawButton.FontWeight = FontWeights.Bold;

            findService(rawButton, "raw");
        }

        private void findService(Button button, string service)
        {
            Thread findServiceThread = new Thread(() =>
            {
                string nextSystem = Navigation.Instance.GetServiceRoute(service);
                Dispatcher?.Invoke(() =>
                {
                    button.Foreground = Brushes.Black;
                    button.FontWeight = FontWeights.Regular;

                    // If 'next system' found, send to clipboard
                    if (nextSystem != null)
                    {
                        Clipboard.SetData(DataFormats.Text, nextSystem);
                    }
                });
            })
            {
                IsBackground = true
            };
            findServiceThread.Start();
        }

        private void materialsUpdated(object sender, DataTransferEventArgs e)
        {
            // Update the material monitor's information
            materialMonitor()?.writeMaterials();
        }
    }
}
