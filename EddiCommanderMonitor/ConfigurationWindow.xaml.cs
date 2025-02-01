using EddiConfigService;
using EddiConfigService.Configurations;
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
        private EddiCommanderMonitor.CommanderMonitor commanderMonitor ()
        {
            return (EddiCommanderMonitor.CommanderMonitor)EDDI.Instance.ObtainMonitor( "Commander monitor" );
        }

        public ConfigurationWindow ()
        {
            InitializeComponent();

            EDDIConfiguration eddiConfiguration = ConfigService.Instance.eddiConfiguration;

            // Setup home system & station from config file
            homeSystemDropDown.ItemsSource = new List<string> { eddiConfiguration.HomeSystem ?? string.Empty };
            homeSystemDropDown.SelectedItem = eddiConfiguration.HomeSystem ?? string.Empty;
            ConfigureHomeStationOptions();
            homeStationDropDown.SelectedItem = eddiConfiguration.HomeStation ?? EddiCommanderMonitor.Properties.Resources.no_station;
            if ( eddiConfiguration.Gender == "Female" )
            {
                eddiGenderFemale.IsChecked = true;
            }
            else if ( eddiConfiguration.Gender == "Male" )
            {
                eddiGenderMale.IsChecked = true;
            }
            else
            {
                eddiGenderNeither.IsChecked = true;
            }
            eddiCommanderPhoneticNameText.Text = eddiConfiguration.PhoneticName ?? string.Empty;
            eddiSquadronNameText.Text = eddiConfiguration.SquadronName ?? string.Empty;
            eddiSquadronIDText.Text = eddiConfiguration.SquadronID ?? string.Empty;
            squadronRankDropDown.SelectedItem = ( eddiConfiguration.SquadronRank ?? SquadronRank.None ).localizedName;
            ConfigureSquadronRankOptions( eddiConfiguration );

            // Setup squadron home system from config file
            squadronSystemDropDown.ItemsSource = new List<string> { eddiConfiguration.SquadronSystem ?? string.Empty };
            squadronSystemDropDown.SelectedItem = eddiConfiguration.SquadronSystem ?? string.Empty;

            squadronFactionDropDown.SelectedItem = eddiConfiguration.SquadronFaction ?? Power.None.localizedName;
            squadronPowerDropDown.SelectedItem = ( eddiConfiguration.SquadronPower ?? Power.None ).localizedName;
            ConfigureSquadronPowerOptions( eddiConfiguration );
        }

        // Handle changes to the editable home system combo box
        private void HomeSystemText_TextChanged ( object sender, TextChangedEventArgs e )
        {
            if ( sender is StarSystemComboBox starSystemComboBox && !starSystemComboBox.IsLoaded )
            { return; }

            EDDIConfiguration eddiConfiguration = ConfigService.Instance.eddiConfiguration;
            void changeHandler ()
            {
                // Reset the home station due to selecting new home system
                if ( eddiConfiguration.HomeStation != null )
                {
                    eddiConfiguration.HomeStation = null;
                    homeStationDropDown.SelectedItem = EddiCommanderMonitor.Properties.Resources.no_station;
                    ConfigureHomeStationOptions();
                    ConfigService.Instance.eddiConfiguration = eddiConfiguration;
                }
            }
            homeSystemDropDown.TextDidChange( sender, e, eddiConfiguration.HomeSystem, changeHandler );
        }

        private void HomeSystemDropDown_SelectionChanged ( object sender, SelectionChangedEventArgs e )
        {
            if ( sender is StarSystemComboBox starSystemComboBox && !starSystemComboBox.IsLoaded )
            { return; }

            void changeHandler ( NavWaypoint newValue )
            {
                // Update configuration to new home system
                EDDI.Instance.setHomeSystem( newValue.systemAddress );

                // Update station options for new system
                ConfigureHomeStationOptions();
            }
            homeSystemDropDown.SelectionDidChange( changeHandler );
        }

        private void HomeSystemDropDown_LostFocus ( object sender, RoutedEventArgs e )
        {
            if ( sender is StarSystemComboBox starSystemComboBox && !starSystemComboBox.IsLoaded )
            { return; }

            EDDIConfiguration eddiConfiguration = ConfigService.Instance.eddiConfiguration;
            homeSystemDropDown.DidLoseFocus( oldValue: eddiConfiguration.HomeSystem );
        }

        private void ConfigureHomeStationOptions ()
        {
            var homeStationOptions = new List<string> { EddiCommanderMonitor.Properties.Resources.no_station };
            if ( EDDI.Instance.HomeStarSystem?.stations != null )
            {
                var systemStations = EDDI.Instance.HomeStarSystem.stations
                    .OrderBy(s => s.name).Select( s => s.name );
                homeStationOptions.AddRange( systemStations );
            }

            // sort but leave "No Station" at the top
            homeStationDropDown.ItemsSource = homeStationOptions;
        }

        private void homeStationDropDownUpdated ( object sender, SelectionChangedEventArgs e )
        {
            EDDIConfiguration eddiConfiguration = ConfigService.Instance.eddiConfiguration;
            string homeStationName = homeStationDropDown.SelectedItem?.ToString();
            if ( eddiConfiguration.HomeStation != homeStationName )
            {
                eddiConfiguration.HomeStation = homeStationName == EddiCommanderMonitor.Properties.Resources.no_station ? null : homeStationName;
                eddiConfiguration = EDDI.Instance.setHomeStation( eddiConfiguration );
                ConfigService.Instance.eddiConfiguration = eddiConfiguration;
            }
        }

        private void isMale_Checked ( object sender, RoutedEventArgs e )
        {
            EDDIConfiguration eddiConfiguration = ConfigService.Instance.eddiConfiguration;
            eddiConfiguration.Gender = "Male";
            ConfigService.Instance.eddiConfiguration = eddiConfiguration;
        }

        private void isFemale_Checked ( object sender, RoutedEventArgs e )
        {
            EDDIConfiguration eddiConfiguration = ConfigService.Instance.eddiConfiguration;
            eddiConfiguration.Gender = "Female";
            ConfigService.Instance.eddiConfiguration = eddiConfiguration;
            if ( EDDI.Instance.Cmdr != null )
            {
                EDDI.Instance.Cmdr.gender = "Female";
            }
        }

        private void isNeitherGender_Checked ( object sender, RoutedEventArgs e )
        {
            EDDIConfiguration eddiConfiguration = ConfigService.Instance.eddiConfiguration;
            eddiConfiguration.Gender = "Neither";
            ConfigService.Instance.eddiConfiguration = eddiConfiguration;
            if ( EDDI.Instance.Cmdr != null )
            {
                EDDI.Instance.Cmdr.gender = "Neither";
            }
        }

        private void commanderPhoneticNameChanged ( object sender, TextChangedEventArgs e )
        {
            // Replace any spaces, maintaining the original caret position
            int caretIndex = eddiCommanderPhoneticNameText.CaretIndex;
            eddiCommanderPhoneticNameText.Text = eddiCommanderPhoneticNameText.Text.Replace( " ", "ˈ" );
            eddiCommanderPhoneticNameText.CaretIndex = Math.Max( caretIndex, eddiCommanderPhoneticNameText.Text.Length );

            // Update our config file
            if ( eddiCommanderPhoneticNameText.IsLoaded )
            {
                EDDIConfiguration eddiConfiguration = ConfigService.Instance.eddiConfiguration;
                if ( eddiConfiguration.PhoneticName != eddiCommanderPhoneticNameText.Text )
                {
                    eddiConfiguration.PhoneticName = string.IsNullOrWhiteSpace( eddiCommanderPhoneticNameText.Text ) ? string.Empty : eddiCommanderPhoneticNameText.Text.Trim();
                    ConfigService.Instance.eddiConfiguration = eddiConfiguration;
                }
            }
        }

        private void eddiCmdrPhoneticNameTestButtonClicked ( object sender, RoutedEventArgs e )
        {
            SpeechService.Instance.Say( null, EDDI.Instance.Cmdr?.SpokenName(), 0 );
        }

        private void ipaClicked ( object sender, RoutedEventArgs e )
        {
            IpaResourcesWindow IpaResources = new IpaResourcesWindow();
            IpaResources.Show();
        }

        private void squadronNameChanged ( object sender, TextChangedEventArgs e )
        {
            EDDIConfiguration eddiConfiguration = ConfigService.Instance.eddiConfiguration;
            if ( eddiConfiguration.SquadronName != eddiSquadronNameText.Text )
            {
                eddiConfiguration.SquadronName = string.IsNullOrWhiteSpace( eddiSquadronNameText.Text ) ? null : eddiSquadronNameText.Text.Trim();
                if ( eddiConfiguration.SquadronName == null )
                {
                    eddiConfiguration.SquadronID = null;
                    eddiSquadronIDText.Text = string.Empty;

                    squadronSystemDropDown.Text = string.Empty;
                }
                eddiConfiguration = resetSquadronRank( eddiConfiguration );
                ConfigService.Instance.eddiConfiguration = eddiConfiguration;

                if ( EDDI.Instance.Cmdr != null )
                {
                    EDDI.Instance.Cmdr.squadronname = eddiConfiguration.SquadronName;
                }
            }
        }

        private void eddiCommanderPhoneticNameText_LostFocus ( object sender, RoutedEventArgs e )
        {
            // Discard invalid results
            if ( eddiCommanderPhoneticNameText.Text == string.Empty )
            {
                EDDIConfiguration eddiConfiguration = ConfigService.Instance.eddiConfiguration;
                eddiConfiguration.PhoneticName = null;
                ConfigService.Instance.eddiConfiguration = eddiConfiguration;
                if ( EDDI.Instance.Cmdr != null )
                {
                    EDDI.Instance.Cmdr.phoneticName = string.Empty;
                }
            }
        }

        private void CommanderDetailsTab_GotFocus ( object sender, RoutedEventArgs e )
        {
            EDDIConfiguration eddiConfiguration = ConfigService.Instance.eddiConfiguration;
            if ( eddiConfiguration.SquadronName != eddiSquadronNameText.Text )
            {
                eddiSquadronNameText.Text = eddiConfiguration.SquadronName;
            }
            if ( eddiConfiguration.SquadronID != eddiSquadronIDText.Text )
            {
                eddiSquadronIDText.Text = eddiConfiguration.SquadronID;
            }
        }

        private void eddiSquadronNameText_LostFocus ( object sender, RoutedEventArgs e )
        {
            // Discard invalid results
            if ( eddiSquadronNameText.Text == string.Empty )
            {
                EDDIConfiguration eddiConfiguration = ConfigService.Instance.eddiConfiguration;
                eddiConfiguration.SquadronName = null;
                ConfigService.Instance.eddiConfiguration = eddiConfiguration;
                if ( EDDI.Instance.Cmdr != null )
                {
                    EDDI.Instance.Cmdr.squadronname = string.Empty;
                }
            }
        }

        private void squadronIDChanged ( object sender, TextChangedEventArgs e )
        {
            EDDIConfiguration eddiConfiguration = ConfigService.Instance.eddiConfiguration;
            if ( eddiConfiguration.SquadronID != eddiSquadronIDText.Text )
            {
                eddiConfiguration.SquadronID = string.IsNullOrWhiteSpace( eddiSquadronIDText.Text ) ? null : eddiSquadronIDText.Text.Trim();
                ConfigService.Instance.eddiConfiguration = eddiConfiguration;

                if ( EDDI.Instance.Cmdr != null )
                {
                    EDDI.Instance.Cmdr.squadronid = eddiConfiguration.SquadronID;
                }
            }
        }

        private void eddiSquadronIDText_LostFocus ( object sender, RoutedEventArgs e )
        {
            // Discard invalid results
            EDDIConfiguration eddiConfiguration = ConfigService.Instance.eddiConfiguration;
            if ( eddiConfiguration.SquadronID != null )
            {
                if ( eddiConfiguration.SquadronID.Contains( " " ) || eddiConfiguration.SquadronID.Length > 4 )
                {
                    eddiConfiguration.SquadronID = null;
                    squadronSystemDropDown.Text = string.Empty;
                    ConfigService.Instance.eddiConfiguration = eddiConfiguration;
                }
            }
        }

        private void squadronRankDropDownUpdated ( object sender, SelectionChangedEventArgs e )
        {
            EDDIConfiguration eddiConfiguration = ConfigService.Instance.eddiConfiguration;
            string squadronRank = squadronRankDropDown.SelectedItem.ToString();

            if ( eddiConfiguration.SquadronRank.edname != squadronRank )
            {
                eddiConfiguration.SquadronRank = SquadronRank.FromName( squadronRank );
                ConfigService.Instance.eddiConfiguration = eddiConfiguration;

                if ( EDDI.Instance.Cmdr != null )
                {
                    EDDI.Instance.Cmdr.squadronrank = eddiConfiguration.SquadronRank;
                }
            }
        }

        // Handle changes to the editable squadron system combo box
        private void SquadronSystemText_TextChanged ( object sender, TextChangedEventArgs e )
        {
            EDDIConfiguration eddiConfiguration = ConfigService.Instance.eddiConfiguration;
            string oldValue = eddiConfiguration.SquadronSystem;
            void changeHandler ()
            {
                // Reset the squadron data due to selecting new squadron system
                if ( eddiConfiguration.SquadronFaction != null )
                {
                    eddiConfiguration.SquadronFaction = null;

                    eddiConfiguration.SquadronAllegiance = Superpower.None;
                    eddiConfiguration.SquadronPower = Power.None;
                    ConfigService.Instance.eddiConfiguration = eddiConfiguration;

                    squadronFactionDropDown.SelectedItem = Power.None.localizedName;
                    ConfigureSquadronFactionOptions();
                    squadronPowerDropDown.SelectedItem = eddiConfiguration.SquadronPower.localizedName;
                    ConfigureSquadronPowerOptions( eddiConfiguration );

                    if ( EDDI.Instance.Cmdr != null )
                    {
                        EDDI.Instance.Cmdr.squadronallegiance = Superpower.None;
                        EDDI.Instance.Cmdr.squadronpower = Power.None;
                    }

                    ConfigService.Instance.eddiConfiguration = eddiConfiguration;
                }
            }
            squadronSystemDropDown.TextDidChange( sender, e, oldValue, changeHandler );
        }

        private void SquadronSystemDropDown_SelectionChanged ( object sender, SelectionChangedEventArgs e )
        {
            void changeHandler ( NavWaypoint newValue )
            {
                // Update configuration to new squadron system
                commanderMonitor().setSquadronSystem( newValue.systemAddress );

                // Update squadron faction options for new system
                ConfigureSquadronFactionOptions();
            }

            if ( sender is StarSystemComboBox comboBox && comboBox.IsLoaded )
            {
                squadronSystemDropDown.SelectionDidChange( changeHandler );
            }
        }

        private void SquadronSystemDropDown_LostFocus ( object sender, RoutedEventArgs e )
        {
            EDDIConfiguration eddiConfiguration = ConfigService.Instance.eddiConfiguration;
            squadronSystemDropDown.DidLoseFocus( oldValue: eddiConfiguration.SquadronSystem );
        }

        private void squadronFactionDropDownUpdated ( object sender, SelectionChangedEventArgs e )
        {
            EDDIConfiguration eddiConfiguration = ConfigService.Instance.eddiConfiguration;
            string squadronFaction = squadronFactionDropDown.SelectedItem?.ToString(); // This can be a localized "None"

            if ( eddiConfiguration.SquadronFaction != squadronFaction )
            {
                eddiConfiguration.SquadronFaction = squadronFaction == Power.None.localizedName ? null : squadronFaction;

                if ( EDDI.Instance.Cmdr != null )
                {
                    EDDI.Instance.Cmdr.squadronfaction = eddiConfiguration.SquadronFaction;
                }

                if ( squadronFaction != Power.None.localizedName )
                {
                    var system = EDDI.Instance.SquadronStarSystem;
                    Faction faction = system?.factions.Find(f => f.name == squadronFaction);

                    if ( faction != null && eddiConfiguration.SquadronAllegiance != faction.Allegiance )
                    {
                        eddiConfiguration.SquadronAllegiance = faction.Allegiance;
                        ConfigService.Instance.eddiConfiguration = eddiConfiguration;

                        if ( EDDI.Instance.Cmdr != null )
                        {
                            EDDI.Instance.Cmdr.squadronallegiance = faction.Allegiance;
                        }

                        squadronPowerDropDown.SelectedItem = Power.None.localizedName;
                        ConfigureSquadronPowerOptions( eddiConfiguration );
                    }
                }
                else
                {
                    eddiConfiguration.SquadronAllegiance = Superpower.None;
                    ConfigService.Instance.eddiConfiguration = eddiConfiguration;

                    if ( EDDI.Instance.Cmdr != null )
                    {
                        EDDI.Instance.Cmdr.squadronallegiance = Superpower.None;
                    }

                    squadronPowerDropDown.SelectedItem = Power.None.localizedName;
                    ConfigureSquadronPowerOptions( eddiConfiguration );
                }
                ConfigService.Instance.eddiConfiguration = eddiConfiguration;
            }
        }

        private void squadronPowerDropDownUpdated ( object sender, SelectionChangedEventArgs e )
        {
            EDDIConfiguration eddiConfiguration = ConfigService.Instance.eddiConfiguration;
            string squadronPower = squadronPowerDropDown.SelectedItem?.ToString();

            if ( ( eddiConfiguration.SquadronPower?.localizedName ?? "" ) != squadronPower )
            {
                eddiConfiguration.SquadronPower = Power.FromName( squadronPower );
                ConfigService.Instance.eddiConfiguration = eddiConfiguration;

                if ( EDDI.Instance.Cmdr != null )
                {
                    EDDI.Instance.Cmdr.squadronpower = eddiConfiguration.SquadronPower;
                }
            }
        }

        private void ConfigureSquadronRankOptions ( EDDIConfiguration configuration )
        {
            List<string> SquadronRankOptions = new List<string>();

            foreach ( SquadronRank squadronrank in SquadronRank.AllOfThem )
            {
                if ( configuration.SquadronName == null && squadronrank != SquadronRank.None )
                {
                    break;
                }
                SquadronRankOptions.Add( squadronrank.localizedName );
            }
            // Don't sort
            squadronRankDropDown.ItemsSource = SquadronRankOptions;
        }

        public void ConfigureSquadronFactionOptions ()
        {
            var squadronFactionOptions = new List<string> { Power.None.localizedName };
            if ( EDDI.Instance.SquadronStarSystem != null )
            {
                var starSystemFactions = EDDI.Instance.SquadronStarSystem.factions
                    .OrderBy( s => s.name ).Select( s => s.name );
                squadronFactionOptions.AddRange( starSystemFactions );
            }
            squadronFactionDropDown.ItemsSource = squadronFactionOptions;
        }

        public void ConfigureSquadronPowerOptions ( EDDIConfiguration configuration )
        {
            List<string> SquadronPowerOptions = new List<string>
            {
                Power.None.localizedName
            };
            if ( configuration.SquadronAllegiance != Superpower.None )
            {
                foreach ( Power power in Power.AllOfThem )
                {
                    if ( configuration.SquadronAllegiance == power.Allegiance )
                    {
                        SquadronPowerOptions.Add( power.localizedName );
                    }
                }
            }
            // sort but leave "None" at the top
            SquadronPowerOptions.Sort( 1, SquadronPowerOptions.Count - 1, null );
            squadronPowerDropDown.ItemsSource = SquadronPowerOptions;
        }

        public EDDIConfiguration resetSquadronRank ( EDDIConfiguration configuration )
        {
            if ( configuration.SquadronName == null )
            {
                configuration.SquadronRank = SquadronRank.None;
                squadronRankDropDown.SelectedItem = configuration.SquadronRank.localizedName;
            }
            ConfigureSquadronRankOptions( configuration );

            return configuration;
        }
    }
}
