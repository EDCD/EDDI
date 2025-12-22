using EddiConfigService;
using EddiCore;
using EddiStarMapService;
using System;
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
    public partial class ConfigurationWindow
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
            try
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
                var edsmService = new StarMapService(null, true);

                // Run the async worker on a background thread and await it so exceptions propagate to this handler.
                var worker = Task.Factory
                    .StartNew(async () => await obtainEdsmLogsAsync(edsmService, progress).ConfigureAwait(false), TaskCreationOptions.LongRunning)
                    .Unwrap();

                await worker.ConfigureAwait(true);

                // Only update last sync time if the background work completed successfully.
                starMapConfiguration.lastFlightLogSync = DateTime.UtcNow;
                ConfigService.Instance.edsmConfiguration = starMapConfiguration;

                // Show success state (Progress may have already set this)
                edsmFetchLogsButton.Content = Properties.EDSMResources.log_button_fetched;
            }
            catch (OperationCanceledException)
            {
                // Operation was cancelled, nothing to do here
                edsmFetchLogsButton.Content = Properties.EDSMResources.log_button;
                Logging.Info("EDSM log fetch cancelled by user.");
            }
            catch (Exception ex)
            {
                // Ensure unexpected exceptions are surfaced to the user
                var message = Properties.EDSMResources.log_button_error_received + ex.Message;
                edsmFetchLogsButton.Content = message;
            }
            finally
            {
                // Restore button state on the UI thread after a short delay
                await Task.Delay( TimeSpan.FromSeconds( 15 ) ).ConfigureAwait( true );
                edsmFetchLogsButton.IsEnabled = true;
                edsmFetchLogsButton.Content = string.IsNullOrEmpty( edsmApiKeyTextBox.Text )
                    ? Properties.EDSMResources.log_button_empty_api_key
                    : Properties.EDSMResources.log_button;
                Logging.Info( "EDSM log fetch completed." );
            }
        }

        private static async Task obtainEdsmLogsAsync(StarMapService edsmService, IProgress<string> progress)
        {
            if (edsmService == null)
            {
                return;
            }

            try
            {
                var flightLogs = await edsmService.getStarMapLogAsync().ConfigureAwait(false);
                var comments = await edsmService.getStarMapCommentsAsync().ConfigureAwait(false);
                var total = flightLogs.Count;
                var i = 0;

                while (i < total)
                {
                    var batchSize = Math.Min(total, StarMapService.syncBatchSize);
                    await EDDI.Instance.DataProvider.SyncEdsmLogBatchAsync(flightLogs.Skip(i).Take(batchSize).ToList(), comments).ConfigureAwait(false);
                    i += batchSize;
                    progress.Report($"{Properties.EDSMResources.log_button_fetching_progress} {i}/{total}");
                }

                progress.Report(Properties.EDSMResources.log_button_fetched);
            }
            catch (EDSMException edsme)
            {
                // Expected EDSM-specific errors are reported to the UI and logged.
                progress.Report(Properties.EDSMResources.log_button_error_received + edsme.Message);
                Logging.Warn(Properties.EDSMResources.log_button_error_received + edsme.Message, edsme);
                // Re-throw so caller can observe and handle as well if desired.
                throw;
            }
            catch (OperationCanceledException)
            {
                progress.Report(Properties.EDSMResources.log_button_error_received + " " + "Operation cancelled.");
                Logging.Info("EDSM log fetch cancelled.");
                throw;
            }
            catch (Exception ex)
            {
                // Unexpected exceptions: report to UI, log and rethrow so caller can react.
                progress.Report(Properties.EDSMResources.log_button_error_received + ex.Message);
                Logging.Warn(Properties.EDSMResources.log_button_error_received + ex.Message, ex);
                throw;
            }
        }
    }
}
