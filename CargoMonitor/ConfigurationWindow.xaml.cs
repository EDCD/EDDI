using Eddi;
using System.Windows.Controls;
using System.Windows.Data;

namespace EddiCargoMonitor
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class ConfigurationWindow : UserControl
    {
        CargoMonitor monitor;

        public ConfigurationWindow()
        {
            InitializeComponent();

            monitor = ((CargoMonitor)EDDI.Instance.ObtainMonitor("Cargo monitor"));
            cargoData.ItemsSource = monitor.inventory;
        }

        private void cargoUpdated(object sender, DataTransferEventArgs e)
        {
            // Update the cargo monitor's information
            monitor.writeInventory();
        }
    }
}
