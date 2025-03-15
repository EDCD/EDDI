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

        private static ConfigurationWindow instance;
        private static readonly object instanceLock = new object();

        public static ConfigurationWindow Instance
        {
            get
            {
                if ( instance == null )
                {
                    lock ( instanceLock )
                    {
                        if ( instance == null )
                        {
                            instance = new ConfigurationWindow();
                        }
                    }
                }

                return instance;
            }
        }

        public ConfigurationWindow ()
        {
            InitializeComponent();

            var configuration = ConfigService.Instance.commanderConfiguration;

            ConfigureCommanderNameOptions( configuration.phoneticName );
            ConfigureCommanderGenderOptions( configuration.gender );

            // Setup home system & station from config file
            ConfigureHomeSystemOptions( configuration.homeSystemName );
            ConfigureHomeStationOptions();

            // Setup squadron system and faction options
            ConfigureSquadronSystemOptions( configuration.squadronSystemName );
            ConfigureSquadronFactionOptions( commanderMonitor().SquadronStarSystem?.factions, configuration.squadronFaction );

            // Setup other squadron options
            ConfigureSquadronNameAndId( configuration.squadronName, configuration.squadronID );
            ConfigureSquadronRankOptions( configuration.SquadronRank );
            ConfigureSquadronPowerOptions( configuration.SquadronAllegiance, configuration.SquadronPower );
        }

        #region Commander Name

        private void ConfigureCommanderNameOptions ( string phoneticName )
        {
            phoneticNameTextBox.Text = phoneticName ?? string.Empty;
        }

        private void phoneticNameChanged ( object sender, TextChangedEventArgs e )
        {
            // Replace any spaces, maintaining the original caret position
            var caretIndex = phoneticNameTextBox.CaretIndex;
            phoneticNameTextBox.Text = phoneticNameTextBox.Text.Replace( " ", "ˈ" );
            phoneticNameTextBox.CaretIndex = Math.Max( caretIndex, phoneticNameTextBox.Text.Length );

            // Update our config file
            if ( phoneticNameTextBox.IsLoaded )
            {
                var configuration = ConfigService.Instance.commanderConfiguration;
                if ( configuration.phoneticName != phoneticNameTextBox.Text )
                {
                    configuration.phoneticName = string.IsNullOrWhiteSpace( phoneticNameTextBox.Text ) ? string.Empty : phoneticNameTextBox.Text.Trim();
                    ConfigService.Instance.commanderConfiguration = configuration;
                }
            }
        }

        private void phoneticNameTestButtonClicked ( object sender, RoutedEventArgs e )
        {
            Application.Current.Dispatcher.Invoke( () =>
            {
                SpeechService.Instance.Say( null, commanderMonitor().Cmdr?.SpokenName(), 0 );
            } );
        }

        private void ipaClicked ( object sender, RoutedEventArgs e )
        {
            var IpaResources = new IpaResourcesWindow();
            IpaResources.Show();
        }

        #endregion

        #region Commander Gender

        private void ConfigureCommanderGenderOptions ( string gender )
        {
            if ( gender == "Female" )
            {
                eddiGenderFemale.IsChecked = true;
            }
            else if ( gender == "Male" )
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
            if ( commanderMonitor().Cmdr != null )
            {
                commanderMonitor().Cmdr.gender = "Female";
            }
        }

        private void isNeitherGender_Checked ( object sender, RoutedEventArgs e )
        {
            var configuration = ConfigService.Instance.commanderConfiguration;
            configuration.gender = "Neither";
            ConfigService.Instance.commanderConfiguration = configuration;
            if ( commanderMonitor().Cmdr != null )
            {
                commanderMonitor().Cmdr.gender = "Neither";
            }
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
                        commanderMonitor().setHomeSystem( newHomeSystem?.systemAddress );
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
                        commanderMonitor().setHomeStation( selectedStation.marketId );
                        ConfigService.Instance.commanderConfiguration = configuration;
                    }
                }
            }
        }

        #endregion

        #region Squadron Name and ID

        private void ConfigureSquadronNameAndId ( string squadronName, string squadronID )
        {
            // Setup squadron home system from config file
            eddiSquadronNameText.Text = squadronName ?? string.Empty;
            eddiSquadronIDText.Text = squadronID ?? string.Empty;
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
                }
                configuration = resetSquadronRank( configuration );
                ConfigService.Instance.commanderConfiguration = configuration;

                if ( commanderMonitor().Cmdr != null )
                {
                    commanderMonitor().Cmdr.squadronname = configuration.squadronName;
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
                if ( commanderMonitor().Cmdr != null )
                {
                    commanderMonitor().Cmdr.squadronname = string.Empty;
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

                if ( commanderMonitor().Cmdr != null )
                {
                    commanderMonitor().Cmdr.squadronid = configuration.squadronID;
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
                    ConfigService.Instance.commanderConfiguration = configuration;
                }
            }
        }

        #endregion

        #region Squadron Rank

        private void ConfigureSquadronRankOptions ( SquadronRank SquadronRank = null )
        {
            squadronRankDropDown.DisplayMemberPath = nameof( SquadronRank.localizedName );
            var SquadronRankOptions = SquadronRank.AllOfThem
                .OrderBy( r => r.rank ).ToList();
            squadronRankDropDown.ItemsSource = SquadronRankOptions;
            squadronRankDropDown.SelectedItem = SquadronRank ?? SquadronRank.None;
        }

        private void squadronRankDropDownUpdated ( object sender, SelectionChangedEventArgs e )
        {
            var configuration = ConfigService.Instance.commanderConfiguration;
            var squadronRank = squadronRankDropDown.SelectedItem.ToString();

            if ( configuration.SquadronRank.edname != squadronRank )
            {
                configuration.SquadronRank = SquadronRank.FromName( squadronRank );
                ConfigService.Instance.commanderConfiguration = configuration;

                if ( commanderMonitor().Cmdr != null )
                {
                    commanderMonitor().Cmdr.squadronrank = configuration.SquadronRank;
                }
            }
        }

        public CommanderConfiguration resetSquadronRank ( CommanderConfiguration configuration )
        {
            configuration.SquadronRank = SquadronRank.None;
            ConfigureSquadronRankOptions( SquadronRank.None );
            return configuration;
        }

        #endregion

        #region Squadron System

        internal void ConfigureSquadronSystemOptions ( string newSquadronSystemName )
        {
            squadronSystemDropDown.Text = newSquadronSystemName ?? string.Empty;
        }

        // Handle changes to the editable squadron system combo box
        private void SquadronSystemDropDown_SelectionChanged ( object sender, SelectionChangedEventArgs e )
        {
            try
            {
                if ( sender is StarSystemComboBox starSystemComboBox )
                {
                    if ( !starSystemComboBox.IsLoaded ) { return; }

                    // Update configuration to new home system
                    if ( e.AddedItems.Count == 1 && e.RemovedItems.Count == 0 )
                    {
                        var newSquadronSystem = e.AddedItems[0] as NavWaypoint;
                        commanderMonitor().setSquadronSystem( newSquadronSystem?.systemAddress, null );
                        ConfigureSquadronFactionOptions( commanderMonitor().SquadronStarSystem?.factions,
                            ConfigService.Instance.commanderConfiguration.squadronFaction );
                    }
                }
            }
            catch ( Exception ex )
            {
                Logging.Error( ex.Message, ex );
            }
        }

        #endregion

        #region Squadron Faction

        public void ConfigureSquadronFactionOptions ( List<Faction> factions, string squadronFaction )
        {
            squadronFactionDropDown.DisplayMemberPath = nameof(Faction.name);
            var noneFaction = new Faction { name = Power.None.localizedName };
            var squadronFactionOptions = ( factions ?? new List<Faction>() )
                .OrderBy( s => s.name )
                .Prepend( noneFaction )
                .ToHashSet();
            squadronFactionDropDown.ItemsSource = squadronFactionOptions;

            var selectedFaction = squadronFactionOptions.FirstOrDefault( f => 
                f.name.Equals( squadronFaction, StringComparison.InvariantCultureIgnoreCase ) );
            squadronFactionDropDown.SelectedItem = selectedFaction ?? noneFaction;
        }

        private void squadronFactionDropDown_SelectionChanged ( object sender, SelectionChangedEventArgs e )
        {
            if ( !squadronSystemDropDown.IsLoaded ) {return;}

            var configuration = ConfigService.Instance.commanderConfiguration;
            var noneValue = Power.None.localizedName;

            foreach ( var obj in e.AddedItems )
            {
                if ( obj is Faction selectedFaction )
                {
                    if ( configuration.squadronFaction == selectedFaction.name )
                    {
                        // Squadron faction is unchanged
                        continue;
                    }

                    if ( selectedFaction.name != noneValue )
                    {
                        // A faction is selected
                        configuration.squadronFaction = selectedFaction.name;
                        if ( commanderMonitor().Cmdr != null )
                        {
                            commanderMonitor().Cmdr.squadronfaction = selectedFaction.name;
                        }

                        if ( configuration.SquadronAllegiance != selectedFaction.Allegiance )
                        {
                            configuration.SquadronAllegiance = selectedFaction.Allegiance;
                            if ( commanderMonitor().Cmdr != null )
                            {
                                commanderMonitor().Cmdr.squadronallegiance = selectedFaction.Allegiance;
                            }
                            ConfigureSquadronPowerOptions( configuration.SquadronAllegiance, Power.None );
                        }
                    }
                    else
                    {
                        // The 'None' faction is selected
                        configuration.SquadronAllegiance = Superpower.None;
                        configuration.squadronFaction = null;
                        ConfigService.Instance.commanderConfiguration = configuration;

                        if ( commanderMonitor().Cmdr != null )
                        {
                            commanderMonitor().Cmdr.squadronallegiance = Superpower.None;
                            commanderMonitor().Cmdr.squadronfaction = null;
                        }

                        ConfigureSquadronPowerOptions( configuration.SquadronAllegiance, Power.None );
                    }
                    ConfigService.Instance.commanderConfiguration = configuration;
                }
            }
        }

        #endregion

        #region Squadron Power

        public void ConfigureSquadronPowerOptions ( Superpower SquadronAllegiance, Power SquadronPower )
        {
            squadronPowerDropDown.DisplayMemberPath = nameof(Superpower.localizedName);

            var squadronPowerOptions = SquadronAllegiance is null
                ? Power.AllOfThem
                    .Except( new[] { Power.None } )
                    .OrderBy( p => p.localizedName )
                    .Prepend( Power.None )
                : Power.AllOfThem
                    .Except( new[] { Power.None } )
                    .Where( p => p.Allegiance == SquadronAllegiance )
                    .OrderBy( p => p.localizedName )
                    .Prepend( Power.None );
            squadronPowerDropDown.ItemsSource = squadronPowerOptions;

            squadronPowerDropDown.SelectedItem = SquadronPower ?? Power.None;
        }

        private void squadronPowerDropDown_SelectionChanged ( object sender, SelectionChangedEventArgs e )
        {
            foreach ( var obj in e.AddedItems )
            {
                if ( obj is Power selectedPower )
                {
                    var configuration = ConfigService.Instance.commanderConfiguration;
                    if ( configuration.SquadronPower != selectedPower )
                    {
                        configuration.SquadronPower = selectedPower;
                        ConfigService.Instance.commanderConfiguration = configuration;

                        if ( commanderMonitor().Cmdr != null )
                        {
                            commanderMonitor().Cmdr.squadronpower = configuration.SquadronPower;
                        }
                    }
                }
            }
        }

        #endregion
    }
}
