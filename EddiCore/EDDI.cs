using EddiCompanionAppService;
using EddiConfigService;
using EddiCore.EventHandling;
using EddiCore.GameState;
using EddiCore.Hotkeys;
using EddiCore.PluginHosting;
using EddiDataDefinitions;
using EddiDataProviderService;
using EddiEvents;
using EddiIPC_Service.Server;
using EddiSpeechService;
using EddiStatusService;
using JetBrains.Annotations;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Utilities;

[assembly: InternalsVisibleTo( "Tests" )]
namespace EddiCore
{
    /// <summary>
    /// Eddi is the controller for all EDDI operations.  Its job is to retain the state of the objects such as the commander, the current system, etc.
    /// and keep them up-to-date with changes that occur.
    /// It also acts as the switchboard for passing events through all parts of the application including both responders and monitors.
    /// </summary>
    public class EDDI: INotifyPropertyChanged, IEddiEventProcessorContext
    {
        // True if the EDDI UI is waiting on a modal dialog window. Accessed by VoiceAttack plugin.
        public bool IsModalDialogOpen { get; set; }

        // VoiceAttack host application version, if supplied by process args or IPC handshake.
        public System.Version VoiceAttackVersion { get; set; }

        private static bool started;
        public bool running;

        #region GameState

        private readonly EddiGameState _gameState = new();
        private readonly EddiGameStateService _gameStateService;

        public IEddiGameState GameState => _gameState;
        internal IEddiGameStateMutator GameStateMutator => _gameStateService;

        #endregion

        public void UpdateCurrentShip ( Ship ship ) => _gameStateService.CurrentShip = ship;

        public void UpdateVehicle ( string vehicle ) => _gameStateService.Vehicle = vehicle;

        public void UpdateFleetCarrier ( FleetCarrier carrier ) => _gameStateService.FleetCarrier = carrier;

        public void UpdateSquadronCarrier ( FleetCarrier carrier ) => _gameStateService.SquadronCarrier = carrier;

        public void UpdateSearchSystem ( StarSystem system, decimal distanceLy )
        {
            _gameStateService.SearchStarSystem = system;
            _gameStateService.SearchDistanceLy = distanceLy;
        }

        public void UpdateSearchStation ( Station station ) => _gameStateService.SearchStation = station;

        public void UpdateDestinationDistance ( decimal distanceLy ) => _gameStateService.DestinationDistanceLy = distanceLy;

        internal EddiEventProcessor EventProcessor { get; }
        internal EddiEventPipeline EventPipeline { get; }

        EddiEventPipeline IEddiEventProcessorContext.EventPipeline => EventPipeline;
        IEddiGameStateMutator IEddiEventProcessorContext.GameStateMutator => _gameStateService;
        OrganicSamplingTracker IEddiEventProcessorContext.OrganicSamplingTracker => _organicSamplingTracker;

        Task IEddiEventProcessorContext.conditionallyRefreshStationProfileAsync (
            string expectedSystemName,
            long expectedLastMarketID,
            bool forceUpdate,
            JObject profileJson ) =>
            conditionallyRefreshStationProfileAsync( expectedSystemName, expectedLastMarketID, forceUpdate, profileJson );

        // EDDI uses APIs which only return data for the "live" galaxy, game version 4.0 or later.
        private readonly System.Version minGameVersion = new(4, 0);

        static EDDI()
        {
            // Set up our app directory
            Directory.CreateDirectory(Constants.DATA_DIR);
        }

        // True if we have been started by VoiceAttack
        public bool FromVA;

        private static void Init()
        {
            if (instance == null)
            {
                lock (instanceLock)
                {
                    if (instance == null)
                    {
                        Logging.Debug("No EDDI instance: creating one");
                        instance = new EDDI();
                    }
                }
            }
        }

        // EDDI Instance
        public static EDDI Instance
        {
            get
            {
                Init();
                return instance;
            }
        }
        private static EDDI instance;
        private static readonly object instanceLock = new();

