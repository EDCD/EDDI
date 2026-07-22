using EddiCore;
using EddiSpeechService;
using System;
using System.Windows;
using System.Windows.Controls;
using Utilities;

namespace EddiCommanderMonitor
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

        private void PhoneticNameTextBox_Loaded ( object sender, RoutedEventArgs e )
        {
            if ( sender is TextBox textBox )
            {
                // Force validation to reapply when the control is loaded
                var bindingExpression = textBox.GetBindingExpression(TextBox.TextProperty);
                if ( bindingExpression != null )
                {
                    bindingExpression.UpdateSource();
                    // Explicitly refresh the validation state
                    var errors = Validation.GetErrors(textBox);
                    if ( errors.Count > 0 )
                    {
                        Validation.ClearInvalid( bindingExpression );
                        Validation.MarkInvalid( bindingExpression, errors[ 0 ] );
                    }
                    else
                    {
                        Validation.ClearInvalid( bindingExpression );
                    }
                    // Force WPF to refresh the visual state
                    textBox.InvalidateVisual();
                    textBox.UpdateLayout();
                }
            }
        }

        private void PhoneticName_TextChanged ( object sender, TextChangedEventArgs e )
        {
            if ( sender is TextBox textBox )
            {
                // Replace any spaces, maintaining the original caret position
                var caretIndex = textBox.CaretIndex;
                textBox.Text = textBox.Text.Replace( " ", "ˈ" );
                textBox.CaretIndex = Math.Max( caretIndex, textBox.Text.Length );
            }
        }

        private async void phoneticNameTestButtonClicked ( object sender, RoutedEventArgs e )
        {
            try
            {
                await SpeechService.Instance.SayAsync( null, commanderMonitor.Cmdr.SpokenName(), 0 ).ConfigureAwait(true);
            }
            catch ( Exception ex )
            {
                Logging.Warn( ex.Message, ex );
            }
        }

        private void ipaClicked ( object sender, RoutedEventArgs e )
        {
            var IpaResources = new IpaResourcesWindow();
            IpaResources.Show();
        }
    }
}
