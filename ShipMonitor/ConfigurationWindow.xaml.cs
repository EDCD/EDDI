using Eddi;
using EddiDataDefinitions;
using EddiSpeechService;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
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
            EDDIConfiguration eddiConfiguration = EDDIConfiguration.FromFile();
            string exportTarget = eddiConfiguration.exporttarget;

            // handle migration
            if (exportTarget == "EDShipyard")
            {
                exportTarget = "EDSY";
                eddiConfiguration.exporttarget = exportTarget;
                eddiConfiguration.ToFile();
            }

            Logging.Debug("Export target from configuration: " + exportTarget);
            exportComboBox.Text = exportTarget ?? "Coriolis";
            DataObject.AddPastingHandler(shipData, pastingDataEventHandler);
        }

        private void onExportTargetChanged(object sender, SelectionChangedEventArgs e)
        {
            string exportTarget = (string)((ComboBox)e.Source).SelectedValue;
            Logging.Debug("Export target: " + exportTarget);

            EDDIConfiguration eddiConfiguration = EDDIConfiguration.FromFile();
            eddiConfiguration.exporttarget = string.IsNullOrWhiteSpace(exportTarget) ? null : exportTarget.Trim();
            eddiConfiguration.ToFile();
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
            SpeechServiceConfiguration speechConfiguration = SpeechServiceConfiguration.FromFile();
            string nameToSpeak = String.IsNullOrEmpty(ship.phoneticname)
                ? ship.name
                : $@"<phoneme alphabet=""ipa"" ph=""{ship.phoneticname}"">{ship.name}</phoneme>";
            string message = String.Format(Properties.ShipMonitor.ship_ready, nameToSpeak);
            SpeechService.Instance.Say(ship, message, 0);
        }

        private void exportShip(object sender, RoutedEventArgs e)
        {
            Ship ship = (Ship)((Button)e.Source).DataContext;
            EDDIConfiguration eddiConfiguration = EDDIConfiguration.FromFile();

            string uri;
            switch (eddiConfiguration.exporttarget)
            {
                case "EDShipyard":
                case "EDSY":
                    uri = ship.EDShipyardUri();
                    break;

                case "Coriolis":
                    uri = ship.CoriolisUri();
                    break;

                default:
                    throw new NotImplementedException($"Export target {eddiConfiguration.exporttarget} not recognized.");
            }

            Logging.Debug("Export target is " + eddiConfiguration.exporttarget + ", URI is " + uri);

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

        private void previewKeyDownHandler(object sender, KeyEventArgs e)
        {
            if (sender is DataGridCell dataGridCell)
            {
                if (dataGridCell.Column is DataGridTextColumn textColumn)
                {
                    if (textColumn.Header.ToString() == Properties.ShipMonitor.header_spoken_name)
                    {
                        if (e.Key == Key.Space)
                        {
                            if (e.Source is TextBox textBox)
                            {
                                int selectionPosition = textBox.SelectionStart;
                                textBox.Text += "ˈ";
                                textBox.SelectionStart = selectionPosition + 1;
                                e.Handled = true;
                            }
                        }
                    }
                }
            }
        }

        private void pastingDataEventHandler(object sender, DataObjectPastingEventArgs e)
        {
            if (sender is DataGrid dataGrid)
            {
                if (dataGrid.CurrentCell is DataGridCellInfo dataGridCell)
                {
                    if (dataGridCell.Column is DataGridTextColumn textColumn)
                    {
                        if (textColumn.Header.ToString() == Properties.ShipMonitor.header_spoken_name)
                        {
                            // Retrieve the paste text
                            var isText = e.SourceDataObject.GetDataPresent(DataFormats.UnicodeText, true);
                            if (!isText) return;
                            var text = e.SourceDataObject.GetData(DataFormats.UnicodeText) as string;

                            // Replace the data object
                            DataObject d = new DataObject();
                            d.SetData(DataFormats.Text, text.Replace(" ", "ˈ"));
                            e.DataObject = d;
                        }
                    }
                }
            }
        }
    }
}
