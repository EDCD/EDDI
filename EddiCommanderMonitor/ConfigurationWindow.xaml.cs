using EddiConfigService;
using EddiConfigService.Configurations;
using EddiCore;
using EddiDataDefinitions;
using EddiSpeechService;
using JetBrains.Annotations;
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

        [CanBeNull]
        internal StarSystem SquadronStarSystem => commanderMonitor().SquadronStarSystem;

        public ConfigurationWindow ()
        {
            InitializeComponent();

            var configuration = ConfigService.Instance.commanderConfiguration;

            ConfigureCommanderNameOptions( configuration );
            ConfigureCommanderGenderOptions( configuration );

            // Setup home system & station from config file
            homeSystemDropDown.Text = configuration.homeSystemName ?? string.Empty;
            ConfigureHomeStationOptions( configuration );

            ConfigureSquadronOptions( configuration );
        }

        #region Commander Name

        private void ConfigureCommanderNameOptions ( CommanderConfiguration configuration )
        {
            eddiCommanderPhoneticNameText.Text = configuration.phoneticName ?? string.Empty;
        }

        private void commanderPhoneticNameChanged ( object sender, TextChangedEventArgs e )
        {
            // Replace any spaces, maintaining the original caret position
            var caretIndex = eddiCommanderPhoneticNameText.CaretIndex;
            eddiCommanderPhoneticNameText.Text = eddiCommanderPhoneticNameText.Text.Replace( " ", "ˈ" );
            eddiCommanderPhoneticNameText.CaretIndex = Math.Max( caretIndex, eddiCommanderPhoneticNameText.Text.Length );

            // Update our config file
            if ( eddiCommanderPhoneticNameText.IsLoaded )
            {
                var configuration = ConfigService.Instance.commanderConfiguration;
                if ( configuration.phoneticName != eddiCommanderPhoneticNameText.Text )
                {
                    configuration.phoneticName = string.IsNullOrWhiteSpace( eddiCommanderPhoneticNameText.Text ) ? string.Empty : eddiCommanderPhoneticNameText.Text.Trim();
                    ConfigService.Instance.commanderConfiguration = configuration;
                }
            }
        }

        private void eddiCommanderPhoneticNameText_LostFocus ( object sender, RoutedEventArgs e )
        {
            // Discard invalid results
            if ( eddiCommanderPhoneticNameText.Text == string.Empty )
            {
                var configuration = ConfigService.Instance.commanderConfiguration;
                configuration.phoneticName = null;
                ConfigService.Instance.commanderConfiguration = configuration;
                if ( EDDI.Instance.Cmdr != null )
                {
                    EDDI.Instance.Cmdr.phoneticName = string.Empty;
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

        #endregion

        #region Commander Gender

        private void ConfigureCommanderGenderOptions ( CommanderConfiguration configuration )
        {
            if ( configuration.gender == "Female" )
            {
                eddiGenderFemale.IsChecked = true;
            }
            else if ( configuration.gender == "Male" )
            {
                eddiGenderMale.IsChecked = true;
            }
            else
            {
                eddiGenderNeither.IsChecked = true;
            }
        }

        private void isMale_Checked ( object sender, RoutedEventArgs e )
        {
            var configuration = ConfigService.Instance.commanderConfiguration;
            configuration.gender = "Male";
            ConfigService.Instance.commanderConfiguration = configuration;
        }

        private void isFemale_Checked ( object sender, RoutedEventArgs e )
        {
            var configuration = ConfigService.Instance.commanderConfiguration;
            configuration.gender = "Female";
            ConfigService.Instance.commanderConfiguration = configuration;
            if ( EDDI.Instance.Cmdr != null )
            {
                EDDI.Instance.Cmdr.gender = "Female";
            }
        }

        private void isNeitherGender_Checked ( object sender, RoutedEventArgs e )
        {
            var configuration = ConfigService.Instance.commanderConfiguration;
            configuration.gender = "Neither";
            ConfigService.Instance.commanderConfiguration = configuration;
            if ( EDDI.Instance.Cmdr != null )
            {
                EDDI.Instance.Cmdr.gender = "Neither";
            }
        }

        #endregion

        #region Home System / Station

        // Handle changes to the editable home system combo box
        private void HomeSystemDropDown_SelectionChanged ( object sender, SelectionChangedEventArgs e )
        {
            try
            {
                if ( sender is StarSystemComboBox starSystemComboBox )
                {
                    if ( !starSystemComboBox.IsLoaded )
                    { return; }

                    // Update configuration to new home system
                    if ( e.AddedItems.Count == 1 && e.RemovedItems.Count == 0 )
                    {
                        var newHomeSystem = e.AddedItems[0] as NavWaypoint;
                        EDDI.Instance.setHomeSystem( newHomeSystem?.systemAddress );
                        ConfigureHomeStationOptions( ConfigService.Instance.commanderConfiguration );
                    }
                }
            }
            catch ( Exception ex )
            {
                Logging.Error( ex.Message, ex );
            }
        }

        private void ConfigureHomeStationOptions ( CommanderConfiguration configuration )
        {
            homeStationDropDown.Text = configuration.homeStationName ?? EddiCommanderMonitor.Properties.Resources.no_station;

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
            var configuration = ConfigService.Instance.commanderConfiguration;
            string homeStationName = homeStationDropDown.SelectedItem?.ToString();
            if ( configuration.homeStationName != homeStationName )
            {
                configuration.homeStationName = homeStationName == EddiCommanderMonitor.Properties.Resources.no_station ? null : homeStationName;
                configuration = EDDI.Instance.setHomeStation( configuration );
                ConfigService.Instance.commanderConfiguration = configuration;
            }
        }

        #endregion

        #region Squadron

        private void ConfigureSquadronOptions ( CommanderConfiguration configuration )
        {
            // Setup squadron home system from config file
            squadronSystemDropDown.Text = configuration.squadronSystemName ?? string.Empty;

            eddiSquadronNameText.Text = configuration.squadronName ?? string.Empty;
            eddiSquadronIDText.Text = configuration.squadronID ?? string.Empty;

            ConfigureSquadronRankOptions( configuration );

            squadronFactionDropDown.Text = configuration.squadronFaction ?? Power.None.localizedName;
            squadronPowerDropDown.Text = ( configuration.SquadronPower ?? Power.None ).localizedName;
            ConfigureSquadronPowerOptions( configuration );
        }

        private void squadronNameChanged ( object sender, TextChangedEventArgs e )
        {
            var configuration = ConfigService.Instance.commanderConfiguration;
            if ( configuration.squadronName != eddiSquadronNameText.Text )
            {
                configuration.squadronName = string.IsNullOrWhiteSpace( eddiSquadronNameText.Text ) ? null : eddiSquadronNameText.Text.Trim();
                if ( configuration.squadronName == null )
                {
                    configuration.squadronID = null;
                    eddiSquadronIDText.Text = string.Empty;

                    squadronSystemDropDown.Text = string.Empty;
                }
                configuration = resetSquadronRank( configuration );
                ConfigService.Instance.commanderConfiguration = configuration;

                if ( EDDI.Instance.Cmdr != null )
                {
                    EDDI.Instance.Cmdr.squadronname = configuration.squadronName;
                }
            }
        }

        private void eddiSquadronNameText_LostFocus ( object sender, RoutedEventArgs e )
        {
            // Discard invalid results
            if ( eddiSquadronNameText.Text == string.Empty )
            {
                var configuration = ConfigService.Instance.commanderConfiguration;
                configuration.squadronName = null;
                ConfigService.Instance.commanderConfiguration = configuration;
                if ( EDDI.Instance.Cmdr != null )
                {
                    EDDI.Instance.Cmdr.squadronname = string.Empty;
                }
            }
        }

        private void squadronIDChanged ( object sender, TextChangedEventArgs e )
        {
            var configuration = ConfigService.Instance.commanderConfiguration;
            if ( configuration.squadronID != eddiSquadronIDText.Text )
            {
                configuration.squadronID = string.IsNullOrWhiteSpace( eddiSquadronIDText.Text ) ? null : eddiSquadronIDText.Text.Trim();
                ConfigService.Instance.commanderConfiguration = configuration;

                if ( EDDI.Instance.Cmdr != null )
                {
                    EDDI.Instance.Cmdr.squadronid = configuration.squadronID;
                }
            }
        }

        private void eddiSquadronIDText_LostFocus ( object sender, RoutedEventArgs e )
        {
            // Discard invalid results
            var configuration = ConfigService.Instance.commanderConfiguration;
            if ( configuration.squadronID != null )
            {
                if ( configuration.squadronID.Contains( " " ) || configuration.squadronID.Length > 4 )
                {
                    configuration.squadronID = null;
                    squadronSystemDropDown.Text = string.Empty;
                    ConfigService.Instance.commanderConfiguration = configuration;
                }
            }
        }

        private void squadronRankDropDownUpdated ( object sender, SelectionChangedEventArgs e )
        {
            var configuration = ConfigService.Instance.commanderConfiguration;
            string squadronRank = squadronRankDropDown.SelectedItem.ToString();

            if ( configuration.SquadronRank.edname != squadronRank )
            {
                configuration.SquadronRank = SquadronRank.FromName( squadronRank );
                ConfigService.Instance.commanderConfiguration = configuration;

                if ( EDDI.Instance.Cmdr != null )
                {
                    EDDI.Instance.Cmdr.squadronrank = configuration.SquadronRank;
                }
            }
        }

        // Handle changes to the editable squadron system combo box
        private void SquadronSystemDropDown_SelectionChanged ( object sender, SelectionChangedEventArgs e )
        {
            try
            {
                if ( sender is StarSystemComboBox starSystemComboBox )
                {
                    if ( !starSystemComboBox.IsLoaded )
                    { return; }

                    // Update configuration to new home system
                    if ( e.AddedItems.Count == 1 && e.RemovedItems.Count == 0 )
                    {
                        var newSquadronSystem = e.AddedItems[0] as NavWaypoint;
                        commanderMonitor().setSquadronSystem( newSquadronSystem?.systemAddress );

                        // Update squadron faction options for new system
                        ConfigureSquadronFactionOptions();
                    }
                }
            }
            catch ( Exception ex )
            {
                Logging.Error( ex.Message, ex );
            }
        }

        private void squadronFactionDropDownUpdated ( object sender, SelectionChangedEventArgs e )
        {
            var configuration = ConfigService.Instance.commanderConfiguration;
            string squadronFaction = squadronFactionDropDown.SelectedItem?.ToString(); // This can be a localized "None"

            if ( configuration.squadronFaction != squadronFaction )
            {
                configuration.squadronFaction = squadronFaction == Power.None.localizedName ? null : squadronFaction;

                if ( EDDI.Instance.Cmdr != null )
                {
                    EDDI.Instance.Cmdr.squadronfaction = configuration.squadronFaction;
                }

                if ( squadronFaction != Power.None.localizedName )
                {
                    var system = SquadronStarSystem;
                    Faction faction = system?.factions.Find(f => f.name == squadronFaction);

                    if ( faction != null && configuration.SquadronAllegiance != faction.Allegiance )
                    {
                        configuration.SquadronAllegiance = faction.Allegiance;
                        ConfigService.Instance.commanderConfiguration = configuration;

                        if ( EDDI.Instance.Cmdr != null )
                        {
                            EDDI.Instance.Cmdr.squadronallegiance = faction.Allegiance;
                        }

                        squadronPowerDropDown.SelectedItem = Power.None.localizedName;
                        ConfigureSquadronPowerOptions( configuration );
                    }
                }
                else
                {
                    configuration.SquadronAllegiance = Superpower.None;
                    ConfigService.Instance.commanderConfiguration = configuration;

                    if ( EDDI.Instance.Cmdr != null )
                    {
                        EDDI.Instance.Cmdr.squadronallegiance = Superpower.None;
                    }

                    squadronPowerDropDown.SelectedItem = Power.None.localizedName;
                    ConfigureSquadronPowerOptions( configuration );
                }
                ConfigService.Instance.commanderConfiguration = configuration;
            }
        }

        private void squadronPowerDropDownUpdated ( object sender, SelectionChangedEventArgs e )
        {
            var configuration = ConfigService.Instance.commanderConfiguration;
            string squadronPower = squadronPowerDropDown.SelectedItem?.ToString();

            if ( ( configuration.SquadronPower?.localizedName ?? "" ) != squadronPower )
            {
                configuration.SquadronPower = Power.FromName( squadronPower );
                ConfigService.Instance.commanderConfiguration = configuration;

                if ( EDDI.Instance.Cmdr != null )
                {
                    EDDI.Instance.Cmdr.squadronpower = configuration.SquadronPower;
                }
            }
        }

        private void ConfigureSquadronRankOptions ( CommanderConfiguration configuration )
        {
            squadronRankDropDown.DisplayMemberPath = nameof( SquadronRank.localizedName );

            var SquadronRankOptions = new List<SquadronRank>();
            foreach ( var squadronrank in SquadronRank.AllOfThem )
            {
                if ( configuration.squadronName == null && squadronrank != SquadronRank.None )
                {
                    break;
                }
                SquadronRankOptions.Add( squadronrank );
            }

            // Don't sort
            squadronRankDropDown.ItemsSource = SquadronRankOptions;
            squadronRankDropDown.SelectedIndex = SquadronRankOptions.IndexOf( configuration.SquadronRank ?? SquadronRank.None );
        }

        public void ConfigureSquadronFactionOptions ()
        {
            var squadronFactionOptions = new List<string> { Power.None.localizedName };
            if ( SquadronStarSystem != null )
            {
                var starSystemFactions = SquadronStarSystem.factions
                    .OrderBy( s => s.name ).Select( s => s.name );
                squadronFactionOptions.AddRange( starSystemFactions );
            }
            squadronFactionDropDown.ItemsSource = squadronFactionOptions;
        }

        public void ConfigureSquadronPowerOptions ( CommanderConfiguration configuration )
        {
            var SquadronPowerOptions = new List<string>
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

        public CommanderConfiguration resetSquadronRank ( CommanderConfiguration configuration )
        {
            if ( configuration.squadronName == null )
            {
                configuration.SquadronRank = SquadronRank.None;
                squadronRankDropDown.SelectedItem = configuration.SquadronRank.localizedName;
            }
            ConfigureSquadronRankOptions( configuration );

            return configuration;
        }

        #endregion
    }
}