        private readonly EddiPluginHost _pluginHost;
        private OrganicSamplingTracker _organicSamplingTracker;

        public List<IEddiMonitor> monitors => _pluginHost.Monitors;
        internal ConcurrentBag<IEddiMonitor> activeMonitors => _pluginHost.ActiveMonitors;
        public List<IEddiResponder> responders => _pluginHost.Responders;
        internal ConcurrentBag<IEddiResponder> activeResponders => _pluginHost.ActiveResponders;

        // IPC Server infrastructure (VoiceAttack plugin mode only)
        private IPCServer _ipcServer;

        public DataProviderService DataProvider { get; internal set; }
        public HotkeyManager HotkeyManager { get; } = new();

        // Information from the last events of each type that we've received (for reference)
        public ConcurrentDictionary<string, Event> lastEventOfType => EventPipeline.LastEventOfType;

        public readonly ObservableConcurrentDictionary<string, object> State = [ ];

        private readonly CancellationTokenSource eventHandlerTS = new();

        private EDDI()
        {
            _gameStateService = new EddiGameStateService(
                _gameState,
                () =>
                {
                    var commanderConfig = ConfigService.Instance.commanderConfiguration;
                    return ( commanderConfig.homeSystemX, commanderConfig.homeSystemY, commanderConfig.homeSystemZ );
                },
                ship => StatusService.Instance.CurrentShip = ship,
                msg => SpeechService.Instance.SayAsync( null, msg, 0 ).SafeFireAndForget( ex => Logging.Error( ex.Message, ex ) ),
                EddiStarMapService.StarMapService.SetGameVersion,
                minGameVersion );
            _gameState.PropertyChanged += ( _, e ) => OnPropertyChanged( e.PropertyName );
            _pluginHost = new EddiPluginHost(
                () => running,
                () => DataProvider?.IsUnitTesting ?? false, 
                null, null, null, null, eventHandlerTS.Token );
            EventProcessor = new EddiEventProcessor( this );
            EventPipeline = new EddiEventPipeline(
                EventProcessor.ProcessEventAsync,
                () => _pluginHost.ActiveMonitors,
                () => _pluginHost.ActiveResponders,
                name => ObtainResponder( name ),
                () => DataProvider?.IsUnitTesting ?? false,
                () => GameState.GameVersion,
                minGameVersion,
                eventHandlerTS.Token );
            running = true;
            try
            {
                Logging.Info(Constants.EDDI_NAME + " " + Constants.EDDI_VERSION + " starting");
                DataProvider = DataProviderService.Create();
                _organicSamplingTracker = new OrganicSamplingTracker( DataProvider, enqueueEvent );

                var configuration = ConfigService.Instance.eddiConfiguration;
                Logging.Verbose = configuration.VerboseLogging;

                // We always start in normal space
                GameStateMutator.Environment = Constants.ENVIRONMENT_NORMAL_SPACE;

                var essentialAsyncTasks = new List<Task>();
                if (running)
                {
                    essentialAsyncTasks.Add( _pluginHost.DiscoverAsync( eventHandlerTS.Token ) );
                }
                else
                {
                    Logging.Warn("Mandatory upgrade required! EDDI initializing in safe mode until upgrade is completed.");
                }

                // Make sure that our essential tasks have completed before we start
                const int discoveryTimeoutMs = 5000;
                if ( !Task.WaitAll( essentialAsyncTasks.ToArray(), discoveryTimeoutMs, eventHandlerTS.Token ) )
                {
                    Logging.Warn( $"Responder/Monitor discovery timed out after {discoveryTimeoutMs}ms" );
                }
                
                // Tasks we can start asynchronously and don't need to wait for
                updateDestinationSystemAsync( configuration.DestinationSystemAddress, configuration.DestinationSystem )
                    .SafeFireAndForget( ex => Logging.Error( ex.Message, ex ) );
                InitializeFrontierApiServiceAsync()
                    .SafeFireAndForget( ex => Logging.Error( ex.Message, ex ) );

                StatusService.Instance.StatusChanged += ( s, _ ) =>
                    OnStatusChangedAsync( s ).SafeFireAndForget( ex => Logging.Error( ex.Message, ex ) );

                Logging.Info(Constants.EDDI_NAME + " " + Constants.EDDI_VERSION + " initialised");
            }
            catch (Exception ex)
            {
                Logging.Error("Failed to initialise", ex);
            }
        }

