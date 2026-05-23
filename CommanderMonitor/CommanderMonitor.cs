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
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using Utilities;

[assembly: InternalsVisibleTo( "Tests" )]
namespace EddiCommanderMonitor
{
    [UsedImplicitly]
    public class CommanderMonitor : IEddiMonitor, INotifyPropertyChanged
    {
        private static readonly object commanderLock = new();
        private readonly CancellationTokenSource cts = new();
        
        #region Monitored Variables

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
        private Commander _cmdr;

        [UsedImplicitly]
        public string PhoneticName
        {
            get => Cmdr.phoneticName;
            set
            {
                if ( Cmdr.phoneticName != value )
                {
                    Cmdr.phoneticName = value;
                    WriteCommander();
                    OnPropertyChanged();
                }
            }
        }

        [UsedImplicitly]
        public Gender SelectedGender
        {
            get => Cmdr.Gender;
            set
            {
                if ( Cmdr.Gender != value )
                {
                    Cmdr.Gender = value;
                    WriteCommander();
                    OnPropertyChanged();
                }
            }
        }

        public class GenderOption
        {
            public Gender Gender { get; set; }
            public string DisplayName { get; set; }
        }

        [UsedImplicitly]
        public static ObservableCollection<GenderOption> GenderOptions { get; set; } =
        [
            new() { Gender = Gender.Male, DisplayName = Properties.Resources.tab_commander_gender_m },
            new() { Gender = Gender.Female, DisplayName = Properties.Resources.tab_commander_gender_f },
            new() { Gender = Gender.Neither, DisplayName = Properties.Resources.tab_commander_gender_n }
        ];

        [CanBeNull]
        public StarSystem HomeStarSystem // May be null when the commander hasn't set a home star system
        {
            get => _homeStarSystem;
            set
            {
                _homeStarSystem = value;
                OnPropertyChanged();
            }
        }

        private StarSystem _homeStarSystem;

        public string HomeSystemName
        {
            get => _homeStarSystem?.systemname;
            set
            {
                if ( value != _homeStarSystem?.systemname )
                {
                    if ( string.IsNullOrEmpty( value ) )
                    {
                        HomeStarSystem = null;
                        return;
                    }                    
                    
                    if ( _isFetchingHomeSystem ) { return; }
                    FetchHomeSystemAsync( value ).SafeFireAndForget( ex => Logging.Error( ex.Message, ex ) );
                }
            }
        }

        private async Task FetchHomeSystemAsync ( string systemName )
        {
            _isFetchingHomeSystem = true;
            try
            {
                var fetchedSystem = await Task.Run( () => EDDI.Instance.DataProvider
                    .GetOrFetchStarSystemAsync( systemName ) ).ConfigureAwait( true );

                if ( fetchedSystem != null )
                {
                    HomeStarSystem = fetchedSystem;
                    UpdateHomeStationOptions();
                    lock ( commanderLock )
                    {
                        Cmdr.homeSystemName = HomeStarSystem?.systemname;
                        Cmdr.homeSystemAddress = HomeStarSystem?.systemAddress;
                        Cmdr.homeSystemX = HomeStarSystem?.x;
                        Cmdr.homeSystemY = HomeStarSystem?.y;
                        Cmdr.homeSystemZ = HomeStarSystem?.z;
                    }
                    WriteCommander();
                    OnPropertyChanged( nameof(HomeSystemName) );
                }
            }
            finally
            {
                _isFetchingHomeSystem = false;
            }
        }

        private static bool _isFetchingHomeSystem;

        public ObservableCollection<Station> HomeStationOptions { get; } = [ ];

