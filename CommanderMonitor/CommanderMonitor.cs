using CommanderMonitor;
using EddiConfigService;
using EddiConfigService.Configurations;
using EddiCore;
using EddiDataDefinitions;
using EddiEvents;
using JetBrains.Annotations;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Utilities;

[assembly: InternalsVisibleTo( "Tests" )]
namespace EddiCommanderMonitor
{
    [UsedImplicitly]
    public class CommanderMonitor : IEddiMonitor, INotifyPropertyChanged
    {
        private static readonly object commanderLock = new object();

        [NotNull]
        public Commander Cmdr // Also includes information from the configuration and companion app service
        {
            get => _cmdr ?? ReadCommander();
            private set
            {
                void childPropertyChangedHandler ( object sender, PropertyChangedEventArgs e )
                {
                    OnPropertyChanged();
                }
                if ( _cmdr != null )
                { _cmdr.PropertyChanged -= childPropertyChangedHandler; }
                value.PropertyChanged += childPropertyChangedHandler;
                _cmdr = value;
                OnPropertyChanged();
            }
        }
        [CanBeNull]
        private Commander _cmdr;

        [CanBeNull]
        private StarSystem SquadronStarSystem // May be null when the commander hasn't set a squadron star system
        {
            get => _squadronStarSystem;
            set
            {
                void childPropertyChangedHandler ( object sender, PropertyChangedEventArgs e )
                {
                    OnPropertyChanged();
                }
                if ( _squadronStarSystem != null ) { _squadronStarSystem.PropertyChanged -= childPropertyChangedHandler; }
                if ( value != null ) { value.PropertyChanged += childPropertyChangedHandler; }
                _squadronStarSystem = value;
                OnPropertyChanged();
                OnPropertyChanged( nameof( SquadronFactions) );
                OnPropertyChanged( nameof( SelectedSquadronFaction ) );
            }
        }
        private StarSystem _squadronStarSystem;

        #region Cmdr Gender View Model

        public Gender SelectedGender
        {
            get => Cmdr.Gender;
            set
            {
                Cmdr.Gender = value;
                WriteCommander();
            }
        }

        public class GenderOption
        {
            public Gender Gender { get; set; }
            public string DisplayName { get; set; }
        }

        public static ObservableCollection<GenderOption> GenderOptions { get; set; } = new ObservableCollection<GenderOption>
        {
            new GenderOption { Gender = Gender.Male, DisplayName = EddiCommanderMonitor.Properties.Resources.tab_commander_gender_m },
            new GenderOption { Gender = Gender.Female, DisplayName = EddiCommanderMonitor.Properties.Resources.tab_commander_gender_f },
            new GenderOption { Gender = Gender.Neither, DisplayName = EddiCommanderMonitor.Properties.Resources.tab_commander_gender_n }
        };

        #endregion region

        #region Squadron View Model

        public NavWaypoint SquadronSystemWaypoint
        {
            get => SquadronStarSystem == null ? null : new NavWaypoint(SquadronStarSystem);
            set
            {
                if ( value != null )
                {
                    Task.Run( async () =>
                    {
                        SquadronStarSystem = await EDDI.Instance.DataProvider
                            .GetOrFetchStarSystemAsync( value.systemAddress ).ConfigureAwait( false );
                        lock ( commanderLock )
                        {
                            Cmdr.squadronSystemName = SquadronSystemWaypoint?.systemName;
                            Cmdr.squadronSystemAddress = SquadronSystemWaypoint?.systemAddress;
                        }
                        WriteCommander();
                    } ).ConfigureAwait( false );
                }
                else
                {
                    lock ( commanderLock )
                    {
                        Cmdr.squadronSystemName = SquadronSystemWaypoint?.systemName;
                        Cmdr.squadronSystemAddress = SquadronSystemWaypoint?.systemAddress;
                    }
                    WriteCommander();
                }
                OnPropertyChanged();
            }
        }

