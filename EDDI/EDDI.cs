﻿using Eddi;
using EddiBgsService;
using EddiCompanionAppService;
using EddiConfigService;
using EddiDataDefinitions;
using EddiDataProviderService;
using EddiEvents;
using EddiInaraService;
using EddiSpeechService;
using EddiStatusService;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using EddiStarMapService;
using Utilities;
using MathNet.Numerics.RootFinding;

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
        public bool SpeechResponderModalWait { get; set; } = false;

        private static bool started;
        internal static bool running = true;

        public bool inTelepresence { get; private set; } = false;

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

        public bool gameIsBeta { get; private set; } = false;

        public string gameVersion
        {
            get => _gameVersion;
            private set
            {
                _gameVersion = value;
                SetGameVersion(value);
                GameVersionUpdated?.Invoke(GameVersion, new PropertyChangedEventArgs(nameof(gameVersion)));
            }
        }
        private string _gameVersion;
        public EventHandler GameVersionUpdated;

        public System.Version GameVersion { get; private set; }

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
                                  System.Version.TryParse(versionRegex.Match(v).Value, out System.Version versionResult)
                        ? versionResult
                        : null;

                    // Set game version in applicable services
                    BgsService.SetGameVersion(GameVersion);
                    CompanionAppService.SetGameVersion(GameVersion);
                    InaraService.SetGameVersion(GameVersion);
                    StarMapService.SetGameVersion(GameVersion, gameVersion, gameBuild);
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
        public string gameBuild { get; private set; }

        static EDDI()
        {
            // Set up our app directory
            Directory.CreateDirectory(Constants.DATA_DIR);
        }

        public static void Init(bool safeMode)
        {
            if (instance == null)
            {
                lock (instanceLock)
                {
                    if (instance == null)
                    {
                        Logging.Debug("No EDDI instance: creating one");
                        instance = new EDDI(safeMode);
                    }
                }
            }
        }

        // EDDI Instance
        public static EDDI Instance
        {
            get
            {
                Init(false);
                return instance;
            }
        }
        private static EDDI instance;
        private static readonly object instanceLock = new object();

        public List<EDDIMonitor> monitors = new List<EDDIMonitor>();
        private ConcurrentBag<EDDIMonitor> activeMonitors = new ConcurrentBag<EDDIMonitor>();
        private static readonly object monitorLock = new object();

        public List<EDDIResponder> responders = new List<EDDIResponder>();
        private ConcurrentBag<EDDIResponder> activeResponders = new ConcurrentBag<EDDIResponder>();
        private static readonly object responderLock = new object();

        public string vaVersion { get; set; }

        // Information obtained from the configuration
        public StarSystem HomeStarSystem // May be null when the commander hasn't set a home star system
        {
            get => homeStarSystem;
            private set
            {
                void childPropertyChangedHandler(object sender, PropertyChangedEventArgs e)
                {
                    OnPropertyChanged();
                }
                if (homeStarSystem != null) { homeStarSystem.PropertyChanged -= childPropertyChangedHandler;}
                if (value != null) { value.PropertyChanged += childPropertyChangedHandler;}
                homeStarSystem = value;
                OnPropertyChanged();
            }
        }
        private StarSystem homeStarSystem;
        
        public Station HomeStation
        {
            get => homeStation;
            private set
            {
                void childPropertyChangedHandler(object sender, PropertyChangedEventArgs e)
                {
                    OnPropertyChanged();
                }
                if (homeStation != null) { homeStation.PropertyChanged -= childPropertyChangedHandler; }
                if (value != null) { value.PropertyChanged += childPropertyChangedHandler; }
                homeStation = value;
                OnPropertyChanged();
            }
        }
        private Station homeStation;

        public StarSystem SquadronStarSystem // May be null when the commander hasn't set a squadron star system
        {
            get => squadronStarSystem;
            private set
            {
                void childPropertyChangedHandler(object sender, PropertyChangedEventArgs e)
                {
                    OnPropertyChanged();
                }
                if (squadronStarSystem != null) { squadronStarSystem.PropertyChanged -= childPropertyChangedHandler; }
                if (value != null) { value.PropertyChanged += childPropertyChangedHandler; }
                squadronStarSystem = value;
                OnPropertyChanged();
            }
        }
        private StarSystem squadronStarSystem;

        // Destination variables
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
        public Commander Cmdr // Also includes information from the configuration and companion app service
        {
            get => cmdr;
            private set
            {
                void childPropertyChangedHandler(object sender, PropertyChangedEventArgs e)
                {
                    OnPropertyChanged(nameof(Cmdr));
                }
                if (cmdr != null) { cmdr.PropertyChanged -= childPropertyChangedHandler; }
                if (value != null) { value.PropertyChanged += childPropertyChangedHandler; }
                cmdr = value;
                OnPropertyChanged();
            }
        }
        private Commander cmdr;
        
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

        public StarSystem CurrentStarSystem 
        { 
            get => currentStarSystem; 
            private set
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

        public StarSystem LastStarSystem
        {
            get => lastStarSystem;
            private set
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

        public StarSystem NextStarSystem
        {
            get => nextStarSystem;
            private set
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

        public FleetCarrier FleetCarrier
        {
            get => fleetCarrier;
            set
            {
                void childPropertyChangedHandler(object sender, PropertyChangedEventArgs e)
                {
                    OnPropertyChanged();
                }
                if (fleetCarrier != null) { fleetCarrier.PropertyChanged -= childPropertyChangedHandler; }
                if (value != null) { value.PropertyChanged += childPropertyChangedHandler; }

                EDDIConfiguration configuration = ConfigService.Instance.eddiConfiguration;
                if (configuration.fleetCarrier != value)
                {
                    configuration.fleetCarrier = value;
                    ConfigService.Instance.eddiConfiguration = configuration;
                }

                fleetCarrier = value;
                OnPropertyChanged();
            }
        }
        private FleetCarrier fleetCarrier;

        public DateTime JournalTimeStamp { get; set; } = DateTime.MinValue;

        // Information from the last events of each type that we've received (for reference)
        private SortedDictionary<string, Event> lastEvents { get; set; } = new SortedDictionary<string, Event>();

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

        public ObservableConcurrentDictionary<string, object> State = new ObservableConcurrentDictionary<string, object>();

        // The event queue
        private BlockingCollection<Event> eventQueue { get; } = new BlockingCollection<Event>();
        private readonly CancellationTokenSource eventHandlerTS = new CancellationTokenSource();
        private Task eventConsumerThread = null;

        private string multicrewVehicleHolder;

        private EDDI(bool safeMode)
        {
            running = !safeMode;
            try
            {
                Logging.Info(Constants.EDDI_NAME + " " + Constants.EDDI_VERSION + " starting");

                // CAUTION: CompanionAppService.Instance must be invoked by the main application thread, before any other threads are generated, 
                // to correctly configure the CompanionAppService to receive DDE messages from its custom URL Protocol.
                CompanionAppService.Instance.gameIsBeta = false;

                var configuration = ConfigService.Instance.eddiConfiguration;
                Logging.Verbose = configuration.Debug;

                // Retrieve commander data
                Cmdr = new Commander();
                Cmdr.name = configuration.CommanderName;
                Cmdr.phoneticName = configuration.PhoneticName;
                Cmdr.gender = configuration.Gender;
                Cmdr.squadronname = configuration.SquadronName;
                Cmdr.squadronid = configuration.SquadronID;
                Cmdr.squadronrank = configuration.SquadronRank;
                Cmdr.squadronallegiance = configuration.SquadronAllegiance;
                Cmdr.squadronpower = configuration.SquadronPower;
                Cmdr.squadronfaction = configuration.SquadronFaction;
                FleetCarrier = configuration.fleetCarrier;

                // We always start in normal space
                Environment = Constants.ENVIRONMENT_NORMAL_SPACE;

                var essentialAsyncTasks = new List<Task>();
                if (running)
                {
                    // Tasks we can start asynchronously but need to complete before other dependent code is called
                    essentialAsyncTasks.AddRange(new List<Task>()
                    {
                        Task.Run(() => responders = findResponders()), // Set up responders
                        Task.Run(() => monitors = findMonitors()), // Set up monitors 
                    });
                }
                else
                {
                    Logging.Info("Mandatory upgrade required! EDDI initializing in safe mode until upgrade is completed.");
                }

                // Make sure that our essential tasks have completed before we start
                Task.WaitAll(essentialAsyncTasks.ToArray());

                // Tasks we can start asynchronously and don't need to wait for

                // If our home system and squadron system are the same, run those tasks in the same thread to prevent fetching from the star system database multiple times.
                // Otherwise, run them in separate threads.
                void ActionUpdateHomeSystemStation()
                {
                    updateHomeSystemStation(configuration);
                }
                void ActionUpdateSquadronSystem()
                {
                    updateSquadronSystem(configuration);
                }
                if (configuration.HomeSystem == configuration.SquadronSystem)
                {
                    // Run both actions on the same thread
                    Task.Run((Action)ActionUpdateHomeSystemStation + ActionUpdateSquadronSystem).ConfigureAwait(false);
                }
                else
                {
                    // Run both actions on distinct threads
                    Task.Run(() => ActionUpdateHomeSystemStation()).ConfigureAwait(false);
                    Task.Run(() => ActionUpdateSquadronSystem()).ConfigureAwait(false);
                }

                Task.Run(() => updateDestinationSystem(configuration.DestinationSystem)).ConfigureAwait(false);
                Task.Run(() =>
                {
                    // Set up the Frontier API service
                    // Try to carry out initial population of the Frontier API profile
                    try
                    {
                        refreshProfile();
                    }
                    catch (Exception ex)
                    {
                        Logging.Debug("Failed to obtain Frontier API profile: " + ex);
                    }

                    if (CompanionAppService.Instance.CurrentState == CompanionAppService.State.Authorized)
                    {
                        Logging.Info("EDDI access to the Frontier API is enabled.");
                        RefreshFleetCarrierFromFrontierAPI(true);
                    }
                    else
                    {
                        Logging.Info("EDDI access to the Frontier API is not enabled.");
                    }
                }).ConfigureAwait(false);

                CompanionAppService.Instance.StateChanged += OnCompanionAppServiceStateChanged;

                Logging.Info(Constants.EDDI_NAME + " " + Constants.EDDI_VERSION + " initialised");
            }
            catch (Exception ex)
            {
                Logging.Error("Failed to initialise", ex);
            }
        }

        private void OnCompanionAppServiceStateChanged(CompanionAppService.State oldstate, CompanionAppService.State newstate)
        {
            // Obtain fleet carrier data once the Frontier API connects
            if (oldstate != CompanionAppService.State.Authorized && 
                newstate is CompanionAppService.State.Authorized)
            {
                RefreshFleetCarrierFromFrontierAPI(true);
            }
        }

        public bool EddiIsBeta() => Constants.EDDI_VERSION.phase < Utilities.Version.TestPhase.rc;

        public bool ShouldUseTestEndpoints()
        {
#if DEBUG
            return true;
#else
            // use test endpoints if the game is in beta
            return EDDI.Instance.gameIsBeta;
#endif
        }

        public void Start()
        {
            if (!started)
            {
                EDDIConfiguration configuration = ConfigService.Instance.eddiConfiguration;

                foreach (EDDIMonitor monitor in monitors)
                {
                    if (!configuration.Plugins.TryGetValue(monitor.MonitorName(), out bool enabled))
                    {
                        // No information; default to enabled
                        enabled = true;
                    }

                    if (!enabled && !monitor.IsRequired())
                    {
                        Logging.Debug(monitor.MonitorName() + " is disabled; not starting");
                    }
                    else
                    {
                        activeMonitors.Add(monitor);
                        if (monitor.NeedsStart())
                        {
                            Thread monitorThread = new Thread(() => keepAlive(monitor.MonitorName(), monitor.Start))
                            {
                                IsBackground = true
                            };
                            Logging.Info("Starting keepalive for " + monitor.MonitorName());
                            monitorThread.Start();
                        }
                    }
                }

                foreach (EDDIResponder responder in responders)
                {
                    if (!configuration.Plugins.TryGetValue(responder.ResponderName(), out bool enabled))
                    {
                        // No information; default to enabled
                        enabled = true;
                    }

                    if (!enabled)
                    {
                        Logging.Debug(responder.ResponderName() + " is disabled; not starting");
                    }
                    else
                    {
                        try
                        {
                            bool responderStarted = responder.Start();
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

                started = true;
            }
        }

        public void Stop()
        {
            running = false; // Otherwise keepalive restarts them
            if (started)
            {
                eventHandlerTS.Cancel();
                foreach (EDDIResponder responder in responders)
                {
                    DisableResponder(responder.ResponderName());
                }
                foreach (EDDIMonitor monitor in monitors)
                {
                    DisableMonitor(monitor.MonitorName());
                }
            }

            started = false;
            Logging.Info(Constants.EDDI_NAME + " " + Constants.EDDI_VERSION + " stopped");
        }

        /// <summary>
        /// Reload all monitors and responders
        /// </summary>
        public void Reload()
        {
            foreach (EDDIResponder responder in responders)
            {
                responder.Reload();
            }
            foreach (EDDIMonitor monitor in monitors)
            {
                monitor.Reload();
            }

            Logging.Info(Constants.EDDI_NAME + " " + Constants.EDDI_VERSION + " reloaded");
        }

        /// <summary>
        /// Obtain a named monitor
        /// </summary>
        public EDDIMonitor ObtainMonitor(string invariantName)
        {
            foreach (EDDIMonitor monitor in monitors)
            {
                if (monitor.MonitorName().Equals(invariantName, StringComparison.InvariantCultureIgnoreCase))
                {
                    return monitor;
                }
            }
            return null;
        }

        /// <summary> Obtain a named responder </summary>
        public EDDIResponder ObtainResponder(string invariantName)
        {
            foreach (EDDIResponder responder in responders)
            {
                if (responder.ResponderName().Equals(invariantName, StringComparison.InvariantCultureIgnoreCase))
                {
                    return responder;
                }
            }
            return null;
        }

        /// <summary> Disable a named responder for this session.  This does not update the on-disk status of the responder </summary>
        public void DisableResponder(string invariantName)
        {
            EDDIResponder responder = ObtainResponder(invariantName);
            if (responder != null)
            {
                lock (responderLock)
                {
                    responder.Stop();
                    ConcurrentBag<EDDIResponder> newResponders = new ConcurrentBag<EDDIResponder>();
                    while (activeResponders.TryTake(out EDDIResponder item))
                    {
                        if (item != responder) { newResponders.Add(item); }
                    }
                    activeResponders = newResponders;
                }
            }
        }

        /// <summary> Enable a named responder for this session.  This does not update the on-disk status of the responder </summary>
        public void EnableResponder(string invariantName)
        {
            EDDIResponder responder = ObtainResponder(invariantName);
            if (responder != null)
            {
                if (!activeResponders.Contains(responder))
                {
                    responder.Start();
                    activeResponders.Add(responder);
                }
            }
        }

        /// <summary> Disable a named monitor for this session.  This does not update the on-disk status of the responder </summary>
        public void DisableMonitor(string invariantName)
        {
            EDDIMonitor monitor = ObtainMonitor(invariantName);
            if (monitor != null)
            {
                lock (monitorLock)
                {
                    monitor.Stop();
                    ConcurrentBag<EDDIMonitor> newMonitors = new ConcurrentBag<EDDIMonitor>();
                    while (activeMonitors.TryTake(out EDDIMonitor item))
                    {
                        if (item != monitor) { newMonitors.Add(item); }
                    }
                    activeMonitors = newMonitors;
                }
            }
        }

        /// <summary> Enable a named monitor for this session.  This does not update the on-disk status of the responder </summary>
        public void EnableMonitor(string invariantName)
        {
            EDDIMonitor monitor = ObtainMonitor(invariantName);
            if (monitor != null)
            {
                if (!activeMonitors.Contains(monitor))
                {
                    if (monitor.NeedsStart())
                    {
                        activeMonitors.Add(monitor);
                        Thread monitorThread = new Thread(() => keepAlive(monitor.MonitorName(), monitor.Start))
                        {
                            IsBackground = true
                        };
                        Logging.Info("Starting keepalive for " + monitor.MonitorName());
                        monitorThread.Start();
                    }
                }
            }
        }

        /// <summary> Reload a specific monitor or responder </summary>
        public void Reload(string name)
        {
            foreach (EDDIResponder responder in responders)
            {
                if (responder.ResponderName() == name)
                {
                    responder.Reload();
                    return;
                }
            }
            foreach (EDDIMonitor monitor in monitors)
            {
                if (monitor.MonitorName() == name)
                {
                    monitor.Reload();
                }
            }

            Logging.Info($"{Constants.EDDI_NAME} {Constants.EDDI_VERSION} module {name} reloaded");
        }

        /// <summary> Keep a monitor thread alive, restarting it as required </summary>
        private void keepAlive(string name, Action start)
        {
            try
            {
                int failureCount = 0;
                while (running && failureCount < 5 && activeMonitors.FirstOrDefault(m => m.MonitorName() == name) != null)
                {
                    try
                    {
                        Thread monitorThread = new Thread(() => start())
                        {
                            Name = name,
                            IsBackground = true
                        };
                        Logging.Info("Starting " + name + " (" + failureCount + ")");
                        monitorThread.Start();
                        monitorThread.Join();
                    }
                    catch (ThreadAbortException tax)
                    {
                        Thread.ResetAbort();
                        if (running)
                        {
                            Logging.Error("Restarting " + name + " after thread abort", tax);
                        }
                    }
                    catch (Exception ex)
                    {
                        if (running)
                        {
                            Logging.Error("Restarting " + name + " after exception", ex);
                        }
                    }
                    failureCount++;
                }
                if (running)
                {
                    DisableMonitor(name);
                    Logging.Warn(name + " stopping after too many failures");
                }
            }
            catch (ThreadAbortException)
            {
                Logging.Debug("Thread aborted");
            }
            catch (Exception ex)
            {
                Logging.Warn("keepAlive for " + name + " failed", ex);
            }
        }

        public void enqueueEvent(Event @event)
        {
            if (@event is null) { return; }

            if (!eventQueue.IsAddingCompleted)
            {
                eventQueue.Add(@event);
            }

            // Start (or restart) our event handler thread
            if (eventConsumerThread?.Status != TaskStatus.Running)
            {
                eventConsumerThread = Task.Run(dequeueEvents, eventHandlerTS.Token);
            }
        }

        private void dequeueEvents()
        {
            try
            {
                foreach (var @event in eventQueue.GetConsumingEnumerable(eventHandlerTS.Token))
                {
                    eventHandler(@event);
                }
            }
            catch (OperationCanceledException)
            {
                // Task canceled. Mark this collection as not accepting any new items.
                eventQueue.CompleteAdding();
            }
        }

        private void eventHandler(Event @event)
        {
            if (@event != null)
            {
                try
                {
                    Logging.Debug("Handling event " + JsonConvert.SerializeObject(@event));
                    // We have some additional processing to do for a number of events
                    bool passEvent = true;
                    if (@event is FileHeaderEvent fileHeaderEvent)
                    {
                        passEvent = eventFileHeader(fileHeaderEvent);
                    }
                    else if (@event is LocationEvent locationEvent)
                    {
                        passEvent = eventLocation(locationEvent);
                    }
                    else if (@event is DockedEvent dockedEvent)
                    {
                        passEvent = eventDocked(dockedEvent);
                    }
                    else if (@event is UndockedEvent undockedEvent)
                    {
                        passEvent = eventUndocked(undockedEvent);
                    }
                    else if (@event is DockingRequestedEvent dockingRequestedEvent)
                    {
                        passEvent = eventDockingRequested(dockingRequestedEvent);
                    }
                    else if (@event is TouchdownEvent touchdownEvent)
                    {
                        passEvent = eventTouchdown(touchdownEvent);
                    }
                    else if (@event is LiftoffEvent liftoffEvent)
                    {
                        passEvent = eventLiftoff(liftoffEvent);
                    }
                    else if (@event is FSDEngagedEvent fsdEngagedEvent)
                    {
                        passEvent = eventFSDEngaged(fsdEngagedEvent);
                    }
                    else if (@event is FSDTargetEvent fsdTargetEvent)
                    {
                        passEvent = eventFSDTarget(fsdTargetEvent);
                    }
                    else if (@event is JumpedEvent jumpedEvent)
                    {
                        passEvent = eventJumped(jumpedEvent);
                    }
                    else if (@event is EnteredSupercruiseEvent enteredSupercruiseEvent)
                    {
                        passEvent = eventEnteredSupercruise(enteredSupercruiseEvent);
                    }
                    else if (@event is EnteredNormalSpaceEvent enteredNormalSpaceEvent)
                    {
                        passEvent = eventEnteredNormalSpace(enteredNormalSpaceEvent);
                    }
                    else if (@event is CommanderLoadingEvent commanderLoadingEvent)
                    {
                        passEvent = eventCommanderLoading(commanderLoadingEvent);
                    }
                    else if (@event is CommanderContinuedEvent commanderContinuedEvent)
                    {
                        passEvent = eventCommanderContinued(commanderContinuedEvent);
                    }
                    else if (@event is CommanderRatingsEvent commanderRatingsEvent)
                    {
                        passEvent = eventCommanderRatings(commanderRatingsEvent);
                    }
                    else if (@event is CrewJoinedEvent crewJoinedEvent)
                    {
                        passEvent = eventCrewJoined(crewJoinedEvent);
                    }
                    else if (@event is CrewLeftEvent crewLeftEvent)
                    {
                        passEvent = eventCrewLeft(crewLeftEvent);
                    }
                    else if (@event is EnteredCQCEvent enteredCqcEvent)
                    {
                        passEvent = eventEnteredCQC(enteredCqcEvent);
                    }
                    else if (@event is SRVLaunchedEvent srvLaunchedEvent)
                    {
                        passEvent = eventSRVLaunched(srvLaunchedEvent);
                    }
                    else if (@event is SRVDockedEvent srvDockedEvent)
                    {
                        passEvent = eventSRVDocked(srvDockedEvent);
                    }
                    else if (@event is FighterLaunchedEvent fighterLaunchedEvent)
                    {
                        passEvent = eventFighterLaunched(fighterLaunchedEvent);
                    }
                    else if (@event is FighterDockedEvent fighterDockedEvent)
                    {
                        passEvent = eventFighterDocked(fighterDockedEvent);
                    }
                    else if (@event is StarScannedEvent starScannedEvent)
                    {
                        passEvent = eventStarScanned(starScannedEvent);
                    }
                    else if (@event is BodyScannedEvent bodyScannedEvent)
                    {
                        passEvent = eventBodyScanned(bodyScannedEvent);
                    }
                    else if (@event is BodyMappedEvent bodyMappedEvent)
                    {
                        passEvent = eventBodyMapped(bodyMappedEvent);
                    }
                    else if (@event is RingMappedEvent ringMappedEvent)
                    {
                        passEvent = eventRingMapped(ringMappedEvent);
                    }
                    else if (@event is VehicleDestroyedEvent vehicleDestroyedEvent)
                    {
                        passEvent = eventVehicleDestroyed(vehicleDestroyedEvent);
                    }
                    else if (@event is NearSurfaceEvent nearSurfaceEvent)
                    {
                        passEvent = eventNearSurface(nearSurfaceEvent);
                    }
                    else if (@event is SquadronStatusEvent squadronStatusEvent)
                    {
                        passEvent = eventSquadronStatus(squadronStatusEvent);
                    }
                    else if (@event is SquadronRankEvent squadronRankEvent)
                    {
                        passEvent = eventSquadronRank(squadronRankEvent);
                    }
                    else if (@event is FriendsEvent friendsEvent)
                    {
                        passEvent = eventFriends(friendsEvent);
                    }
                    else if (@event is MarketEvent marketEvent)
                    {
                        passEvent = eventMarket(marketEvent);
                    }
                    else if (@event is OutfittingEvent outfittingEvent)
                    {
                        passEvent = eventOutfitting(outfittingEvent);
                    }
                    else if (@event is ShipyardEvent shipyardEvent)
                    {
                        passEvent = eventShipyard(shipyardEvent);
                    }
                    else if (@event is DiscoveryScanEvent discoveryScanEvent)
                    {
                        passEvent = eventDiscoveryScan(discoveryScanEvent);
                    }
                    else if (@event is SystemScanComplete systemScanComplete)
                    {
                        passEvent = eventSystemScanComplete(systemScanComplete);
                    }
                    else if (@event is PowerplayEvent powerplayEvent)
                    {
                        passEvent = eventPowerplay(powerplayEvent);
                    }
                    else if (@event is PowerDefectedEvent powerDefectedEvent)
                    {
                        passEvent = eventPowerDefected(powerDefectedEvent);
                    }
                    else if (@event is PowerJoinedEvent powerJoinedEvent)
                    {
                        passEvent = eventPowerJoined(powerJoinedEvent);
                    }
                    else if (@event is PowerLeftEvent powerLeftEvent)
                    {
                        passEvent = eventPowerLeft(powerLeftEvent);
                    }
                    else if (@event is PowerPreparationVoteCast powerPreparationVoteCast)
                    {
                        passEvent = eventPowerPreparationVoteCast(powerPreparationVoteCast);
                    }
                    else if (@event is PowerSalaryClaimedEvent powerSalaryClaimedEvent)
                    {
                        passEvent = eventPowerSalaryClaimed(powerSalaryClaimedEvent);
                    }
                    else if (@event is PowerVoucherReceivedEvent powerVoucherReceivedEvent)
                    {
                        passEvent = eventPowerVoucherReceived(powerVoucherReceivedEvent);
                    }
                    else if (@event is CarrierBankTransferEvent carrierBankTransferEvent)
                    {
                        passEvent = eventCarrierBankTransfer(carrierBankTransferEvent);
                    }
                    else if (@event is CarrierDecommissionCancelledEvent carrierDecommissionCancelledEvent)
                    {
                        passEvent = eventCarrierDecommissionCancelled(carrierDecommissionCancelledEvent);
                    }
                    else if (@event is CarrierDecommissionScheduledEvent carrierDecommissionScheduledEvent)
                    {
                        passEvent = eventCarrierDecommissionScheduled(carrierDecommissionScheduledEvent);
                    }
                    else if (@event is CarrierDockingPermissionEvent carrierDockingPermissionEvent)
                    {
                        passEvent = eventCarrierDockingPermission(carrierDockingPermissionEvent);
                    }
                    else if (@event is CarrierFuelDepositEvent carrierDepositFuelEvent)
                    {
                        passEvent = eventCarrierDepositFuel(carrierDepositFuelEvent);
                    }
                    else if (@event is CarrierJumpEngagedEvent carrierJumpEngagedEvent)
                    {
                        passEvent = eventCarrierJumpEngaged(carrierJumpEngagedEvent);
                    }
                    else if (@event is CarrierJumpedEvent carrierJumpedEvent)
                    {
                        passEvent = eventCarrierJumped(carrierJumpedEvent);
                    }
                    else if (@event is CarrierFinanceEvent carrierFinanceEvent)
                    {
                        passEvent = eventCarrierFinance(carrierFinanceEvent);
                    }
                    else if (@event is CarrierStatsEvent carrierStatsEvent)
                    {
                        passEvent = eventCarrierStats(carrierStatsEvent);
                    }
                    else if (@event is CarrierNameChangeEvent carrierNameChangeEvent)
                    {
                        passEvent = eventCarrierNameChange(carrierNameChangeEvent);
                    }
                    else if (@event is DisembarkEvent disembarkEvent)
                    {
                        passEvent = eventDisembark(disembarkEvent);
                    }
                    else if (@event is EmbarkEvent embarkEvent)
                    {
                        passEvent = eventEmbark(embarkEvent);
                    }
                    else if (@event is CommanderPromotionEvent commanderPromotionEvent)
                    {
                        passEvent = eventCommanderPromotion(commanderPromotionEvent);
                    }
                    else if (@event is UnderAttackEvent underAttackEvent)
                    {
                        passEvent = eventUnderAttack(underAttackEvent);
                    }
                    else if (@event is SettlementApproachedEvent settlementApproachedEvent)
                    {
                        passEvent = eventSettlementApproached(settlementApproachedEvent);
                    }

                    // Additional processing is over, send to the event responders if required
                    if (passEvent)
                    {
                        OnEvent(@event);
                    }
                }
                catch (Exception ex)
                {
                    Logging.Error($"EDDI core failed to handle {@event.type} event {@event.raw}.", ex);

                    // Even if an error occurs, we still need to pass the raw data 
                    // to the EDDN responder to maintain it's integrity.
                    Instance.ObtainResponder("EDDN responder").Handle(@event);
                }
            }
        }

        private bool eventCarrierStats(CarrierStatsEvent carrierStatsEvent)
        {
            if (FleetCarrier is null || FleetCarrier.carrierID != carrierStatsEvent.carrierID)
            {
                FleetCarrier = new FleetCarrier(carrierStatsEvent.carrierID);
            }

            FleetCarrier.name = carrierStatsEvent.name;
            FleetCarrier.callsign = carrierStatsEvent.callsign;
            FleetCarrier.dockingAccess = carrierStatsEvent.dockingAccess;
            FleetCarrier.notoriousAccess = carrierStatsEvent.notoriousAccess;
            FleetCarrier.fuel = carrierStatsEvent.fuel;
            FleetCarrier.usedCapacity = carrierStatsEvent.usedCapacity;
            FleetCarrier.freeCapacity = carrierStatsEvent.freeCapacity;
            FleetCarrier.bankBalance = carrierStatsEvent.bankBalance;
            FleetCarrier.bankReservedBalance = carrierStatsEvent.bankReservedBalance;
            FleetCarrier.bankPurchaseAllocationsBalance = carrierStatsEvent.bankBalance -
                                                          carrierStatsEvent.bankReservedBalance -
                                                          carrierStatsEvent.bankAvailableBalance;
            return true;
        }

        private bool eventCarrierNameChange(CarrierNameChangeEvent carrierNameChangeEvent)
        {
            if (FleetCarrier is null || FleetCarrier.carrierID != carrierNameChangeEvent.carrierID)
            {
                FleetCarrier = new FleetCarrier(carrierNameChangeEvent.carrierID);
            }
            FleetCarrier.name = carrierNameChangeEvent.name;
            return true;
        }

        private bool eventCarrierFinance(CarrierFinanceEvent carrierFinanceEvent)
        {
            if (FleetCarrier is null || FleetCarrier.carrierID != carrierFinanceEvent.carrierID)
            {
                FleetCarrier = new FleetCarrier(carrierFinanceEvent.carrierID);
            }
            FleetCarrier.bankBalance = carrierFinanceEvent.bankBalance;
            FleetCarrier.bankReservedBalance = carrierFinanceEvent.bankReservedBalance;
            FleetCarrier.bankPurchaseAllocationsBalance = carrierFinanceEvent.bankBalance
                                                          - carrierFinanceEvent.bankReservedBalance
                                                          - carrierFinanceEvent.bankAvailableBalance;
            return true;
        }

        private bool eventCarrierDockingPermission(CarrierDockingPermissionEvent carrierDockingPermissionEvent)
        {
            if (FleetCarrier is null || FleetCarrier.carrierID != carrierDockingPermissionEvent.carrierID)
            {
                FleetCarrier = new FleetCarrier(carrierDockingPermissionEvent.carrierID);
            }
            FleetCarrier.dockingAccess = carrierDockingPermissionEvent.dockingAccess;
            FleetCarrier.notoriousAccess = carrierDockingPermissionEvent.allowNotorious;
            return true;
        }

        private bool eventCarrierDepositFuel(CarrierFuelDepositEvent carrierFuelDepositEvent)
        {
            if (FleetCarrier is null || FleetCarrier.carrierID != carrierFuelDepositEvent.carrierID)
            {
                FleetCarrier = new FleetCarrier(carrierFuelDepositEvent.carrierID);
            }
            FleetCarrier.fuel = carrierFuelDepositEvent.total;
            return true;
        }

        private bool eventCarrierDecommissionScheduled(CarrierDecommissionScheduledEvent carrierDecommissionScheduledEvent)
        {
            if (FleetCarrier is null || FleetCarrier.carrierID != carrierDecommissionScheduledEvent.carrierID)
            {
                FleetCarrier = new FleetCarrier(carrierDecommissionScheduledEvent.carrierID);
            }
            FleetCarrier.state = "pendingDecommission";
            return true;
        }

        private bool eventCarrierDecommissionCancelled(CarrierDecommissionCancelledEvent carrierDecommissionCancelledEvent)
        {
            if (FleetCarrier is null || FleetCarrier.carrierID != carrierDecommissionCancelledEvent.carrierID)
            {
                FleetCarrier = new FleetCarrier(carrierDecommissionCancelledEvent.carrierID);
            }
            FleetCarrier.state = "normalOperation";
            return true;
        }

        private bool eventCarrierBankTransfer(CarrierBankTransferEvent carrierBankTransferEvent)
        {
            if (FleetCarrier is null || FleetCarrier.carrierID != carrierBankTransferEvent.carrierID)
            {
                FleetCarrier = new FleetCarrier(carrierBankTransferEvent.carrierID);
            }
            Cmdr.credits = carrierBankTransferEvent.cmdrBalance;
            FleetCarrier.bankBalance = carrierBankTransferEvent.bankBalance;
            return true;
        }

        private bool eventSettlementApproached(SettlementApproachedEvent settlementApproachedEvent)
        {
            if (CurrentStarSystem?.systemAddress == settlementApproachedEvent.systemAddress)
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
                    CurrentStarSystem?.stations.Add(station);
                }
            }
            return true;
        }

        private bool eventUnderAttack(UnderAttackEvent underAttackEvent)
        {
            bool passEvent = true;
            // Suppress repetitious `Under attack` events when loading or
            // when the target has already been reported as under attack within the last 10 seconds.
            var lastEvent = lastEvents.TryGetValue(nameof(UnderAttackEvent), out Event ev) 
                ? (UnderAttackEvent)ev 
                : null;
            if (underAttackEvent.fromLoad || (
                lastEvent != null 
                && lastEvent.target == underAttackEvent.target 
                && (underAttackEvent.timestamp - lastEvent.timestamp).TotalSeconds < 10 
                ))
            {
                passEvent = false;
            }
            lastEvents[nameof(UnderAttackEvent)] = underAttackEvent;
            return passEvent;
        }

        private bool eventCommanderPromotion(CommanderPromotionEvent commanderPromotionEvent)
        {
            // Capture commander ratings and add them to the commander object
            if (commanderPromotionEvent.ratingObject is CombatRating combatRating)
            {
                // There is a bug with the journal where it reports superpower increases in rank as combat increases
                // Hence we check to see if this is a real event by comparing our known combat rating to the promoted rating
                if (Cmdr?.combatrating == null || commanderPromotionEvent.rank != Cmdr.combatrating.localizedName)
                {
                    // Real event. 
                    if (Cmdr != null) { Cmdr.combatrating = combatRating; }
                    return true;
                }
                // False event
                return false;
            }
            if (commanderPromotionEvent.ratingObject is CQCRating cqcRating)
            {
                if (Cmdr != null) { Cmdr.cqcrating = cqcRating; }
                return true;
            }
            if (commanderPromotionEvent.ratingObject is EmpireRating empireRating)
            {
                if (Cmdr != null) { Cmdr.empirerating = empireRating; }
                return true;
            }
            if (commanderPromotionEvent.ratingObject is ExplorationRating explorationRating)
            {
                if (Cmdr != null) { Cmdr.explorationrating = explorationRating; }
                return true;
            }
            if (commanderPromotionEvent.ratingObject is ExobiologistRating exobiologistRating)
            {
                if (Cmdr != null) { Cmdr.exobiologistrating = exobiologistRating; }
                return true;
            }
            if (commanderPromotionEvent.ratingObject is FederationRating federationRating)
            {
                if (Cmdr != null) { Cmdr.federationrating = federationRating; }
                return true;
            }
            if (commanderPromotionEvent.ratingObject is MercenaryRating mercenaryRating)
            {
                if (Cmdr != null) { Cmdr.mercenaryrating = mercenaryRating; }
                return true;
            }
            if (commanderPromotionEvent.ratingObject is TradeRating tradeRating)
            {
                // Capture commander ratings and add them to the commander object
                if (Cmdr != null) { Cmdr.traderating = tradeRating; }
                return true;
            }
            return false;
        }

        private bool eventDisembark(DisembarkEvent disembarkEvent) 
        {
            Vehicle = Constants.VEHICLE_LEGS;
            Logging.Info($"Disembarked to {Vehicle}");
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

        private bool eventCarrierJumpEngaged(CarrierJumpEngagedEvent @event)
        {
            // Only update our information if we are still docked at the carrier
            if (Environment == Constants.ENVIRONMENT_DOCKED && @event.carrierId == CurrentStation?.marketId)
            {
                // We are in witch space and in the ship.
                @event.docked = true;
                Environment = Constants.ENVIRONMENT_WITCH_SPACE;
                Vehicle = Constants.VEHICLE_SHIP;

                // Make sure we have at least basic information about the destination star system
                if (NextStarSystem is null)
                {
                    NextStarSystem = new StarSystem()
                    {
                        systemname = @event.systemname,
                        systemAddress = @event.systemAddress
                    };
                }

                // Remove the carrier from its prior location in the origin system so that we can re-save it with a new location
                CurrentStarSystem?.stations.RemoveAll(s => s.marketId == @event.carrierId);

                // Set the destination system as the current star system
                updateCurrentSystem(@event.systemname);

                // Update our station information
                CurrentStation = CurrentStarSystem?.stations.FirstOrDefault(s => s.marketId == @event.carrierId) ?? new Station();
                CurrentStation.marketId = @event.carrierId;
                CurrentStation.systemname = @event.systemname;
                CurrentStation.systemAddress = @event.systemAddress;

                // Add the carrier to the destination system
                CurrentStarSystem?.stations.Add(CurrentStation);

                // (When jumping near a body) Set the destination body as the current stellar body
                if (@event.bodyname != null)
                {
                    updateCurrentStellarBody(@event.bodyname, @event.systemname, @event.systemAddress);
                    CurrentStellarBody.bodyId = @event.bodyId;
                }
            }
            else if (!string.IsNullOrEmpty(@event.originSystemName))
            {
                // Remove the carrier from its prior location in the origin system so that we can re-save it with a new location
                StarSystem starSystem = StarSystemSqLiteRepository.Instance.GetOrFetchStarSystem(@event.originSystemName);
                Station carrier = starSystem?.stations.FirstOrDefault(s => s.marketId == @event.carrierId);
                starSystem?.stations.RemoveAll(s => s.marketId == @event.carrierId);
                // Save the carrier to the updated star system
                if (carrier != null)
                {
                    carrier.systemname = @event.systemname;
                    carrier.systemAddress = @event.systemAddress;
                    if (@event.systemname == CurrentStarSystem?.systemname)
                    {
                        CurrentStarSystem?.stations.Add(carrier);
                        StarSystemSqLiteRepository.Instance.SaveStarSystem(starSystem);
                    }
                    else
                    {
                        StarSystem updatedStarSystem = StarSystemSqLiteRepository.Instance.GetOrFetchStarSystem(@event.systemname);
                        if (updatedStarSystem != null)
                        {
                            updatedStarSystem.stations.Add(carrier);
                            StarSystemSqLiteRepository.Instance.SaveStarSystem(updatedStarSystem);
                        }
                    }
                }
            }

            return true;
        }

        private bool eventCarrierJumped(CarrierJumpedEvent @event)
        {
            if (@event.docked)
            {
                Logging.Info("Carrier jumped to: " + @event.systemname);

                // We are docked and in the ship
                Environment = Constants.ENVIRONMENT_DOCKED;
                Vehicle = Constants.VEHICLE_SHIP;

                // Remove the carrier from its prior location (of any) so that we can re-save it with a new location
                // If we haven't already updated our current star system, the carrier should be in `CurrentStarSystem`. If we have, it should be in `LastStarSystem`.
                if (CurrentStation?.marketId == @event.carrierId || CurrentStation?.name == @event.carriername)
                {
                    CurrentStarSystem?.stations.RemoveAll(s => s.marketId == @event.carrierId);
                }
                else
                {
                    CurrentStation = CurrentStarSystem.stations.FirstOrDefault(s => s.marketId == @event.carrierId || s.name == @event.carriername);
                    if (CurrentStation is null)
                    {
                        CurrentStation = LastStarSystem.stations.FirstOrDefault(s => s.marketId == @event.carrierId || s.name == @event.carriername);
                        if (CurrentStation != null)
                        {
                            LastStarSystem.stations.RemoveAll(s => s.marketId == @event.carrierId);
                            StarSystemSqLiteRepository.Instance.SaveStarSystem(LastStarSystem);
                        }
                    }
                }
                if (CurrentStation == null)
                {
                    // This carrier is unknown to us, might not be in our data source or we might not have connectivity.  Use a placeholder.
                    CurrentStation = new Station();
                }

                // Update current station properties
                CurrentStation.name = @event.carriername;
                CurrentStation.marketId = @event.carrierId;
                CurrentStation.systemname = @event.systemname;
                CurrentStation.systemAddress = @event.systemAddress;
                CurrentStation.Faction = @event.carrierFaction;
                CurrentStation.LargestPad = LandingPadSize.Large; // Carriers always include large pads
                CurrentStation.Model = @event.carrierType;
                CurrentStation.economyShares = @event.carrierEconomies;
                CurrentStation.stationServices = @event.carrierServices;

                // Update our current star system and carrier location
                updateCurrentSystem(@event.systemname);

                // Update our system properties
                if (CurrentStarSystem != null)
                {
                    CurrentStarSystem.systemAddress = @event.systemAddress;
                    CurrentStarSystem.x = @event.x;
                    CurrentStarSystem.y = @event.y;
                    CurrentStarSystem.z = @event.z;

                    // Add our carrier to the new current star system
                    CurrentStarSystem.stations.Add(CurrentStation);

                    // Update the mutable system data from the journal
                    if (@event.population != null)
                    {
                        CurrentStarSystem.population = @event.population;
                        CurrentStarSystem.Economies = new List<Economy> { @event.systemEconomy, @event.systemEconomy2 };
                        CurrentStarSystem.securityLevel = @event.securityLevel;
                        CurrentStarSystem.Faction = @event.controllingsystemfaction;
                    }

                    // Update system faction data if available
                    if (@event.factions != null)
                    {
                        CurrentStarSystem.factions = @event.factions;

                        // Update station controlling faction data
                        foreach (Station station in CurrentStarSystem.stations)
                        {
                            Faction stationFaction =
                                @event.factions.FirstOrDefault(f => f.name == station.Faction.name);
                            if (stationFaction != null)
                            {
                                station.Faction = stationFaction;
                            }
                        }

                        // Check if current system is inhabited by or HQ for squadron faction
                        Faction squadronFaction = @event.factions.FirstOrDefault(f =>
                        {
                            var squadronhomesystem = f.presences
                                .FirstOrDefault(p => p.systemName == CurrentStarSystem.systemname)?.squadronhomesystem;
                            return squadronhomesystem != null && ((bool)squadronhomesystem || f.squadronfaction);
                        });
                        if (squadronFaction != null)
                        {
                            updateSquadronData(squadronFaction, CurrentStarSystem.systemname);
                        }
                    }

                    // (When near a body) Update the body
                    if (@event.bodyname != null && CurrentStellarBody?.bodyname != @event.bodyname)
                    {
                        updateCurrentStellarBody(@event.bodyname, @event.systemname, @event.systemAddress);
                        if (CurrentStellarBody != null)
                        {
                            CurrentStellarBody.bodyId = @event.bodyId;
                            CurrentStellarBody.bodyType = @event.bodyType;                            
                        }
                    }

                    // (When pledged) Powerplay information
                    CurrentStarSystem.Power = @event.Power != null && @event.Power != Power.None
                        ? @event.Power
                        : CurrentStarSystem.Power;
                    CurrentStarSystem.powerState = @event.powerState != null && @event.powerState != PowerplayState.None
                        ? @event.powerState
                        : CurrentStarSystem.powerState;

                    // Update to most recent information
                    CurrentStarSystem.visitLog.Add(@event.timestamp);
                    CurrentStarSystem.updatedat = Dates.fromDateTimeToSeconds(@event.timestamp);
                    StarSystemSqLiteRepository.Instance.SaveStarSystem(CurrentStarSystem);
                }
                
                // Kick off the profile refresh if the companion API is available
                if (CompanionAppService.Instance.CurrentState == CompanionAppService.State.Authorized)
                {
                    // Refresh station data
                    if (@event.fromLoad) { return true; } // Don't fire this event when loading pre-existing logs
                    Thread updateThread = new Thread(() =>
                    {
                        Thread.Sleep(5000);
                        conditionallyRefreshStationProfile();
                    })
                    {
                        IsBackground = true
                    };
                    updateThread.Start();
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

        private bool eventPowerVoucherReceived(PowerVoucherReceivedEvent @event)
        {
            Cmdr.Power = @event.Power;
            return true;
        }

        private bool eventPowerSalaryClaimed(PowerSalaryClaimedEvent @event)
        {
            Cmdr.Power = @event.Power;
            return true;
        }

        private bool eventPowerPreparationVoteCast(PowerPreparationVoteCast @event)
        {
            Cmdr.Power = @event.Power;
            return true;
        }

        private bool eventPowerLeft(PowerLeftEvent @event)
        {
            Cmdr.Power = Power.None;
            Cmdr.powermerits = null;
            Cmdr.powerrating = 0;

            // Store power merits
            EDDIConfiguration configuration = ConfigService.Instance.eddiConfiguration;
            configuration.powerMerits = Cmdr.powermerits;
            ConfigService.Instance.eddiConfiguration = configuration;

            return true;
        }

        private bool eventPowerJoined(PowerJoinedEvent @event)
        {
            Cmdr.Power = @event.Power;
            Cmdr.powermerits = 0;
            Cmdr.powerrating = 1;

            // Store power merits
            EDDIConfiguration configuration = ConfigService.Instance.eddiConfiguration;
            configuration.powerMerits = Cmdr.powermerits;
            ConfigService.Instance.eddiConfiguration = configuration;

            return true;
        }

        private bool eventPowerDefected(PowerDefectedEvent @event)
        {
            Cmdr.Power = @event.toPower;
            // Merits are halved upon defection
            Cmdr.powermerits = (int)Math.Round((double)(Cmdr.powermerits ?? 0) / 2, 0);
            if (Cmdr.powermerits > 10000)
            {
                Cmdr.powerrating = 4;
            }
            if (Cmdr.powermerits > 1500)
            {
                Cmdr.powerrating = 3;
            }
            if (Cmdr.powermerits > 750)
            {
                Cmdr.powerrating = 2;
            }
            if (Cmdr.powermerits > 100)
            {
                Cmdr.powerrating = 1;
            }
            else
            {
                Cmdr.powerrating = 0;
            }

            // Store power merits
            EDDIConfiguration configuration = ConfigService.Instance.eddiConfiguration;
            configuration.powerMerits = Cmdr.powermerits;
            ConfigService.Instance.eddiConfiguration = configuration;

            return true;
        }

        private bool eventPowerplay(PowerplayEvent @event)
        {
            if (Cmdr.powermerits is null)
            {
                // Per the journal, this is written at startup. In actuality, it can also be written whenever switching FSD states
                // and needs to be filtered to prevent redundant outputs.
                Cmdr.Power = @event.Power;
                Cmdr.powerrating = @event.rank;
                Cmdr.powermerits = @event.merits;

                // Store power merits
                EDDIConfiguration configuration = ConfigService.Instance.eddiConfiguration;
                configuration.powerMerits = @event.merits;
                ConfigService.Instance.eddiConfiguration = configuration;

                return true;
            }
            else
            {
                return false;
            }
        }

        private bool eventSystemScanComplete(SystemScanComplete @event)
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
                foreach (var body in CurrentStarSystem.bodies.Where(b => b.scanned is null))
                {
                    body.scanned = @event.timestamp;
                    bodiesToUpdate.Add(body);
                }
                if (bodiesToUpdate.Any()) { CurrentStarSystem.AddOrUpdateBodies(bodiesToUpdate); }
                // Save the updated star system data
                StarSystemSqLiteRepository.Instance.SaveStarSystem(CurrentStarSystem);
            }
            return true;
        }

        private bool eventDiscoveryScan(DiscoveryScanEvent @event)
        {
            if (CurrentStarSystem != null)
            {
                CurrentStarSystem.totalbodies = @event.totalbodies;

                if (@event.progress == 100 && CurrentStarSystem.scannedbodies < @event.totalbodies) // Fully scanned system, make sure that all bodies are marked as scanned
                {
                    // Update any bodies that aren't yet recorded as scanned (these were likely scanned while EDDI was not running)
                    var bodiesToUpdate = new List<Body>();
                    foreach (var body in CurrentStarSystem.bodies.Where(b => b.scanned is null))
                    {
                        body.scanned = @event.timestamp;
                        bodiesToUpdate.Add(body);
                    }
                    if (bodiesToUpdate.Any()) { CurrentStarSystem.AddOrUpdateBodies(bodiesToUpdate); }
                }

                StarSystemSqLiteRepository.Instance.SaveStarSystem(CurrentStarSystem);
            }
            return true;
        }

        private bool eventFriends(FriendsEvent @event)
        {
            var passEvent = false;
            var friend = new Friend
            {
                name = @event.name,
                status = @event.status
            };

            // Does this friend exist in our friends list?
            int index = Cmdr.friends.FindIndex(f => f.name == @event.name);
            if (index >= 0)
            {
                if (Cmdr.friends[index].status != @event.status)
                {
                    // This is a known friend with a revised status: replace in situ (this is more efficient than removing and re-adding).
                    Cmdr.friends[index] = friend;
                    passEvent = true;
                }
            }
            else
            {
                // This is a new friend, add them to the list
                Cmdr.friends.Add(friend);
            }
            return passEvent;
        }

        private async void OnEvent(Event @event)
        {
            // We send the event to all monitors to ensure that their info is up-to-date
            // All changes to state must be handled here, so this must be synchronous
            passToMonitorPreHandlers(@event);

            // Now we pass the data to the responders to process asynchronously, waiting for all to complete
            // Responders must not change global states.
            await passToRespondersAsync(@event);

            // We also pass the event to all active monitors in case they have asynchronous follow-on work, waiting for all to complete
            await passToMonitorPostHandlersAsync(@event);
        }

        private void passToMonitorPreHandlers(Event @event)
        {
            foreach (EDDIMonitor monitor in activeMonitors)
            {
                try
                {
                    monitor.PreHandle(@event);
                }
                catch (Exception ex)
                {
                    Logging.Error($"{monitor.MonitorName()} failed to handle {@event.type} event {@event.raw}", ex);
                }
            }
        }

        private async Task passToRespondersAsync(Event @event)
        {
            List<Task> responderTasks = new List<Task>();
            foreach (EDDIResponder responder in activeResponders)
            {
                var responderTask = Task.Run(() =>
                {
                    try
                    {
                        responder.Handle(@event);
                    }
                    catch (Exception ex)
                    {
                        Logging.Error($"{responder.ResponderName()} failed to handle {@event.type} event {@event.raw}", ex);
                    }
                });
                responderTasks.Add(responderTask);
            }
            await Task.WhenAll(responderTasks.ToArray());
        }

        private async Task passToMonitorPostHandlersAsync(Event @event)
        {
            List<Task> monitorTasks = new List<Task>();
            foreach (EDDIMonitor monitor in activeMonitors)
            {
                var monitorTask = Task.Run(() =>
                {
                    try
                    {
                        monitor.PostHandle(@event);
                    }
                    catch (Exception ex)
                    {
                        Logging.Error($"{monitor.MonitorName()} failed to post-handle {@event.type} event {@event.raw}", ex);
                    }
                });
                monitorTasks.Add(monitorTask);
            }
            await Task.WhenAll(monitorTasks.ToArray());
        }

        private bool eventLocation(LocationEvent theEvent)
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

            updateCurrentSystem(theEvent.systemname);
            // Our data source may not include the system address
            CurrentStarSystem.systemAddress = theEvent.systemAddress;
            // Always update the current system with the current co-ordinates, just in case things have changed
            CurrentStarSystem.x = theEvent.x;
            CurrentStarSystem.y = theEvent.y;
            CurrentStarSystem.z = theEvent.z;

            // Update the mutable system data from the journal
            if (theEvent.population != null)
            {
                CurrentStarSystem.population = theEvent.population;
                CurrentStarSystem.Economies = new List<Economy> { theEvent.Economy, theEvent.Economy2 };
                CurrentStarSystem.securityLevel = theEvent.securityLevel;
                CurrentStarSystem.Faction = theEvent.controllingsystemfaction;
            }

            // Update system faction data if available
            if (theEvent.factions != null)
            {
                CurrentStarSystem.factions = theEvent.factions;

                // Update station controlling faction data
                foreach (Station station in CurrentStarSystem.stations)
                {
                    Faction stationFaction = theEvent.factions.FirstOrDefault(f => f.name == station.Faction.name);
                    if (stationFaction != null)
                    {
                        station.Faction = stationFaction;
                    }
                }

                // Check if current system is inhabited by or HQ for squadron faction
                Faction squadronFaction = theEvent.factions.FirstOrDefault(f => (f.presences.
                    FirstOrDefault(p => p.systemName == CurrentStarSystem.systemname)?.squadronhomesystem ?? false) || f.squadronfaction);
                if (squadronFaction != null)
                {
                    updateSquadronData(squadronFaction, CurrentStarSystem.systemname);
                }
            }

            // (When pledged) Powerplay information
            CurrentStarSystem.Power = theEvent.Power is null ? CurrentStarSystem.Power : theEvent.Power;
            CurrentStarSystem.powerState = theEvent.powerState is null ? CurrentStarSystem.powerState : theEvent.powerState;

            if (theEvent.docked || theEvent.bodytype.ToLowerInvariant() == "station")
            {
                // In this case body = station and our body information is invalid
                CurrentStellarBody = null;

                // Update the station
                string stationName = theEvent.docked ? theEvent.station : theEvent.bodyname;

                Logging.Debug("Now at station " + stationName);
                Station station = CurrentStarSystem.stations.Find(s => s.name == stationName);
                if (station == null)
                {
                    // This station is unknown to us, might not be in our data source or we might not have connectivity.  Use a placeholder
                    station = new Station
                    {
                        name = stationName,
                        systemname = theEvent.systemname
                    };
                    CurrentStarSystem.stations.Add(station);
                }
                CurrentStation = station;

                if (theEvent.docked)
                {
                    // We are docked
                    Environment = Constants.ENVIRONMENT_DOCKED;

                    // If we're not in a taxi or multicrew then we're in our own ship.
                    if (!theEvent.taxi && !theEvent.multicrew) { Vehicle = Constants.VEHICLE_SHIP; }

                    // Update station properties known from this event
                    station.marketId = theEvent.marketId;
                    station.systemAddress = theEvent.systemAddress;
                    station.Faction = theEvent.controllingstationfaction;
                    station.Model = theEvent.stationModel;
                    station.distancefromstar = theEvent.distancefromstar;

                    // Kick off the profile refresh if the companion API is available
                    if (CompanionAppService.Instance.CurrentState == CompanionAppService.State.Authorized)
                    {
                        // Refresh station data
                        if (theEvent.fromLoad) { return true; } // Don't fire this event when loading pre-existing logs
                        Thread updateThread = new Thread(() => conditionallyRefreshStationProfile())
                        {
                            IsBackground = true
                        };
                        updateThread.Start();
                    }
                }
                else
                {
                    Environment = Constants.ENVIRONMENT_NORMAL_SPACE;
                }
            }
            else if (theEvent.bodyname != null)
            {
                // If we are not at a station then our station information is invalid 
                CurrentStation = null;

                // Update the body 
                Logging.Debug("Now at body " + theEvent.bodyname);
                updateCurrentStellarBody(theEvent.bodyname, theEvent.systemname, theEvent.systemAddress);

                if (theEvent.latitude != null && theEvent.longitude != null)
                {
                    Environment = Constants.ENVIRONMENT_LANDED;
                }
                else
                {
                    Environment = Constants.ENVIRONMENT_NORMAL_SPACE;
                }
            }
            else
            {
                // We are near neither a stellar body nor a station. 
                CurrentStellarBody = null;
                CurrentStation = null;
            }

            return true;
        }

        private bool eventDockingRequested(DockingRequestedEvent theEvent)
        {
            bool passEvent = !string.IsNullOrEmpty(theEvent.station);
            Station station = CurrentStarSystem.stations.Find(s => s.name == theEvent.station);
            if (station == null)
            {
                // This station is unknown to us, might not be in our data source or we might not have connectivity.  Use a placeholder
                station = new Station
                {
                    name = theEvent.station,
                    marketId = theEvent.marketId,
                    systemname = CurrentStarSystem.systemname,
                    systemAddress = CurrentStarSystem.systemAddress
                };
                CurrentStarSystem.stations.Add(station);
            }
            station.Model = theEvent.stationDefinition;
            return passEvent;
        }

        private bool eventDocked(DockedEvent theEvent)
        {
            bool passEvent = !string.IsNullOrEmpty(theEvent.station);
            updateCurrentSystem(theEvent.system);

            Station station = CurrentStarSystem.stations.Find(s => s.name == theEvent.station);
            if (Environment == Constants.ENVIRONMENT_DOCKED && CurrentStation?.marketId == station?.marketId)
            {
                // We are already at this station
                Logging.Debug("Already at station " + theEvent.station);
                passEvent = false;
            }
            else
            {
                // Update the station
                Logging.Debug("Now at station " + theEvent.station);
                if (station == null)
                {
                    // This station is unknown to us, might not be in our data source or we might not have connectivity.  Use a placeholder
                    station = new Station
                    {
                        name = theEvent.station,
                        systemname = theEvent.system
                    };
                    CurrentStarSystem.stations.Add(station);
                }
            }

            // We are docked
            if (station != null)
            {
                Environment = Constants.ENVIRONMENT_DOCKED;

                // Not all stations in our database will have a system address or market id, so we set them here
                station.systemAddress = theEvent.systemAddress;
                station.marketId = theEvent.marketId;

                // Information from the event might be more current than our data source so use it in preference
                station.Faction = theEvent.controllingfaction;
                station.stationServices = theEvent.stationServices;
                station.economyShares = theEvent.economyShares;

                // Update other station information available from the event
                station.Model = theEvent.stationModel;
                station.stationServices = theEvent.stationServices;
                station.distancefromstar = theEvent.distancefromstar;

                CurrentStation = station;
                CurrentStellarBody = null;

                // Kick off the profile refresh if the companion API is available
                if (CompanionAppService.Instance.CurrentState == CompanionAppService.State.Authorized)
                {
                    // Refresh station data
                    if (theEvent.fromLoad || !passEvent) { return false; } // Don't fire this event when loading pre-existing logs or if we were already at this station
                    Thread updateThread = new Thread(() => conditionallyRefreshStationProfile())
                    {
                        IsBackground = true
                    };
                    updateThread.Start();
                }
            }
            return passEvent;
        }

        private bool eventUndocked(UndockedEvent theEvent)
        {
            Environment = Constants.ENVIRONMENT_NORMAL_SPACE;

            // Call refreshProfile() to ensure that our ship is up-to-date
            refreshProfile();

            return true;
        }

        private bool eventTouchdown(TouchdownEvent theEvent)
        {
            updateCurrentSystem(theEvent.systemname);
            updateCurrentStellarBody(theEvent.bodyname, theEvent.systemname, theEvent.systemAddress);

            if (theEvent.taxi != null && theEvent.taxi == true)
            {
                Vehicle = Constants.VEHICLE_TAXI;
            }
            else if (theEvent.multicrew != null && theEvent.multicrew == true)
            {
                Vehicle = Constants.VEHICLE_MULTICREW;
            }

            // Only pass on this event if our longitude and lattitude are set
            // (if not then this is probably being written prior to a `Location` event))
            if (theEvent.latitude != null && theEvent.longitude != null)
            {
                Environment = Constants.ENVIRONMENT_LANDED;
                if (theEvent.taxi != null && theEvent.taxi == true)
                {
                    Vehicle = Constants.VEHICLE_TAXI;
                }
                else if (theEvent.multicrew != null && theEvent.multicrew == true)
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
                Logging.Info($"Touchdown in {Vehicle}");
                return true;
            }
            Logging.Info($"Touchdown in {Vehicle}");
            return false;
        }

        private bool eventLiftoff(LiftoffEvent theEvent)
        {
            updateCurrentSystem(theEvent.systemname);
            updateCurrentStellarBody(theEvent.bodyname, theEvent.systemname, theEvent.systemAddress);

            Environment = Constants.ENVIRONMENT_NORMAL_SPACE;

            if (theEvent.taxi != null && theEvent.taxi == true)
            {
                Vehicle = Constants.VEHICLE_TAXI;
            }
            else if (theEvent.multicrew != null && theEvent.multicrew == true)
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

        private bool eventMarket(MarketEvent theEvent)
        {
            // Don't proceed if we've already viewed the market while docked or when loading pre-existing logs
            if (theEvent.fromLoad) { return false; }

            // Don't proceed if the event data isn't what we expect
            if (theEvent.system != CurrentStarSystem?.systemname) { return false; }

            var items = theEvent.info.Items
                .Select(q => q.ToCommodityMarketQuote())
                .ToList();

            if (theEvent.info.Items.Count == items.Count) // We've successfully parsed all commodity items
            {
                // Update the current station commodities
                if (CurrentStation != null && CurrentStation?.marketId == theEvent.marketId)
                {
                    CurrentStation.commodities = theEvent.info.Items.Select(q => q.ToCommodityMarketQuote()).ToList();
                    CurrentStation.commoditiesupdatedat = Dates.fromDateTimeToSeconds(theEvent.timestamp);

                    // Update the current station information in our backend DB
                    Logging.Debug("Star system information updated from remote server; updating local copy");
                    StarSystemSqLiteRepository.Instance.SaveStarSystem(CurrentStarSystem);

                    // Post an update event for new market data
                    enqueueEvent(new MarketInformationUpdatedEvent(theEvent.info.timestamp, theEvent.system, theEvent.station, theEvent.marketId, new HashSet<string> { "market" }) { raw = theEvent.raw });
                    return true;
                }
                else
                {
                    var station = CurrentStarSystem?.stations.Where(s => s.marketId == theEvent.marketId).FirstOrDefault();
                    if (station != null)
                    {
                        station.commodities = theEvent.info.Items.Select(q => q.ToCommodityMarketQuote()).ToList();
                        station.commoditiesupdatedat = Dates.fromDateTimeToSeconds(theEvent.timestamp);
                        StarSystemSqLiteRepository.Instance.SaveStarSystem(CurrentStarSystem);
                    }
                }
            }
            return false;
        }

        private bool eventOutfitting(OutfittingEvent theEvent)
        {
            // Don't proceed if we've already viewed outfitting while docked or when loading pre-existing logs
            if (theEvent.fromLoad) { return false; }

            // Don't proceed if the event data isn't what we expect
            if (theEvent.system != CurrentStarSystem?.systemname) { return false; }

            var modules = theEvent.info.Items
                .Select(i => EddiDataDefinitions.Module.FromOutfittingInfo(i))
                .Where(i => i != null)
                .ToList();

            if (theEvent.info.Items.Count == modules.Count) // We've successfully parsed all module items
            {
                // Update the current station outfitting
                if (CurrentStation?.marketId != null && CurrentStation?.marketId == theEvent.marketId)
                {
                    CurrentStation.outfitting = modules;
                    CurrentStation.outfittingupdatedat = Dates.fromDateTimeToSeconds(theEvent.info.timestamp);

                    // Update the current station information in our backend DB
                    Logging.Debug("Star system information updated from remote server; updating local copy");
                    StarSystemSqLiteRepository.Instance.SaveStarSystem(CurrentStarSystem);

                    // Post an update event for new outfitting data
                    enqueueEvent(new MarketInformationUpdatedEvent(theEvent.info.timestamp, theEvent.system, theEvent.station, theEvent.marketId, new HashSet<string> { "outfitting" }) { raw = theEvent.raw });
                    return true;
                }
                else
                {
                    var station = CurrentStarSystem?.stations.Where(s => s.marketId == theEvent.marketId).FirstOrDefault();
                    if (station != null)
                    {
                        station.outfitting = modules;
                        station.outfittingupdatedat = Dates.fromDateTimeToSeconds(theEvent.info.timestamp);
                        StarSystemSqLiteRepository.Instance.SaveStarSystem(CurrentStarSystem);
                    }
                }
            }
            return false;
        }

        private bool eventShipyard(ShipyardEvent theEvent)
        {
            // Don't proceed if we've already viewed outfitting while docked or when loading pre-existing logs
            if (theEvent.fromLoad) { return false; }

            // Don't proceed if the event data isn't what we expect
            if (theEvent.system != CurrentStarSystem?.systemname) { return false; }

            var ships = theEvent.info.PriceList
                .Select(s => Ship.FromShipyardInfo(s))
                .Where(s => s != null)
                .ToList();

            if (theEvent.info.PriceList.Count == ships.Count) // We've successfully parsed all ship items
            {
                if (CurrentStation?.marketId != null && CurrentStation?.marketId == theEvent.marketId)
                {
                    // Update the current station shipyard
                    CurrentStation.shipyard = ships;
                    CurrentStation.shipyardupdatedat = Dates.fromDateTimeToSeconds(theEvent.info.timestamp);

                    // Update the current station information in our backend DB
                    StarSystemSqLiteRepository.Instance.SaveStarSystem(CurrentStarSystem);

                    // Post an update event for new shipyard data
                    enqueueEvent(new MarketInformationUpdatedEvent(theEvent.info.timestamp, theEvent.system, theEvent.station, theEvent.marketId, new HashSet<string> { "shipyard" }) { raw = theEvent.raw });
                    return true;
                }
                else
                {
                    var station = CurrentStarSystem?.stations.Where(s => s.marketId == theEvent.marketId).FirstOrDefault();
                    if (station != null)
                    {
                        station.shipyard = ships;
                        station.shipyardupdatedat = Dates.fromDateTimeToSeconds(theEvent.info.timestamp);
                        StarSystemSqLiteRepository.Instance.SaveStarSystem(CurrentStarSystem);
                    }
                }
            }
            return false;
        }

        private void updateCurrentSystem(string name)
        {
            if (name == null || CurrentStarSystem?.systemname == name) { return; }

            // Discard any signal sources from the current star system
            if (CurrentStarSystem != null)
            {
                CurrentStarSystem.signalSources = ImmutableList<SignalSource>.Empty;
            }

            // We have changed system so update the old one as to when we left
            StarSystemSqLiteRepository.Instance.LeaveStarSystem(CurrentStarSystem);

            LastStarSystem = CurrentStarSystem;
            if (NextStarSystem != null && NextStarSystem.systemname == name)
            {
                CurrentStarSystem = NextStarSystem;
                NextStarSystem = null;
            }
            else
            {
                CurrentStarSystem = StarSystemSqLiteRepository.Instance.GetOrCreateStarSystem(name);
            }

            // Clear our destination system once we arrive
            if (destinationStarSystem?.systemname == currentStarSystem?.systemname)
            {
                updateDestinationSystem(null);
            }

            setCommanderTitle();
        }

        private void updateCurrentStellarBody(string bodyName, string systemName, ulong? systemAddress = null)
        {
            // Make sure our system information is up to date
            if (CurrentStarSystem == null || CurrentStarSystem.systemname != systemName)
            {
                updateCurrentSystem(systemName);
            }
            // Update the body 
            if (CurrentStarSystem != null)
            {
                var body = CurrentStarSystem.bodies?.Find(s => s.bodyname == bodyName);
                if (body == null)
                {
                    // We may be near a ring. For rings, we want to select the parent body
                    var ringedBodies = CurrentStarSystem.bodies?
                        .Where(b => b?.rings?.Count > 0).ToList() ?? new List<Body>();
                    foreach (var ringedBody in ringedBodies)
                    {
                        var ring = ringedBody.rings.FirstOrDefault(r => r.name == bodyName);
                        if (ring != null)
                        {
                            body = ringedBody;
                            break;
                        }
                    }
                }
                if (body == null)
                {
                    // This body is unknown to us, might not be in EDDB or we might not have connectivity.  Use a placeholder 
                    body = new Body
                    {
                        bodyname = bodyName,
                        systemname = systemName,
                        systemAddress = systemAddress,
                    };
                    CurrentStarSystem.AddOrUpdateBody(body);
                }
                CurrentStellarBody = body;
            }
        }

        private bool eventFSDEngaged(FSDEngagedEvent @event)
        {
            // Keep track of our environment
            if (@event.target == "Supercruise")
            {
                Environment = Constants.ENVIRONMENT_SUPERCRUISE;
            }
            else
            {
                Environment = Constants.ENVIRONMENT_WITCH_SPACE;
            }
            
            // Remove information about the current station and stellar body 
            CurrentStation = null;
            CurrentStellarBody = null;

            // Set the destination system as the current star system
            updateCurrentSystem(@event.system);

            // Save a copy of this event for later reference
            lastEvents[nameof(FSDEngagedEvent)] = @event;;

            return true;
        }

        private bool eventFSDTarget(FSDTargetEvent @event)
        {
            // Set and prepare data about the next star system
            NextStarSystem = StarSystemSqLiteRepository.Instance.GetOrFetchStarSystem(@event.system);
            if (NextStarSystem != null && !NextStarSystem.bodies.Any(b => b.mainstar ?? false))
            {
                // This system is unknown to us, might not be recorded, or we might not have connectivity.  Use a placeholder main star
                var mainStar = new Body
                {
                    bodyType = BodyType.FromEDName("Star"),
                    systemname = NextStarSystem.systemname,
                    systemAddress = nextStarSystem.systemAddress,
                    distance = 0M,
                    stellarclass = @event.starclass
                };
                NextStarSystem.AddOrUpdateBody(mainStar);
                StarSystemSqLiteRepository.Instance.SaveStarSystem(NextStarSystem);
            }
            return true;
        }

        private bool eventFileHeader(FileHeaderEvent @event)
        {
            // Test whether we're in beta by checking the filename, version described by the header,
            // and certain version / build combinations. Test the most common situations first.
            gameIsBeta =
                (
                    @event.filename.Contains("Alpha") ||
                    @event.filename.Contains("Beta") ||
                    @event.version.Contains("Beta") ||
                    @event.version.Contains("Alpha") ||
                    (
                        @event.version.Contains("2.2") &&
                        (
                            @event.build.Contains("r121645/r0") ||
                            @event.build.Contains("r129516/r0")
                        )
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

        private bool eventJumped(JumpedEvent theEvent)
        {
            bool passEvent;
            Logging.Info("Jumped to " + theEvent.system);
            if (CurrentStarSystem == null || CurrentStarSystem.systemname != theEvent.system)
            {
                // The 'StartJump' event must have been missed
                updateCurrentSystem(theEvent.system);
            }

            passEvent = true;

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

            if (CurrentStarSystem != null)
            {
                CurrentStarSystem.systemAddress = theEvent.systemAddress;
                CurrentStarSystem.x = theEvent.x;
                CurrentStarSystem.y = theEvent.y;
                CurrentStarSystem.z = theEvent.z;
                CurrentStarSystem.Faction = theEvent.controllingfaction;
                CurrentStellarBody = CurrentStarSystem.bodies.Find(b => b.bodyname == theEvent.star)
                                     ?? CurrentStarSystem.bodies.Find(b => b.distance == 0);
                CurrentStarSystem.conflicts = theEvent.conflicts;

                // Update system faction data if available
                if (theEvent.factions != null)
                {
                    CurrentStarSystem.factions = theEvent.factions;

                    // Update station controlling faction data
                    foreach (Station station in CurrentStarSystem.stations)
                    {
                        Faction stationFaction = theEvent.factions.Find(f => f.name == station.Faction.name);
                        if (stationFaction != null)
                        {
                            station.Faction = stationFaction;
                        }
                    }

                    // Check if current system is inhabited by or HQ for squadron faction
                    Faction squadronFaction = theEvent.factions.Find(f =>
                        (f.presences.Find(p => p.systemName == CurrentStarSystem.systemname)?.squadronhomesystem ?? false) ||
                        f.squadronfaction);
                    if (squadronFaction != null)
                    {
                        updateSquadronData(squadronFaction, CurrentStarSystem.systemname);
                    }
                }

                CurrentStarSystem.Economies = new List<Economy> { theEvent.Economy, theEvent.Economy2 };
                CurrentStarSystem.securityLevel = theEvent.securityLevel;
                if (theEvent.population != null)
                {
                    CurrentStarSystem.population = theEvent.population;
                }

                // If we don't have any information about bodies in the system yet, create a basic star from current and saved event data
                if ((CurrentStellarBody == null || string.IsNullOrEmpty(currentStellarBody.bodyname)) &&
                    !string.IsNullOrEmpty(theEvent.star))
                {
                    CurrentStellarBody = new Body()
                    {
                        bodyname = theEvent.star,
                        bodyType = BodyType.FromEDName("Star"),
                        stellarclass = (lastEvents.TryGetValue(nameof(FSDEngagedEvent), out Event ev)
                                ? (FSDEngagedEvent)ev
                                : null)
                            ?.stellarclass,
                    };
                    CurrentStarSystem.AddOrUpdateBody(CurrentStellarBody);
                }

                // (When pledged) Powerplay information
                CurrentStarSystem.Power = theEvent.Power is null ? CurrentStarSystem.Power : theEvent.Power;
                CurrentStarSystem.powerState =
                    theEvent.powerState is null ? CurrentStarSystem.powerState : theEvent.powerState;

                // Update to most recent information
                CurrentStarSystem.visitLog.Add(theEvent.timestamp);
                CurrentStarSystem.updatedat = Dates.fromDateTimeToSeconds(theEvent.timestamp);
                StarSystemSqLiteRepository.Instance.SaveStarSystem(CurrentStarSystem);
            }

            // After jump has completed we are always in supercruise
            Environment = Constants.ENVIRONMENT_SUPERCRUISE;

            // No longer in 'station instance'
            CurrentStation = null;

            return passEvent;
        }

        private bool eventEnteredSupercruise(EnteredSupercruiseEvent theEvent)
        {
            Environment = Constants.ENVIRONMENT_SUPERCRUISE;
            updateCurrentSystem(theEvent.system);

            if (theEvent.systemAddress != null) { CurrentStarSystem.systemAddress = theEvent.systemAddress; }

            // No longer in 'station instance'
            CurrentStation = null;

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

        private bool eventEnteredNormalSpace(EnteredNormalSpaceEvent theEvent)
        {
            Environment = Constants.ENVIRONMENT_NORMAL_SPACE;

            if (theEvent.bodyType == BodyType.FromEDName("Station"))
            {
                // In this case body == station
                Station station = CurrentStarSystem.stations.Find(s => s.name == theEvent.bodyname);
                if (station == null)
                {
                    // This station is unknown to us, might not be in our data source or we might not have connectivity.  Use a placeholder
                    station = new Station
                    {
                        name = theEvent.bodyname,
                        systemname = theEvent.systemname
                    };
                }
                CurrentStation = station;
            }
            else if (theEvent.bodyname != null)
            {
                updateCurrentStellarBody(theEvent.bodyname, theEvent.systemname, theEvent.systemAddress);
            }
            updateCurrentSystem(theEvent.systemname);

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

        private bool eventCrewJoined(CrewJoinedEvent theEvent)
        {
            inTelepresence = true;
            multicrewVehicleHolder = Vehicle;
            Vehicle = Constants.VEHICLE_MULTICREW;
            Logging.Info("Entering multicrew session");
            return true;
        }

        private bool eventCrewLeft(CrewLeftEvent theEvent)
        {
            inTelepresence = false;
            Vehicle = multicrewVehicleHolder;
            Logging.Info($"Leaving multicrew session to vehicle {Vehicle}");
            return true;
        }

        private bool eventCommanderLoading(CommanderLoadingEvent theEvent)
        {
            // Set our commander name and ID
            if (Cmdr.name != theEvent.name)
            {
                Cmdr.name = theEvent.name;
                ObtainResponder("EDSM Responder").Reload();
            }
            Cmdr.EDID = theEvent.frontierID;
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

            // Set our commander name and ID
            if (Cmdr.name != theEvent.commander)
            {
                Cmdr.name = theEvent.commander;
                ObtainResponder("EDSM Responder").Reload();
            }
            Cmdr.EDID = theEvent.frontierID;

            // Identify active game version
            inHorizons = theEvent.horizons;
            inOdyssey = theEvent.odyssey;
            gameBuild = theEvent.gamebuild;
            gameVersion = theEvent.gameversion;

            return true;
        }

        private bool eventCommanderRatings(CommanderRatingsEvent theEvent)
        {
            // Capture commander ratings and add them to the commander object
            if (Cmdr != null)
            {
                Cmdr.combatrating = theEvent.combat;
                Cmdr.traderating = theEvent.trade;
                Cmdr.explorationrating = theEvent.exploration;
                Cmdr.cqcrating = theEvent.cqc;
                Cmdr.empirerating = theEvent.empire;
                Cmdr.federationrating = theEvent.federation;
            }
            return true;
        }

        private bool eventSquadronStartup(SquadronStartupEvent theEvent)
        {
            SquadronRank rank = SquadronRank.FromRank(theEvent.rank + 1);

            // Update the configuration file
            EDDIConfiguration configuration = ConfigService.Instance.eddiConfiguration;
            configuration.SquadronName = theEvent.name;
            configuration.SquadronRank = rank;
            ConfigService.Instance.eddiConfiguration = configuration;

            // Update the squadron UI data
            Application.Current?.Dispatcher?.Invoke(() =>
            {
                if (Application.Current?.MainWindow != null)
                {
                    ((MainWindow)Application.Current.MainWindow).eddiSquadronNameText.Text = theEvent.name;
                    ((MainWindow)Application.Current.MainWindow).squadronRankDropDown.SelectedItem = rank.localizedName;
                }
            });

            // Update the commander object, if it exists
            if (Cmdr != null)
            {
                Cmdr.squadronname = theEvent.name;
                Cmdr.squadronrank = rank;
            }
            return true;
        }

        private bool eventSquadronStatus(SquadronStatusEvent theEvent)
        {
            EDDIConfiguration configuration = ConfigService.Instance.eddiConfiguration;

            switch (theEvent.status)
            {
                case "created":
                    {
                        SquadronRank rank = SquadronRank.FromRank(1);

                        // Update the configuration file
                        configuration.SquadronName = theEvent.name;
                        configuration.SquadronRank = rank;

                        // Update the squadron UI data
                        Application.Current?.Dispatcher?.Invoke(() =>
                        {
                            if (Application.Current?.MainWindow != null)
                            {
                                ((MainWindow)Application.Current.MainWindow).eddiSquadronNameText.Text = theEvent.name;
                                ((MainWindow)Application.Current.MainWindow).squadronRankDropDown.SelectedItem = rank.localizedName;
                                configuration = ((MainWindow)Application.Current.MainWindow).resetSquadronRank(configuration);
                            }
                        });

                        // Update the commander object, if it exists
                        if (Cmdr != null)
                        {
                            Cmdr.squadronname = theEvent.name;
                            Cmdr.squadronrank = rank;
                        }
                        break;
                    }
                case "joined":
                    {
                        // Update the configuration file
                        configuration.SquadronName = theEvent.name;

                        // Update the squadron UI data
                        Application.Current?.Dispatcher?.Invoke(() =>
                        {
                            if (Application.Current?.MainWindow != null)
                            {
                                ((MainWindow)Application.Current.MainWindow).eddiSquadronNameText.Text = theEvent.name;
                            }
                        });

                        // Update the commander object, if it exists
                        if (Cmdr != null)
                        {
                            Cmdr.squadronname = theEvent.name;
                        }
                        break;
                    }
                case "disbanded":
                case "kicked":
                case "left":
                    {
                        // Update the configuration file
                        configuration.SquadronName = null;
                        configuration.SquadronID = null;

                        // Update the squadron UI data
                        Application.Current?.Dispatcher?.Invoke(() =>
                        {
                            if (Application.Current?.MainWindow != null)
                            {
                                ((MainWindow)Application.Current.MainWindow).eddiSquadronNameText.Text = string.Empty;
                                ((MainWindow)Application.Current.MainWindow).eddiSquadronIDText.Text = string.Empty;
                                configuration = ((MainWindow)Application.Current.MainWindow).resetSquadronRank(configuration);
                            }
                        });

                        // Update the commander object, if it exists
                        if (Cmdr != null)
                        {
                            Cmdr.squadronname = null;
                        }
                        break;
                    }
            }
            ConfigService.Instance.eddiConfiguration = configuration;
            return true;
        }

        private bool eventSquadronRank(SquadronRankEvent theEvent)
        {
            SquadronRank rank = SquadronRank.FromRank(theEvent.newrank + 1);

            // Update the configuration file
            EDDIConfiguration configuration = ConfigService.Instance.eddiConfiguration;
            configuration.SquadronName = theEvent.name;
            configuration.SquadronRank = rank;
            ConfigService.Instance.eddiConfiguration = configuration;

            // Update the squadron UI data
            Application.Current?.Dispatcher?.Invoke(() =>
            {
                if (Application.Current?.MainWindow != null)
                {
                    ((MainWindow)Application.Current.MainWindow).eddiSquadronNameText.Text = theEvent.name;
                    ((MainWindow)Application.Current.MainWindow).squadronRankDropDown.SelectedItem = rank.localizedName;
                }
            });

            // Update the commander object, if it exists
            if (Cmdr != null)
            {
                Cmdr.squadronname = theEvent.name;
                Cmdr.squadronrank = rank;
            }
            return true;
        }

        private bool eventEnteredCQC(EnteredCQCEvent theEvent)
        {
            // In CQC we don't want to report anything, so set our Telepresence flag
            inTelepresence = true;
            return true;
        }

        private bool eventSRVLaunched(SRVLaunchedEvent theEvent)
        {
            // SRV is always player-controlled, so we are in the SRV
            Vehicle = Constants.VEHICLE_SRV;
            return true;
        }

        private bool eventSRVDocked(SRVDockedEvent theEvent)
        {
            // We are back in the ship
            Vehicle = Constants.VEHICLE_SHIP;
            return true;
        }

        private bool eventFighterLaunched(FighterLaunchedEvent theEvent)
        {
            if (theEvent.playercontrolled)
            {
                // We are in the fighter
                Vehicle = Constants.VEHICLE_FIGHTER;
            }
            else
            {
                // We are (still) in the ship
                Vehicle = Constants.VEHICLE_SHIP;
            }
            return true;
        }

        private bool eventFighterDocked(FighterDockedEvent theEvent)
        {
            // We are back in the ship
            Vehicle = Constants.VEHICLE_SHIP;
            return true;
        }

        private bool eventVehicleDestroyed(VehicleDestroyedEvent theEvent)
        {
            // We are back in the ship
            Vehicle = Constants.VEHICLE_SHIP;
            return true;
        }

        private bool eventNearSurface(NearSurfaceEvent theEvent)
        {
            // We won't update CurrentStation with this event, as doing so triggers false / premature updates from the Frontier API
            CurrentStation = null;

            if (theEvent.approaching_surface)
            {
                // Update the body 
                Body body = CurrentStarSystem?.bodies?.Find(s => s.bodyname == theEvent.bodyname);
                if (body == null)
                {
                    // This body is unknown to us, might not be in our data source or we might not have connectivity.  Use a placeholder 
                    body = new Body
                    {
                        bodyname = theEvent.bodyname,
                        systemname = theEvent.systemname
                    };
                }
                // System address may not be included in our data source, so we add it here. 
                body.systemAddress = theEvent.systemAddress;
                CurrentStellarBody = body;
            }
            else
            {
                // Clear the body we are leaving 
                CurrentStellarBody = null;
            }
            updateCurrentSystem(theEvent.systemname);
            return true;
        }

        private bool eventStarScanned(StarScannedEvent theEvent)
        {
            // We just scanned a star.  We can only proceed if we know our current star system
            updateCurrentSystem(theEvent.star?.systemname);
            if (CurrentStarSystem == null) { return false; }

            // We use an un-named temporary star at distance 0M during the FSD Target event.
            // Try to match and replace that temporary star if it exists. Otherwise, match by body name.
            Body star = CurrentStarSystem.bodies?
                .Where(s => s.bodyType == BodyType.FromEDName("Star")).ToList()
                .Find(s => 
                    (string.IsNullOrEmpty(s.bodyname) && s.distance == 0M && s.distance == theEvent.distance) || 
                    s.bodyname == theEvent.bodyname);
            if (star?.scanned is null)
            {
                CurrentStarSystem.AddOrUpdateBody(theEvent.star);
                StarSystemSqLiteRepository.Instance.SaveStarSystem(CurrentStarSystem);
                return true;
            }
            return false;
        }

        private bool eventBodyScanned(BodyScannedEvent theEvent)
        {
            // We just scanned a body.  We can only proceed if we know our current star system
            updateCurrentSystem(theEvent.body?.systemname);
            if (CurrentStarSystem == null) { return false; }

            // Add this body if it hasn't been previously added to our database, but don't
            // replace prior data which isn't re-obtainable from this event. 
            // (e.g. alreadydiscovered, scanned, alreadymapped, mapped, mappedEfficiently, etc.)
            Body body = CurrentStarSystem.bodies?.Find(s => s.bodyname == theEvent.bodyname);
            if (body?.scanned is null)
            {
                CurrentStarSystem.AddOrUpdateBody(theEvent.body);

                Logging.Debug("Saving data for scanned body " + theEvent.bodyname);
                StarSystemSqLiteRepository.Instance.SaveStarSystem(CurrentStarSystem);
                return true;
            }
            return false;
        }

        private bool eventBodyMapped(BodyMappedEvent theEvent)
        {
            if (CurrentStarSystem != null && theEvent.systemAddress == CurrentStarSystem?.systemAddress)
            {
                // We've already updated the body (via the journal monitor) if the CurrentStarSystem isn't null
                // Here, we just need to save the data and update our current stellar body
                StarSystemSqLiteRepository.Instance.SaveStarSystem(CurrentStarSystem);
                updateCurrentStellarBody(theEvent.bodyName, CurrentStarSystem?.systemname, CurrentStarSystem?.systemAddress);
            }
            return true;
        }

        private bool eventRingMapped(RingMappedEvent theEvent)
        {
            if (CurrentStarSystem != null && theEvent.systemAddress == CurrentStarSystem?.systemAddress)
            {
                updateCurrentStellarBody(theEvent.ringname, CurrentStarSystem?.systemname, CurrentStarSystem?.systemAddress);
            }
            return true;
        }

        /// <summary>Obtain information from the companion API and use it to refresh our own data</summary>
        public bool refreshProfile(bool refreshStation = false)
        {
            bool success = true;
            if (CompanionAppService.Instance?.CurrentState == CompanionAppService.State.Authorized)
            {
                try
                {
                    var profileJson = CompanionAppService.Instance.ProfileEndpoint.GetProfile();
                    if (profileJson != null)
                    {
                        var profile = FrontierApiProfile.FromJson(profileJson);

                        // Update our commander object
                        var updatedCmdr = Commander.FromFrontierApiCmdr(Cmdr, profile.Cmdr, profile.timestamp, JournalTimeStamp, out bool cmdrMatches);

                        // Stop if the commander returned from the profile does not match our expected commander name
                        if (!cmdrMatches) { return false; }

                        Cmdr = updatedCmdr;

                        bool updatedCurrentStarSystem = false;

                        // Only set the current star system if it is not present, otherwise we leave it to events
                        if (CurrentStarSystem == null)
                        {
                            updateCurrentSystem(profile.currentStarSystem);
                            setCommanderTitle();

                            if (profile.docked && profile.currentStarSystem == CurrentStarSystem?.systemname && CurrentStarSystem?.stations != null)
                            {
                                // Only set the current station if it is not present, otherwise we leave it to events
                                CurrentStation = CurrentStation ?? CurrentStarSystem.stations.FirstOrDefault(s => s.marketId == profile.LastStationMarketID)
                                                 ?? CurrentStarSystem.stations.FirstOrDefault(s => s.name == profile.LastStationName);
                                if (CurrentStation != null)
                                {
                                    Logging.Debug("Set current station to " + CurrentStation.name);
                                    CurrentStation.updatedat = Dates.fromDateTimeToSeconds(DateTime.UtcNow);
                                    updatedCurrentStarSystem = true;
                                }
                            }
                        }

                        if (refreshStation && CurrentStation != null && Environment == Constants.ENVIRONMENT_DOCKED)
                        {
                            // Refresh station data
                            Thread updateThread = new Thread(() => conditionallyRefreshStationProfile())
                            {
                                IsBackground = true
                            };
                            updateThread.Start();
                        }

                        if (updatedCurrentStarSystem)
                        {
                            Logging.Debug("Star system information updated from remote server; updating local copy");
                            StarSystemSqLiteRepository.Instance.SaveStarSystem(CurrentStarSystem);
                        }

                        foreach (EDDIMonitor monitor in activeMonitors)
                        {
                            try
                            {
                                Thread monitorThread = new Thread(() =>
                                {
                                    try
                                    {
                                        monitor.HandleProfile(profile.json);
                                    }
                                    catch (Exception ex)
                                    {
                                        Logging.Warn("Monitor failed", ex);
                                    }
                                })
                                {
                                    Name = monitor.MonitorName(),
                                    IsBackground = true
                                };
                                monitorThread.Start();
                            }
                            catch (ThreadAbortException tax)
                            {
                                Thread.ResetAbort();
                                Logging.Debug("Thread aborted", tax);
                                success = false;
                            }
                            catch (Exception ex)
                            {
                                Logging.Error("Monitor " + monitor.MonitorName() + " failed to handle profile.", ex);
                                success = false;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logging.Error("Exception obtaining profile", ex);
                    success = false;
                }
            }
            return success;
        }

        /// <summary>Obtain fleet carrier information from the companion API and use it to refresh our own data</summary>
        public void RefreshFleetCarrierFromFrontierAPI(bool forceRefresh = false)
        {
            if (CompanionAppService.Instance?.CurrentState == CompanionAppService.State.Authorized)
            {
                var frontierApiCarrierJson = CompanionAppService.Instance.FleetCarrierEndpoint.GetFleetCarrier(forceRefresh);
                if (frontierApiCarrierJson != null)
                {
                    var timestamp = frontierApiCarrierJson["timestamp"]?.ToObject<DateTime>() ?? DateTime.MinValue;

                    // Update our Fleet Carrier object
                    LockManager.GetLock(nameof(FleetCarrier), () =>
                    {
                        if (FleetCarrier is null)
                        {
                            FleetCarrier = new FleetCarrier(frontierApiCarrierJson, timestamp);
                        }
                        else
                        {
                            FleetCarrier.UpdateFrom(frontierApiCarrierJson, timestamp);
                        }
                    });
                }
            }
        }

        private void setSystemDistanceFromHome(StarSystem system)
        {
            if (system is null || HomeStarSystem is null) { return; }
            system.distancefromhome = getSystemDistance(system, HomeStarSystem);
            Logging.Debug("Distance from home is " + system.distancefromhome);
        }

        public void setSystemDistanceFromDestination(StarSystem system)
        {
            if (DestinationStarSystem is null) { return; }
            DestinationDistanceLy = getSystemDistance(system, DestinationStarSystem);
            Logging.Debug("Distance from destination system is " + DestinationDistanceLy);
        }

        public decimal getSystemDistance(StarSystem curr, StarSystem dest)
        {
            return curr?.DistanceFromStarSystem(dest) ?? 0;
        }

        /// <summary>Work out the title for the commander in the current system</summary>
        private const int minEmpireRankForTitle = 3;
        private const int minFederationRankForTitle = 1;
        private void setCommanderTitle()
        {
            if (Cmdr != null)
            {
                Cmdr.title = Eddi.Properties.EddiResources.Commander;
                if (CurrentStarSystem != null)
                {
                    if (CurrentStarSystem.Faction?.Allegiance?.invariantName == "Federation" && Cmdr.federationrating != null && Cmdr.federationrating.rank > minFederationRankForTitle)
                    {
                        Cmdr.title = Cmdr.federationrating.localizedName;
                    }
                    else if (CurrentStarSystem.Faction?.Allegiance?.invariantName == "Empire" && Cmdr.empirerating != null && Cmdr.empirerating.rank > minEmpireRankForTitle)
                    {
                        Cmdr.title = Cmdr.empirerating.maleRank.localizedName;
                    }
                }
            }
        }

        /// <summary>
        /// Find all monitors
        /// </summary>
        public List<EDDIMonitor> findMonitors()
        {
            var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            if (string.IsNullOrEmpty(path))
            {
                Logging.Warn("Unable to start EDDI Monitors, application directory path not found.");
                return null;
            }

            DirectoryInfo dir = new DirectoryInfo(path);
            List<EDDIMonitor> foundMonitors = new List<EDDIMonitor>();
            Type pluginType = typeof(EDDIMonitor);
            foreach (FileInfo file in dir.GetFiles("*Monitor.dll", SearchOption.AllDirectories))
            {
                Logging.Debug("Checking potential plugin at " + file.FullName);
                try
                {
                    Assembly assembly = Assembly.LoadFrom(file.FullName);
                    foreach (Type type in assembly.GetTypes())
                    {
                        if (type.IsInterface || type.IsAbstract)
                        {
                            continue;
                        }
                        else
                        {
                            if (type.GetInterface(pluginType.FullName) != null)
                            {
                                try
                                {
                                    Logging.Debug("Instantiating monitor plugin at " + file.FullName);
                                    EDDIMonitor monitor = type.InvokeMember(null,
                                                               BindingFlags.CreateInstance,
                                                               null, null, null) as EDDIMonitor;
                                    foundMonitors.Add(monitor);
                                }
                                catch (TargetInvocationException)
                                {
                                    Logging.Warn($"Error loading {file.Name}. Failed to load {type.Name} from {type.Assembly}.");
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
                    StringBuilder sb = new StringBuilder();
                    foreach (Exception exSub in ex.LoaderExceptions)
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
                    Logging.Warn("Failed to instantiate plugin at " + file.FullName + ":\n" + sb.ToString());
                }
                catch (FileLoadException flex)
                {
                    string msg = string.Format(Eddi.Properties.EddiResources.problem_load_monitor_file, dir.FullName);
                    Logging.Error(msg, flex);
                    SpeechService.Instance.Say(null, msg, 0);
                }
                catch (Exception ex)
                {
                    string msg = string.Format(Eddi.Properties.EddiResources.problem_load_monitor, $"{file.Name}.\n{ex.Message} {ex.InnerException?.Message ?? ""}");
                    Logging.Error(msg, ex);
                    SpeechService.Instance.Say(null, msg, 0);
                }
            }
            return foundMonitors;
        }

        /// <summary>
        /// Find all responders
        /// </summary>
        public List<EDDIResponder> findResponders()
        {
            var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            if (string.IsNullOrEmpty(path))
            {
                Logging.Warn("Unable to start EDDI Responders, application directory path not found.");
                return null;
            }
            DirectoryInfo dir = new DirectoryInfo(path);
            List<EDDIResponder> foundResponders = new List<EDDIResponder>();
            Type pluginType = typeof(EDDIResponder);
            foreach (FileInfo file in dir.GetFiles("*Responder.dll", SearchOption.AllDirectories))
            {
                Logging.Debug("Checking potential plugin at " + file.FullName);
                try
                {
                    Assembly assembly = Assembly.LoadFrom(file.FullName);
                    foreach (Type type in assembly.GetTypes())
                    {
                        if (type.IsInterface || type.IsAbstract)
                        {
                            continue;
                        }
                        else
                        {
                            if (type.GetInterface(pluginType.FullName) != null)
                            {
                                Logging.Debug("Instantiating responder plugin at " + file.FullName);
                                EDDIResponder responder = type.InvokeMember(null,
                                                           BindingFlags.CreateInstance,
                                                           null, null, null) as EDDIResponder;
                                foundResponders.Add(responder);
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
                    StringBuilder sb = new StringBuilder();
                    foreach (Exception exSub in ex.LoaderExceptions)
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
                    Logging.Warn("Failed to instantiate plugin at " + file.FullName + ":\n" + sb.ToString());
                }
            }
            return foundResponders;
        }

        private Ship _currentShip;

        /// <summary>
        /// Update the profile when requested, ensuring that we meet the condition in the updated profile
        /// </summary>
        private void conditionallyRefreshStationProfile()
        {

            if (CompanionAppService.Instance.CurrentState == CompanionAppService.State.Authorized)
            {
                try
                {
                    // Make sure we know where we are
                    if (CurrentStarSystem is null || string.IsNullOrEmpty(CurrentStarSystem.systemname))
                    {
                        return;
                    }

                    // We do need to fetch an updated station profile; do so
                    Logging.Debug("Starting conditional station profile fetch");
                    var result =
                        CompanionAppService.Instance.CombinedStationEndpoints.GetCombinedStation(
                            Cmdr?.name, CurrentStarSystem?.systemname, CurrentStation?.name);
                    if (result != null)
                    {
                        var profile = FrontierApiProfile.FromJson(result["profileJson"]?.ToObject<JObject>());
                        var profileStation = FrontierApiStation.FromJson(result["marketJson"]?.ToObject<JObject>(), result["shipyardJson"]?.ToObject<JObject>());

                        // Post an update event\
                        var updates = new HashSet<string>();
                        if (profileStation.eddnCommodityMarketQuotes != null)
                        {
                            updates.Add("market");
                        }
                        if (profileStation.outfitting != null)
                        {
                            updates.Add("outfitting");
                        }
                        if (profileStation.ships != null)
                        {
                            updates.Add("shipyard");
                        }
                        var @event = new MarketInformationUpdatedEvent(
                            profile.timestamp,
                            profile.currentStarSystem,
                            profileStation.name,
                            profileStation.marketId,
                            updates);
                        enqueueEvent(@event);

                        // We have the required station information
                        Logging.Debug("Current station matches profile information; updating info");
                        Station station =
                            CurrentStarSystem?.stations.Find(s => s.name == profileStation.name);
                        station = profileStation.UpdateStation(
                            profileStation.commoditiesupdatedat, station);

                        // Update the current station information in our backend DB
                        Logging.Debug(
                            "Star system information updated from Frontier API server; updating local copy");
                        CurrentStation = station;
                        StarSystemSqLiteRepository.Instance.SaveStarSystem(CurrentStarSystem);
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

        public void updateDestinationSystem(string destinationSystem)
        {
            EDDIConfiguration configuration = ConfigService.Instance.eddiConfiguration;
            if (destinationSystem != null)
            {
                StarSystem system = StarSystemSqLiteRepository.Instance.GetOrFetchStarSystem(destinationSystem);

                //Ignore null & empty systems
                if (system != null)
                {
                    if (system.systemname != DestinationStarSystem?.systemname)
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
            ConfigService.Instance.eddiConfiguration = configuration;
        }

        public void updateHomeSystemStation(EDDIConfiguration configuration)
        {
            updateHomeSystem(configuration);
            updateHomeStation(configuration);
            ConfigService.Instance.eddiConfiguration = configuration;
        }

        public EDDIConfiguration updateHomeSystem(EDDIConfiguration configuration)
        {
            if (!string.IsNullOrEmpty(configuration.HomeSystem))
            {
                StarSystem system = StarSystemSqLiteRepository.Instance.GetOrFetchStarSystem(configuration.HomeSystem);

                //Ignore null & empty systems
                if (system != null && system.bodies?.Count > 0)
                {
                    if (system.systemname != HomeStarSystem?.systemname)
                    {
                        HomeStarSystem = system;
                        Logging.Debug("Home star system is " + HomeStarSystem.systemname);
                        configuration.HomeSystem = system.systemname;
                    }
                }
            }
            else
            {
                HomeStarSystem = null;
            }
            return configuration;
        }

        public EDDIConfiguration updateHomeStation(EDDIConfiguration configuration)
        {
            if (!string.IsNullOrEmpty(configuration.HomeStation) && HomeStarSystem?.stations != null)
            {
                string homeStationName = configuration.HomeStation.Trim();
                foreach (Station station in HomeStarSystem.stations)
                {
                    if (station.name == homeStationName)
                    {
                        HomeStation = station;
                        Logging.Debug("Home station is " + HomeStation.name);
                        configuration.HomeStation = station.name;
                        break;
                    }
                }
            }
            return configuration;
        }

        public EDDIConfiguration updateSquadronSystem(EDDIConfiguration configuration)
        {
            if (!string.IsNullOrEmpty(configuration.SquadronSystem))
            {
                StarSystem system = StarSystemSqLiteRepository.Instance.GetOrFetchStarSystem(configuration.SquadronSystem.Trim());

                //Ignore null & empty systems
                if (system != null && system?.bodies.Count > 0)
                {
                    if (system.systemname != SquadronStarSystem?.systemname)
                    {
                        SquadronStarSystem = system;
                        if (SquadronStarSystem?.factions != null)
                        {
                            Logging.Debug("Squadron star system is " + SquadronStarSystem.systemname);
                            configuration.SquadronSystem = system.systemname;
                        }
                    }
                }
            }
            else
            {
                SquadronStarSystem = null;
            }
            return configuration;
        }

        public void updateSquadronData(Faction faction, string systemName)
        {
            if (faction != null)
            {
                EDDIConfiguration configuration = ConfigService.Instance.eddiConfiguration;

                //Update the squadron faction, if changed
                if (configuration.SquadronFaction == null || configuration.SquadronFaction != faction.name)
                {
                    configuration.SquadronFaction = faction.name;

                    Application.Current?.Dispatcher?.Invoke(() =>
                    {
                        if (Application.Current?.MainWindow != null)
                        {
                            ((MainWindow)Application.Current.MainWindow).squadronFactionDropDown.SelectedItem = faction.name;
                        }
                    });

                    Cmdr.squadronfaction = faction.name;
                }

                // Update system, allegiance, & power when in squadron home system
                if ((faction.presences.FirstOrDefault(p => p.systemName == systemName)?.squadronhomesystem ?? false))
                {
                    // Update the squadron system data, if changed
                    string system = CurrentStarSystem.systemname;
                    if (configuration.SquadronSystem == null || configuration.SquadronSystem != system)
                    {
                        configuration.SquadronSystem = system;

                        var configurationCopy = configuration;
                        Application.Current?.Dispatcher?.Invoke(() =>
                        {
                            if (Application.Current?.MainWindow != null)
                            {
                                ((MainWindow)Application.Current.MainWindow).squadronSystemDropDown.Text = system;
                                ((MainWindow)Application.Current.MainWindow).ConfigureSquadronFactionOptions(configurationCopy);
                            }
                        });

                        configuration = updateSquadronSystem(configuration);
                    }

                    //Update the squadron allegiance, if changed
                    Superpower allegiance = CurrentStarSystem?.Faction?.Allegiance ?? Superpower.None;

                    //Prioritize UI entry if squadron system allegiance not specified
                    if (allegiance != Superpower.None)
                    {
                        if (configuration.SquadronAllegiance == Superpower.None || configuration.SquadronAllegiance != allegiance)
                        {
                            configuration.SquadronAllegiance = allegiance;
                            Cmdr.squadronallegiance = allegiance;
                        }
                    }

                    // Update the squadron power, if changed
                    Power power = Power.FromName(CurrentStarSystem?.power) ?? Power.None;

                    //Prioritize UI entry if squadron system power not specified
                    if (power != Power.None)
                    {
                        if (configuration.SquadronPower == Power.None && configuration.SquadronPower != power)
                        {
                            configuration.SquadronPower = power;

                            Application.Current?.Dispatcher?.Invoke(() =>
                            {
                                if (Application.Current?.MainWindow != null)
                                {
                                    ((MainWindow)Application.Current.MainWindow).squadronPowerDropDown.SelectedItem = power.localizedName;
                                    ((MainWindow)Application.Current.MainWindow).ConfigureSquadronPowerOptions(configuration);
                                }
                            });
                        }
                    }
                }
                ConfigService.Instance.eddiConfiguration = configuration;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName]string propertyName = null) 
        { 
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
