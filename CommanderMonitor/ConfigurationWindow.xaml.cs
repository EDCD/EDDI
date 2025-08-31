using EddiConfigService;
using EddiCore;
using EddiDataDefinitions;
using EddiSpeechService;
using System;
using System.Collections.Generic;
using System.Linq;
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
        public EddiCommanderMonitor.CommanderMonitor commanderMonitor = (EddiCommanderMonitor.CommanderMonitor)EDDI.Instance.ObtainMonitor( "Commander Monitor" );

        public ConfigurationWindow ()
        {
            InitializeComponent();
            DataContext = commanderMonitor;

            var configuration = ConfigService.Instance.commanderConfiguration;
            
            // Setup home system & station from config file
            ConfigureHomeSystemOptions( configuration.homeSystemName );
            ConfigureHomeStationOptions();
        }

        #region Commander Name

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

        #endregion

        #region Home System

        internal void ConfigureHomeSystemOptions ( string newHomeSystemName )
        {
            homeSystemDropDown.Text = newHomeSystemName ?? string.Empty;
        }

        // Handle changes to the editable home system combo box
        private void HomeSystemDropDown_SelectionChanged ( object sender, SelectionChangedEventArgs e )
        {
            try
            {
                if ( sender is StarSystemComboBox starSystemComboBox )
                {
                    if ( !starSystemComboBox.IsLoaded ) { return; }

                    // Update configuration to new home system
                    if ( e.AddedItems.Count == 1 && e.RemovedItems.Count == 0 )
                    {
                        var newHomeSystem = e.AddedItems[0] as NavWaypoint;
                        //commanderMonitor().setHomeSystemAsync( newHomeSystem?.systemAddress ).GetAwaiter().GetResult();
                        ConfigureHomeStationOptions();
                    }
                }
            }
            catch ( Exception ex )
            {
                Logging.Error( ex.Message, ex );
            }
        }

        #endregion

        #region Home Station

        internal void ConfigureHomeStationOptions ()
        {
            homeStationDropDown.DisplayMemberPath = nameof(Station.name);
            var homeStationOptions = ( EDDI.Instance.HomeStarSystem?.stations.ToList() ?? new List<Station>() )
                .OrderBy( s => s.name )
                .Prepend( new Station { name = EddiCommanderMonitor.Properties.Resources.no_station } ).ToHashSet();
            homeStationDropDown.ItemsSource = homeStationOptions;
            homeStationDropDown.SelectedItem = EDDI.Instance.HomeStation ??
                                               new Station { name = EddiCommanderMonitor.Properties.Resources.no_station };
        }

        private void homeStationDrop_SelectionChanged ( object sender, SelectionChangedEventArgs e )
        {
            foreach ( var obj in e.AddedItems )
            {
                if ( obj is Station selectedStation )
                {
                    var configuration = ConfigService.Instance.commanderConfiguration;
                    if ( configuration.homeStationMarketID != selectedStation.marketId )
                    {
                        //commanderMonitor().setHomeStation( selectedStation.marketId );
                        ConfigService.Instance.commanderConfiguration = configuration;
                    }
                }
            }
        }

        #endregion
    }
}