        private void UpdateHomeStationOptions ()
        {
            try
            {
                // Ensure collection modifications happen on the UI thread
                if ( System.Windows.Application.Current?.Dispatcher?.CheckAccess() == false )
                {
                    System.Windows.Application.Current.Dispatcher.Invoke( UpdateHomeStationOptions );
                    return;
                }
                
                HomeStationOptions.Clear();
                foreach ( var station in ( HomeStarSystem?.stations ?? Enumerable.Empty<Station>() )
                         .OrderBy( s => s.name )
                         .Prepend( new Station { name = Properties.Resources.no_station } )
                         .ToHashSet() )
                {
                    HomeStationOptions.Add( station );
                }
                OnPropertyChanged( nameof( HomeStationOptions ) );
            }
            catch ( Exception e )
            {
                Logging.Error(e.Message, e);
            }
        }

        [CanBeNull]
        public Station HomeStation
        {
            get => _homeStation ?? HomeStationOptions.FirstOrDefault(s => s.marketId == Cmdr.homeStationMarketID );
            set
            {
                if ( Cmdr.homeStationMarketID != value?.marketId )
                {
                    _homeStation = value;
                    lock ( commanderLock )
                    {
                        Cmdr.homeStationName = value?.name;
                        Cmdr.homeStationMarketID = value?.marketId;
                    }
                    OnPropertyChanged();
                    WriteCommander();
                }
            }
        }
        private Station _homeStation;
        
        [CanBeNull]
        private StarSystem SquadronStarSystem // May be null when the commander hasn't set a squadron star system
        {
            get => _squadronStarSystem;
            set
            {
                _squadronStarSystem = value;
                OnPropertyChanged();
            }
        }

        private StarSystem _squadronStarSystem;

        public string SquadronSystemName
        {
            get => _squadronStarSystem?.systemname;
            set
            {
                if ( value != _squadronStarSystem?.systemname )
                {
                    if ( string.IsNullOrEmpty( value ) )
                    {
                        SquadronStarSystem = null;
                        return;
                    }                    
                    
                    if ( _isFetchingSquadronSystem ) { return; }
                    FetchSquadronSystemAsync( value ).SafeFireAndForget( ex => Logging.Error( ex.Message, ex ) );
                }
            }
        }

        private async Task FetchSquadronSystemAsync(string systemName)
        {
            _isFetchingSquadronSystem = true;                    
            try
            {
                // Ensure collection modifications happen on the UI thread
                if ( System.Windows.Application.Current?.Dispatcher?.CheckAccess() == false )
                {
                    await System.Windows.Application.Current.Dispatcher.InvokeAsync( async () =>
                        await FetchSquadronSystemAsync( systemName ) );
                    return;
                }
                
                var fetchedSystem = await Task.Run( () => EDDI.Instance.DataProvider
                    .GetOrFetchStarSystemAsync( systemName ) ).ConfigureAwait( true );
                
                if ( fetchedSystem != null )
                {
                    SquadronStarSystem = fetchedSystem;
                    UpdateSquadronFactions();
                    lock ( commanderLock )
                    {
                        Cmdr.squadronSystemName = SquadronStarSystem?.systemname;
                        Cmdr.squadronSystemAddress = SquadronStarSystem?.systemAddress;
                    }
                    WriteCommander();
                    OnPropertyChanged( nameof(SquadronSystemName) );
                }
            }
            finally
            {
                _isFetchingSquadronSystem = false;
            }
        }

        private static bool _isFetchingSquadronSystem;

        public ObservableCollection<Faction> SquadronFactions { get; } = [ ];

        private void UpdateSquadronFactions ()
        {
            SquadronFactions.Clear();
            foreach ( var faction in ( SquadronStarSystem?.factions ?? Enumerable.Empty<Faction>() )
                     .OrderBy( f => f.name )
                     .Prepend( new Faction { name = Power.None.localizedName } )
                     .ToHashSet() )
            {
                SquadronFactions.Add( faction );
            }
            OnPropertyChanged( nameof( SquadronFactions ) );
        }

