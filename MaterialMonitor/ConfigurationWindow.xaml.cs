using Eddi;
using System.Windows.Controls;
using System.Windows.Data;
using Utilities;

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

            I18NForComponents();
        }

        private void materialsUpdated(object sender, DataTransferEventArgs e)
        {
            // Update the material monitor's information
            monitor.writeMaterials();
        }

        public void I18NForComponents()
        {
            par1Text.Text = I18N.GetString("material_monitor_p1");
            par2Text.Text = I18N.GetString("material_monitor_p2");
            materialsData.Columns[0].Header = I18N.GetString("material_monitor_name_header");
            materialsData.Columns[1].Header = I18N.GetString("material_monitor_type_header");
            materialsData.Columns[2].Header = I18N.GetString("material_monitor_inventory_header");
            materialsData.Columns[3].Header = I18N.GetString("material_monitor_min_header");
            materialsData.Columns[4].Header = I18N.GetString("material_monitor_desired_header");
            materialsData.Columns[5].Header = I18N.GetString("material_monitor_max_header");
        }
    }
}
