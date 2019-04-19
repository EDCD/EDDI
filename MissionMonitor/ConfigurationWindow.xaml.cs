using Eddi;
using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace EddiMissionMonitor
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class ConfigurationWindow : UserControl
    {
        private MissionMonitor missionMonitor()
        {
            return (MissionMonitor)EDDI.Instance.ObtainMonitor("Mission monitor");
        }

        public ConfigurationWindow()
        {
            InitializeComponent();

            missionsData.ItemsSource = missionMonitor()?.missions;

            MissionMonitorConfiguration configuration = MissionMonitorConfiguration.FromFile();
            missionWarningInt.Text = configuration.missionWarning?.ToString(CultureInfo.InvariantCulture);
        }

        private void missionsUpdated(object sender, DataTransferEventArgs e)
        {
            // Update the mission monitor's information
            missionMonitor()?.writeMissions();
        }

        private void warningChanged(object sender, TextChangedEventArgs e)
        {
            MissionMonitorConfiguration configuration = MissionMonitorConfiguration.FromFile();
            try
            {
                int? warning = string.IsNullOrWhiteSpace(missionWarningInt.Text) ? 60 : Convert.ToInt32(missionWarningInt.Text, CultureInfo.InvariantCulture);
                missionMonitor().missionWarning = warning;
                configuration.missionWarning = warning;
                configuration.ToFile();
            }
            catch
            {
                // Bad user input; ignore it
            }
        }

        private void getRoute(object sender, RoutedEventArgs e)
        {
            Button updateButton = (Button)sender;
            updateButton.Foreground = Brushes.Red;
            updateButton.FontWeight = FontWeights.Bold;

            Thread getRouteThread = new Thread(() =>
            {
                string nextSystem = missionMonitor().GetMissionsRoute();
                Dispatcher?.Invoke(() =>
                {
                    updateButton.Foreground = Brushes.Black;
                    updateButton.FontWeight = FontWeights.Regular;

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
            getRouteThread.Start();
        }

        private void nextInRoute(object sender, RoutedEventArgs e)
        {
            string nextSystem = missionMonitor().SetNextRoute();

            // If 'next system' found, send to clipboard
            if (nextSystem != null)
            {
                Clipboard.SetData(DataFormats.Text, nextSystem);
            }

        }

        private void updateRoute(object sender, RoutedEventArgs e)
        {
            Button updateButton = (Button)sender;
            updateButton.Foreground = Brushes.Red;
            updateButton.FontWeight = FontWeights.Bold;

            Thread updateRouteThread = new Thread(() =>
            {
                string nextSystem = missionMonitor().UpdateRoute();
                Dispatcher?.Invoke(() =>
                {
                    updateButton.Foreground = Brushes.Black;
                    updateButton.FontWeight = FontWeights.Regular;

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
            updateRouteThread.Start();
        }

        private void clearRoute(object sender, RoutedEventArgs e)
        {
            missionMonitor().CancelRoute();
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
