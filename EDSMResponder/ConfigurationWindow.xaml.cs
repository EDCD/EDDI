using EddiConfigService;
using EddiDataProviderService;
using EddiStarMapService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Utilities;

namespace EddiEdsmResponder
{
    /// <summary>
    /// Interaction logic for ConfigurationWindow.xaml
    /// </summary>
    public partial class ConfigurationWindow : UserControl
    {
        private readonly EDSMResponder edsmResponder;

        public ConfigurationWindow(EDSMResponder edsmResponder)
        {
            this.edsmResponder = edsmResponder;
            InitializeComponent();

            var starMapConfiguration = ConfigService.Instance.edsmConfiguration;
            edsmApiKeyTextBox.Text = starMapConfiguration.apiKey;
            edsmCommanderNameTextBox.Text = starMapConfiguration.commanderName;
            edsmFetchLogsButton.Content = String.IsNullOrEmpty(edsmApiKeyTextBox.Text) ? Properties.EDSMResources.log_button_empty_api_key : Properties.EDSMResources.log_button;
        }

        private void edsmCommanderNameChanged(object sender, TextChangedEventArgs e)
        {
            edsmFetchLogsButton.IsEnabled = true;
            edsmFetchLogsButton.Content = Properties.EDSMResources.log_button;
            updateEdsmConfiguration();
        }

        private void edsmApiKeyChanged(object sender, TextChangedEventArgs e)
        {
            edsmFetchLogsButton.IsEnabled = true;
            edsmFetchLogsButton.Content = String.IsNullOrEmpty(edsmApiKeyTextBox.Text) ? Properties.EDSMResources.log_button_empty_api_key : Properties.EDSMResources.log_button;
            updateEdsmConfiguration();
        }

        private void updateEdsmConfiguration()
        {
            var edsmConfiguration = ConfigService.Instance.edsmConfiguration;
            if (!string.IsNullOrWhiteSpace(edsmApiKeyTextBox.Text))
            {
                edsmConfiguration.apiKey = edsmApiKeyTextBox.Text.Trim();
            }
            if (!string.IsNullOrWhiteSpace(edsmCommanderNameTextBox.Text))
            {
                edsmConfiguration.commanderName = edsmCommanderNameTextBox.Text.Trim();
            }
            ConfigService.Instance.edsmConfiguration = edsmConfiguration;
            edsmResponder.Reload();
        }

        /// <summary>
        /// Obtain the EDSM log and sync it with the local datastore
        /// </summary>
        private async void edsmObtainLogClicked(object sender, RoutedEventArgs e)
        {
            var starMapConfiguration = ConfigService.Instance.edsmConfiguration;

            if (string.IsNullOrEmpty(starMapConfiguration.apiKey))
            {
                edsmFetchLogsButton.IsEnabled = false;
                edsmFetchLogsButton.Content = Properties.EDSMResources.log_button_empty_api_key;
                return;
            }
            
            edsmFetchLogsButton.IsEnabled = false;
            edsmFetchLogsButton.Content = Properties.EDSMResources.log_button_fetching;

            var progress = new Progress<string>(s => edsmFetchLogsButton.Content = s);
            IEdsmService edsmService = new StarMapService(null, true);
            await Task.Factory.StartNew(() => obtainEdsmLogs(edsmService, progress), TaskCreationOptions.LongRunning);

            starMapConfiguration.lastFlightLogSync = DateTime.UtcNow;
            ConfigService.Instance.edsmConfiguration = starMapConfiguration;
        }

        public static void obtainEdsmLogs(IEdsmService edsmService, IProgress<string> progress)
        {
            if (edsmService != null)
            {
                try
                {
                    DataProviderService dataProviderService = new DataProviderService(edsmService);
                    List<StarMapResponseLogEntry> flightLogs = edsmService.getStarMapLog();
                    Dictionary<string, string> comments = edsmService.getStarMapComments();
                    int total = flightLogs.Count;
                    int i = 0;

                    while (i < total)
                    {
                        int batchSize = Math.Min(total, StarMapService.syncBatchSize);
                        List<StarMapResponseLogEntry> flightLogBatch = flightLogs.Skip(i).Take(batchSize).ToList();
                        dataProviderService.syncEdsmLogBatch(flightLogs.Skip(i).Take(batchSize).ToList(), comments);
                        i += batchSize;
                        progress.Report($"{Properties.EDSMResources.log_button_fetching_progress} {i}/{total}");
                    }
                    progress.Report(Properties.EDSMResources.log_button_fetched);
                }
                catch (EDSMException edsme)
                {
                    progress.Report(Properties.EDSMResources.log_button_error_received + edsme.Message);
                    Logging.Warn(Properties.EDSMResources.log_button_error_received + edsme.Message, edsme);
                }
            }
        }
    }
}
