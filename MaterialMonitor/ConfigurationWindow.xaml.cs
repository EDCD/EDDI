using Eddi;
using System.Windows.Controls;
using System.Windows.Data;

namespace EddiMaterialMonitor
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class ConfigurationWindow : UserControl
    {
        MaterialMonitor monitor;

        public ConfigurationWindow()
        {
            InitializeComponent();

            monitor = ((MaterialMonitor)EDDI.Instance.ObtainMonitor("Material monitor"));
            materialsData.ItemsSource = monitor.inventory;
        }

        private void materialsUpdated(object sender, DataTransferEventArgs e)
        {
            // Update the material monitor's information
            monitor.writeMaterials();
        }
    }
}
