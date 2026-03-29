using EddiCompanionAppService;
using EddiConfigService;
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
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
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
    public class EDDI: INotifyPropertyChanged
    {
        // True if the Speech Responder tab is waiting on a modal dialog window. Accessed by VoiceAttack plugin.
        public bool SpeechResponderModalWait { get; set; }

        private static bool started;
        public bool running;

        public bool inTelepresence { get; internal set; }

        public bool inHorizons 
        {
            get => _inHorizons;
            private set
            {
                _inHorizons = value;
                OnPropertyChanged();
            }
        } 
        private bool _inHorizons = true;

        public bool inOdyssey 
        { 
            get => _inOdyssey;
            private set
            {
                _inOdyssey = value;
                OnPropertyChanged();
            }
        } 
        private bool _inOdyssey = true;

        public bool gameIsBeta { get; internal set; }

        private string gameVersion
        {
            get => _gameVersion;
            set
            {
                _gameVersion = value;
                SetGameVersion(value);
            }
        }
        private string _gameVersion;

        private readonly StarSystemSignalSourceManager signalSourceManager = new();

        public System.Version GameVersion { get; internal set; }

        // EDDI uses APIs which only return data for the "live" galaxy, game version 4.0 or later.
        private readonly System.Version minGameVersion = new(4, 0);

        private void SetGameVersion(string v)
        {
            try
            {
                if (!string.IsNullOrEmpty(v))
                {
                    // The game version is typically a Semantic Version string (e.g. "4.0.0.102")
                    // but may sometimes include additional information (e.g. "4.0.0.32 (Alpha Phase 4 Hotfix 9)")
                    // or may be missing a Semantic Version altogether (e.g. "Fleet Carriers Update - Patch 11")
                    var versionRegex = new Regex(@"^(?<engine>0|[1-9]\d*)\.(?<major>0|[1-9]\d*)(?:\.(?<minor>\d*))?(?:\.(?<patch>\d*))?");
                    GameVersion = !string.IsNullOrEmpty(v) &&
                                  System.Version.TryParse(versionRegex.Match(v).Value, out var versionResult)
                        ? versionResult
                        : null;

                    // We rely on and use 3rd party APIs which only support data from the live galaxy. 
                    if ( GameVersion != null && GameVersion < minGameVersion )
                    {
                        // Alert the user that we are operating in legacy mode
                        const string msg = "Legacy game version detected. EDDI shall resume processing events after you return to the live galaxy.";
                        Logging.Warn(msg);
                        SpeechService.Instance.SayAsync( null, msg, 0 ).SafeFireAndForget( ex => Logging.Error( ex.Message, ex ) );
                    }

                    // We need to report our game version when sending journal data to EDSM
                    EddiStarMapService.StarMapService.SetGameVersion(GameVersion, gameVersion, gameBuild);
                }
            }
            catch (Exception e)
            {
                Logging.Error("Failed to set game version", e);
            }
        }

        /// <summary>
        /// Set this prior to setting the game version so that services requiring both receive correct data.
        /// </summary>
        private string gameBuild { get; set; }

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

        // Information obtained from the configuration

        // Destination variables
        [CanBeNull]
        public StarSystem DestinationStarSystem
        {
            get => destinationStarSystem;
            private set
            {
                void childPropertyChangedHandler(object sender, PropertyChangedEventArgs e)
                {
                    OnPropertyChanged();
                }
                if (destinationStarSystem != null) { destinationStarSystem.PropertyChanged -= childPropertyChangedHandler; }
                if (value != null) { value.PropertyChanged += childPropertyChangedHandler; }
                destinationStarSystem = value;
                OnPropertyChanged();
            }
        }
        private StarSystem destinationStarSystem;

        public decimal DestinationDistanceLy 
        {
            get => destinationDistanceLy;
            set
            {
                if (Equals(value, destinationDistanceLy)) { return; }
                destinationDistanceLy = value;
                OnPropertyChanged();
            }
        }
        private decimal destinationDistanceLy;

        // Information obtained from the player journal

        public string Environment
        {
            get => environment;
            private set
            {
                if (Equals(value, environment)) { return; }
                environment = value;
                OnPropertyChanged();
            }
        }
        private string environment;

        [CanBeNull]
        public StarSystem CurrentStarSystem 
        { 
            get => currentStarSystem;
            internal set
            {
                setSystemDistanceFromHome(value);
                setSystemDistanceFromDestination(value);
                void childPropertyChangedHandler(object sender, PropertyChangedEventArgs e)
                {
                    OnPropertyChanged();
                }
                if (currentStarSystem != null) { currentStarSystem.PropertyChanged -= childPropertyChangedHandler; }
                if (value != null) { value.PropertyChanged += childPropertyChangedHandler; }
                currentStarSystem = value;
                OnPropertyChanged(); 
            } 
        }
        private StarSystem currentStarSystem;

        [CanBeNull]
        public StarSystem LastStarSystem
        {
            get => lastStarSystem;
            internal set
            {
                setSystemDistanceFromHome(value);
                void childPropertyChangedHandler(object sender, PropertyChangedEventArgs e)
                {
                    OnPropertyChanged();
                }
                if (lastStarSystem != null) { lastStarSystem.PropertyChanged -= childPropertyChangedHandler; }
                if (value != null) { value.PropertyChanged += childPropertyChangedHandler; }
                lastStarSystem = value;
                OnPropertyChanged();
            }
        }
        private StarSystem lastStarSystem;

        [CanBeNull]
        public StarSystem NextStarSystem
        {
            get => nextStarSystem;
            internal set
            {
                setSystemDistanceFromHome(value);
                void childPropertyChangedHandler(object sender, PropertyChangedEventArgs e)
                {
                    OnPropertyChanged();
                }
                if (nextStarSystem != null) { nextStarSystem.PropertyChanged -= childPropertyChangedHandler; }
                if (value != null) { value.PropertyChanged += childPropertyChangedHandler; }
                nextStarSystem = value;
                OnPropertyChanged();
            }
        }
        private StarSystem nextStarSystem;

        /// <summary>
        /// The currently docked station, if any
        /// </summary>
        [CanBeNull]
        public Station CurrentStation
        {
            get => currentStation;
            private set
            {
                void childPropertyChangedHandler(object sender, PropertyChangedEventArgs e)
                {
                    OnPropertyChanged();
                }
                if (currentStation != null) { currentStation.PropertyChanged -= childPropertyChangedHandler; }
                if (value != null) { value.PropertyChanged += childPropertyChangedHandler; }
                currentStation = value;
                OnPropertyChanged();
            }
        }
        private Station currentStation;

        /// <summary>
        /// The currently nearby star system (within the gravity well), if any
        /// </summary>
        [CanBeNull]
        public Body CurrentStellarBody 
        {
            get => currentStellarBody;
            private set
            {
                void childPropertyChangedHandler(object sender, PropertyChangedEventArgs e)
                {
                    OnPropertyChanged();
                }
                if (currentStellarBody != null) { currentStellarBody.PropertyChanged -= childPropertyChangedHandler; }
                if (value != null) { value.PropertyChanged += childPropertyChangedHandler; }
                currentStellarBody = value;
                OnPropertyChanged();
            }
        }
        private Body currentStellarBody;

        [CanBeNull]
        public FleetCarrier FleetCarrier
        {
            get => _fleetCarrier;
            set
            {
                void childPropertyChangedHandler(object sender, PropertyChangedEventArgs e)
                {
                    OnPropertyChanged();
                }
                if (_fleetCarrier != null) { _fleetCarrier.PropertyChanged -= childPropertyChangedHandler; }
                if (value != null) { value.PropertyChanged += childPropertyChangedHandler; }

                _fleetCarrier = value;
                OnPropertyChanged();
            }
        }
        private FleetCarrier _fleetCarrier;

        [CanBeNull]
        public FleetCarrier SquadronCarrier
        {
            get => _squadronCarrier;
            set
            {
                void childPropertyChangedHandler ( object sender, PropertyChangedEventArgs e )
                {
                    OnPropertyChanged();
                }
                if ( _squadronCarrier != null )
                { _squadronCarrier.PropertyChanged -= childPropertyChangedHandler; }
                if ( value != null )
                { value.PropertyChanged += childPropertyChangedHandler; }

                _squadronCarrier = value;
                OnPropertyChanged();
            }
        }
        private FleetCarrier _squadronCarrier;

        [CanBeNull]
        public Ship CurrentShip
        {
            get => _currentShip;
            set
            {
                if (Equals(value, _currentShip)) return;
                void childPropertyChangedHandler(object sender, PropertyChangedEventArgs e)
                {
                    OnPropertyChanged();
                }
                if (_currentShip != null) { _currentShip.PropertyChanged -= childPropertyChangedHandler; }
                if (value != null) { value.PropertyChanged += childPropertyChangedHandler; }
                _currentShip = value;
                StatusService.Instance.CurrentShip = value;
                OnPropertyChanged();
            }
        }

        // Information from the last events of each type that we've received (for reference)
        public ConcurrentDictionary<string, Event> lastEventOfType { get; } = [ ];

        // Current vehicle of player
        public string Vehicle
        {
            get => vehicle;
            set
            {
                vehicle = value;
                OnPropertyChanged();
            }
        }
        private string vehicle = Constants.VEHICLE_SHIP;

        public readonly ObservableConcurrentDictionary<string, object> State = [ ];

        // The event queue
        private BlockingCollection<Event> eventQueue { get; } = [ ];
        private readonly CancellationTokenSource eventHandlerTS = new();
        private Task eventConsumerThread;

        private string multicrewVehicleHolder;

        private EDDI()
        {
            running = true;
            try
            {
                Logging.Info(Constants.EDDI_NAME + " " + Constants.EDDI_VERSION + " starting");
                DataProvider = DataProviderService.Create();

                var configuration = ConfigService.Instance.eddiConfiguration;
                Logging.Verbose = configuration.VerboseLogging;

                // We always start in normal space
                Environment = Constants.ENVIRONMENT_NORMAL_SPACE;

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

        public static bool ShouldUseTestEndpoints()
        {
#if DEBUG
            return true;
#else
            // use test endpoints if the game is in beta
            return Instance.gameIsBeta;
#endif
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

                signalSourceManager.Dispose();
                DataProvider.cts.Cancel();
                Utilities.TelemetryService.Telemetry.Stop();
                eventHandlerTS.Cancel();
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
                    var delay = DataProvider.unitTesting 
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

        public void enqueueEvent(Event @event)
        {
            if (@event is null) { return; }

            if (!eventQueue.IsAddingCompleted)
            {
                eventQueue.Add(@event);
            }

            // Start (or restart) our event handler thread (as long as we are not unit testing)
            if ( !eventHandlerTS.Token.IsCancellationRequested && 
                 ( eventConsumerThread is null || eventConsumerThread?.Status >= TaskStatus.RanToCompletion ) && 
                 !DataProvider.unitTesting )
            {
                eventConsumerThread?.Dispose();
                eventConsumerThread = Task.Run(dequeueEventsAsync, eventHandlerTS.Token);
            }
        }

        private async Task dequeueEventsAsync()
        {
            try
            {
                foreach (var @event in eventQueue.GetConsumingEnumerable(eventHandlerTS.Token))
                {
                    await HandleEventAsync( @event ).ConfigureAwait(false);
                    await Task.Yield();
                }
            }
            catch ( TaskCanceledException )
            {
                // Task canceled. Mark this collection as not accepting any new items.
                eventQueue.CompleteAdding();
            }
        }

        internal async Task HandleEventAsync ( Event @event )
        {
            if ( @event != null )
            {
                // Event handling is disabled when running a legacy game version.
                if ( GameVersion != null && GameVersion < minGameVersion && @event is not FileHeaderEvent ) { return; }

                try
                {
                    Logging.Debug( $"Handling event: {@event.type}", @event );

                    // We have some additional processing to do for a number of events
                    var passEvent = true;
                    if ( @event is FileHeaderEvent fileHeaderEvent )
                    {
                        passEvent = eventFileHeader( fileHeaderEvent );
                    }
                    else if ( @event is LocationEvent locationEvent )
                    {
                        passEvent = await eventLocationAsync( locationEvent ).ConfigureAwait( false );
                    }
                    else if ( @event is DockedEvent dockedEvent )
                    {
                        passEvent = await eventDockedAsync( dockedEvent ).ConfigureAwait( false );
                    }
                    else if ( @event is UndockedEvent undockedEvent )
                    {
                        passEvent = eventUndocked( undockedEvent );
                    }
                    else if ( @event is DockingRequestedEvent dockingRequestedEvent )
                    {
                        passEvent = eventDockingRequested( dockingRequestedEvent );
                    }
                    else if ( @event is TouchdownEvent touchdownEvent )
                    {
                        passEvent = await eventTouchdownAsync( touchdownEvent ).ConfigureAwait( false );
                    }
                    else if ( @event is LiftoffEvent liftoffEvent )
                    {
                        passEvent = await eventLiftoffAsync( liftoffEvent ).ConfigureAwait( false );
                    }
                    else if ( @event is FSDEngagedEvent fsdEngagedEvent )
                    {
                        passEvent = await eventFSDEngagedAsync( fsdEngagedEvent ).ConfigureAwait( false );
                    }
                    else if ( @event is FSDTargetEvent fsdTargetEvent )
                    {
                        passEvent = await eventFSDTargetAsync( fsdTargetEvent ).ConfigureAwait( false );
                    }
                    else if ( @event is JumpedEvent jumpedEvent )
                    {
                        passEvent = await eventJumpedAsync( jumpedEvent ).ConfigureAwait( false );
                    }
                    else if ( @event is EnteredSupercruiseEvent enteredSupercruiseEvent )
                    {
                        passEvent = await eventEnteredSupercruiseAsync( enteredSupercruiseEvent )
                            .ConfigureAwait( false );
                    }
                    else if ( @event is EnteredNormalSpaceEvent enteredNormalSpaceEvent )
                    {
                        passEvent = await eventEnteredNormalSpaceAsync( enteredNormalSpaceEvent )
                            .ConfigureAwait( false );
                    }
                    else if ( @event is CommanderContinuedEvent commanderContinuedEvent )
                    {
                        passEvent = eventCommanderContinued( commanderContinuedEvent );
                    }
                    else if ( @event is CrewJoinedEvent crewJoinedEvent )
                    {
                        passEvent = eventCrewJoined( crewJoinedEvent );
                    }
                    else if ( @event is CrewLeftEvent )
                    {
                        passEvent = eventCrewLeft();
                    }
                    else if ( @event is EnteredCQCEvent )
                    {
                        passEvent = eventEnteredCQC();
                    }
                    else if ( @event is SRVLaunchedEvent )
                    {
                        passEvent = eventSRVLaunched();
                    }
                    else if ( @event is SRVDockedEvent )
                    {
                        passEvent = eventSRVDocked();
                    }
                    else if ( @event is FighterLaunchedEvent fighterLaunchedEvent )
                    {
                        passEvent = eventFighterLaunched( fighterLaunchedEvent );
                    }
                    else if ( @event is FighterDockedEvent )
                    {
                        passEvent = eventFighterDocked();
                    }
                    else if ( @event is StarScannedEvent starScannedEvent )
                    {
                        passEvent = await eventStarScannedAsync( starScannedEvent ).ConfigureAwait( false );
                    }
                    else if ( @event is BodyScannedEvent bodyScannedEvent )
                    {
                        passEvent = await eventBodyScannedAsync( bodyScannedEvent ).ConfigureAwait( false );
                    }
                    else if ( @event is BodyMappedEvent bodyMappedEvent )
                    {
                        passEvent = await eventBodyMappedAsync( bodyMappedEvent ).ConfigureAwait( false );
                    }
                    else if ( @event is RingHotspotsEvent ringHotspotsEvent )
                    {
                        passEvent = await eventRingHotspotsAsync( ringHotspotsEvent ).ConfigureAwait( false );
                    }
                    else if ( @event is VehicleDestroyedEvent )
                    {
                        passEvent = eventVehicleDestroyed();
                    }
                    else if ( @event is NearSurfaceEvent nearSurfaceEvent )
                    {
                        passEvent = await eventNearSurfaceAsync( nearSurfaceEvent ).ConfigureAwait( false );
                    }
                    else if ( @event is FriendsEvent friendsEvent )
                    {
                        passEvent = eventFriends( friendsEvent );
                    }
                    else if ( @event is MarketEvent marketEvent )
                    {
                        passEvent = await eventMarketAsync( marketEvent ).ConfigureAwait( false );
                    }
                    else if ( @event is OutfittingEvent outfittingEvent )
                    {
                        passEvent = await eventOutfittingAsync( outfittingEvent ).ConfigureAwait( false );
                    }
                    else if ( @event is ShipyardEvent shipyardEvent )
                    {
                        passEvent = await eventShipyardAsync( shipyardEvent ).ConfigureAwait( false );
                    }
                    else if ( @event is DiscoveryScanEvent discoveryScanEvent )
                    {
                        passEvent = await eventDiscoveryScanAsync( discoveryScanEvent ).ConfigureAwait( false );
                    }
                    else if ( @event is SystemScanComplete systemScanComplete )
                    {
                        passEvent = await eventSystemScanCompleteAsync( systemScanComplete ).ConfigureAwait( false );
                    }
                    else if ( @event is CarrierJumpEngagedEvent carrierJumpEngagedEvent )
                    {
                        passEvent = await eventCarrierJumpEngagedAsync( carrierJumpEngagedEvent ).ConfigureAwait( false );
                    }
                    else if ( @event is CarrierJumpedEvent carrierJumpedEvent )
                    {
                        passEvent = await eventCarrierJumpedAsync( carrierJumpedEvent ).ConfigureAwait( false );
                    }
                    else if ( @event is DisembarkEvent disembarkEvent )
                    {
                        passEvent = await eventDisembarkAsync( disembarkEvent ).ConfigureAwait(false);
                    }
                    else if ( @event is EmbarkEvent embarkEvent )
                    {
                        passEvent = eventEmbark( embarkEvent );
                    }
                    else if ( @event is UnderAttackEvent underAttackEvent )
                    {
                        passEvent = eventUnderAttack( underAttackEvent );
                    }
                    else if ( @event is SettlementApproachedEvent settlementApproachedEvent )
                    {
                        passEvent = eventSettlementApproached( settlementApproachedEvent );
                    }
                    else if ( @event is SignalDetectedEvent signalDetectedEvent )
                    {
                        passEvent = eventSignalDetected( signalDetectedEvent );
                    }

                    // Additional processing is over, send to the event monitors and responders if required
                    if ( passEvent )
                    {
                        await OnEventAsync( @event ).ConfigureAwait( false );
                    }

                    lastEventOfType[ @event.type ] = @event;
                }
                catch ( Exception ex )
                {
                    Logging.Error( $"EDDI core failed to handle {@event.type} event {@event.raw}.", ex );

                    // Even if an error occurs, we still need to pass the raw data 
                    // to the EDDN responder to maintain it's integrity and to the Inara / EDSM reponders to keep external services up-to-date.
                    await Instance.ObtainResponder( "EDDN Responder" ).HandleAsync( @event ).ConfigureAwait( false );
                    await Instance.ObtainResponder( "EDSM Responder" ).HandleAsync( @event ).ConfigureAwait( false );
                    await Instance.ObtainResponder( "Inara Responder" ).HandleAsync( @event ).ConfigureAwait( false );
                }
            }
        }

        private async Task<bool> eventRingHotspotsAsync ( RingHotspotsEvent @event )
        {
            var ring = CurrentStarSystem?.bodies?
                .Where(b => b.rings is { } list && list.Count > 0 )
                .SelectMany(b => b.rings)
                .FirstOrDefault(r => r.name == @event.bodyname);
            if ( ring != null )
            {
                ring.mapped = @event.timestamp;
                ring.hotspots = @event.hotspots;
                await DataProvider.SaveStarSystemAsync( CurrentStarSystem ).ConfigureAwait(false);
            }

            return true;
        }

        internal bool eventSignalDetected ( SignalDetectedEvent @event )
        {
            if ( Instance.CurrentStarSystem != null &&
                 Instance.CurrentStarSystem.systemAddress == @event.systemAddress )
            {
                // If more signal detected events are have been received and are waiting to be processed, simply enqueue the current signal source. Otherwise, batch add the signals to the star system.
                if ( StarSystemSignalSourceManager.newSignalSources.TryGetValue( @event.systemAddress, out var newSignalSources ) )
                {
                    @event.unique =
                        !Instance.CurrentStarSystem.signalsources.Contains( @event.signalSource.localizedName ) &&
                        !newSignalSources.Select( s => s.localizedName ).Contains( @event.signalSource.localizedName );
                    newSignalSources.Add( @event.signalSource );
                }
                else
                {
                    @event.unique = true;
                    newSignalSources = [ @event.signalSource ];
                    StarSystemSignalSourceManager.newSignalSources.Add( @event.systemAddress, newSignalSources );
                }

                if ( !eventQueue.Any( e => e is SignalDetectedEvent ) )
                {
                    Instance.CurrentStarSystem.AddOrUpdateSignalSources( newSignalSources );
                    newSignalSources.Clear();
                }

                return true;
            }

            return false;
        }

        private bool eventSettlementApproached(SettlementApproachedEvent settlementApproachedEvent)
        {
            if (CurrentStarSystem?.systemAddress == settlementApproachedEvent.systemAddress
                && settlementApproachedEvent.marketId != null )
            {
                var station = CurrentStarSystem?.stations.FirstOrDefault(s => s.marketId == settlementApproachedEvent.marketId);
                if (station is null)
                {
                    // This station is unknown to us, might not be in our data source or we might not have connectivity.  Use a placeholder
                    station = new Station
                    {
                        name = settlementApproachedEvent.name,
                        marketId = settlementApproachedEvent.marketId,
                        systemname = CurrentStarSystem?.systemname,
                        systemAddress = settlementApproachedEvent.systemAddress
                    };
                    CurrentStarSystem?.AddOrUpdateStation( station );
                }
                station.Faction = settlementApproachedEvent.controllingFaction;
                station.stationServices = settlementApproachedEvent.stationServices;
                station.economyShares = settlementApproachedEvent.economyShares;
            }
            return true;
        }

        private bool eventUnderAttack(UnderAttackEvent underAttackEvent)
        {
            // Suppress repetitious `Under attack` events when loading or
            // when the target has already been reported as under attack within the last 10 seconds.
            var passEvent = !(underAttackEvent.fromLoad || (
                lastEventOfType.TryGetValue( underAttackEvent.type, out var ev ) && ev is UnderAttackEvent lastEvent
                && lastEvent.target == underAttackEvent.target
                && ( underAttackEvent.timestamp - lastEvent.timestamp ).TotalSeconds < 10
            ));
            return passEvent;
        }

        private async Task<bool> eventDisembarkAsync(DisembarkEvent @event) 
        {
            Vehicle = Constants.VEHICLE_LEGS;
            Logging.Info($"Disembarked to {Vehicle}");

            if ( @event.onplanet != true ) { return true; }
            await updateCurrentStellarBodyAsync( @event.bodyname, @event.bodyId, @event.systemname, @event.systemAddress )
                .ConfigureAwait( false );
            var body = CurrentStarSystem?.BodyWithID( @event.bodyId );
            if ( body != null && body.alreadyfootfalled == false )
            {
                // This is a first footfall event
                @event.firstfootfall = true;
                body.alreadyfootfalled = true;
                body.footfalledDateTime = @event.timestamp;
                await DataProvider.SaveStarSystemAsync( CurrentStarSystem ).ConfigureAwait( false );
            }

            return true;
        }

        private bool eventEmbark(EmbarkEvent embarkEvent) 
        {
            if (embarkEvent.tomulticrew)
            {
                Vehicle = Constants.VEHICLE_MULTICREW;
            }
            if (embarkEvent.toship)
            {
                Vehicle = Constants.VEHICLE_SHIP;
            }
            if (embarkEvent.tosrv)
            {
                Vehicle = Constants.VEHICLE_SRV;
            }
            if (embarkEvent.totransport)
            {
                Vehicle = Constants.VEHICLE_TAXI;
            }
            Logging.Info($"Embarked to {Vehicle}");
            return true;
        }

        private async Task<bool> eventCarrierJumpEngagedAsync( CarrierJumpEngagedEvent @event )
        {
            // Update our current environment, vehicle, and station information if we are still docked at the carrier
            if (Environment == Constants.ENVIRONMENT_DOCKED && @event.carrierID == CurrentStation?.marketId)
            {
                // We are in witch space and in the ship.
                @event.docked = true;
                Environment = Constants.ENVIRONMENT_WITCH_SPACE;
                Vehicle = Constants.VEHICLE_SHIP;

                // Make sure we have at least basic information about the destination star system
                NextStarSystem = await DataProvider.GetOrCreateStarSystemAsync( @event.systemAddress, @event.systemname ).ConfigureAwait(false);

                // Remove the carrier from its prior location in the origin system so that we can re-save it with a new location
                CurrentStarSystem?.RemoveStation( @event.carrierID );

                // Set the destination system as the current star system
                await updateCurrentSystemAsync( @event.systemname, @event.systemAddress ).ConfigureAwait(false);

                // Update our station information
                CurrentStation = CurrentStarSystem?.stations.FirstOrDefault(s => s.marketId == @event.carrierID) ?? new Station();
                CurrentStation.marketId = @event.carrierID;
                CurrentStation.systemname = @event.systemname;
                CurrentStation.systemAddress = @event.systemAddress;

                // Add the carrier to the destination system
                CurrentStarSystem?.AddOrUpdateStation(CurrentStation);
            }
            else if (!string.IsNullOrEmpty(@event.originSystemName))
            {
                // Remove the carrier from its prior location in the origin system so that we can re-save it with a new location
                var originStarSystem = await DataProvider.GetOrFetchStarSystemAsync(@event.originSystemAddress ).ConfigureAwait(false);
                var carrier = originStarSystem?.stations.FirstOrDefault(s => s.marketId == @event.carrierID);
                originStarSystem?.RemoveStation( @event.carrierID );
                // Save the carrier to the updated star system
                if ( carrier != null)
                {
                    carrier.systemname = @event.systemname;
                    carrier.systemAddress = @event.systemAddress;
                    if (@event.systemAddress == CurrentStarSystem?.systemAddress)
                    {
                        CurrentStarSystem?.AddOrUpdateStation( carrier );
                        await DataProvider.SaveStarSystemAsync( originStarSystem ).ConfigureAwait( false );
                    }
                    else
                    {
                        var updatedStarSystem = await DataProvider.GetOrCreateStarSystemAsync( @event.systemAddress, @event.systemname ).ConfigureAwait(false);
                        updatedStarSystem.AddOrUpdateStation( carrier);
                        await DataProvider.SaveStarSystemAsync( updatedStarSystem ).ConfigureAwait( false );
                    }
                }
            }

            return true;
        }

        private async Task<bool> eventCarrierJumpedAsync( CarrierJumpedEvent @event )
        {
            Logging.Info( "Carrier jumped to: " + @event.systemname );
            
            if ( @event.docked || @event.onFoot )
            {
                // We are either docked and in a ship or on foot and in normal space.
                Environment = @event.docked ? Constants.ENVIRONMENT_DOCKED : Constants.ENVIRONMENT_NORMAL_SPACE;
                Vehicle = @event.docked ? Constants.VEHICLE_SHIP : Constants.VEHICLE_LEGS;

                // Remove the carrier from its prior location (of any) so that we can re-save it with a new location
                // If we haven't already updated our current star system, the carrier should be in `CurrentStarSystem`. If we have, it should be in `LastStarSystem`.

                // There's a journal bug here where carrier market information is missing if we are on foot but present if we are docked
                // so we fall back to our saved FleetCarrier object information if event information is missing.
                var carrierID = @event.carrierID ?? ( @event.carrierType == StationModel.FleetCarrier
                    ? FleetCarrier?.carrierID
                    : @event.carrierType == StationModel.SquadronCarrier
                        ? SquadronCarrier?.carrierID
                        : null );
                if ( carrierID != @event.carrierID )
                {
                    @event.carrierID = carrierID;
                }
                
                var carrierCallsign = @event.carriername ?? FleetCarrier?.callsign;

                // Remove the carrier from the current star system or last star system
                CurrentStation = CurrentStarSystem?.stations.FirstOrDefault( s => 
                    s.marketId == carrierID || s.name == carrierCallsign );
                if ( CurrentStation != null )
                {
                    CurrentStarSystem?.RemoveStation( carrierID ?? 0 );
                }
                else if ( LastStarSystem != null )
                {
                    CurrentStation = LastStarSystem.stations.FirstOrDefault( s =>
                        s.marketId == carrierID || s.name == carrierCallsign );
                    if ( CurrentStation != null )
                    {
                        LastStarSystem.RemoveStation( carrierID ?? 0 );
                        await DataProvider.SaveStarSystemAsync( LastStarSystem ).ConfigureAwait(false);
                    }
                }

                // If the carrier is not found in the current or last star system but a fleet carrier object is present,
                // we can generate current station information from the FleetCarrier object
                CurrentStation ??= FleetCarrier?.Market?.UpdateStation( @event.timestamp, new Station() );

                // Update current station properties
                if ( CurrentStation != null )
                {
                    CurrentStation.systemname = @event.systemname;
                    CurrentStation.systemAddress = @event.systemAddress;
                    CurrentStation.name = carrierCallsign;
                    CurrentStation.marketId = carrierID;
                    CurrentStation.Faction = @event.carrierFaction;
                    CurrentStation.Model = @event.carrierType;
                    CurrentStation.economyShares = @event.carrierEconomies;
                    CurrentStation.stationServices = @event.carrierServices;
                } 

                // Update our current star system and carrier location
                await updateCurrentSystemAsync( @event.systemname, @event.systemAddress ).ConfigureAwait(false);

                // Update our system properties
                if ( CurrentStarSystem is null ) { return false; }

                CurrentStarSystem.x = @event.x;
                CurrentStarSystem.y = @event.y;
                CurrentStarSystem.z = @event.z;

                // Update Thargoid war data, when available
                CurrentStarSystem.ThargoidWar = @event.ThargoidWar;

                if ( currentStation != null )
                {
                    // Add our carrier to the new current star system
                    CurrentStarSystem.AddOrUpdateStation( CurrentStation );
                }

                // Update the mutable system data from the journal
                if ( @event.population != null )
                {
                    CurrentStarSystem.population = @event.population;
                    CurrentStarSystem.Economies = [ @event.systemEconomy, @event.systemEconomy2 ];
                    CurrentStarSystem.securityLevel = @event.securityLevel;
                    CurrentStarSystem.Faction = @event.controllingsystemfaction;
                }

                // Update system faction data if available
                if ( @event.factions != null )
                {
                    CurrentStarSystem.factions = @event.factions;
                    CurrentStarSystem.conflicts = @event.conflicts;

                    // Update station controlling faction data
                    foreach ( var station in CurrentStarSystem.stations )
                    {
                        var stationFaction = @event.factions
                            .FirstOrDefault( f => f.name == station.Faction?.name );
                        if ( stationFaction != null )
                        {
                            station.Faction = stationFaction;
                        }
                    }
                }

                // (When pledged) Powerplay information
                CurrentStarSystem.Power = @event.Power;
                CurrentStarSystem.NearbyPowers = @event.NearbyPowers;
                CurrentStarSystem.powerState = @event.PowerState ?? CurrentStarSystem.powerState;
                CurrentStarSystem.powerAcquisitionProgress = @event.powerAcquisitionProgress;
                CurrentStarSystem.powerControlProgress = @event.powerControlProgress;
                CurrentStarSystem.powerReinforcementControlPoints = @event.powerReinforcementControlPoints;
                currentStarSystem.powerUnderminingControlPoints = @event.powerUnderminingControlPoints;

                // Update to most recent information
                CurrentStarSystem.visitLog.Add( @event.timestamp );
                CurrentStarSystem.updatedat = Dates.fromDateTimeToSeconds( @event.timestamp );
                await DataProvider.SaveStarSystemAsync( CurrentStarSystem ).ConfigureAwait(false);

                // Kick off the profile refresh if the companion API is available
                if (CompanionAppService.Instance.CurrentState == CompanionAppService.State.Authorized && carrierID != null)
                {
                    // Refresh station data
                    if (@event.fromLoad) { return true; } // Don't fire this event when loading pre-existing logs

                    conditionallyRefreshStationProfileAsync( @event.systemname, @event.carrierID ?? 0 )
                        .SafeFireAndForget( ex => Logging.Error( ex.Message, ex ) );
                }
            }
            else
            {
                // We shouldn't be here - `the CarrierJump` event is only supposed to be written when docked with a fleet carrier as it jumps.
                Environment = Constants.ENVIRONMENT_NORMAL_SPACE;
                Logging.Error("Whoops! CarrierJump event recorded when not docked.", @event);
                throw new NotImplementedException();
            }

            return true;
        }

        internal async Task<bool> eventSystemScanCompleteAsync(SystemScanComplete @event)
        {
            // There is a bug in the player journal output (as of player journal v.25) that can cause the `SystemScanComplete` event to fire multiple times 
            // in rapid succession when performing a system scan of a star system with only stars and no other bodies.
            if (CurrentStarSystem != null)
            {
                if (CurrentStarSystem.systemScanCompleted)
                {
                    // We will suppress repetitions of the event within the same star system.
                    return false;
                }
                CurrentStarSystem.systemScanCompleted = true;
                // Update any bodies that aren't yet recorded as scanned (these were likely scanned while EDDI was not running)
                var bodiesToUpdate = new List<Body>();
                foreach (var body in CurrentStarSystem.bodies.Where(b => b.scannedDateTime is null))
                {
                    body.scannedDateTime = @event.timestamp;
                    bodiesToUpdate.Add(body);
                }
                if ( bodiesToUpdate.Count > 0 ) { CurrentStarSystem.AddOrUpdateBodies(bodiesToUpdate); }
                // Save the updated star system data
                await DataProvider.SaveStarSystemAsync(CurrentStarSystem).ConfigureAwait(false);
            }
            return true;
        }

        private async Task<bool> eventDiscoveryScanAsync(DiscoveryScanEvent @event)
        {
            if (CurrentStarSystem != null)
            {
                CurrentStarSystem.totalbodies = @event.totalbodies;

                if (@event.progress == 100 && CurrentStarSystem.scannedbodies < @event.totalbodies) // Fully scanned system, make sure that all bodies are marked as scanned
                {
                    // Update any bodies that aren't yet recorded as scanned (these were likely scanned while EDDI was not running)
                    var bodiesToUpdate = new List<Body>();
                    foreach (var body in CurrentStarSystem.bodies.Where(b => b.scannedDateTime is null))
                    {
                        body.scannedDateTime = @event.timestamp;
                        bodiesToUpdate.Add(body);
                    }
                    if ( bodiesToUpdate.Count > 0 ) { CurrentStarSystem.AddOrUpdateBodies(bodiesToUpdate); }
                }

                await DataProvider.SaveStarSystemAsync(CurrentStarSystem).ConfigureAwait(false);
            }
            return true;
        }

        internal bool eventFriends ( FriendsEvent @event )
        {
            var passEvent = false;
            var friend = new Friend
            {
                name = @event.name,
                status = @event.status
            };

            // Does this friend exist in our friends list?
            var commanderMonitorVariables = ObtainMonitor( "Commander Monitor" ).GetVariables();
            if ( commanderMonitorVariables.TryGetValue( "cmdr", out var tuple ) && tuple.Item2 is Commander Cmdr )
            {
                var index = Cmdr.friends.FindIndex( f => f.name == @event.name );
                if ( index >= 0 )
                {
                    if ( Cmdr.friends[ index ].status != @event.status )
                    {
                        // This is a known friend with a revised status: replace in situ (this is more efficient than removing and re-adding).
                        Cmdr.friends[ index ] = friend;
                        passEvent = true;
                    }
                }
                else
                {
                    // This is a new friend, add them to the list
                    Cmdr.friends.Add( friend );
                }
            }

            return passEvent;
        }

        private async Task OnEventAsync ( Event @event )
        {
            try
            {
                // We send the event to all monitors to ensure that their info is up-to-date
                await passToMonitorPreHandlersAsync( @event ).ConfigureAwait( false );

                // Now we pass the data to the responders.
                // Responders must not change global states.
                await passToResponders( @event ).ConfigureAwait( false );

                // We also pass the event to all active monitors in case they have asynchronous follow-on work, waiting for all to complete
                await passToMonitorPostHandlers( @event ).ConfigureAwait( false );
            }
            catch ( Exception ex )
            {
                Logging.Error( "Failed to pass event to all monitors and responders", ex );
            }
        }

        private async Task passToMonitorPreHandlersAsync ( Event @event )
        {
            // All changes to state must be handled here
            var monitorTasks = new List<Task>();
            foreach ( var monitor in activeMonitors)
            {
                var monitorTask = monitor.PreHandleAsync(@event);
                monitorTask.ContinueWith( task =>
                    {
                        if ( task.IsFaulted )
                        {
                            Logging.Error(
                                $"{monitor.MonitorName()} failed to handle {@event.type} event {@event.raw}",
                                task.Exception );
                        }
                    }, TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously )
                    .SafeFireAndForget( e => Logging.Error( e.Message, e ) );
                monitorTasks.Add( monitorTask );
            }

            try
            {
                await Task.WhenAll( monitorTasks.ToArray() );
            }
            catch ( TaskCanceledException )
            {
                // Task(s) cancelled. Nothing to do here.
            }
        }

        private async Task passToResponders ( Event @event )
        {
            // Wait for all to complete
            var responderTasks = new List<Task>();
            foreach ( var responder in activeResponders )
            {
                var responderTask = responder.HandleAsync( @event );
                responderTask.ContinueWith( task =>
                    {
                        if ( task.IsFaulted )
                        {
                            Logging.Error(
                                $"{responder.ResponderName()} failed to handle {@event.type} event {@event.raw}",
                                task.Exception );
                        }
                    }, TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously )
                    .SafeFireAndForget( e => Logging.Error( e.Message, e ) );
                responderTasks.Add( responderTask );
            }

            try
            {
                await Task.WhenAll( responderTasks.ToArray() );
            }
            catch ( TaskCanceledException )
            {
                // Task(s) cancelled. Nothing to do here.
            }
        }

        private async Task passToMonitorPostHandlers ( Event @event )
        {
            // Pass back to monitors for follow-on work, wait for all to complete
            var monitorTasks = new List<Task>();
            foreach ( var monitor in activeMonitors )
            {
                var monitorTask = monitor.PostHandleAsync( @event );
                monitorTask.ContinueWith( task =>
                    {
                        if ( task.IsFaulted )
                        {
                            Logging.Error(
                                $"{monitor.MonitorName()} failed to post-handle {@event.type} event {@event.raw}",
                                task.Exception );
                        }
                    }, TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously )
                    .SafeFireAndForget( e => Logging.Error( e.Message, e ) );
                monitorTasks.Add( monitorTask );
            }

            try
            {
                await Task.WhenAll( monitorTasks.ToArray() );
            }
            catch ( TaskCanceledException )
            {
                // Task(s) cancelled. Nothing to do here.
            }
        }

        internal async Task<bool> eventLocationAsync( LocationEvent theEvent )
        {
            Logging.Info("Location StarSystem: " + theEvent.systemname);

            // Set our vehicle
            if (theEvent.taxi)
            {
                Vehicle = Constants.VEHICLE_TAXI;
            }
            else if (theEvent.multicrew)
            {
                Vehicle = Constants.VEHICLE_MULTICREW;
            }
            else if (theEvent.inSRV)
            {
                Vehicle = Constants.VEHICLE_SRV;
            }
            else if (theEvent.onFoot)
            {
                Vehicle = Constants.VEHICLE_LEGS;
            }
            // If none of these are true we may either be in our ship or in a fighter.
            Logging.Info($"Vehicle mode is {Vehicle}");

            await updateCurrentSystemAsync( theEvent.systemname, theEvent.systemAddress ).ConfigureAwait(false);
            if ( CurrentStarSystem is null ) { return false; }

            // Always update the current system with the current co-ordinates, just in case things have changed or coordinates are not yet known
            CurrentStarSystem.x = theEvent.x;
            CurrentStarSystem.y = theEvent.y;
            CurrentStarSystem.z = theEvent.z;

            // Update Thargoid war data, as applicable
            CurrentStarSystem.ThargoidWar = theEvent.ThargoidWar;

            // Update the mutable system data from the journal
            if ( theEvent.population != null )
            {
                CurrentStarSystem.population = theEvent.population;
                CurrentStarSystem.Economies = [ theEvent.Economy, theEvent.Economy2 ];
                CurrentStarSystem.securityLevel = theEvent.securityLevel;
                CurrentStarSystem.Faction = theEvent.controllingsystemfaction;
            }

            // Update system faction data if available
            if ( theEvent.factions != null )
            {
                CurrentStarSystem.factions = theEvent.factions;
                CurrentStarSystem.conflicts = theEvent.conflicts;

                // Update station controlling faction data
                foreach ( var station in CurrentStarSystem.stations )
                {
                    var stationFaction = theEvent.factions.FirstOrDefault( f => f.name == station?.Faction?.name );
                    if ( stationFaction != null )
                    {
                        station.Faction = stationFaction;
                    }
                }
            }

            // (When pledged) Powerplay information
            CurrentStarSystem.Power = theEvent.Power;
            CurrentStarSystem.NearbyPowers = theEvent.NearbyPowers;
            CurrentStarSystem.powerState = theEvent.PowerState ?? CurrentStarSystem.powerState;
            CurrentStarSystem.powerAcquisitionProgress = theEvent.powerAcquisitionProgress;
            CurrentStarSystem.powerControlProgress = theEvent.powerControlProgress;
            CurrentStarSystem.powerReinforcementControlPoints = theEvent.powerReinforcementControlPoints;
            CurrentStarSystem.powerUnderminingControlPoints = theEvent.powerUnderminingControlPoints;

            if ( theEvent.docked )
            {
                // Update the station
                Logging.Debug( "Now at station " + theEvent.station );
                var station = CurrentStarSystem.stations.Find( s => s.marketId == theEvent.marketId );
                if ( station == null )
                {
                    // This station is unknown to us, might not be in our data source or we might not have connectivity.  Use a placeholder
                    station = new Station { name = theEvent.station, marketId = theEvent.marketId, systemname = theEvent.systemname, systemAddress = theEvent.systemAddress };
                    CurrentStarSystem.AddOrUpdateStation( station );
                }

                // We are docked
                Environment = Constants.ENVIRONMENT_DOCKED;

                // If we're not in a taxi or multicrew then we're in our own ship.
                if ( !theEvent.taxi && !theEvent.multicrew ) { Vehicle = Constants.VEHICLE_SHIP; }

                // Update station properties known from this event
                station.systemAddress = theEvent.systemAddress;
                station.Faction = theEvent.controllingstationfaction;
                station.Model = theEvent.stationModel;
                station.distancefromstar = theEvent.distancefromstar;

                CurrentStation = station;

                // Kick off the profile refresh if the companion API is available
                if ( CompanionAppService.Instance.CurrentState == CompanionAppService.State.Authorized && theEvent.marketId != null )
                {
                    // Refresh station data
                    if ( theEvent.fromLoad ) { return true; } // Don't fire this event when loading pre-existing logs

                    conditionallyRefreshStationProfileAsync( theEvent.systemname, theEvent.marketId ?? 0 )
                        .SafeFireAndForget( ex => Logging.Error( ex.Message, ex ) );
                }
            }
            else if ( theEvent.latitude != null && theEvent.longitude != null )
            {
                Environment = Constants.ENVIRONMENT_LANDED;
            }
            else
            {
                Environment = Constants.ENVIRONMENT_NORMAL_SPACE;
            }

            if ( theEvent.bodyType == BodyType.Planet )
            {
                theEvent.bodyType = CurrentStarSystem.bodies.FirstOrDefault( b => b.bodyId != null && b.bodyId == theEvent.bodyId )?.bodyType ?? theEvent.bodyType;
            }
            if ( theEvent.bodyname != null && ( theEvent.bodyType == BodyType.Moon ||
                                                theEvent.bodyType == BodyType.Planet ) )
            {
                // Update the body 
                Logging.Debug( "Now at body " + theEvent.bodyname );
                await updateCurrentStellarBodyAsync( theEvent.bodyname, theEvent.bodyId, theEvent.systemname, theEvent.systemAddress ).ConfigureAwait(false);
            }

            // Update to most recent information
            CurrentStarSystem.updatedat = Dates.fromDateTimeToSeconds( theEvent.timestamp );
            await DataProvider.SaveStarSystemAsync( CurrentStarSystem ).ConfigureAwait(false);

            return true;
        }

        private bool eventDockingRequested(DockingRequestedEvent theEvent)
        {
            var passEvent = !string.IsNullOrEmpty(theEvent.station);
            var station = CurrentStarSystem?.stations.Find(s => s.name == theEvent.station);
            if (station is null && CurrentStarSystem != null)
            {
                // This station is unknown to us, might not be in our data source or we might not have connectivity.  Use a placeholder
                station = new Station
                {
                    name = theEvent.station,
                    marketId = theEvent.marketId,
                    systemname = CurrentStarSystem.systemname,
                    systemAddress = CurrentStarSystem.systemAddress
                };
                CurrentStarSystem?.AddOrUpdateStation( station );
            }

            if ( station != null )
            {
                station.Model = theEvent.stationDefinition;
                station.landingPads = theEvent.landingPads;
            }

            return passEvent;
        }

        private async Task<bool> eventDockedAsync ( DockedEvent @event )
        {
            await updateCurrentSystemAsync( @event.system, @event.systemAddress ).ConfigureAwait(false);

            if ( CurrentStarSystem == null ) { return false; }

            var station = CurrentStarSystem.stations.Find( s => s.marketId == @event.Station.marketId );
            if ( Environment == Constants.ENVIRONMENT_DOCKED && CurrentStation?.marketId == station?.marketId )
            {
                // We are already at this station
                Logging.Debug( $"Already at station {@event.station} ({@event.marketId})." );
                return false;
            }

            // Update the station
            Logging.Debug( $"Now at station {@event.station} ({@event.marketId})." );
            if ( station == null && @event.Station != null )
            {
                // This station is unknown to us
                station = @event.Station;
            }

            Environment = Constants.ENVIRONMENT_DOCKED;
            CurrentStarSystem.AddOrUpdateStation( station );
            CurrentStation = station;

            // Kick off the profile refresh if the companion API is available
            if ( CompanionAppService.Instance.CurrentState == CompanionAppService.State.Authorized && @event.marketId != null )
            {
                // Refresh station data
                if ( @event.fromLoad )
                {
                    return false;
                } // Don't fire this event when loading pre-existing logs or if we were already at this station

                conditionallyRefreshStationProfileAsync( @event.system, @event.marketId ?? 0 )
                    .SafeFireAndForget( ex => Logging.Error( ex.Message, ex ) );
            }

            return true;
        }

        private bool eventUndocked ( UndockedEvent @event )
        {
            Environment = Constants.ENVIRONMENT_NORMAL_SPACE;
            var station = CurrentStarSystem?.stations.Find( s => s.marketId == @event.marketId );
            if ( station != null )
            {
                station.marketUpdatedThisVisit = false;
                station.outfittingUpdatedThisVisit = false;
                station.shipyardUpdatedThisVisit = false;
            }

            CurrentStation = null;

            // Kick off the profile refresh if the companion API is available
            if ( CompanionAppService.Instance.CurrentState == CompanionAppService.State.Authorized &&
                 @event.marketId != null && CurrentStarSystem != null )
            {
                // Refresh station data
                // Don't fire this event when loading pre-existing logs or if we were already at this station
                if ( @event.fromLoad )
                {
                    return false;
                }

                conditionallyRefreshStationProfileAsync( CurrentStarSystem.systemname, @event.marketId ?? 0 )
                    .SafeFireAndForget( ex => Logging.Error( ex.Message, ex ) );
            }

            return true;
        }

        private async Task<bool> eventTouchdownAsync( TouchdownEvent @event )
        {
            await updateCurrentStellarBodyAsync( @event.bodyname, @event.bodyId, @event.systemname, @event.systemAddress ).ConfigureAwait(false);

            if (@event.taxi is true)
            {
                Vehicle = Constants.VEHICLE_TAXI;
            }
            else if (@event.multicrew is true)
            {
                Vehicle = Constants.VEHICLE_MULTICREW;
            }

            // Only pass on this event if our longitude and lattitude are set
            // (if not then this is probably being written prior to a `Location` event))
            if (@event.latitude != null && @event.longitude != null)
            {
                Environment = Constants.ENVIRONMENT_LANDED;
                if (@event.taxi is true)
                {
                    Vehicle = Constants.VEHICLE_TAXI;
                }
                else if (@event.multicrew is true)
                {
                    Vehicle = Constants.VEHICLE_MULTICREW;
                }
                else if (@event.playercontrolled)
                {
                    Vehicle = Constants.VEHICLE_SHIP;
                }
                else
                {
                    Vehicle = Constants.VEHICLE_SRV;
                }

                var body = CurrentStarSystem?.BodyWithID( @event.bodyId );
                if ( body != null && body.alreadyfootfalled == false )
                {
                    // We can be the first to set foot on this world
                    @event.canfirstfootfall = true;
                }

                return true;
            }
            
            Logging.Info($"Touchdown in {Vehicle}");
            return false;
        }

        private async Task<bool> eventLiftoffAsync( LiftoffEvent theEvent )
        {
            await updateCurrentStellarBodyAsync( theEvent.bodyname, theEvent.bodyId, theEvent.systemname, theEvent.systemAddress ).ConfigureAwait(false);

            Environment = Constants.ENVIRONMENT_NORMAL_SPACE;

            if (theEvent.taxi is true)
            {
                Vehicle = Constants.VEHICLE_TAXI;
            }
            else if (theEvent.multicrew is true)
            {
                Vehicle = Constants.VEHICLE_MULTICREW;
            }
            else if (theEvent.playercontrolled)
            {
                Vehicle = Constants.VEHICLE_SHIP;
            }
            else
            {
                Vehicle = Constants.VEHICLE_SRV;
            }
            Logging.Info($"Liftoff in {Vehicle}");
            return true;
        }

        private async Task<bool> eventMarketAsync(MarketEvent theEvent)
        {
            // Don't proceed if loading pre-existing logs
            if (theEvent.fromLoad) { return false; }

            // Don't proceed if the event data isn't what we expect
            if ( theEvent.system != CurrentStarSystem?.systemname) { return false; }

            var items = theEvent.info?.Items?
                .Select(q => q.ToCommodityMarketQuote())
                .ToList();

            if (theEvent.info?.Items?.Count == items?.Count && items != null) // We've successfully parsed all commodity items
            {
                // Update the current station commodities
                if (CurrentStation != null && CurrentStation?.marketId == theEvent.marketId)
                {
                    CurrentStation.commodities = items;
                    CurrentStation.commoditiesupdatedat = Dates.fromDateTimeToSeconds(theEvent.timestamp);

                    // Update the current station information in our backend DB
                    Logging.Debug("Star system information updated from remote server; updating local copy");
                    await DataProvider.SaveStarSystemAsync(CurrentStarSystem).ConfigureAwait(false);

                    // Post an update event for new market data
                    // Don't proceed if the data was already updated this visit
                    if ( !currentStation.marketUpdatedThisVisit )
                    {
                        currentStation.marketUpdatedThisVisit = true;
                        enqueueEvent( new MarketInformationUpdatedEvent( theEvent.timestamp, theEvent.marketId,
                            theEvent.station, theEvent.system, [ "market" ] ) { raw = theEvent.raw } );
                    }

                    return true;
                }
                else
                {
                    var station = CurrentStarSystem?.stations.FirstOrDefault(s => s.marketId == theEvent.marketId);
                    if (station != null)
                    {
                        station.commodities = theEvent.info.Items.Select(q => q.ToCommodityMarketQuote()).ToList();
                        station.commoditiesupdatedat = Dates.fromDateTimeToSeconds(theEvent.timestamp);
                        await DataProvider.SaveStarSystemAsync(CurrentStarSystem).ConfigureAwait(false);
                    }
                }
            }
            return false;
        }

        private async Task<bool> eventOutfittingAsync(OutfittingEvent theEvent)
        {
            // Don't proceed when loading pre-existing logs
            if (theEvent.fromLoad) { return false; }

            // Don't proceed if the event data isn't what we expect
            if ( theEvent.system != CurrentStarSystem?.systemname) { return false; }

            var modules = theEvent.info?.Items?
                .Select(EddiDataDefinitions.Module.FromOutfittingInfo)
                .Where(i => i != null)
                .ToList();

            if (theEvent.info?.Items?.Count == modules?.Count && modules != null) // We've successfully parsed all module items
            {
                // Update the current station outfitting
                if (CurrentStation?.marketId != null && CurrentStation?.marketId == theEvent.marketId)
                {
                    CurrentStation.outfitting = modules;
                    CurrentStation.outfittingupdatedat = Dates.fromDateTimeToSeconds(theEvent.timestamp);

                    // Update the current station information in our backend DB
                    Logging.Debug("Star system information updated from remote server; updating local copy");
                    await DataProvider.SaveStarSystemAsync(CurrentStarSystem).ConfigureAwait(false);

                    // Post an update event for new outfitting data
                    // Don't proceed if the data was already updated this visit
                    if ( !currentStation.outfittingUpdatedThisVisit )
                    {
                        currentStation.outfittingUpdatedThisVisit = true;
                        enqueueEvent( new MarketInformationUpdatedEvent( theEvent.timestamp, theEvent.marketId,
                            theEvent.station, theEvent.system, [ "outfitting" ] )
                        {
                            raw = theEvent.raw
                        } );
                    }

                    return true;
                }
                else
                {
                    var station = CurrentStarSystem?.stations.FirstOrDefault(s => s.marketId == theEvent.marketId);
                    if (station != null)
                    {
                        station.outfitting = modules;
                        station.outfittingupdatedat = Dates.fromDateTimeToSeconds(theEvent.timestamp);
                        await DataProvider.SaveStarSystemAsync(CurrentStarSystem).ConfigureAwait(false);
                    }
                }
            }
            return false;
        }

        private async Task<bool> eventShipyardAsync(ShipyardEvent theEvent)
        {
            // Don't proceed when loading pre-existing logs
            if (theEvent.fromLoad) { return false; }

            // Don't proceed if the event data isn't what we expect
            if (theEvent.system != CurrentStarSystem?.systemname) { return false; }

            var ships = theEvent.info?.PriceList?
                .Select(Ship.FromShipyardInfo)
                .Where(s => s != null)
                .ToList();

            if (theEvent.info?.PriceList?.Count == ships?.Count && ships != null) // We've successfully parsed all ship items
            {
                if (CurrentStation?.marketId != null && CurrentStation?.marketId == theEvent.marketId)
                {
                    // Update the current station shipyard
                    CurrentStation.shipyard = ships;
                    CurrentStation.shipyardupdatedat = Dates.fromDateTimeToSeconds(theEvent.timestamp);

                    // Update the current station information in our backend DB
                    await DataProvider.SaveStarSystemAsync(CurrentStarSystem).ConfigureAwait(false);

                    // Post an update event for new shipyard data
                    // Don't proceed if the data was already updated this visit
                    if ( !currentStation.shipyardUpdatedThisVisit )
                    {
                        currentStation.shipyardUpdatedThisVisit = true;
                        enqueueEvent( new MarketInformationUpdatedEvent( theEvent.timestamp, theEvent.marketId,
                            theEvent.station, theEvent.system, [ "shipyard" ] )
                        {
                            raw = theEvent.raw
                        } );
                    }

                    return true;
                }
                else
                {
                    var station = CurrentStarSystem?.stations.FirstOrDefault(s => s.marketId == theEvent.marketId);
                    if (station != null)
                    {
                        station.shipyard = ships;
                        station.shipyardupdatedat = Dates.fromDateTimeToSeconds(theEvent.timestamp);
                        await DataProvider.SaveStarSystemAsync(CurrentStarSystem).ConfigureAwait(false);
                    }
                }
            }
            return false;
        }

        internal async Task updateCurrentSystemAsync([NotNull] string systemName, ulong systemAddress )
        {
            try
            { 
                if ( string.IsNullOrEmpty(systemName) || CurrentStarSystem?.systemAddress == systemAddress )
                {
                    return;
                }
            
                if ( CurrentStarSystem != null )
                {
                    // Discard signal sources from star system we are leaving
                    CurrentStarSystem.signalSources = ImmutableList<SignalSource>.Empty;

                    // Unregister the old star system and stop managing its signal source expiries
                    signalSourceManager.Unregister( CurrentStarSystem );

                    // We have changed system so update the old one as to when we left
                    await DataProvider.SaveStarSystemAsync( CurrentStarSystem ).ConfigureAwait(false);
                }

                // Update the CurrentStarSystem to the one we are entering
                LastStarSystem = CurrentStarSystem;
                if ( NextStarSystem != null && NextStarSystem.systemAddress == systemAddress )
                {
                    CurrentStarSystem = NextStarSystem;
                    NextStarSystem = null;
                }
                else
                {
                    CurrentStarSystem = await DataProvider.GetOrCreateStarSystemAsync( systemAddress, systemName ).ConfigureAwait(false);
                }

                // Register our new star system and manage its signal source expiries
                signalSourceManager.Register( CurrentStarSystem );

                // If we've arrived at our destination system then clear it
                if ( destinationStarSystem?.systemAddress == currentStarSystem.systemAddress )
                {
                    await updateDestinationSystemAsync( null ).ConfigureAwait(false);
                }
            }
            catch ( Exception e )
            {
                Logging.Error(e.Message, e);
            }
        }

        private async Task updateCurrentStellarBodyAsync(string bodyName, int? bodyId, string systemName, ulong systemAddress )
        {
            // Make sure our system information is up to date
            await updateCurrentSystemAsync( systemName, systemAddress ).ConfigureAwait(false);

            // Update the body 
            if ( CurrentStarSystem != null)
            {
                var body = CurrentStarSystem.bodies?.Find(s => s.bodyname == bodyName);
                if (body == null)
                {
                    // This body is unknown to us, might not be in our 3rd party API data,
                    // or we might not have connectivity.  Use a placeholder 
                    body = new Body
                    {
                        bodyname = bodyName,
                        bodyId = bodyId,
                        systemname = systemName,
                        systemAddress = systemAddress,
                    };
                    CurrentStarSystem.AddOrUpdateBody(body);
                }
                CurrentStellarBody = body;
            }
        }

        internal async Task<bool> eventFSDEngagedAsync( FSDEngagedEvent @event )
        {
            // Keep track of our environment
            Environment = @event.target == "Supercruise" 
                ? Constants.ENVIRONMENT_SUPERCRUISE 
                : Constants.ENVIRONMENT_WITCH_SPACE;

            // Set the destination system as the current star system
            if ( @event.systemAddress != null )
            {
                await updateCurrentSystemAsync( @event.systemname, (ulong)@event.systemAddress ).ConfigureAwait(false);
            }

            // Remove information about the current station and stellar body 
            CurrentStation = null;
            CurrentStellarBody = null;

            return true;
        }

        private async Task<bool> eventFSDTargetAsync( FSDTargetEvent @event )
        {
            // Set and prepare data about the next star system
            NextStarSystem = await DataProvider.GetOrCreateStarSystemAsync( @event.systemAddress, @event.system ).ConfigureAwait(false);
            if (NextStarSystem != null && !NextStarSystem.bodies.Any(b => b.mainstar ?? false))
            {
                // This system is unknown to us, might not be recorded, or we might not have connectivity.  Use a placeholder main star
                var mainStar = new Body
                {
                    bodyType = BodyType.Star,
                    systemname = NextStarSystem.systemname,
                    systemAddress = nextStarSystem.systemAddress,
                    distance = 0M,
                    stellarclass = @event.starclass
                };
                NextStarSystem.AddOrUpdateBody(mainStar);
                await DataProvider.SaveStarSystemAsync(NextStarSystem).ConfigureAwait(false);
            }
            return true;
        }

        private bool eventFileHeader(FileHeaderEvent @event)
        {
            // Test whether we're in beta by checking the filename, version described by the header,
            // and certain version / build combinations. Test the most common situations first.
            gameIsBeta = @event.filename.Contains("Alpha") ||
                         @event.filename.Contains("Beta") ||
                         @event.version.Contains("Beta") ||
                         @event.version.Contains("Alpha") ||
                         (
                             @event.version.Contains("2.2") &&
                             (
                                 @event.build.Contains("r121645/r0") ||
                                 @event.build.Contains("r129516/r0")
                             )
                         );
            CompanionAppService.Instance.gameIsBeta = gameIsBeta;
            if (gameIsBeta)
            {
                Logging.Info("Beta game version detected");
            }

            gameBuild = @event.build;
            gameVersion = @event.version;

            return true;
        }

        internal async Task<bool> eventJumpedAsync( JumpedEvent theEvent )
        {
            bool passEvent;

            if ( theEvent.taxi is true )
            {
                Vehicle = Constants.VEHICLE_TAXI;
            }
            else if ( theEvent.multicrew is true )
            {
                Vehicle = Constants.VEHICLE_MULTICREW;
            }
            else
            {
                Vehicle = Constants.VEHICLE_SHIP;
            }

            if ( LastStarSystem?.systemAddress > 0 && LastStarSystem.systemAddress == theEvent.systemAddress )
            {
                // Thargoid Hyperdiction
                Logging.Info( $"Jump Interrupted: Hyperdicted in {theEvent.system}" );

                // After hyperdiction we are in normal space rather than supercruise
                Environment = Constants.ENVIRONMENT_NORMAL_SPACE;

                // Generate a hyperdiction event
                enqueueEvent(new HyperdictedEvent( theEvent.timestamp, theEvent.fuelused, theEvent.fuelremaining, theEvent.boostused, theEvent.taxi, theEvent.multicrew, theEvent.ThargoidWar ) { raw = null, fromLoad = theEvent.fromLoad } );

                passEvent = false;
            }
            else
            {
                // Normal FSD jump
                Logging.Info( "Jumped to " + theEvent.system );

                // After jump has completed we are always in supercruise
                Environment = Constants.ENVIRONMENT_SUPERCRUISE;

                passEvent = true;
            }

            await updateCurrentSystemAsync( theEvent.system, theEvent.systemAddress ).ConfigureAwait(false);
            if ( CurrentStarSystem is null ) { return false; }
            CurrentStarSystem.systemAddress = theEvent.systemAddress;
            CurrentStarSystem.x = theEvent.x;
            CurrentStarSystem.y = theEvent.y;
            CurrentStarSystem.z = theEvent.z;
            CurrentStarSystem.Faction = theEvent.controllingfaction;
            CurrentStarSystem.conflicts = theEvent.conflicts;
            CurrentStarSystem.ThargoidWar = theEvent.ThargoidWar;

            // Update system faction data if available
            if ( theEvent.factions != null )
            {
                CurrentStarSystem.factions = theEvent.factions;

                // Update station controlling faction data
                foreach ( var station in CurrentStarSystem.stations )
                {
                    var stationFaction = theEvent.factions.Find(f => f.name == station?.Faction?.name);
                    if ( stationFaction != null )
                    {
                        station.Faction = stationFaction;
                    }
                }
            }

            CurrentStarSystem.Economies = [ theEvent.Economy, theEvent.Economy2 ];
            CurrentStarSystem.securityLevel = theEvent.securityLevel;
            if ( theEvent.population != null )
            {
                CurrentStarSystem.population = theEvent.population;
            }

            // Powerplay information
            CurrentStarSystem.Power = theEvent.Power;
            CurrentStarSystem.NearbyPowers = theEvent.NearbyPowers;
            CurrentStarSystem.powerState = theEvent.PowerState;
            CurrentStarSystem.powerAcquisitionProgress = theEvent.powerAcquisitionProgress;
            CurrentStarSystem.powerControlProgress = theEvent.powerControlProgress;
            CurrentStarSystem.powerReinforcementControlPoints = theEvent.powerReinforcementControlPoints;
            currentStarSystem.powerUnderminingControlPoints = theEvent.powerUnderminingControlPoints;

            // Update to most recent information
            CurrentStarSystem.visitLog.Add( theEvent.timestamp );
            CurrentStarSystem.updatedat = Dates.fromDateTimeToSeconds( theEvent.timestamp );
            await DataProvider.SaveStarSystemAsync( CurrentStarSystem ).ConfigureAwait(false);

            return passEvent;
        }

        private async Task<bool> eventEnteredSupercruiseAsync( EnteredSupercruiseEvent theEvent )
        {
            Environment = Constants.ENVIRONMENT_SUPERCRUISE;
            await updateCurrentSystemAsync( theEvent.system, theEvent.systemAddress ).ConfigureAwait(false);

            if (theEvent.taxi is true)
            {
                Vehicle = Constants.VEHICLE_TAXI;
            }
            else if (theEvent.multicrew is true)
            {
                Vehicle = Constants.VEHICLE_MULTICREW;
            }
            else
            {
                Vehicle = Constants.VEHICLE_SHIP;
            }

            return true;
        }

        private async Task<bool> eventEnteredNormalSpaceAsync( EnteredNormalSpaceEvent theEvent )
        {
            Environment = Constants.ENVIRONMENT_NORMAL_SPACE;

            if (theEvent.bodyname != null && (theEvent.bodyType == BodyType.Moon || theEvent.bodyType == BodyType.Planet) )
            {
                await updateCurrentStellarBodyAsync( theEvent.bodyname, theEvent.bodyId, theEvent.systemname, theEvent.systemAddress).ConfigureAwait(false);
            }
            else
            {
                await updateCurrentSystemAsync( theEvent.systemname, theEvent.systemAddress ).ConfigureAwait( false );
            }

            if ( theEvent.bodyType == BodyType.Planet )
            {
                theEvent.bodyType = CurrentStarSystem?.bodies
                                        .FirstOrDefault( b => b.bodyId != null && b.bodyId == theEvent.bodyId )
                                        ?.bodyType ??
                                    theEvent.bodyType;
            }

            if (theEvent.taxi is true)
            {
                Vehicle = Constants.VEHICLE_TAXI;
            }
            else if (theEvent.multicrew is true)
            {
                Vehicle = Constants.VEHICLE_MULTICREW;
            }
            else
            {
                Vehicle = Constants.VEHICLE_SHIP;
            }

            return true;
        }

        private bool eventCrewJoined(CrewJoinedEvent @event)
        {
            inTelepresence = @event.telepresence ?? false;
            multicrewVehicleHolder = Vehicle;
            Vehicle = Constants.VEHICLE_MULTICREW;
            Logging.Info("Entering multicrew session");
            return true;
        }

        private bool eventCrewLeft()
        {
            inTelepresence = false;
            Vehicle = multicrewVehicleHolder;
            Logging.Info($"Leaving multicrew session to vehicle {Vehicle}");
            return true;
        }

        private bool eventCommanderContinued(CommanderContinuedEvent theEvent)
        {
            // Set Vehicle state for commander from ship model
            if (theEvent.shipEDModel.Contains("Suit"))
            {
                Vehicle = Constants.VEHICLE_LEGS;
            }
            else if (theEvent.shipEDModel == "TestBuggy" || theEvent.shipEDModel.Contains("SRV"))
            {
                Vehicle = Constants.VEHICLE_SRV;
            }
            else
            {
                Vehicle = Constants.VEHICLE_SHIP;
            }
            Logging.Debug($"Commander Continued: vehicle is {Vehicle}");

            // Set Environment state for the ship if 'startlanded' is present in the event
            if (theEvent.startlanded ?? false)
            {
                Environment = Constants.ENVIRONMENT_LANDED;
            }
            else if (theEvent.startlanded != null)
            {
                Environment = Constants.ENVIRONMENT_NORMAL_SPACE;
            }

            // If we see this it means that we aren't in Telepresence
            inTelepresence = false;

            // Identify active game version
            inHorizons = theEvent.horizons;
            inOdyssey = theEvent.odyssey;
            gameBuild = theEvent.gamebuild;
            gameVersion = theEvent.gameversion;

            return true;
        }

        private bool eventEnteredCQC()
        {
            // In CQC we don't want to report anything, so set our Telepresence flag
            inTelepresence = true;
            return true;
        }

        private bool eventSRVLaunched()
        {
            // SRV is always player-controlled, so we are in the SRV
            Vehicle = Constants.VEHICLE_SRV;
            return true;
        }

        private bool eventSRVDocked()
        {
            // We are back in the ship
            Vehicle = Constants.VEHICLE_SHIP;
            return true;
        }

        private bool eventFighterLaunched(FighterLaunchedEvent theEvent)
        {
            Vehicle = theEvent.playercontrolled 
                ? Constants.VEHICLE_FIGHTER // We are in the fighter
                : Constants.VEHICLE_SHIP; // We are (still) in the ship
            return true;
        }

        private bool eventFighterDocked()
        {
            // We are back in the ship
            Vehicle = Constants.VEHICLE_SHIP;
            return true;
        }

        private bool eventVehicleDestroyed()
        {
            // We are back in the ship
            Vehicle = Constants.VEHICLE_SHIP;
            return true;
        }

        private async Task<bool> eventNearSurfaceAsync( NearSurfaceEvent theEvent )
        {
            if ( theEvent.approaching_surface )
            {
                // Update the body 
                var body = CurrentStarSystem?.bodies?.Find( s => s.bodyname == theEvent.bodyname ) ??
                           new Body
                           {
                               // This body is unknown to us, might not be in our data source or we might not have connectivity.
                               // Use a placeholder 
                               bodyname = theEvent.bodyname, systemname = theEvent.systemname
                           };

                // System address may not be included in our data source, so we add it here. 
                body.systemAddress = theEvent.systemAddress;
                CurrentStellarBody = body;
            }
            else
            {
                // Clear the body we are leaving 
                CurrentStellarBody = null;
            }
            await updateCurrentSystemAsync( theEvent.systemname, theEvent.systemAddress ).ConfigureAwait(false);
            return true;
        }

        private async Task<bool> eventStarScannedAsync(StarScannedEvent theEvent)
        {
            // We just scanned a star.  We can only proceed if we know our current star system
            if (CurrentStarSystem == null) { return false; }

            // Clear any temporary / placeholder stars (e.g. from FSDTarget events)
            if ( theEvent.star.mainstar ?? false )
            {
                CurrentStarSystem.ClearTemporaryStars();
            }

            // We use an un-named temporary star at distance 0M during the FSD Target event.
            // Try to match and replace that temporary star if it exists. Otherwise, match by body name.
            var star = CurrentStarSystem.bodies?
                .Where(s => s.bodyType == BodyType.Star).ToList()
                .Find(s => 
                    (string.IsNullOrEmpty(s.bodyname) && s.distance == 0M && s.distance == theEvent.distance) || 
                    s.bodyname == theEvent.bodyname);
            if (star?.scannedDateTime is null)
            {
                CurrentStarSystem.AddOrUpdateBody(theEvent.star);
                await DataProvider.SaveStarSystemAsync(CurrentStarSystem).ConfigureAwait(false);
                return true;
            }
            return false;
        }

        internal async Task<bool> eventBodyScannedAsync(BodyScannedEvent theEvent)
        {
            // We just scanned a body.  We can only proceed if we know our current star system
            if (CurrentStarSystem == null) { return false; }

            // Suppress repetitious `Body scanned` events generated within 10 seconds after mapping.
            var systemBody = CurrentStarSystem.bodies.FirstOrDefault( s => s.bodyname == theEvent.bodyname );
            if ( systemBody?.scannedDateTime != null && systemBody.mappedDateTime < ( theEvent.timestamp + TimeSpan.FromSeconds( 10 ) ) )
            {
                return false;
            }

            CurrentStarSystem.AddOrUpdateBody(theEvent.body);

            Logging.Debug("Saving data for scanned body " + theEvent.bodyname);
            await DataProvider.SaveStarSystemAsync(CurrentStarSystem).ConfigureAwait(false);

            return true;
        }

        internal async Task<bool> eventBodyMappedAsync(BodyMappedEvent theEvent)
        {
            if (CurrentStarSystem != null && theEvent.systemAddress == CurrentStarSystem.systemAddress)
            {
                // We've already updated the body (via the journal monitor) if the CurrentStarSystem isn't null
                // Here, we just need to save the data.
                await DataProvider.SaveStarSystemAsync(CurrentStarSystem).ConfigureAwait(false);
            }
            return true;
        }

        /// <summary>Obtain information from the companion API and use it to refresh our own data</summary>
        public async Task<bool> refreshProfileAsync(bool refreshStation = false)
        {
            if ( CompanionAppService.unitTesting ||
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

                    if (CurrentStarSystem == null && 
                        profile.docked && profile.currentStarSystem == CurrentStarSystem?.systemname && 
                        CurrentStarSystem?.stations != null)
                    {
                        // Only set the current station if it is not present, otherwise we leave it to events
                        CurrentStation ??= CurrentStarSystem.stations.FirstOrDefault(s => s.marketId == profile.LastStationMarketID)
                            ?? CurrentStarSystem.stations.FirstOrDefault(s => s.name == profile.LastStationName);
                        if (CurrentStation != null)
                        {
                            Logging.Debug("Set current station to " + CurrentStation.name);
                            CurrentStation.updatedat = Dates.fromDateTimeToSeconds(DateTime.UtcNow);
                            updatedCurrentStarSystem = true;
                        }
                    }

                    if (refreshStation && CurrentStation != null && Environment == Constants.ENVIRONMENT_DOCKED)
                    {
                        // Refresh station data
                        await conditionallyRefreshStationProfileAsync( profile.currentStarSystem, profile.LastStationMarketID ?? 0 ).ConfigureAwait(false);
                    }

                    if (updatedCurrentStarSystem)
                    {
                        Logging.Debug( "Star system information updated from Frontier API; updating local copy" );
                        await DataProvider.SaveStarSystemAsync(CurrentStarSystem).ConfigureAwait(false);
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

        private static void setSystemDistanceFromHome(StarSystem system)
        {
            var commanderConfig = ConfigService.Instance.commanderConfiguration;
            var homeX = commanderConfig.homeSystemX;
            var homeY = commanderConfig.homeSystemY;
            var homeZ = commanderConfig.homeSystemZ;

            if ( system is null || homeX is null || homeY is null || homeZ is null ) { return; }

            system.distancefromhome = system.DistanceFromStarSystem(homeX, homeY, homeZ) ?? 0;
            Logging.Debug("Distance from home is " + system.distancefromhome);
        }

        private void setSystemDistanceFromDestination(StarSystem system)
        {
            if (DestinationStarSystem is null) { return; }
            DestinationDistanceLy = system.DistanceFromStarSystem(destinationStarSystem) ?? 0;
            Logging.Debug("Distance from destination system is " + DestinationDistanceLy);
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

        private Ship _currentShip;

        /// <summary>
        /// Update the profile when requested, ensuring that we meet the condition in the updated profile
        /// </summary>
        private async Task conditionallyRefreshStationProfileAsync ( string expectedSystemName, long expectedLastMarketID, bool forceUpdate = false, JObject profileJson = null )
        {
            if (CompanionAppService.Instance.CurrentState == CompanionAppService.State.Authorized)
            {
                try
                {
                    // Make sure we know where we are
                    if (CurrentStarSystem is null || CurrentStarSystem.systemAddress == 0)
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
                        var station = CurrentStarSystem?.stations.Find(s => s.marketId == profileStation.marketId);
                        if ( station != null )
                        {
                            Logging.Debug( "Current station matches profile information; updating info" );
                            profileStation.UpdateStation( profileStation.commoditiesupdatedat, station );

                            // Update the current station information in our backend DB
                            Logging.Debug( "Star system information updated from Frontier API server; updating local copy" );
                            await DataProvider.SaveStarSystemAsync( CurrentStarSystem ).ConfigureAwait(false);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logging.Error("Exception obtaining station profile", ex);
                }
            }
        }

        internal static class NativeMethods
        {
            // Required to restart app after upgrade
            [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
            internal static extern uint RegisterApplicationRestart(string pwzCommandLine, RestartFlags dwFlags);
        }

        // Flags for upgrade
        [Flags]
        internal enum RestartFlags
        {
            NONE = 0,
            RESTART_CYCLICAL = 1,
            RESTART_NOTIFY_SOLUTION = 2,
            RESTART_NOTIFY_FAULT = 4,
            RESTART_NO_CRASH = 8,
            RESTART_NO_HANG = 16,
            RESTART_NO_PATCH = 32,
            RESTART_NO_REBOOT = 64
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
                    if (system.systemAddress != DestinationStarSystem?.systemAddress )
                    {
                        Logging.Debug("Destination star system is " + system.systemname);
                        DestinationStarSystem = system;
                    }
                }
                else { destinationSystem = null; }
            }
            else
            {
                DestinationStarSystem = null;
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

