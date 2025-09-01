using EddiCore;
using EddiSpeechService;
using System;
using System.Windows;
using System.Windows.Controls;
using Utilities;

namespace CommanderMonitor
{
    /// <summary>
    /// Interaction logic for ConfigurationWindow.xaml
    /// </summary>
    public partial class ConfigurationWindow : UserControl
    {
        private static EddiCommanderMonitor.CommanderMonitor commanderMonitor => (EddiCommanderMonitor.CommanderMonitor)EDDI.Instance.ObtainMonitor( "Commander Monitor" );

        public ConfigurationWindow ()
        {
            InitializeComponent();
            DataContext = commanderMonitor;

            var config = EddiConfigService.ConfigService.Instance.commanderConfiguration;
            HomeSystemComboBox.Text = config.homeSystemName;
            SquadronSystemComboBox.Text = config.squadronSystemName;
        }

        private void PhoneticName_TextChanged ( object sender, TextChangedEventArgs e )
        {
            // Replace any spaces, maintaining the original caret position
            var caretIndex = phoneticNameTextBox.CaretIndex;
            phoneticNameTextBox.Text = phoneticNameTextBox.Text.Replace( " ", "ˈ" );
            phoneticNameTextBox.CaretIndex = Math.Max( caretIndex, phoneticNameTextBox.Text.Length );
        }

        private void phoneticNameTestButtonClicked ( object sender, RoutedEventArgs e )
        {
            SpeechService.Instance.Say( null, commanderMonitor.Cmdr.SpokenName(), 0 );
        }

        private void ipaClicked ( object sender, RoutedEventArgs e )
        {
            var IpaResources = new IpaResourcesWindow();
            IpaResources.Show();
        }
    }
}