        public ObservableCollection<Faction> SquadronFactions => new ObservableCollection<Faction>( ( SquadronStarSystem?.factions ?? new List<Faction>() )
            .OrderBy( f => f.name )
            .Prepend( new Faction { name = Power.None.localizedName })
            .ToHashSet() );

        public Faction SelectedSquadronFaction
        {
            get => SquadronFactions.FirstOrDefault(f => f.name.Equals( Cmdr.squadronfaction, StringComparison.OrdinalIgnoreCase ) );
            set
            {
                lock ( commanderLock )
                {
                    Cmdr.squadronfaction = value?.name == Power.None.localizedName ? null : value?.name;
                    Cmdr.squadronallegiance = value?.name == Power.None.localizedName ? null : value?.Allegiance;
                }

                WriteCommander();
            }
        }

        public ObservableCollection<Power> SquadronPowers => new ObservableCollection<Power>( Power.AllOfThem
            .Except( new [] { Power.None } )
            .OrderBy( p => p.localizedName )
            .Prepend( Power.None )
            .ToHashSet() );

        public Power SelectedSquadronPower
        {
            get => SquadronPowers.FirstOrDefault( p => p == Cmdr.squadronpower );
            set
            {
                lock ( commanderLock )
                {
                    Cmdr.squadronpower = value == Power.None ? null : value;
                }
                WriteCommander();
            }
        }

        #endregion

        private DateTime JournalTimeStamp { get; set; } = DateTime.MinValue;

        private DateTime updatedAt { get; set; } = DateTime.MinValue;

        public string MonitorName () => "Commander monitor";

        public string LocalizedMonitorName () => Properties.Resources.MonitorName;

        public string MonitorDescription () => Properties.Resources.MonitorDescription;

        public bool IsRequired () => true;

        public bool NeedsStart () => false;

        public CommanderMonitor ()
        {
            Cmdr = ReadCommander();
            Logging.Info( $"Initialized {MonitorName()}" );
        }

        public void Start ()
        { }

        public void Stop ()
        { }

        public void Reload ()
        {
            ReloadCommanderMonitor();
        }

        private void ReloadCommanderMonitor ()
        {
            lock ( commanderLock )
            {
                Cmdr = ReadCommander();
            }
            Logging.Info( $"Reloaded {MonitorName()}" );
        }

        public IDictionary<string, Tuple<Type, object>> GetVariables ()
        {
            return new Dictionary<string, Tuple<Type, object>>
            {
                { "cmdr", new Tuple<Type, object>( typeof(Commander), Cmdr ) },
                { "squadronsystem", new Tuple<Type, object>( typeof(StarSystem), SquadronStarSystem ) }
            };
        }

        public UserControl ConfigurationTabItem () => new ConfigurationWindow();

