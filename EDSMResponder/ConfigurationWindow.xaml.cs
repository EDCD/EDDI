using Eddi;
using EddiDataDefinitions;
using EddiDataProviderService;
using EddiStarMapService;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
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

namespace EddiEdsmResponder
{
    /// <summary>
    /// Interaction logic for ConfigurationWindow.xaml
    /// </summary>
    public partial class ConfigurationWindow : UserControl
    {
        public ConfigurationWindow()
        {
            InitializeComponent();

            StarMapConfiguration starMapConfiguration = StarMapConfiguration.FromFile();
            edsmApiKeyTextBox.Text = starMapConfiguration.apiKey;
            edsmCommanderNameTextBox.Text = starMapConfiguration.commanderName;
            edsmFetchLogsButton.Content = String.IsNullOrEmpty(edsmApiKeyTextBox.Text) ? "Please enter EDSM API key  to obtain log" : "Obtain log";
        }

        private void edsmCommanderNameChanged(object sender, TextChangedEventArgs e)
        {
            edsmFetchLogsButton.IsEnabled = true;
            edsmFetchLogsButton.Content = "Obtain log";
            updateEdsmConfiguration();
        }

        private void edsmApiKeyChanged(object sender, TextChangedEventArgs e)
        {
            edsmFetchLogsButton.IsEnabled = true;
            edsmFetchLogsButton.Content = String.IsNullOrEmpty(edsmApiKeyTextBox.Text) ? "Please enter EDSM API key  to obtain log" : "Obtain log";
            updateEdsmConfiguration();
        }

        private void updateEdsmConfiguration()
        {
            StarMapConfiguration edsmConfiguration = StarMapConfiguration.FromFile();
            if (!string.IsNullOrWhiteSpace(edsmApiKeyTextBox.Text))
            {
                edsmConfiguration.apiKey = edsmApiKeyTextBox.Text.Trim();
            }
            if (!string.IsNullOrWhiteSpace(edsmCommanderNameTextBox.Text))
            {
                edsmConfiguration.commanderName = edsmCommanderNameTextBox.Text.Trim();
            }
            edsmConfiguration.ToFile();
            EDDI.Instance.Reload("EDSM responder");
        }

        /// <summary>
        /// Obtain the EDSM log and sync it with the local datastore
        /// </summary>
        private async void edsmObtainLogClicked(object sender, RoutedEventArgs e)
        {
            StarMapConfiguration starMapConfiguration = StarMapConfiguration.FromFile();

            if (string.IsNullOrEmpty(starMapConfiguration.apiKey))
            {
                edsmFetchLogsButton.IsEnabled = false;
                edsmFetchLogsButton.Content = "Please enter EDSM API key  to obtain log";
                return;
            }

            string commanderName;
            if (string.IsNullOrEmpty(starMapConfiguration.commanderName))
            {
                // Fetch the commander name from the companion app
                Commander cmdr = EDDI.Instance.Cmdr;
                if (cmdr != null && cmdr.name != null)
                {
                    commanderName = cmdr.name;
                }
                else
                {
                    edsmFetchLogsButton.IsEnabled = false;
                    edsmFetchLogsButton.Content = "Companion app not configured and no name supplied; cannot obtain logs";
                    return;
                }
            }
            else
            {
                commanderName = starMapConfiguration.commanderName;
            }

            edsmFetchLogsButton.IsEnabled = false;
            edsmFetchLogsButton.Content = "Obtaining log...";

            var progress = new Progress<string>(s => edsmFetchLogsButton.Content = s);
            await Task.Factory.StartNew(() => obtainEdsmLogs(starMapConfiguration, commanderName, progress),
                                            TaskCreationOptions.LongRunning);

            starMapConfiguration.lastSync = DateTime.UtcNow;
            starMapConfiguration.ToFile();
        }

        public static void obtainEdsmLogs(StarMapConfiguration starMapConfiguration, string commanderName, IProgress<string> progress)
        {
            StarMapService starMapService = new StarMapService(starMapConfiguration.apiKey, commanderName);
            try
            {
                Dictionary<string, StarMapLogInfo> systems = starMapService.getStarMapLog();
                Dictionary<string, string> comments = starMapService.getStarMapComments();
                int total = systems.Count;
                int i = 0;
                foreach (string system in systems.Keys)
                {
                    progress.Report("Obtaining log " + i++ + "/" + total);
                    StarSystem CurrentStarSystem = StarSystemSqLiteRepository.Instance.GetOrCreateStarSystem(system, false);
                    CurrentStarSystem.visits = systems[system].visits;
                    CurrentStarSystem.lastvisit = systems[system].lastVisit;
                    if (comments.ContainsKey(system))
                    {
                        CurrentStarSystem.comment = comments[system];
                    }
                    StarSystemSqLiteRepository.Instance.SaveStarSystem(CurrentStarSystem);
                }
                progress.Report("Obtained log");
            }
            catch (EDSMException edsme)
            {
                progress.Report("EDSM error received: " + edsme.Message);
            }
        }
    }
}
