using EddiCore;
using EddiDataDefinitions;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Data;

namespace EddiMaterialMonitor
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class ConfigurationWindow : UserControl
    {
        public IEnumerable<MaterialCategory> categories => MaterialCategory.AllOfThem;

        public IEnumerable<Rarity> rarities => Rarity.AllOfThem;

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
