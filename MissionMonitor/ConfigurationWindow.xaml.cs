using EddiCore;
using EddiConfigService;
using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using Utilities;

namespace EddiMissionMonitor
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class ConfigurationWindow : UserControl
    {

        private MissionMonitorConfiguration missionsConfig = new MissionMonitorConfiguration();

        private MissionMonitor missionMonitor()
        {
            return (MissionMonitor)EDDI.Instance.ObtainMonitor("Mission monitor");
        }

        public ConfigurationWindow()
        {
            InitializeComponent();

            missionsData.ItemsSource = missionMonitor()?.missions;

            missionsConfig = ConfigService.Instance.missionMonitorConfiguration;
            missionWarningInt.Text = missionsConfig.missionWarning?.ToString(CultureInfo.InvariantCulture);
        }

        private void missionsUpdated(object sender, DataTransferEventArgs e)
        {
            // Update the mission monitor's information
            missionMonitor()?.writeMissions();
        }

        private void warningChanged(object sender, TextChangedEventArgs e)
        {
            missionsConfig = ConfigService.Instance.missionMonitorConfiguration;
            try
            {
                int? warning = string.IsNullOrWhiteSpace(missionWarningInt.Text) ? Constants.missionWarningDefault
                    : Convert.ToInt32(missionWarningInt.Text, CultureInfo.InvariantCulture);
                missionMonitor().missionWarning = warning;
                missionsConfig.missionWarning = warning;
                ConfigService.Instance.missionMonitorConfiguration = missionsConfig;
            }
            catch
            {
                // Bad user input; ignore it
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