        private async Task InitializeFrontierApiServiceAsync()
        {
            // Set up the Frontier API service
            // Try to carry out initial population of the Frontier API profile
            try
            {
                await Task.Delay( 500 ).ConfigureAwait(false);
                await refreshProfileAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Logging.Debug("Failed to obtain Frontier API profile: " + ex);
            }

            Logging.Info( CompanionAppService.Instance.CurrentState == CompanionAppService.State.Authorized
                ? "EDDI access to the Frontier API is enabled."
                : "EDDI access to the Frontier API is not enabled." );
        }

        private async Task OnStatusChangedAsync ( object sender )
        {
            if ( sender is Status status )
            {
                if ( _organicSamplingTracker is not null )
                {
                    await _organicSamplingTracker.HandleStatusAsync( status ).ConfigureAwait( false );
                }
                await _pluginHost.HandleStatusAsync( status ).ConfigureAwait( false );
            }
        }

        public static bool EddiIsBeta() => Constants.EDDI_VERSION.phase < Utilities.Version.TestPhase.rc;

        public bool ShouldUseTestEndpoints()
        {
            // use test endpoints if the game is in beta or EDDI is in a test phase
            return GameState.gameIsBeta || 
                   Constants.EDDI_VERSION.phase < Utilities.Version.TestPhase.rc ;
        }

        public void Start()
        {
            if (!started)
            {
                _pluginHost.Start( ConfigService.Instance.eddiConfiguration );

                // Initialize IPC server (the VoiceAttack plugin can connect to this server as an IPC client)
                try
                {
                    _ipcServer = new IPCServer();
                    _ipcServer.InitializeIpcServer();
                    Logging.Info( "IPC server initialized for standalone mode; listening for plugin connections" );
                }
                catch ( Exception ex )
                {
                    Logging.Error( "Failed to initialize IPC server", ex );
                    // Don't throw - allow EDDI to continue even if IPC fails
                }

                started = true;
            }
        }

        public void Stop()
        {
            if ( running )
            {
                running = false; // Otherwise keepalive restarts them

                // Shutdown IPC server
                if (_ipcServer?.IsRunning ?? false)
                {
                    try
                    {
                        _ipcServer.StopAsync().GetResultOrTimeout( TimeSpan.FromSeconds( 2 ) );
                    }
                    catch (Exception ex)
                    {
                        Logging.Error("Error stopping IPC server", ex);
                    }
                }

                EventProcessor.Dispose();
                DataProvider.CancelPendingRequests();
                Utilities.TelemetryService.Telemetry.Stop();
                eventHandlerTS.Cancel();
                EventPipeline.Stop();
                _pluginHost.StopAll();

                Logging.Info( Constants.EDDI_NAME + " " + Constants.EDDI_VERSION + " stopped" );
            }
        }

        /// <summary>
        /// Reload all monitors and responders
        /// </summary>
        public void Reload()
        {
            _pluginHost.Reload();
            Logging.Info(Constants.EDDI_NAME + " " + Constants.EDDI_VERSION + " reloaded");
        }

        /// <summary>
        /// Obtain a named monitor
        /// </summary>
        public IEddiMonitor ObtainMonitor(string invariantName, StringComparison stringComparison = StringComparison.InvariantCultureIgnoreCase )
        {
            return _pluginHost.ObtainMonitor( invariantName, stringComparison );
        }

        /// <summary> Obtain a named responder </summary>
        public IEddiResponder ObtainResponder(string invariantName, StringComparison stringComparison = StringComparison.InvariantCultureIgnoreCase)
        {
            return _pluginHost.ObtainResponder( invariantName, stringComparison );
        }

