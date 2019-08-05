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
using Utilities;

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

            MaterialMonitorConfiguration configuration = MaterialMonitorConfiguration.FromFile();
            maxStationDistanceInt.Text = configuration.maxStationDistanceFromStarLs?.ToString(CultureInfo.InvariantCulture);
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
            int distance = materialMonitor().maxStationDistanceFromStarLs ?? Constants.maxStationDistanceDefault;
            Thread findServiceThread = new Thread(() =>
            {
                string nextSystem = Navigation.Instance.GetServiceRoute(service, distance);
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

        private void maxStationDistance_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                maxStationDistance_Changed();
            }
        }

        private void maxStationDistance_LostFocus(object sender, RoutedEventArgs e)
        {
            maxStationDistance_Changed();
        }

        private void maxStationDistance_Changed()
        {
            MaterialMonitorConfiguration configuration = MaterialMonitorConfiguration.FromFile();
            try
            {
                int? distance = string.IsNullOrWhiteSpace(maxStationDistanceInt.Text)
                    ? 10000 : Convert.ToInt32(maxStationDistanceInt.Text, CultureInfo.InvariantCulture);
                if (distance != configuration.maxStationDistanceFromStarLs)
                {
                    materialMonitor().maxStationDistanceFromStarLs = distance;
                    configuration.maxStationDistanceFromStarLs = distance;
                    configuration.ToFile();
                }
            }
            catch
            {
                // Bad user input; ignore it
            }
        }

        private void materialsUpdated(object sender, DataTransferEventArgs e)
        {
            // Update the material monitor's information
            materialMonitor()?.writeMaterials();
        }

        private void cancelDestination(object sender, RoutedEventArgs e)
        {
            Navigation.Instance.CancelRoute();
        }

        private void setDestination(object sender, RoutedEventArgs e)
        {
            string system = Navigation.Instance.SetRoute();

            // If 'destination system' found, send to clipboard
            if (system != null)
            {
                Clipboard.SetData(DataFormats.Text, system);
            }
        }

        private void EnsureValidInteger(object sender, TextCompositionEventArgs e)
        {
            // Match valid characters
            Regex regex = new Regex(@"[0-9]");
            // Swallow the character doesn't match the regex
            e.Handled = !regex.IsMatch(e.Text);
        }
    }
}
