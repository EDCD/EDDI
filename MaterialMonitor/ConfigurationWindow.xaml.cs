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
        private MaterialMonitor materialMonitor()
        {
            return (MaterialMonitor)EDDI.Instance.ObtainMonitor("Material monitor");
        }

        public ConfigurationWindow()
        {
            InitializeComponent();
            materialsData.ItemsSource = materialMonitor()?.inventory;
        }

        private void materialsUpdated(object sender, DataTransferEventArgs e)
        {
            // Update the material monitor's information
            materialMonitor()?.writeMaterials();
        }
    }
}