        [UsedImplicitly]
        public Faction SelectedSquadronFaction
        {
            get => _selectedSquadronFaction ?? 
                   SquadronFactions.FirstOrDefault(f => f.name.Equals( Cmdr.squadronfaction, StringComparison.OrdinalIgnoreCase ) );
            set
            {
                if ( value?.name != Cmdr.squadronfaction )
                {
                    _selectedSquadronFaction = value;
                    lock ( commanderLock )
                    {
                        Cmdr.squadronfaction = value?.name == Power.None.localizedName ? null : value?.name;
                        Cmdr.squadronallegiance = value?.name == Power.None.localizedName ? null : value?.Allegiance;
                    }

                    WriteCommander();
                    OnPropertyChanged();
                }
            }
        }
        private Faction _selectedSquadronFaction;

        public static ObservableCollection<Power> SquadronPowers => new( Power.AllOfThem
            .Except( [ Power.None ] )
            .OrderBy( p => p.localizedName )
            .Prepend( Power.None )
            .ToHashSet() );

        [UsedImplicitly]
        public Power SelectedSquadronPower
        {
            get => SquadronPowers.FirstOrDefault( p => p == Cmdr.squadronpower );
            set
            {
                if ( value?.edname != Cmdr.squadronpower?.edname )
                {
                    lock ( commanderLock )
                    {
                        Cmdr.squadronpower = value == Power.None ? null : value;
                    }
                    WriteCommander();
                    OnPropertyChanged();
                }
            }
        }