        /// <summary> Disable a named responder for this session.  This does not update the on-disk status of the responder </summary>
        public void DisableResponder(string invariantName, StringComparison stringComparison = StringComparison.InvariantCultureIgnoreCase )
        {
            _pluginHost.DisableResponder( invariantName, stringComparison );
        }

        /// <summary> Enable a named responder for this session.  This does not update the on-disk status of the responder </summary>
        public void EnableResponder(string invariantName, StringComparison stringComparison = StringComparison.InvariantCultureIgnoreCase )
        {
            _pluginHost.EnableResponder( invariantName, stringComparison );
        }

        /// <summary> Disable a named monitor for this session.  This does not update the on-disk status of the responder </summary>
        public void DisableMonitor(string invariantName, StringComparison stringComparison = StringComparison.InvariantCultureIgnoreCase )
        {
            _pluginHost.DisableMonitor( invariantName, stringComparison );
        }

        public void DisableMonitor ( IEddiMonitor monitor )
        {
            _pluginHost.DisableMonitor( monitor );
        }

        /// <summary> Enable a named monitor for this session.  This does not update the on-disk status of the responder </summary>
        public void EnableMonitor(string invariantName, StringComparison stringComparison = StringComparison.InvariantCultureIgnoreCase )
        {
            _pluginHost.EnableMonitor( invariantName, stringComparison );
        }

        public void EnableMonitor ( IEddiMonitor monitor )
        {
            _pluginHost.EnableMonitor( monitor );
        }

        /// <summary> Reload a specific monitor or responder </summary>
        public void Reload(string name, StringComparison stringComparison = StringComparison.InvariantCultureIgnoreCase )
        {
            _pluginHost.Reload( name, stringComparison );
            Logging.Info($"{Constants.EDDI_NAME} {Constants.EDDI_VERSION} module {name} reloaded");
        }

        public void enqueueEvent ( Event @event ) => EventPipeline.Enqueue( @event );

        internal Task HandleEventAsync ( Event @event ) => EventPipeline.HandleEventAsync( @event );

        /// <summary>Obtain information from the companion API and use it to refresh our own data</summary>
        public async Task<bool> refreshProfileAsync(bool refreshStation = false)
        {
            if ( CompanionAppService.Instance.unitTesting ||
                 CompanionAppService.Instance?.CurrentState != CompanionAppService.State.Authorized )
            {
                return true;
            }

            var success = true;
            try
            {
                var profileJson = await CompanionAppService.Instance.ProfileEndpoint.GetProfileAsync().ConfigureAwait(false);
                if (profileJson != null)
                {
                    var profile = FrontierApiProfile.FromJson(profileJson);

                    var updatedCurrentStarSystem = false;

                    if (GameState.CurrentStarSystem == null && 
                        profile.docked && profile.currentStarSystem == GameState.CurrentStarSystem?.systemname && 
                        GameState.CurrentStarSystem?.stations != null)
                    {
                        // Only set the current station if it is not present, otherwise we leave it to events
                        GameStateMutator.CurrentStation ??= GameState.CurrentStarSystem.stations.FirstOrDefault(s => s.marketId == profile.LastStationMarketID)
                            ?? GameState.CurrentStarSystem.stations.FirstOrDefault(s => s.name == profile.LastStationName);
                        if (GameState.CurrentStation != null)
                        {
                            Logging.Debug("Set current station to " + GameState.CurrentStation.name);
                            GameState.CurrentStation.updatedat = Dates.fromDateTimeToSeconds(DateTime.UtcNow);
                            updatedCurrentStarSystem = true;
                        }
                    }

                    if (refreshStation && GameState.CurrentStation != null && GameState.Environment == Constants.ENVIRONMENT_DOCKED)
                    {
                        // Refresh station data
                        await conditionallyRefreshStationProfileAsync( profile.currentStarSystem, profile.LastStationMarketID ?? 0 ).ConfigureAwait(false);
                    }

                    if (updatedCurrentStarSystem)
                    {
                        Logging.Debug( "Star system information updated from Frontier API; updating local copy" );
                        await DataProvider.SaveStarSystemAsync(GameState.CurrentStarSystem).ConfigureAwait(false);
                    }

                    success = await _pluginHost.HandleProfileAsync( profile.json ).ConfigureAwait( false );
                }
            }
            catch (Exception ex)
            {
                Logging.Error("Exception obtaining profile", ex);
                success = false;
            }
            return success;
        }