        public void PreHandle ( Event @event )
        {
            if ( @event is CarrierBankTransferEvent carrierBankTransferEvent )
            {
                handleCarrierBankTransferEvent( carrierBankTransferEvent );
            }
            else if ( @event is CarrierJumpedEvent carrierJumpedEvent )
            {
                handleCarrierJumpedEvent( carrierJumpedEvent );
            }
            else if ( @event is CommanderLoadingEvent commanderLoadingEvent )
            {
                handleCommanderLoadingEvent( commanderLoadingEvent );
            }
            else if ( @event is CommanderPromotionEvent commanderPromotionEvent )
            {
                handleCommanderPromotionEvent( commanderPromotionEvent );
            }
            else if ( @event is CommanderRatingsEvent commanderRatingsEvent )
            {
                handleCommanderRatingsEvent( commanderRatingsEvent );
            }
            else if ( @event is JumpedEvent jumpedEvent )
            {
                handleJumpedEvent( jumpedEvent );
            }
            else if ( @event is LocationEvent locationEvent )
            {
                handleLocationEvent( locationEvent );
            }
            else if ( @event is PowerJoinedEvent powerJoinedEvent )
            {
                handlePowerJoinedEvent( powerJoinedEvent );
            }
            else if ( @event is PowerLeftEvent powerLeftEvent )
            {
                handlePowerLeftEvent( powerLeftEvent );
            }
            else if ( @event is PowerMeritsEvent powerMeritsEvent )
            {
                handlePowerMeritsEvent( powerMeritsEvent );
            }
            else if ( @event is PowerplayEvent powerplayEvent )
            {
                handlePowerPlayEvent( powerplayEvent );
            }
            else if ( @event is PowerRankEvent powerRankEvent )
            {
                handlePowerRankEvent( powerRankEvent );
            }
            else if ( @event is PowerVoucherReceivedEvent powerVoucherReceivedEvent )
            {
                handlePowerVoucherReceivedEvent( powerVoucherReceivedEvent );
            }
            else if ( @event is SquadronRankEvent squadronRankEvent )
            {
                handleSquadronRankEvent( squadronRankEvent );
            }
            else if ( @event is SquadronStartupEvent squadronStartupEvent )
            {
                handleSquadronStartupEvent( squadronStartupEvent );
            }
            else if ( @event is SquadronStatusEvent squadronStatusEvent )
            {
                handleSquadronStatusEvent( squadronStatusEvent );
            }

            JournalTimeStamp = @event.timestamp;
        }

        private void handleCarrierBankTransferEvent ( CarrierBankTransferEvent @event )
        {
            if ( @event.timestamp >= updatedAt  )
            {
                lock ( commanderLock )
                {
                    Cmdr.credits = @event.cmdrBalance;
                }
                WriteCommander();
            }
        }

        private void handleCarrierJumpedEvent ( CarrierJumpedEvent @event )
        {
            if ( @event.controllingsystemfaction != null )
            {
                SetCommanderTitle( @event.controllingsystemfaction.Allegiance );
            }
            if ( ( @event.docked || @event.onFoot ) && @event.factions.Any() && EDDI.Instance.CurrentStarSystem != null )
            {
                if ( @event.timestamp >= updatedAt && 
                     TryUpdateSquadronHomeSystem( @event.systemAddress, @event.factions ) )
                {
                    WriteCommander();
                }
            }
        }

        private void handleCommanderLoadingEvent ( CommanderLoadingEvent @event )
        {
            // We need to reload the EDSM responder if the commander name has changed
            if ( ConfigService.Instance.commanderConfiguration.commanderName != @event.name )
            {
                EDDI.Instance.ObtainResponder( "EDSM Responder" ).Reload();
            }
        }

        private void handleCommanderPromotionEvent ( CommanderPromotionEvent @event )
        {
            if ( @event.timestamp >= updatedAt )
            {
                // Capture commander ratings and add them to the commander object
                lock ( commanderLock )
                {
                    if ( @event.ratingObject is CombatRating combatRating )
                    {
                        // There is a bug with the journal where it reports superpower increases in rank as combat increases
                        // Hence we check to see if this is a real event by comparing our known combat rating to the promoted rating
                        if ( Cmdr.combatrating == null || @event.rank != Cmdr.combatrating.localizedName )
                        {
                            // Real event. 
                            Cmdr.combatrating = combatRating;
                        }
                        // False event
                    }
                    else if ( @event.ratingObject is CQCRating cqcRating )
                    {
                        Cmdr.cqcrating = cqcRating;
                    }
                    else if ( @event.ratingObject is EmpireRating empireRating )
                    {
                        Cmdr.empirerating = empireRating;
                    }
                    else if ( @event.ratingObject is ExplorationRating explorationRating )
                    {
                        Cmdr.explorationrating = explorationRating;
                    }
                    else if ( @event.ratingObject is ExobiologistRating exobiologistRating )
                    {
                        Cmdr.exobiologistrating = exobiologistRating;
                    }
                    else if ( @event.ratingObject is FederationRating federationRating )
                    {
                        Cmdr.federationrating = federationRating;
                    }
                    else if ( @event.ratingObject is MercenaryRating mercenaryRating )
                    {
                        Cmdr.mercenaryrating = mercenaryRating;
                    }
                    else if ( @event.ratingObject is TradeRating tradeRating )
                    {
                        // Capture commander ratings and add them to the commander object
                        Cmdr.traderating = tradeRating;
                    }
                }

                WriteCommander();
            }
        }

