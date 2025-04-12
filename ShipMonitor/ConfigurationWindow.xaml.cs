using EddiConfigService;
using EddiCore;
using EddiDataDefinitions;
using EddiSpeechService;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Utilities;

namespace EddiShipMonitor
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class ConfigurationWindow : UserControl
    {
        private ShipMonitor shipMonitor()
        {
            return (ShipMonitor)EDDI.Instance.ObtainMonitor("Ship monitor");
        }

        public ConfigurationWindow()
        {
            InitializeComponent();
            shipData.ItemsSource = shipMonitor().shipyard;
            var config = ConfigService.Instance.shipMonitorConfiguration;
            var exportTarget = config.exporttarget;

            // handle migration
            if (exportTarget == "EDShipyard")
            {
                exportTarget = "EDSY";
                config.exporttarget = exportTarget;
                ConfigService.Instance.shipMonitorConfiguration = config;
            }

            Logging.Debug("Export target from configuration: " + exportTarget);
            exportComboBox.Text = exportTarget ?? "Coriolis";
        }

        private void onExportTargetChanged(object sender, SelectionChangedEventArgs e)
        {
            var exportTarget = (string)((ComboBox)e.Source).SelectedValue;
            Logging.Debug("Export target: " + exportTarget);

            var config = ConfigService.Instance.shipMonitorConfiguration;
            config.exporttarget = string.IsNullOrWhiteSpace(exportTarget) ? null : exportTarget.Trim();
            ConfigService.Instance.shipMonitorConfiguration = config;
        }

        private void ipaClicked(object sender, RoutedEventArgs e)
        {
            IpaResourcesWindow IpaResources = new IpaResourcesWindow();
            IpaResources.Show();
        }

        private void testShipName(object sender, RoutedEventArgs e)
        {
            Ship ship = (Ship)((Button)e.Source).DataContext;
            ship.health = 100;
            string nameToSpeak = ship.phoneticname;
            string message = string.Format(Properties.ShipMonitor.ship_ready, nameToSpeak);
            SpeechService.Instance.Say(ship, message, 0);
        }

        private void exportShip(object sender, RoutedEventArgs e)
        {
            var ship = (Ship)((Button)e.Source).DataContext;
            var config = ConfigService.Instance.shipMonitorConfiguration;

            string uri;
            switch (config.exporttarget)
            {
                case "EDShipyard":
                case "EDSY":
                    uri = ship.EDShipyardUri();
                    break;

                case "Coriolis":
                    uri = ship.CoriolisUri();
                    break;

                case "Coriolis (Beta)":
                    uri = ship.CoriolisUri(true);
                    break;

                default:
                    throw new NotImplementedException($"Export target {config.exporttarget} not recognized.");
            }

            Logging.Debug("Export target is " + config.exporttarget + ", URI is " + uri);

            // URI can be very long so we can't use a simple Process.Start(), as that fails
            try
            {
                ProcessStartInfo proc = new ProcessStartInfo(Net.GetDefaultBrowserPath(), uri)
                {
                    UseShellExecute = false
                };
                Process.Start(proc);
            }
            catch (Exception ex)
            {
                Logging.Error("Failed: ", ex);
                try
                {
                    // Last-gasp attempt if we have a shorter URL
                    if (uri.Length < 2048)
                    {
                        Process.Start(uri);
                    }
                    else
                    {
                        Logging.Info("Export failed: Target URL is too long.");
                    }
                }
                catch (Exception)
                {
                    Logging.Error("Failed to find a way of opening URL \"" + uri + "\"");
                }
            }
        }

        private void shipsUpdated(object sender, DataTransferEventArgs e)
        {
            // Update the ship monitor's 'PhoneticName' information
            if (e.OriginalSource is TextBlock)
            {
                var dg = (DataGrid)e.Source;
                if (dg.SelectedItem is Ship)
                {
                    shipMonitor()?.Save();
                }
            }
        }

        private void shipsUpdated(object sender, SelectionChangedEventArgs e)
        {
            // Update the ship monitor's 'Role' information
            if (e.RemovedItems.Count > 0)
            {
                shipMonitor()?.Save();
            }
        }

        // Fixup IPA text by replacing spaces
        private void PhoneticName_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                if (textBox.IsLoaded)
                {
                    // Replace any spaces, maintaining the original caret position
                    int caretIndex = textBox.CaretIndex;
                    textBox.Text = textBox.Text.Replace(" ", "ˈ");
                    textBox.CaretIndex = Math.Max(caretIndex, textBox.Text.Length);
                }
            }
        }
    }
}
