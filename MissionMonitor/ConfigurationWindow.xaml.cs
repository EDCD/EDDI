using Eddi;
using System.Windows.Controls;
using System.Windows.Data;

namespace EddiMissionMonitor
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class ConfigurationWindow : UserControl
    {
        MissionMonitor monitor;

        public ConfigurationWindow()
        {
            InitializeComponent();

            monitor = ((MissionMonitor)EDDI.Instance.ObtainMonitor("Mission monitor"));
            missionsData.ItemsSource = monitor.missions;
        }

        private void missionsUpdated(object sender, DataTransferEventArgs e)
        {
            // Update the mission monitor's information
            monitor.writeMissions();
        }
    }
}