        /// <summary>
        /// Find all monitors
        /// </summary>
        public static List<IEddiMonitor> findMonitors()
        {
            return EddiPluginHost.FindMonitors();
        }

        /// <summary>
        /// Find all responders
        /// </summary>
        public static List<IEddiResponder> findResponders()
        {
            return EddiPluginHost.FindResponders();
        }

        /// <summary>
        /// Update the profile when requested, ensuring that we meet the condition in the updated profile
        /// </summary>
        internal async Task conditionallyRefreshStationProfileAsync ( string expectedSystemName, long expectedLastMarketID, bool forceUpdate = false, JObject profileJson = null )
        {
            if (CompanionAppService.Instance.CurrentState == CompanionAppService.State.Authorized)
            {
                try
                {
                    // Make sure we know where we are
                    if (GameState.CurrentStarSystem is null || GameState.CurrentStarSystem.systemAddress == 0)
                    {
                        Logging.Debug( "Skipping conditional station profile fetch - current location data is incomplete" );
                        return;
                    }

                    // We do need to fetch an updated station profile; do so
                    Logging.Debug("Starting conditional station profile fetch");
                    var commanderName = ConfigService.Instance.commanderConfiguration.commanderName;
                    var result = await CompanionAppService.Instance.CombinedStationEndpoints.GetCombinedStationAsync(
                        commanderName, expectedSystemName, expectedLastMarketID, forceUpdate, profileJson).ConfigureAwait(false);
                    if (result != null)
                    {
                        var profileStation = FrontierApiStation.FromJson(result["marketJson"]?.ToObject<JObject>(), result["shipyardJson"]?.ToObject<JObject>());

                        // We have the required station information
                        var station = GameState.CurrentStarSystem?.stations.Find(s => s.marketId == profileStation.marketId);
                        if ( station != null )
                        {
                            Logging.Debug( "Current station matches profile information; updating info" );
                            profileStation.UpdateStation( profileStation.commoditiesupdatedat, station );

                            // Update the current station information in our backend DB
                            Logging.Debug( "Star system information updated from Frontier API server; updating local copy" );
                            await DataProvider.SaveStarSystemAsync( GameState.CurrentStarSystem ).ConfigureAwait(false);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logging.Error("Exception obtaining station profile", ex);
                }
            }
        }

        public async Task updateDestinationSystemAsync ( ulong? destinationSystemAddress, string destinationSystem = null )
        {
            var configuration = ConfigService.Instance.eddiConfiguration;
            if ( destinationSystemAddress > 0 )
            {
                var system = await DataProvider.GetOrFetchStarSystemAsync((ulong)destinationSystemAddress ).ConfigureAwait(false);

                //Ignore null & empty systems
                if (system != null)
                {
                    if (system.systemAddress != GameState.DestinationStarSystem?.systemAddress )
                    {
                        Logging.Debug("Destination star system is " + system.systemname);
                        GameStateMutator.DestinationStarSystem = system;
                    }
                }
                else { destinationSystem = null; }
            }
            else
            {
                GameStateMutator.DestinationStarSystem = null;
            }
            configuration.DestinationSystem = destinationSystem;
            configuration.DestinationSystemAddress = destinationSystemAddress;
            ConfigService.Instance.eddiConfiguration = configuration;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        public virtual void OnPropertyChanged([CallerMemberName]string propertyName = null) 
        { 
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}