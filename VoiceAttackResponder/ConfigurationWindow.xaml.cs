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
            Process.Start("https://youtube.com/");
        }

        private void VAVariablesClicked(object sender, RoutedEventArgs e)
        {
            Process.Start("https://github.com/cmdrmcdonald/EliteDangerousDataProvider/blob/feature/journal/VoiceAttack.md");
        }
    }
}
