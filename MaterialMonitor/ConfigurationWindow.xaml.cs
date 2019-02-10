using System.Windows.Controls;
using System.Windows.Data;

namespace EddiMaterialMonitor
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class ConfigurationWindow : UserControl
    {
        public ConfigurationWindow()
        {
            InitializeComponent();
            materialsData.ItemsSource = MaterialMonitor.Instance.inventory;
        }

        private void materialsUpdated(object sender, DataTransferEventArgs e)
        {
            // Update the material monitor's information
            MaterialMonitor.Instance.writeMaterials();
        }
    }
}
