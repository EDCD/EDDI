using System.Windows.Controls;
using System.Windows.Data;

namespace EddiCargoMonitor
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class ConfigurationWindow : UserControl
    {
        public ConfigurationWindow()
        {
            InitializeComponent();
            cargoData.ItemsSource = CargoMonitor.Instance.inventory;
        }

        private void cargoUpdated(object sender, DataTransferEventArgs e)
        {
            // Update the cargo monitor's information
            CargoMonitor.Instance.writeInventory();
        }
    }
}