        private void handleCommanderRatingsEvent ( CommanderRatingsEvent @event )
        {
            if ( @event.timestamp >= updatedAt )
            {
                lock ( commanderLock )
                {
                    Cmdr.combatrating = @event.combat;
                    Cmdr.traderating = @event.trade;
                    Cmdr.explorationrating = @event.exploration;
                    Cmdr.cqcrating = @event.cqc;
                    Cmdr.empirerating = @event.empire;
                    Cmdr.federationrating = @event.federation;
                }

                WriteCommander();
            }
        }

        private void handleJumpedEvent ( JumpedEvent @event )
        {
            if ( @event.controllingfaction != null )
            {
                SetCommanderTitle( @event.controllingfaction?.Allegiance );
            }
            if ( @event.factions.Any() && EDDI.Instance.CurrentStarSystem != null )
            {
                if ( @event.timestamp >= updatedAt &&
                     TryUpdateSquadronHomeSystem( @event.systemAddress, @event.factions ) )
                {
                    WriteCommander();
                }
            }
        }

        private void handleLocationEvent ( LocationEvent @event )
        {
            if ( @event.controllingsystemfaction != null )
            {
                SetCommanderTitle( @event.controllingsystemfaction.Allegiance );
            }
            if ( @event.factions.Any() && EDDI.Instance.CurrentStarSystem != null )
            {
                if ( @event.timestamp >= updatedAt &&
                     TryUpdateSquadronHomeSystem( @event.systemAddress, @event.factions ) )
                {
                    WriteCommander();
                }
            }
        }

        private void handlePowerJoinedEvent ( PowerJoinedEvent @event )
        {
            if ( @event.timestamp >= updatedAt )
            {
                lock ( commanderLock )
                {
                    Cmdr.Power = @event.Power;
                    Cmdr.powermerits = 0;
                    Cmdr.powerrating = 0;
                }

                WriteCommander();
            }
        }

        private void handlePowerLeftEvent ( PowerLeftEvent @event )
        {
            if ( @event.timestamp >= updatedAt )
            {
                lock ( commanderLock )
                {
                    Cmdr.Power = Power.None;
                    Cmdr.powermerits = null;
                    Cmdr.powerrating = 0;
                }

                WriteCommander();
            }
        }

        private void handlePowerMeritsEvent ( PowerMeritsEvent @event )
        {
            if ( @event.timestamp >= updatedAt )
            {
                lock ( commanderLock )
                {
                    Cmdr.Power = @event.Power;
                    Cmdr.powermerits = @event.total;
                }

                WriteCommander();
            }
        }

        private void handlePowerPlayEvent ( PowerplayEvent @event )
        {
            if ( @event.timestamp >= updatedAt )
            {
                lock ( commanderLock )
                {
                    Cmdr.Power = @event.Power;
                    Cmdr.powerrating = @event.rank;
                    Cmdr.powermerits = @event.merits;
                }

                WriteCommander();
            }
        }

        private void handlePowerRankEvent ( PowerRankEvent @event )
        {
            if ( @event.timestamp >= updatedAt )
            {
                lock ( commanderLock )
                {
                    Cmdr.Power = @event.Power;
                    Cmdr.powerrating = @event.rank;
                }

                WriteCommander();
            }
        }

        private void handlePowerVoucherReceivedEvent ( PowerVoucherReceivedEvent @event )
        {
            if ( @event.timestamp >= updatedAt )
            {
                lock ( commanderLock )
                {
                    Cmdr.Power = @event.Power;
                }
                
                WriteCommander();
            }
        }

