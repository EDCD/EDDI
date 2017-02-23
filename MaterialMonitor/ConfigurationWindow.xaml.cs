<<<<<<< 7cbcc8de1cbcbf428f25b18ac4a1658a1f0ba9cf
﻿using Eddi;
using System.Windows.Controls;
using System.Windows.Data;
=======
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
>>>>>>> Add material monitor.

namespace EddiMaterialMonitor
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class ConfigurationWindow : UserControl
    {
<<<<<<< 7cbcc8de1cbcbf428f25b18ac4a1658a1f0ba9cf
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
=======
        public ConfigurationWindow()
        {
            InitializeComponent();
>>>>>>> Add material monitor.
        }
    }
}
