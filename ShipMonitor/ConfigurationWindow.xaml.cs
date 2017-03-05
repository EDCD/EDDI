using Eddi;
using System;
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

namespace EddiShipMonitor
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class ConfigurationWindow : UserControl
    {
        ShipMonitor monitor;

        public ConfigurationWindow()
        {
            InitializeComponent();

            monitor = ((ShipMonitor)EDDI.Instance.ObtainMonitor("Ship monitor"));
            shipData.ItemsSource = monitor.shipyard;
        }

        private void shipsUpdated(object sender, DataTransferEventArgs e)
        {
            // Update the ship monitor's information
            monitor.writeShips();
        }
    }

    public class DataGridNumericColumn : DataGridTextColumn
    {
        protected override object PrepareCellForEdit(FrameworkElement editingElement, RoutedEventArgs editingEventArgs)
        {
            TextBox edit = editingElement as TextBox;
            edit.PreviewTextInput += OnPreviewTextInput;

            return base.PrepareCellForEdit(editingElement, editingEventArgs);
        }

        private void OnPaste(object sender, DataObjectPastingEventArgs e)
        {
            var data = e.SourceDataObject.GetData(DataFormats.Text);
            if (!IsDataValid(data)) e.CancelCommand();
        }

        void OnPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsDataValid(e.Text);
        }

        bool IsDataValid(object data)
        {
            try
            {
                Convert.ToInt32(data);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
