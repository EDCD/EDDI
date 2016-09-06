using EDDI;
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
using System.Windows.Shapes;

namespace configuration
{
    /// <summary>
    /// Interaction logic for EditScriptWindow.xaml
    /// </summary>
    public partial class EditScriptWindow : Window
    {
        private EventScript script;

        private static readonly string DEFAULT_DESCRIPTION = "foo";

        public EditScriptWindow(EventScript script)
        {
            this.script = script;
            InitializeComponent();

            descriptionText.Text = script.Description == null ? DEFAULT_DESCRIPTION : script.Description;
            scriptText.Text = script.Value;
        }

        private void acceptButtonClick(object sender, RoutedEventArgs e)
        {
        }

        private void cancelButtonClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void resetButtonClick(object sender, RoutedEventArgs e)
        {
            scriptText.Text = script.Value;
        }

        private void defaultButtonClick(object sender, RoutedEventArgs e)
        {
            scriptText.Text = script.DefaultValue;
        }

        private void testButtonClick(object sender, RoutedEventArgs e)
        {
            Eddi.Instance.Say(scriptText.Text);
        }
    }
}
