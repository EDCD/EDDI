using EddiCompanionAppService;
using EddiCore.EventHandling;
using EddiConfigService;
using EddiCore.GameState;
using EddiCore.Hotkeys;
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
using System.Collections.Immutable;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
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

        public List<IEddiMonitor> monitors = [ ];
        internal ConcurrentBag<IEddiMonitor> activeMonitors = [ ];
        private static readonly object monitorLock = new();
        private readonly Dictionary<string, CancellationTokenSource> _monitorCancellationTokens = [ ];
        private bool IsMonitorActive ( string name ) => activeMonitors.Any( m => m.MonitorName().Equals(name, StringComparison.OrdinalIgnoreCase) );

        public List<IEddiResponder> responders = [ ];
        private ConcurrentBag<IEddiResponder> activeResponders = [ ];
        private static readonly object responderLock = new();

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
            EventProcessor = new EddiEventProcessor( this );
            EventPipeline = new EddiEventPipeline(
                EventProcessor.ProcessEventAsync,
                () => activeMonitors,
                () => activeResponders,
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

                var configuration = ConfigService.Instance.eddiConfiguration;
                Logging.Verbose = configuration.VerboseLogging;

                // We always start in normal space
                GameStateMutator.Environment = Constants.ENVIRONMENT_NORMAL_SPACE;

                var essentialAsyncTasks = new List<Task>();
                if (running)
                {
                    // Tasks we can start asynchronously but need to complete before other dependent code is called
                    var discoveryTasks = new List<Task>
                    {
                        Task.Run( () => {
                            try
                            {
                                responders = findResponders();
                                Logging.Debug( $"Discovered {responders.Count} responders" );
                            }
                            catch ( Exception ex )
                            {
                                Logging.Error( "Failed to discover responders", ex );
                                responders = [ ];
                            }
                        }, eventHandlerTS.Token ),
                        Task.Run( () => {
                            try
                            {
                                monitors = findMonitors();
                                Logging.Debug( $"Discovered {monitors.Count} monitors" );
                            }
                            catch ( Exception ex )
                            {
                                Logging.Error( "Failed to discover monitors", ex );
                                monitors = [ ];
                            }
                        }, eventHandlerTS.Token )
                    };

                    essentialAsyncTasks.AddRange( discoveryTasks );
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
                var monitorTasks = new List<Task>();
                foreach ( var monitor in activeMonitors )
                {
                    var monitorTask = monitor.HandleStatusAsync( status );
                    monitorTask.ContinueWith( task =>
                        {
                            if ( task.IsFaulted )
                            {
                                var dict = new Dictionary<string, object>
                                {
                                    [ "status" ] = status, [ "exception" ] = task.Exception
                                };
                                Logging.Error( $"{monitor.MonitorName()} failed to handle status", dict );
                            }
                        }, TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously )
                        .SafeFireAndForget( e => Logging.Error( e.Message, e ) );
                    monitorTasks.Add( monitorTask );
                }

                var responderTasks = new List<Task>();
                foreach ( var responder in activeResponders )
                {
                    var responderTask = responder.HandleStatusAsync( status );
                    responderTask.ContinueWith( task =>
                        {
                            if ( task.IsFaulted )
                            {
                                var dict = new Dictionary<string, object>
                                {
                                    [ "status" ] = status, [ "exception" ] = task.Exception
                                };
                                Logging.Error( $"{responder.ResponderName()} failed to handle status", dict );
                            }
                        }, TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously )
                        .SafeFireAndForget( e => Logging.Error( e.Message, e ) );
                    responderTasks.Add( responderTask );
                }

                try
                {
                    await Task.WhenAll( monitorTasks ).ConfigureAwait( false );
                    await Task.WhenAll( responderTasks ).ConfigureAwait( false );
                }
                catch ( TaskCanceledException )
                {
                    // Task(s) cancelled. Nothing to do here.
                }
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
                var configuration = ConfigService.Instance.eddiConfiguration;
                foreach (var monitor in monitors)
                {
                    if (!configuration.Plugins.TryGetValue(monitor.MonitorName(), out var enabled))
                    {
                        // No information; default to enabled
                        enabled = true;
                    }

                    if (!enabled && !monitor.IsRequired())
                    {
                        Logging.Info( $"{monitor.MonitorName()} is disabled; not starting" );
                    }
                    else
                    {
                        EnableMonitor( monitor );
                    }
                }

                foreach (var responder in responders)
                {
                    if (!configuration.Plugins.TryGetValue(responder.ResponderName(), out var enabled))
                    {
                        // No information; default to enabled
                        enabled = true;
                    }

                    if (!enabled)
                    {
                        Logging.Info( $"{responder.ResponderName()} is disabled; not starting" );
                    }
                    else if ( activeResponders.Any( r => r.ResponderName() == responder.ResponderName() ) )
                    {
                        Logging.Warn( $"{responder.ResponderName()} is already running." );
                    }
                    else
                    {
                        try
                        {
                            var responderStarted = responder.Start();
                            if (responderStarted)
                            {
                                activeResponders.Add(responder);
                                Logging.Info("Started " + responder.ResponderName());
                            }
                            else
                            {
                                Logging.Warn("Failed to start " + responder.ResponderName());
                            }
                        }
                        catch (Exception ex)
                        {
                            Logging.Error("Failed to start " + responder.ResponderName(), ex);
                        }
                    }
                }

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
                foreach ( var responder in responders )
                {
                    DisableResponder( responder );
                }
                foreach ( var monitor in monitors )
                {
                    DisableMonitor( monitor );
                }

                Logging.Info( Constants.EDDI_NAME + " " + Constants.EDDI_VERSION + " stopped" );
            }
        }

        /// <summary>
        /// Reload all monitors and responders
        /// </summary>
        public void Reload()
        {
            foreach (var responder in responders)
            {
                responder.Reload();
            }
            foreach (var monitor in monitors)
            {
                monitor.Reload();
            }

            Logging.Info(Constants.EDDI_NAME + " " + Constants.EDDI_VERSION + " reloaded");
        }

        /// <summary>
        /// Obtain a named monitor
        /// </summary>
        public IEddiMonitor ObtainMonitor(string invariantName, StringComparison stringComparison = StringComparison.InvariantCultureIgnoreCase )
        {
            foreach (var monitor in monitors)
            {
                if (monitor.MonitorName().Equals(invariantName, stringComparison))
                {
                    return monitor;
                }
            }
            return null;
        }

        /// <summary> Obtain a named responder </summary>
        public IEddiResponder ObtainResponder(string invariantName, StringComparison stringComparison = StringComparison.InvariantCultureIgnoreCase)
        {
            foreach (var responder in responders)
            {
                if (responder.ResponderName().Equals(invariantName, stringComparison ) )
                {
                    return responder;
                }
            }
            return null;
        }

        /// <summary> Disable a named responder for this session.  This does not update the on-disk status of the responder </summary>
        public void DisableResponder(string invariantName, StringComparison stringComparison = StringComparison.InvariantCultureIgnoreCase )
        {
            var responder = ObtainResponder(invariantName, stringComparison);
            DisableResponder(responder);
        }

        private void DisableResponder(IEddiResponder responder)
        {
            if (responder != null)
            {
                lock (responderLock)
                {
                    // Remove the responder from the active list.
                    var newResponders = new ConcurrentBag<IEddiResponder>();
                    while (activeResponders.TryTake(out var item))
                    {
                        if (item != responder) { newResponders.Add(item); }
                    }
                    activeResponders = newResponders;

                    // Stop the responder only after it's been removed from the active list.
                    responder.Stop();
                }
            }
        }

        /// <summary> Enable a named responder for this session.  This does not update the on-disk status of the responder </summary>
        public void EnableResponder(string invariantName, StringComparison stringComparison = StringComparison.InvariantCultureIgnoreCase )
        {
            var responder = ObtainResponder(invariantName, stringComparison);
            EnableResponder(responder);
        }

        private void EnableResponder(IEddiResponder responder)
        {
            if (responder != null)
            {
                if (!activeResponders.Contains(responder))
                {
                    activeResponders.Add( responder );
                    responder.Start();
                }
            }
        }

        /// <summary> Disable a named monitor for this session.  This does not update the on-disk status of the responder </summary>
        public void DisableMonitor(string invariantName, StringComparison stringComparison = StringComparison.InvariantCultureIgnoreCase )
        {
            var monitor = ObtainMonitor(invariantName, stringComparison);
            DisableMonitor(monitor);
        }

        public void DisableMonitor ( IEddiMonitor monitor )
        {
            if ( monitor != null )
            {
                lock ( monitorLock )
                {
                    var monitorName = monitor.MonitorName();

                    // Signal cancellation for this monitor's keepalive loop
                    if ( _monitorCancellationTokens.TryGetValue( monitorName, out var cts ) )
                    {
                        cts.Cancel();
                        cts.Dispose();
                        _monitorCancellationTokens.Remove( monitorName );
                    }

                    // Remove the monitor from the active list.
                    var newMonitors = new ConcurrentBag<IEddiMonitor>();
                    while ( activeMonitors.TryTake( out var item ) )
                    {
                        if ( item != monitor )
                        {
                            newMonitors.Add( item );
                        }
                    }

                    activeMonitors = newMonitors;

                    // Stop the monitor only after it's been removed from the active list.
                    monitor.Stop();

                    Logging.Info( $"{monitorName} disabled." );
                }
            }
        }

        /// <summary> Enable a named monitor for this session.  This does not update the on-disk status of the responder </summary>
        public void EnableMonitor(string invariantName, StringComparison stringComparison = StringComparison.InvariantCultureIgnoreCase )
        {
            var monitor = ObtainMonitor(invariantName, stringComparison);
            EnableMonitor(monitor);
        }

        public void EnableMonitor ( IEddiMonitor monitor )
        {
            if ( monitor != null )
            {
                if ( !activeMonitors.Contains( monitor ) )
                {
                    activeMonitors.Add( monitor );
                    if ( monitor.NeedsStart() )
                    {
                        var monitorName = monitor.MonitorName();
                        var cts = new CancellationTokenSource();
                        _monitorCancellationTokens[ monitorName ] = cts;

                        // Queue to thread pool instead of creating new thread
                        ThreadPool.QueueUserWorkItem( _ => keepAlive( monitorName, monitor.Start, cts.Token ), null );

                        Logging.Debug( "Queued keepalive for " + monitorName + " to thread pool" );
                    }
                }
                else
                {
                    Logging.Warn( $"{monitor.MonitorName()} is already running." );
                }
            }
        }

        /// <summary> Reload a specific monitor or responder </summary>
        public void Reload(string name, StringComparison stringComparison = StringComparison.InvariantCultureIgnoreCase )
        {
            foreach (var responder in responders)
            {
                if (responder.ResponderName().Contains( name, stringComparison ) )
                {
                    responder.Reload();
                    return;
                }
            }
            foreach (var monitor in monitors)
            {
                if (monitor.MonitorName().Contains( name, stringComparison ) )
                {
                    monitor.Reload();
                }
            }

            Logging.Info($"{Constants.EDDI_NAME} {Constants.EDDI_VERSION} module {name} reloaded");
        }

        /// <summary> Keep a monitor thread alive, restarting it as required </summary>
        private void keepAlive ( string name, Action start, CancellationToken monitorCancellationToken = default )
        {
            var token = monitorCancellationToken != CancellationToken.None 
                ? monitorCancellationToken 
                : eventHandlerTS.Token;
            const int maxConsecutiveFailures = 5;
            var stableRunResetsFailures = TimeSpan.FromMinutes(5);
            var consecutiveFailures = 0;
            var rng = new Random( unchecked(( System.Environment.TickCount * 31 ) + System.Environment.CurrentManagedThreadId) );

            try
            {
                while (running && !token.IsCancellationRequested && IsMonitorActive(name) )
                {
                    var runStartTs = Stopwatch.GetTimestamp();
                    Exception failure = null;

                    try
                    {
                        Logging.Info( $"Starting {name} (consecutiveFailures={consecutiveFailures})" );
                        start(); // expected to block until monitor stops
                    }
                    catch ( Exception ex ) when ( !token.IsCancellationRequested )
                    {
                        failure = ex; // capture so we can apply consistent failure logic below
                    }

                    // If we are stopping or the monitor was disabled, exit cleanly.
                    if ( !running || token.IsCancellationRequested || !IsMonitorActive( name ) )
                    {
                        break;
                    }

                    // Unexpected exit/crash while still enabled.
                    // Count as failure but reset the streak if it had been stable for longer than the `stableRunResetsFailures` timespan.
                    var ranFor = ElapsedSince(runStartTs);
                    if ( ranFor >= stableRunResetsFailures )
                    {
                        consecutiveFailures = 0;
                    }
                    consecutiveFailures++;
                    Logging.Warn( $"{name} exited unexpectedly after {ranFor.TotalMilliseconds} ms. Restarting." );

                    if ( failure != null )
                    {
                        Logging.Error( $"{name} crashed. Restarting. Consecutive failures: {consecutiveFailures}", failure );
                    }
                    else
                    {
                        Logging.Warn( $"{name} exited unexpectedly. Restarting. Consecutive failures: {consecutiveFailures}" );
                    }

                    if ( consecutiveFailures >= maxConsecutiveFailures )
                    {
                        DisableMonitor( name );
                        Logging.Warn( $"{name} disabled after {consecutiveFailures} consecutive failures" );
                        break;
                    }

                    // Exponential backoff (max 30s) + small jitter, except when unit testing
                    var exponent = Math.Min(Math.Max(0, consecutiveFailures - 1), 5);
                    var backoffSeconds = Math.Min(30, 1 << exponent);
                    var jitterMs = rng.Next(0, 500);
                    var delay = DataProvider.IsUnitTesting 
                        ? TimeSpan.Zero 
                        : TimeSpan.FromSeconds(backoffSeconds) + TimeSpan.FromMilliseconds(jitterMs);

                    // Cancellation-friendly wait
                    token.WaitHandle.WaitOne( delay );
                }
            }
            catch ( OperationCanceledException )
            {
                Logging.Debug( "Monitor keepAlive cancelled" );
            }
            catch (ThreadAbortException)
            {
                Logging.Debug("Thread aborted");
            }
            catch (Exception ex)
            {
                Logging.Warn( $"keepAlive for {name} failed", ex );
            }

            return;

            static TimeSpan ElapsedSince ( long startTimestamp )
            {
                var delta = Stopwatch.GetTimestamp() - startTimestamp;
                var seconds = (double)delta / Stopwatch.Frequency;
                return TimeSpan.FromSeconds( seconds );
            }
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

                    try
                    {
                        var monitorTasks = new List<Task>();
                        foreach ( var monitor in activeMonitors )
                        {
                            var monitorTask = monitor.HandleProfileAsync( profile.json );
                            monitorTask.ContinueWith( task =>
                                    {
                                        if ( task.IsFaulted )
                                        {
                                            Logging.Warn(
                                                $"Monitor {monitor.MonitorName()} failed to handle Frontier API update",
                                                task.Exception );
                                            success = false;
                                        }
                                    },
                                    TaskContinuationOptions.OnlyOnFaulted |
                                    TaskContinuationOptions.ExecuteSynchronously )
                                .SafeFireAndForget( e => Logging.Error( e.Message, e ) );
                            monitorTasks.Add( monitorTask );
                        }

                        await Task.WhenAll( monitorTasks ).ConfigureAwait( false );
                    }
                    catch ( TaskCanceledException tce )
                    {
                        Logging.Debug( "Task cancelled", tce );
                        success = false;
                    }
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
            var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            if (string.IsNullOrEmpty(path))
            {
                Logging.Warn("Unable to start EDDI Monitors, application directory path not found.");
                return null;
            }

            var dir = new DirectoryInfo(path);
            List<IEddiMonitor> foundMonitors = [ ];
            var pluginType = typeof(IEddiMonitor);
            foreach (var file in dir.GetFiles("*Monitor.dll", SearchOption.AllDirectories))
            {
                try
                {
                    var assembly = Assembly.LoadFrom(file.FullName);
                    foreach (var type in assembly.GetTypes())
                    {
                        if ( !type.IsInterface && !type.IsAbstract )
                        {
                            if ( type.GetInterface( pluginType.FullName ) != null )
                            {
                                try
                                {
                                    Logging.Debug( "Instantiating monitor plugin at " + file.FullName );
                                    var monitor = type.InvokeMember( null,
                                        BindingFlags.CreateInstance,
                                        null, null, null ) as IEddiMonitor;
                                    foundMonitors.Add( monitor );
                                }
                                catch ( TargetInvocationException )
                                {
                                    Logging.Warn(
                                        $"Error loading {file.Name}. Failed to load {type.Name} from {type.Assembly}." );
                                }
                            }
                        }
                    }
                }
                catch (BadImageFormatException)
                {
                    // Ignore this; probably due to CPU architecture mismatch
                }
                catch (ReflectionTypeLoadException ex)
                {
                    var sb = new StringBuilder();
                    foreach (var exSub in ex.LoaderExceptions)
                    {
                        sb.AppendLine(exSub.Message);
                        if (exSub is FileNotFoundException exFileNotFound)
                        {
                            if (!string.IsNullOrEmpty(exFileNotFound.FusionLog))
                            {
                                sb.AppendLine("Fusion Log:");
                                sb.AppendLine(exFileNotFound.FusionLog);
                            }
                        }
                        sb.AppendLine();
                    }
                    Logging.Warn("Failed to instantiate plugin at " + file.FullName + ":\n" + sb);
                }
                catch (FileLoadException flex)
                {
                    var msg = string.Format(Properties.Resources.problem_load_monitor_file, dir.FullName);
                    Logging.Error(msg, flex);
                    SpeechService.Instance.SayAsync( null, msg, 0 ).SafeFireAndForget( e => Logging.Error( e.Message, e ) );
                }
                catch (Exception ex)
                {
                    var msg = string.Format(Properties.Resources.problem_load_monitor, $"{file.Name}.\n{ex.Message} {ex.InnerException?.Message ?? ""}");
                    Logging.Error(msg, ex);
                    SpeechService.Instance.SayAsync( null, msg, 0 ).SafeFireAndForget( e => Logging.Error( e.Message, e ) );
                }
            }
            return foundMonitors;
        }

        /// <summary>
        /// Find all responders
        /// </summary>
        public static List<IEddiResponder> findResponders()
        {
            var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            if (string.IsNullOrEmpty(path))
            {
                Logging.Warn("Unable to start EDDI Responders, application directory path not found.");
                return null;
            }
            var dir = new DirectoryInfo(path);
            List<IEddiResponder> foundResponders = [ ];
            var pluginType = typeof(IEddiResponder);
            foreach (var file in dir.GetFiles("*Responder.dll", SearchOption.AllDirectories))
            {
                try
                {
                    var assembly = Assembly.LoadFrom(file.FullName);
                    foreach (var type in assembly.GetTypes())
                    {
                        if ( !type.IsInterface && !type.IsAbstract && pluginType.FullName is not null )
                        {
                            if ( type.GetInterface( pluginType.FullName ) != null )
                            {
                                Logging.Debug( "Instantiating responder plugin at " + file.FullName );
                                var responder = type.InvokeMember( type.Name,
                                    BindingFlags.CreateInstance,
                                    null, null, null ) as IEddiResponder;
                                foundResponders.Add( responder );
                            }
                        }
                    }
                }
                catch (BadImageFormatException)
                {
                    // Ignore this; probably due to CPU architecure mismatch
                }
                catch (ReflectionTypeLoadException ex)
                {
                    var sb = new StringBuilder();
                    foreach (var exSub in ex.LoaderExceptions)
                    {
                        if ( exSub is null ) { continue; }
                        sb.AppendLine(exSub.Message);
                        if (exSub is FileNotFoundException exFileNotFound)
                        {
                            if (!string.IsNullOrEmpty(exFileNotFound.FusionLog))
                            {
                                sb.AppendLine("Fusion Log:");
                                sb.AppendLine(exFileNotFound.FusionLog);
                            }
                        }
                        sb.AppendLine();
                    }
                    Logging.Warn("Failed to instantiate plugin at " + file.FullName + ":\n" + sb);
                }
            }
            return foundResponders;
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
