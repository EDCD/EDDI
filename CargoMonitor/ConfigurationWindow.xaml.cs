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
        private CargoMonitor cargoMonitor()
        {
            return (CargoMonitor)EDDI.Instance.ObtainMonitor("Cargo monitor");
        }

        public ConfigurationWindow()
        {
            InitializeComponent();
            cargoData.ItemsSource = cargoMonitor()?.inventory;
        }

        private void cargoUpdated(object sender, DataTransferEventArgs e)
        {
            // Update the cargo monitor's information
            cargoMonitor()?.writeInventory();
        }
    }
}