        private void handleSquadronRankEvent ( SquadronRankEvent @event )
        {
            if ( @event.timestamp >= updatedAt )
            {
                // Update the commander object, if it exists
                lock ( commanderLock )
                {
                    Cmdr.squadronname = @event.name;
                    Cmdr.squadronrank = @event.newrank;
                }

                WriteCommander();
            }
        }

        private void handleSquadronStartupEvent ( SquadronStartupEvent @event )
        {
            if ( @event.timestamp >= updatedAt )
            {
                // Update the commander object, if it exists
                lock ( commanderLock )
                {
                    Cmdr.squadronname = @event.name;
                    Cmdr.squadronrank = @event.rank;
                }

                WriteCommander();
            }
        }

        private void handleSquadronStatusEvent ( SquadronStatusEvent @event )
        {
            switch ( @event.status )
            {
                case "created":
                    {
                        // Update the commander object, if it exists
                        lock ( commanderLock )
                        {
                            Cmdr.squadronname = @event.name;
                        }
                        break;
                    }
                case "joined":
                    {
                        // Update the commander object, if it exists
                        lock ( commanderLock )
                        {
                            Cmdr.squadronname = @event.name;
                        }
                        break;
                    }
                case "disbanded":
                case "kicked":
                case "left":
                    {
                        // Update the commander object, if it exists
                        lock ( commanderLock )
                        {
                            Cmdr.squadronname = null;
                            Cmdr.squadronrank = null;
                        }
                        break;
                    }
            }
            
            WriteCommander();
        }

        public async Task setHomeSystemAsync ( ulong? newSystemAddress )
        {
            StarSystem newSystem = null;
            if ( newSystemAddress != null )
            {
                newSystem = await EDDI.Instance.DataProvider.GetOrFetchStarSystemAsync( (ulong)newSystemAddress ).ConfigureAwait(false);
            }

            //Ignore null & empty systems
            if ( newSystem?.bodies?.Count > 0 )
            {
                if ( newSystem.systemAddress != EDDI.Instance.HomeStarSystem?.systemAddress )
                {
                    EDDI.Instance.HomeStarSystem = newSystem;
                    Logging.Debug( "Home star system is " + EDDI.Instance.HomeStarSystem.systemname );

                    var configuration = ConfigService.Instance.commanderConfiguration;
                    if ( newSystem.systemAddress != configuration.homeSystemAddress )
                    {
                        configuration.homeSystemName = newSystem.systemname;
                        configuration.homeSystemAddress = newSystem.systemAddress;
                        configuration.homeStationName = null;
                        configuration.homeStationMarketID = null;
                        ConfigService.Instance.commanderConfiguration = configuration;
                    }
                }
            }
            else
            {
                EDDI.Instance.HomeStarSystem = null;
            }

            Application.Current?.Dispatcher?.Invoke( () =>
            {
                if ( Application.Current?.MainWindow != null )
                {
                    ConfigurationWindow.Instance.ConfigureHomeSystemOptions( newSystem?.systemname );
                }
            } );
        }

        public void setHomeStation ( long? newMarketId )
        {
            if ( newMarketId != null && EDDI.Instance.HomeStarSystem?.stations != null )
            {
                foreach ( var station in EDDI.Instance.HomeStarSystem.stations )
                {
                    if ( station.marketId == newMarketId )
                    {
                        EDDI.Instance.HomeStation = station;
                        
                        var configuration = ConfigService.Instance.commanderConfiguration;
                        if ( newMarketId != configuration.homeStationMarketID )
                        {
                            configuration.homeStationName = station.name;
                            configuration.homeStationMarketID = station.marketId;
                            ConfigService.Instance.commanderConfiguration = configuration;
                        }

                        Logging.Debug( "Home station is " + EDDI.Instance.HomeStation.name );

                        Application.Current?.Dispatcher?.Invoke( () =>
                        {
                            if ( Application.Current?.MainWindow != null )
                            {
                                ConfigurationWindow.Instance.ConfigureHomeStationOptions();
                            }
                        } );
                    }
                }
            }
        }

