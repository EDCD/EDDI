using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace EddiVoiceAttackResponder
{
    /// <summary>
    /// Interaction logic for ConfigurationWindow.xaml
    /// </summary>
    public partial class ConfigurationWindow : UserControl
    {
        public ConfigurationWindow()
        {
            InitializeComponent();
        }

        private void VAExampleClicked(object sender, RoutedEventArgs e)
        {
            // TODO: Add links to a youtube playlist?
            var processInfo = new ProcessStartInfo { FileName = "https://youtube.com/", UseShellExecute = true };
            Process.Start( processInfo );
        }

        private void VAEventsClicked(object sender, RoutedEventArgs e)
        {
            var processInfo = new ProcessStartInfo { FileName = "https://github.com/EDCD/EDDI/wiki/Events", UseShellExecute = true };
            Process.Start( processInfo );
        }

        private void VAVariablesClicked ( object sender, RoutedEventArgs e )
        {
            var processInfo = new ProcessStartInfo { FileName = "https://github.com/EDCD/EDDI/wiki/VoiceAttack-Integration", UseShellExecute = true };
            Process.Start( processInfo );
        }
    }
}