        #endregion

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
        {
            cts.Cancel();
        }

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
                { "homesystem", new Tuple<Type, object>( typeof(StarSystem), HomeStarSystem ) },
                { "homestation", new Tuple<Type, object>( typeof(Station), HomeStation ) },
                { "squadronsystem", new Tuple<Type, object>( typeof(StarSystem), SquadronStarSystem ) }
            };
        }

        public UserControl ConfigurationTabItem ()
        {
            return new ConfigurationWindow();
        }
        
        public Task PreHandleAsync ( Event @event )
        {
            if ( @event is CarrierBankTransferEvent carrierBankTransferEvent )
            {
                handleCarrierBankTransferEvent( carrierBankTransferEvent );
            }
            else if ( @event is CarrierJumpedEvent carrierJumpedEvent )
            {
                handleCarrierJumpedEvent( carrierJumpedEvent );
            }
            else if ( @event is CommanderContinuedEvent commanderContinuedEvent )
            {
                handleCommanderContinuedEvent( commanderContinuedEvent );
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

            updatedAt = @event.timestamp;

            return Task.CompletedTask;
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
            if ( ( @event.docked || @event.onFoot ) && @event.factions.Count > 0 && EDDI.Instance.GameState.CurrentStarSystem != null )
            {
                if ( @event.timestamp >= updatedAt && 
                     TryUpdateSquadronHomeSystem( @event.systemAddress, @event.factions ) )
                {
                    WriteCommander();
                }
            }
        }

        private void handleCommanderContinuedEvent ( CommanderContinuedEvent @event )
        {
            if ( @event.timestamp >= updatedAt )
            {
                lock ( commanderLock )
                {
                    Cmdr.name = @event.commander;
                    Cmdr.EDID = @event.frontierID;
                    Cmdr.credits = @event.credits;
                    Cmdr.debt = @event.loan;
                }
                WriteCommander();
            }
        }

        private void handleCommanderLoadingEvent ( CommanderLoadingEvent @event )
        {
            lock ( commanderLock )
            {
                Cmdr.EDID = @event.frontierID;
            }

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
            if ( @event.factions.Count > 0 && EDDI.Instance.GameState.CurrentStarSystem != null )
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
            if ( @event.factions.Count > 0 && EDDI.Instance.GameState.CurrentStarSystem != null )
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
        
        private bool TryUpdateSquadronHomeSystem ( ulong currentSystemAddress, List<Faction> systemFactions )
        {
            var update = false;

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
                    if ( EDDI.Instance.GameState.CurrentStarSystem?.systemAddress == currentSystemAddress )
                    {
                        // Update the squadron system data, if changed
                        SquadronSystemName = EDDI.Instance.GameState.CurrentStarSystem.systemname;
                        Cmdr.squadronSystemName = EDDI.Instance.GameState.CurrentStarSystem.systemname;
                        Cmdr.squadronSystemAddress = EDDI.Instance.GameState.CurrentStarSystem.systemAddress;
                        Cmdr.squadronfaction = squadronFaction.name;

                        // Update the squadron allegiance according to the faction info from the journal
                        Cmdr.squadronallegiance = EDDI.Instance.GameState.CurrentStarSystem?.Faction?.Allegiance ?? Superpower.None;

                        // Update the squadron power to match the HQ system's controlling power if it has not been previously set
                        Cmdr.squadronpower = EDDI.Instance.GameState.CurrentStarSystem?.Power ?? Power.None;
                    }
                }

                update = true;
            }

            return update;
        }

        public Task PostHandleAsync ( Event @event )
        {
            return Task.CompletedTask;
        }

        public Task HandleStatusAsync ( Status status )
        {
            lock ( commanderLock )
            {
                Cmdr.credits = status.credit_balance;
            }

            return Task.CompletedTask;
        }

        public Task HandleProfileAsync ( JObject profile )
        {
            // Update our commander object
            var frontierApiProfile = FrontierApiProfile.FromJson( profile );
            var updatedCmdr = Commander.FromFrontierApiCmdr( Cmdr, frontierApiProfile.Cmdr,
                frontierApiProfile.timestamp, updatedAt, out var cmdrMatches );

            if ( cmdrMatches )
            {
                Logging.Debug( "Commander information updated from Frontier API; updating local copy" );
                lock ( commanderLock )
                {
                    Cmdr = updatedCmdr;
                }
            }
            else
            {
                Logging.Debug( "Skipping profile update - Frontier API commander information doesn't match journal information" );
            }
            return Task.CompletedTask;
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
                         Cmdr.federationrating is { } rating &&
                         rating.rank > minFederationRankForTitle )
                    {
                        Cmdr.title = Cmdr.federationrating.localizedName;
                    }
                    else if ( controllingFactionAllegiance.invariantName == Superpower.Empire.invariantName &&
                              Cmdr.empirerating is { } empireRating &&
                              empireRating.rank > minEmpireRankForTitle )
                    {
                        Cmdr.title = Cmdr.empirerating.maleRank.localizedName;
                    }
                }
            }
        }

        private Commander ReadCommander ( CommanderConfiguration configuration = null )
        {
            // Obtain current commander from our configuration
            configuration ??= ConfigService.Instance.commanderConfiguration;
            var commander = new Commander
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
                
                // Home system information
                homeSystemName = configuration.homeSystemName,
                homeSystemAddress = configuration.homeSystemAddress,
                homeStationName = configuration.homeStationName,
                homeStationMarketID = configuration.homeStationMarketID,

                // Squadron information
                squadronname = configuration.squadronName,
                squadrontag = configuration.squadronTag,
                squadronrank = configuration.SquadronRank,
                squadronSystemName = configuration.squadronSystemName,
                squadronSystemAddress = configuration.squadronSystemAddress,
                squadronfaction = configuration.squadronFaction,
                squadronallegiance = configuration.SquadronAllegiance ?? Superpower.None,
                squadronpower = configuration.SquadronPower ?? Power.None
            };

            HomeSystemName = configuration.homeSystemName;
            SquadronSystemName = configuration.squadronSystemName;

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
                configuration.homeSystemName = Cmdr.homeSystemName;
                configuration.homeSystemAddress = Cmdr.homeSystemAddress;
                configuration.homeSystemX = Cmdr.homeSystemX;
                configuration.homeSystemY = Cmdr.homeSystemY;
                configuration.homeSystemZ = Cmdr.homeSystemZ;
                configuration.homeStationName = Cmdr.homeStationName;
                configuration.homeStationMarketID = Cmdr.homeStationMarketID;
                
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