        private bool TryUpdateSquadronHomeSystem ( ulong currentSystemAddress, List<Faction> systemFactions )
        {
            bool update = false;

            // Check if current system is inhabited by or HQ for squadron faction
            var squadronFaction = systemFactions.FirstOrDefault( f =>
            {
                return f.squadronfaction ||
                       ( f.presences.FirstOrDefault( p => p.systemAddress == currentSystemAddress )?.squadronhomesystem ??
                         false );
            } );

            if ( squadronFaction != null )
            {
                lock ( commanderLock )
                {
                    //Update the squadron faction, if changed
                    Cmdr.squadronfaction = squadronFaction.name;

                    // Update system, allegiance, & power when in squadron home system
                    if ( EDDI.Instance.CurrentStarSystem?.systemAddress == currentSystemAddress )
                    {
                        // Update the squadron system data, if changed
                        SquadronSystemWaypoint = new NavWaypoint( EDDI.Instance.CurrentStarSystem );
                        Cmdr.squadronSystemName = EDDI.Instance.CurrentStarSystem.systemname;
                        Cmdr.squadronSystemAddress = EDDI.Instance.CurrentStarSystem.systemAddress;
                        Cmdr.squadronfaction = squadronFaction.name;

                        // Update the squadron allegiance according to the faction info from the journal
                        Cmdr.squadronallegiance =
                            EDDI.Instance.CurrentStarSystem?.Faction?.Allegiance ?? Superpower.None;

                        // Update the squadron power to match the HQ system's controlling power if it has not been previously set
                        Cmdr.squadronpower = EDDI.Instance.CurrentStarSystem?.Power ?? Power.None;
                    }
                }

                update = true;
            }

            return update;
        }

        public void PostHandle ( Event @event )
        { }

        public void HandleStatus ( Status status )
        {
            lock ( commanderLock )
            {
                Cmdr.credits = status.credit_balance;
            }
        }

        public void HandleProfile ( JObject profile )
        {
            // Update our commander object
            var frontierApiProfile = FrontierApiProfile.FromJson( profile );
            var updatedCmdr = Commander.FromFrontierApiCmdr( Cmdr, frontierApiProfile.Cmdr,
                frontierApiProfile.timestamp, JournalTimeStamp, out var cmdrMatches );

            // Stop if the commander returned from the profile does not match our expected commander name
            if ( !cmdrMatches )
            {
                Logging.Debug( "Skipping profile update - Frontier API commander information doesn't match journal information" );
                return;
            }

            Logging.Debug( "Commander information updated from Frontier API; updating local copy" );
            lock ( commanderLock )
            {
                Cmdr = updatedCmdr;
            }
        }

        /// <summary>Work out the title for the commander in the current system</summary>
        private const int minEmpireRankForTitle = 3;
        private const int minFederationRankForTitle = 1;
        private void SetCommanderTitle ( Superpower controllingFactionAllegiance )
        {
            lock ( commanderLock )
            {
                Cmdr.title = EddiCore.Properties.Resources.Commander;
                if ( controllingFactionAllegiance != null )
                {
                    if ( controllingFactionAllegiance.invariantName == Superpower.Federation.invariantName &&
                         Cmdr.federationrating != null && Cmdr.federationrating.rank > minFederationRankForTitle )
                    {
                        Cmdr.title = Cmdr.federationrating.localizedName;
                    }
                    else if ( controllingFactionAllegiance.invariantName == Superpower.Empire.invariantName &&
                              Cmdr.empirerating != null && Cmdr.empirerating.rank > minEmpireRankForTitle )
                    {
                        Cmdr.title = Cmdr.empirerating.maleRank.localizedName;
                    }
                }
            }
        }

