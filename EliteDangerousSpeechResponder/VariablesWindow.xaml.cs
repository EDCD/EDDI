using System;
using System.Collections.Generic;
using System.IO;
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

namespace EliteDangerousSpeechResponder
{
    /// <summary>
    /// Interaction logic for VariablesWindow.xaml
    /// </summary>
    public partial class VariablesWindow : Window
    {
        public VariablesWindow()
        {
            InitializeComponent();

            // Read Markdown and convert it to HTML
            string markdown = File.ReadAllText("Variables.md");
            string html = CommonMark.CommonMarkConverter.Convert(markdown);

            // Insert the HTML
            textBrowser.NavigateToString(html);
        }
    }
}