        private Commander ReadCommander ( CommanderConfiguration configuration = null )
        {
            // Obtain current commander from our configuration
            configuration = configuration ?? ConfigService.Instance.commanderConfiguration;
            var commander = new Commander()
            {
                name = configuration.commanderName,
                credits = configuration.credits,
                friends = configuration.friends,
                EDID = configuration.frontierID,
                gender = configuration.gender,
                phoneticName = configuration.phoneticName,

                // Power information
                Power = configuration.Power,
                powermerits = configuration.powerMerits,
                powerrating = configuration.powerRank,

                // Squadron information
                squadronname = configuration.squadronName,
                squadrontag = configuration.squadronTag,
                squadronrank = configuration.SquadronRank,
                squadronSystemName = configuration.squadronSystemName,
                squadronSystemAddress = configuration.squadronSystemAddress
            };

            // Fetch squadron star system objects
            try
            {
                Task.Run( async () =>
                {
                    if ( configuration.squadronSystemAddress is null &&
                         !string.IsNullOrEmpty( configuration.squadronSystemName ) )
                    {
                        // Legacy configurations may not have system address values stored. Fix that here.
                        SquadronSystemWaypoint = await EDDI.Instance.DataProvider
                            .GetOrFetchSystemWaypointAsync( configuration.squadronSystemName );
                    }
                    else if ( configuration.squadronSystemAddress != null )
                    {
                        SquadronSystemWaypoint = new NavWaypoint( await EDDI.Instance.DataProvider
                            .GetOrFetchQuickStarSystemAsync( (ulong)configuration.squadronSystemAddress ) );
                    }
                } );
            }
            catch ( OperationCanceledException )
            {
                // Nothing to do here.
            }

            commander.squadronfaction = configuration.squadronFaction;
            commander.squadronallegiance = configuration.SquadronAllegiance ?? Superpower.None;
            commander.squadronpower = configuration.SquadronPower ?? Power.None;

            updatedAt = configuration.updatedat;

            return commander;
        }

        internal void WriteCommander ()
        {
            lock ( commanderLock )
            {
                // Write our current commander configuration
                var configuration = ConfigService.Instance.commanderConfiguration;

                configuration.commanderName = Cmdr.name;
                configuration.credits = Cmdr.credits;
                configuration.friends = Cmdr.friends;
                configuration.frontierID = Cmdr.EDID;
                configuration.gender = Cmdr.gender;
                configuration.phoneticName = Cmdr.phoneticName;

                // Write power information
                configuration.Power = Cmdr.Power;
                configuration.powerMerits = Cmdr.powermerits;
                configuration.powerRank = Cmdr.powerrating;

                // Write squadron information
                configuration.squadronName = Cmdr.squadronname;
                configuration.squadronTag = Cmdr.squadrontag;
                configuration.SquadronRank = Cmdr.squadronrank;
                configuration.squadronFaction = Cmdr.squadronfaction;
                configuration.squadronAllegiance =
                    Cmdr.squadronallegiance is null || Cmdr.squadronallegiance == Superpower.None
                        ? null
                        : Cmdr.squadronallegiance.edname;
                configuration.SquadronPower = Cmdr.squadronpower is null || Cmdr.squadronpower == Power.None
                    ? null
                    : Cmdr.squadronpower;

                // Write squadron star system information
                configuration.squadronSystemName = Cmdr.squadronSystemName;
                configuration.squadronSystemAddress = Cmdr.squadronSystemAddress;

                // Write home system information
                configuration.homeSystemName = EDDI.Instance.HomeStarSystem?.systemname;
                configuration.homeSystemAddress = EDDI.Instance.HomeStarSystem?.systemAddress;
                configuration.homeSystemX = EDDI.Instance.HomeStarSystem?.x;
                configuration.homeSystemY = EDDI.Instance.HomeStarSystem?.y;
                configuration.homeSystemZ = EDDI.Instance.HomeStarSystem?.z;

                configuration.updatedat = updatedAt;
                ConfigService.Instance.commanderConfiguration = configuration;
            }
        }
        
        #region Implement INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged ( [CallerMemberName] string propertyName = null )
        {
            PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( propertyName ) );
        }        

        #endregion
    }
}
