using EddiCompanionAppService;
using EddiDataDefinitions;
using EddiDataProviderService;
using EddiEvents;
using EddiSpeechService;
using EddiStarMapService;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Utilities;

namespace Eddi
{
    /// <summary>
    /// Eddi is the controller for all EDDI operations.  Its job is to retain the state of the objects such as the commander, the current system, etc.
    /// and keep them up-to-date with changes that occur.  It also passes on messages to responders to handle as required.
    /// </summary>
    public class EDDI
    {
        private static EDDI instance;

        // True if we have been started by VoiceAttack
        public static bool FromVA = false;

        // True if the Speech Responder tab is waiting on a modal dialog window. Accessed by VoiceAttack plugin.
        public bool SpeechResponderModalWait { get; set; } = false;

        private static bool started;
        internal static bool running = true;

        private static bool allowMarketUpdate = false;
        private static bool allowOutfittingUpdate = false;
        private static bool allowShipyardUpdate = false;

        public bool inCQC { get; private set; } = false;
        public bool inCrew { get; private set; } = false;
        public bool inHorizons { get; private set; } = true;

        private bool _inBeta = false;
        public bool inBeta
        {
            get => _inBeta;
            private set
            {
                _inBeta = value;
                CompanionAppService.Instance.inBeta = value;
            }
        }

        static EDDI()
        {
            // Set up our app directory
            Directory.CreateDirectory(Constants.DATA_DIR);
        }

        private static readonly object instanceLock = new object();
        public static EDDI Instance
        {
            get
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
                return instance;
            }
        }

        // Upgrade information
        public bool UpgradeAvailable = false;
        public bool UpgradeRequired = false;
        public string UpgradeVersion;
        public string UpgradeLocation;
        public string Motd;
        public List<string> ProductionBuilds = new List<string>() { "r131487/r0" };

        public List<EDDIMonitor> monitors = new List<EDDIMonitor>();
        private ConcurrentBag<EDDIMonitor> activeMonitors = new ConcurrentBag<EDDIMonitor>();
        private static readonly object monitorLock = new object();

        public List<EDDIResponder> responders = new List<EDDIResponder>();
        private ConcurrentBag<EDDIResponder> activeResponders = new ConcurrentBag<EDDIResponder>();
        private static readonly object responderLock = new object();

        public string vaVersion { get; set; }

        // Information obtained from the companion app service
        public DateTime ApiTimeStamp { get; private set; }

        // Information obtained from the configuration
        public StarSystem HomeStarSystem { get; private set; } = new StarSystem();
        public Station HomeStation { get; private set; }
        public StarSystem SquadronStarSystem { get; private set; } = new StarSystem();

        // Destination variables
        public StarSystem DestinationStarSystem { get; private set; }
        public Station DestinationStation { get; private set; }
        public decimal DestinationDistanceLy { get; set; }

        // Information obtained from the player journal
        public Commander Cmdr { get; private set; } // Also includes information from the configuration and companion app service
        public string Environment { get; set; }
        public StarSystem CurrentStarSystem { get; private set; }
        public StarSystem LastStarSystem { get; private set; }
        public StarSystem NextStarSystem { get; private set; }
        public Station CurrentStation { get; private set; }
        public Body CurrentStellarBody { get; private set; }
        public DateTime JournalTimeStamp { get; set; } = DateTime.MinValue;

        // Information from the last jump we initiated (for reference)
        public FSDEngagedEvent LastFSDEngagedEvent { get; private set; }

        // Current vehicle of player
        public string Vehicle { get; set; } = Constants.VEHICLE_SHIP;
        public Ship CurrentShip { get; set; }

        // Our main window, made accessible via the applicable EDDI Instance
        public MainWindow MainWindow { get; internal set; }

        public ObservableConcurrentDictionary<string, object> State = new ObservableConcurrentDictionary<string, object>();

        // The event queue
        public ConcurrentQueue<Event> eventQueue { get; private set; } = new ConcurrentQueue<Event>();

        private EDDI()
        {
            try
            {
                Logging.Info(Constants.EDDI_NAME + " " + Constants.EDDI_VERSION + " starting");

                // Start by fetching information from the update server, and handling appropriately
                CheckUpgrade();
                if (UpgradeRequired)
                {
                    // We are too old to continue; initialize in a "safe mode". 
                    running = false;
                }

                // Ensure that our primary data structures have something in them.  This allows them to be updated from any source
                Cmdr = new Commander();
                // Set up the Elite configuration
                EliteConfiguration eliteConfiguration = EliteConfiguration.FromFile();
                inBeta = eliteConfiguration.Beta;
                Logging.Info(inBeta ? "On beta" : "On live");
                inHorizons = eliteConfiguration.Horizons;

                // Retrieve commander preferences
                EDDIConfiguration configuration = EDDIConfiguration.FromFile();

                List<Task> essentialAsyncTasks = new List<Task>();
                if (running)
                {
                    // Tasks we can start asynchronously but need to complete before other dependent code is called
                    essentialAsyncTasks.AddRange(new List<Task>()
                    {
                        Task.Run(() => responders = findResponders()), // Set up responders
                        Task.Run(() => monitors = findMonitors()), // Set up monitors 
                        Task.Run(() => updateHomeSystemStation(configuration)), // Set up home system details
                        Task.Run(() => // Set up squadron details
                        {
                            updateSquadronSystem(configuration);
                            Cmdr.squadronname = configuration.SquadronName;
                            Cmdr.squadronid = configuration.SquadronID;
                            Cmdr.squadronrank = configuration.SquadronRank;
                            Cmdr.squadronallegiance = configuration.SquadronAllegiance;
                            Cmdr.squadronpower = configuration.SquadronPower;
                            Cmdr.squadronfaction = configuration.SquadronFaction;
                        }),
                    });
                }
                else
                {
                    Logging.Info("Mandatory upgrade required! EDDI initializing in safe mode until upgrade is completed.");
                }

                // Tasks we can start asynchronously and don't need to wait for
                Cmdr.gender = configuration.Gender;
                Task.Run(() => updateDestinationSystemStation(configuration));
                Task.Run(() =>
                {
                    // Set up the Frontier API service
                    if (CompanionAppService.Instance.CurrentState == CompanionAppService.State.Authorized)
                    {
                        // Carry out initial population of profile
                        try
                        {
                            refreshProfile();
                        }
                        catch (Exception ex)
                        {
                            Logging.Debug("Failed to obtain Frontier API profile: " + ex);
                        }
                    }
                    if (CompanionAppService.Instance.CurrentState == CompanionAppService.State.Authorized)
                    {
                        Logging.Info("EDDI access to the Frontier API is enabled.");
                        // Pass our commander's Frontier API name to the StarMapService (if it has been set) 
                        // (the Frontier API name may differ from the EDSM name)
                        if (Cmdr?.name != null)
                        {
                            StarMapService.commanderFrontierApiName = Cmdr.name;
                        }
                    }
                    else
                    {
                        Logging.Info("EDDI access to the Frontier API is not enabled.");
                    }
                });

                // Make sure that our essential tasks have completed before we start
                Task.WaitAll(essentialAsyncTasks.ToArray());

                // We always start in normal space
                Environment = Constants.ENVIRONMENT_NORMAL_SPACE;

                Logging.Info(Constants.EDDI_NAME + " " + Constants.EDDI_VERSION + " initialised");
            }
            catch (Exception ex)
            {
                Logging.Error("Failed to initialise", ex);
            }
        }

        public bool ShouldUseTestEndpoints()
        {
#if DEBUG
            return true;
#else
            // use test endpoints if the game is in beta or EDDI is not release candidate or final
            return EDDI.Instance.inBeta || (Constants.EDDI_VERSION.phase < Utilities.Version.TestPhase.rc);
#endif
        }

        /// <summary>
        /// Check to see if an upgrade is available and populate relevant variables
        /// </summary>
        public void CheckUpgrade()
        {
            // Clear the old values
            UpgradeRequired = false;
            UpgradeAvailable = false;
            UpgradeLocation = null;
            UpgradeVersion = null;
            Motd = null;

            try
            {
                ServerInfo updateServerInfo = ServerInfo.FromServer(Constants.EDDI_SERVER_URL);
                if (updateServerInfo == null)
                {
                    throw new Exception("Failed to contact update server");
                }
                else
                {
                    EDDIConfiguration configuration = EDDIConfiguration.FromFile();
                    InstanceInfo info = configuration.Beta ? updateServerInfo.beta : updateServerInfo.production;
                    string spokenVersion = info.version.Replace(".", $" {Properties.EddiResources.point} ");
                    Motd = info.motd;
                    if (updateServerInfo.productionbuilds != null)
                    {
                        ProductionBuilds = updateServerInfo.productionbuilds;
                    }
                    Utilities.Version minVersion = new Utilities.Version(info.minversion);
                    if (minVersion > Constants.EDDI_VERSION)
                    {
                        // There is a mandatory update available
                        if (!FromVA)
                        {
                            string message = String.Format(Properties.EddiResources.mandatory_upgrade, spokenVersion);
                            SpeechService.Instance.Say(null, message, 0);
                        }
                        UpgradeRequired = true;
                        UpgradeLocation = info.url;
                        UpgradeVersion = info.version;
                        return;
                    }

                    Utilities.Version latestVersion = new Utilities.Version(info.version);
                    if (latestVersion > Constants.EDDI_VERSION)
                    {
                        // There is an update available
                        if (!FromVA)
                        {
                            string message = String.Format(Properties.EddiResources.update_available, spokenVersion);
                            SpeechService.Instance.Say(null, message, 0);
                        }
                        UpgradeAvailable = true;
                        UpgradeLocation = info.url;
                        UpgradeVersion = info.version;
                    }
                }
            }
            catch (Exception ex)
            {
                SpeechService.Instance.Say(null, Properties.EddiResources.update_server_unreachable, 0);
                Logging.Warn("Failed to access " + Constants.EDDI_SERVER_URL, ex);
            }
        }

        [SecurityPermissionAttribute(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        [SecurityPermissionAttribute(SecurityAction.Demand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        public void Upgrade()
        {
            try
            {
                if (UpgradeLocation != null)
                {
                    Logging.Info("Downloading upgrade from " + UpgradeLocation);
                    SpeechService.Instance.Say(null, Properties.EddiResources.downloading_upgrade, 0);
                    string updateFile = Net.DownloadFile(UpgradeLocation, @"EDDI-update.exe");
                    if (updateFile == null)
                    {
                        SpeechService.Instance.Say(null, Properties.EddiResources.download_failed, 0);
                    }
                    else
                    {
                        // Inno setup will attempt to restart this application so register it
                        NativeMethods.RegisterApplicationRestart(null, RestartFlags.NONE);

                        Logging.Info("Downloaded update to " + updateFile);
                        Logging.Info("Path is " + Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
                        File.SetAttributes(updateFile, FileAttributes.Normal);
                        SpeechService.Instance.Say(null, Properties.EddiResources.starting_upgrade, 0);
                        Logging.Info("Starting upgrade.");

                        Process.Start(updateFile, @"/closeapplications /restartapplications /silent /log /nocancel /noicon /dir=""" + Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"""");
                    }
                }
            }
            catch (Exception ex)
            {
                SpeechService.Instance.Say(null, Properties.EddiResources.upgrade_failed, 0);
                Logging.Error("Upgrade failed", ex);
            }
        }

        public void Start()
        {
            if (!started)
            {
                EDDIConfiguration configuration = EDDIConfiguration.FromFile();

                foreach (EDDIMonitor monitor in monitors)
                {
                    if (!configuration.Plugins.TryGetValue(monitor.MonitorName(), out bool enabled))
                    {
                        // No information; default to enabled
                        enabled = true;
                    }

                    if (!enabled)
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
                foreach (EDDIResponder responder in responders)
                {
                    DisableResponder(responder.ResponderName());
                }
                foreach (EDDIMonitor monitor in monitors)
                {
                    DisableMonitor(monitor.MonitorName());
                }
            }

            Logging.Info(Constants.EDDI_NAME + " " + Constants.EDDI_VERSION + " stopped");

            started = false;
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
            eventQueue.Enqueue(@event);

            try
            {
                Thread eventHandler = new Thread(() => dequeueEvent())
                {
                    Name = "EventHandler",
                    IsBackground = true
                };
                eventHandler.Start();
                eventHandler.Join();
            }
            catch (ThreadAbortException tax)
            {
                Thread.ResetAbort();
                Logging.Debug("Thread aborted", tax);
            }
            catch (Exception ex)
            {
                Dictionary<string, object> data = new Dictionary<string, object>
                {
                    { "event", JsonConvert.SerializeObject(@event) },
                    { "exception", ex.Message },
                    { "stacktrace", ex.StackTrace }
                };
                Logging.Error("Failed to enqueue event", data);
            }
        }

        private void dequeueEvent()
        {
            if (eventQueue.TryDequeue(out Event @event))
            {
                eventHandler(@event);
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
                    if (@event is FileHeaderEvent)
                    {
                        passEvent = eventFileHeader((FileHeaderEvent)@event);
                    }
                    else if (@event is LocationEvent)
                    {
                        passEvent = eventLocation((LocationEvent)@event);
                    }
                    else if (@event is DockedEvent)
                    {
                        passEvent = eventDocked((DockedEvent)@event);
                    }
                    else if (@event is UndockedEvent)
                    {
                        passEvent = eventUndocked((UndockedEvent)@event);
                    }
                    else if (@event is TouchdownEvent)
                    {
                        passEvent = eventTouchdown((TouchdownEvent)@event);
                    }
                    else if (@event is LiftoffEvent)
                    {
                        passEvent = eventLiftoff((LiftoffEvent)@event);
                    }
                    else if (@event is FSDEngagedEvent)
                    {
                        passEvent = eventFSDEngaged((FSDEngagedEvent)@event);
                    }
                    else if (@event is FSDTargetEvent)
                    {
                        passEvent = eventFSDTarget((FSDTargetEvent)@event);
                    }
                    else if (@event is JumpedEvent)
                    {
                        passEvent = eventJumped((JumpedEvent)@event);
                    }
                    else if (@event is EnteredSupercruiseEvent)
                    {
                        passEvent = eventEnteredSupercruise((EnteredSupercruiseEvent)@event);
                    }
                    else if (@event is EnteredNormalSpaceEvent)
                    {
                        passEvent = eventEnteredNormalSpace((EnteredNormalSpaceEvent)@event);
                    }
                    else if (@event is CommanderContinuedEvent)
                    {
                        passEvent = eventCommanderContinued((CommanderContinuedEvent)@event);
                    }
                    else if (@event is CommanderRatingsEvent)
                    {
                        passEvent = eventCommanderRatings((CommanderRatingsEvent)@event);
                    }
                    else if (@event is CombatPromotionEvent)
                    {
                        passEvent = eventCombatPromotion((CombatPromotionEvent)@event);
                    }
                    else if (@event is TradePromotionEvent)
                    {
                        passEvent = eventTradePromotion((TradePromotionEvent)@event);
                    }
                    else if (@event is ExplorationPromotionEvent)
                    {
                        passEvent = eventExplorationPromotion((ExplorationPromotionEvent)@event);
                    }
                    else if (@event is FederationPromotionEvent)
                    {
                        passEvent = eventFederationPromotion((FederationPromotionEvent)@event);
                    }
                    else if (@event is EmpirePromotionEvent)
                    {
                        passEvent = eventEmpirePromotion((EmpirePromotionEvent)@event);
                    }
                    else if (@event is CrewJoinedEvent)
                    {
                        passEvent = eventCrewJoined((CrewJoinedEvent)@event);
                    }
                    else if (@event is CrewLeftEvent)
                    {
                        passEvent = eventCrewLeft((CrewLeftEvent)@event);
                    }
                    else if (@event is EnteredCQCEvent)
                    {
                        passEvent = eventEnteredCQC((EnteredCQCEvent)@event);
                    }
                    else if (@event is SRVLaunchedEvent)
                    {
                        passEvent = eventSRVLaunched((SRVLaunchedEvent)@event);
                    }
                    else if (@event is SRVDockedEvent)
                    {
                        passEvent = eventSRVDocked((SRVDockedEvent)@event);
                    }
                    else if (@event is FighterLaunchedEvent)
                    {
                        passEvent = eventFighterLaunched((FighterLaunchedEvent)@event);
                    }
                    else if (@event is FighterDockedEvent)
                    {
                        passEvent = eventFighterDocked((FighterDockedEvent)@event);
                    }
                    else if (@event is StarScannedEvent)
                    {
                        passEvent = eventStarScanned((StarScannedEvent)@event);
                    }
                    else if (@event is BodyScannedEvent)
                    {
                        passEvent = eventBodyScanned((BodyScannedEvent)@event);
                    }
                    else if (@event is BodyMappedEvent)
                    {
                        passEvent = eventBodyMapped((BodyMappedEvent)@event);
                    }
                    else if (@event is RingMappedEvent)
                    {
                        passEvent = eventRingMapped((RingMappedEvent)@event);
                    }
                    else if (@event is VehicleDestroyedEvent)
                    {
                        passEvent = eventVehicleDestroyed((VehicleDestroyedEvent)@event);
                    }
                    else if (@event is NearSurfaceEvent)
                    {
                        passEvent = eventNearSurface((NearSurfaceEvent)@event);
                    }
                    else if (@event is SquadronStatusEvent)
                    {
                        passEvent = eventSquadronStatus((SquadronStatusEvent)@event);
                    }
                    else if (@event is SquadronRankEvent)
                    {
                        passEvent = eventSquadronRank((SquadronRankEvent)@event);
                    }
                    else if (@event is FriendsEvent)
                    {
                        passEvent = eventFriends((FriendsEvent)@event);
                    }
                    else if (@event is MarketEvent)
                    {
                        passEvent = eventMarket((MarketEvent)@event);
                    }
                    else if (@event is OutfittingEvent)
                    {
                        passEvent = eventOutfitting((OutfittingEvent)@event);
                    }
                    else if (@event is ShipyardEvent)
                    {
                        passEvent = eventShipyard((ShipyardEvent)@event);
                    }
                    else if (@event is SettlementApproachedEvent)
                    {
                        passEvent = eventSettlementApproached((SettlementApproachedEvent)@event);
                    }
                    else if (@event is DiscoveryScanEvent)
                    {
                        passEvent = eventDiscoveryScan((DiscoveryScanEvent)@event);
                    }
                    else if (@event is SystemScanComplete)
                    {
                        passEvent = eventSystemScanComplete((SystemScanComplete)@event);
                    }

                    // Additional processing is over, send to the event responders if required
                    if (passEvent)
                    {
                        OnEvent(@event);
                    }
                }
                catch (Exception ex)
                {
                    Dictionary<string, object> data = new Dictionary<string, object>
                    {
                        { "event", JsonConvert.SerializeObject(@event) },
                        { "exception", ex.Message },
                        { "stacktrace", ex.StackTrace }
                    };

                    Logging.Error("EDDI core failed to handle event " + @event.type, data);

                    // Even if an error occurs, we still need to pass the raw data 
                    // to the EDDN responder to maintain it's integrity.
                    Instance.ObtainResponder("EDDN responder").Handle(@event);
                }
            }
        }

        private bool eventSystemScanComplete(SystemScanComplete @event)
        {
            // There is a bug in the player journal output (as of player journal v.25) that can cause the `SystemScanComplete` event to fire multiple times 
            // in rapid succession when performing a system scan of a star system with only stars and no other bodies.
            if (CurrentStarSystem != null && (bool)CurrentStarSystem?.systemScanCompleted)
            {
                // We will suppress repetitions of the event within the same star system.
                return false;
            }
            CurrentStarSystem.systemScanCompleted = true;
            return true;
        }

        private bool eventDiscoveryScan(DiscoveryScanEvent @event)
        {
            CurrentStarSystem.discoverableBodies = @event.bodies;
            return true;
        }

        private bool eventSettlementApproached(SettlementApproachedEvent @event)
        {
            bool passEvent = true;
            Station station = CurrentStarSystem?.stations?.Find(s => s.name == @event.name);
            if (station == null && @event.systemAddress == CurrentStarSystem?.systemAddress)
            {
                // This settlement is unknown to us, might not be in our data source or we might not have connectivity.  Use a placeholder
                station = new Station
                {
                    name = @event.name,
                    marketId = @event.marketId,
                    systemname = CurrentStarSystem.systemname
                };
                CurrentStarSystem.stations.Add(station);
            }
            return passEvent;
        }

        private bool eventFriends(FriendsEvent @event)
        {
            bool passEvent = false;
            Friend cmdr = new Friend
            {
                name = @event.name,
                status = @event.status
            };

            /// Does this friend exist in our friends list?
            int index = Cmdr.friends.FindIndex(friend => friend.name == @event.name);
            if (index >= 0)
            {
                if (Cmdr.friends[index].status != @event.status)
                {
                    /// This is a known friend with a revised status: replace in situ (this is more efficient than removing and re-adding).
                    Cmdr.friends[index] = cmdr;
                    passEvent = true;
                }
            }
            else
            {
                /// This is a new friend, add them to the list
                Cmdr.friends.Add(cmdr);
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
                    Dictionary<string, object> data = new Dictionary<string, object>
                    {
                        { "event", JsonConvert.SerializeObject(@event) },
                        { "exception", ex.Message },
                        { "stacktrace", ex.StackTrace }
                    };
                    Logging.Error(monitor.MonitorName() + " failed to handle event " + @event.type, data);
                }
            }
        }

        private async Task passToRespondersAsync(Event @event)
        {
            List<Task> responderTasks = new List<Task>();
            foreach (EDDIResponder responder in activeResponders)
            {
                try
                {
                    var responderTask = Task.Run(() =>
                    {
                        try
                        {
                            responder.Handle(@event);
                        }
                        catch (Exception ex)
                        {
                            Logging.Error(responder.ResponderName() + " failed to handle event " + JsonConvert.SerializeObject(@event), ex);
                        }
                    });
                    responderTasks.Add(responderTask);
                }
                catch (Exception ex)
                {
                    Dictionary<string, object> data = new Dictionary<string, object>
                    {
                        { "event", JsonConvert.SerializeObject(@event) },
                        { "exception", ex.Message },
                        { "stacktrace", ex.StackTrace }
                    };
                    Logging.Error(responder.ResponderName() + " failed to handle event " + @event.type, data);
                }
            }
            await Task.WhenAll(responderTasks.ToArray());
        }

        private async Task passToMonitorPostHandlersAsync(Event @event)
        {
            List<Task> monitorTasks = new List<Task>();
            foreach (EDDIMonitor monitor in activeMonitors)
            {
                try
                {
                    var monitorTask = Task.Run(() =>
                    {
                        try
                        {
                            monitor.PostHandle(@event);
                        }
                        catch (Exception ex)
                        {
                            Logging.Error(monitor.MonitorName() + " failed to post-handle event " + JsonConvert.SerializeObject(@event), ex);
                        }
                    });
                    monitorTasks.Add(monitorTask);
                }
                catch (ThreadAbortException tax)
                {
                    Thread.ResetAbort();
                    Logging.Debug("Thread aborted", tax);
                }
                catch (Exception ex)
                {
                    Dictionary<string, object> data = new Dictionary<string, object>
                    {
                        { "event", JsonConvert.SerializeObject(@event) },
                        { "exception", ex.Message },
                        { "stacktrace", ex.StackTrace }
                    };
                    Logging.Error(monitor.MonitorName() + " failed to post-handle event " + @event.type, data);
                }
                await Task.WhenAll(monitorTasks.ToArray());
            }
        }

        private bool eventLocation(LocationEvent theEvent)
        {
            Logging.Info("Location StarSystem: " + theEvent.systemname);

            updateCurrentSystem(theEvent.systemname);
            // Our data source may not include the system address
            CurrentStarSystem.systemAddress = theEvent.systemAddress;
            // Always update the current system with the current co-ordinates, just in case things have changed
            CurrentStarSystem.x = theEvent.x;
            CurrentStarSystem.y = theEvent.y;
            CurrentStarSystem.z = theEvent.z;
            setSystemDistanceFromHome(CurrentStarSystem);

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
                Faction squadronFaction = theEvent.factions.FirstOrDefault(f => (bool)f.presences.
                    FirstOrDefault(p => p.systemName == CurrentStarSystem.systemname)?.squadronhomesystem || f.squadronfaction);
                if (squadronFaction != null)
                {
                    updateSquadronData(squadronFaction, CurrentStarSystem.systemname);
                }
            }

            // (When pledged) Powerplay information
            CurrentStarSystem.Power = theEvent.Power;
            CurrentStarSystem.powerState = theEvent.powerState;

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
                    // We are docked and in the ship
                    Environment = Constants.ENVIRONMENT_DOCKED;
                    Vehicle = Constants.VEHICLE_SHIP;

                    // Our data source may not include the market id or system address
                    station.marketId = theEvent.marketId;
                    station.systemAddress = theEvent.systemAddress;
                    station.Faction = theEvent.controllingstationfaction;

                    // Kick off the profile refresh if the companion API is available
                    if (CompanionAppService.Instance.CurrentState == CompanionAppService.State.Authorized)
                    {
                        // Refresh station data
                        if (theEvent.fromLoad) { return true; } // Don't fire this event when loading pre-existing logs
                        profileUpdateNeeded = true;
                        profileStationRequired = CurrentStation.name;
                        Thread updateThread = new Thread(() => conditionallyRefreshProfile())
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
                Environment = Constants.ENVIRONMENT_NORMAL_SPACE;

                // If we are not at a station then our station information is invalid 
                CurrentStation = null;

                // Update the body 
                Logging.Debug("Now at body " + theEvent.bodyname);
                updateCurrentStellarBody(theEvent.bodyname, theEvent.systemname, theEvent.systemAddress);
            }
            else
            {
                // We are near neither a stellar body nor a station. 
                CurrentStellarBody = null;
                CurrentStation = null;
            }

            return true;
        }

        private bool eventDocked(DockedEvent theEvent)
        {
            updateCurrentSystem(theEvent.system);

            // Upon docking, allow manual station updates once
            allowMarketUpdate = true;
            allowOutfittingUpdate = true;
            allowShipyardUpdate = true;

            if (Environment == Constants.ENVIRONMENT_DOCKED)
            {
                // We are already at this station; nothing to do
                Logging.Debug("Already at station " + theEvent.station);
                return false;
            }

            // We are docked and in the ship
            Environment = Constants.ENVIRONMENT_DOCKED;
            Vehicle = Constants.VEHICLE_SHIP;

            // Update the station
            Logging.Debug("Now at station " + theEvent.station);
            Station station = CurrentStarSystem.stations.Find(s => s.name == theEvent.station);
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

            // Not all stations in our database will have a system address or market id, so we set them here
            station.systemAddress = theEvent.systemAddress;
            station.marketId = theEvent.marketId;

            // Information from the event might be more current than our data source so use it in preference
            station.Faction = theEvent.controllingfaction;
            station.stationServices = theEvent.stationServices;
            station.economyShares = theEvent.economyShares;

            CurrentStation = station;
            CurrentStellarBody = null;

            // Kick off the profile refresh if the companion API is available
            if (CompanionAppService.Instance.CurrentState == CompanionAppService.State.Authorized)
            {
                // Refresh station data
                if (theEvent.fromLoad) { return true; } // Don't fire this event when loading pre-existing logs
                profileUpdateNeeded = true;
                profileStationRequired = CurrentStation.name;
                Thread updateThread = new Thread(() => conditionallyRefreshProfile())
                {
                    IsBackground = true
                };
                updateThread.Start();
            }

            return true;
        }

        private bool eventUndocked(UndockedEvent theEvent)
        {
            Environment = Constants.ENVIRONMENT_NORMAL_SPACE;
            Vehicle = Constants.VEHICLE_SHIP;

            // Call refreshProfile() to ensure that our ship is up-to-date
            refreshProfile();

            return true;
        }

        private bool eventTouchdown(TouchdownEvent theEvent)
        {
            Environment = Constants.ENVIRONMENT_LANDED;
            if (theEvent.playercontrolled)
            {
                Vehicle = Constants.VEHICLE_SHIP;
            }
            else
            {
                Vehicle = Constants.VEHICLE_SRV;
            }
            return true;
        }

        private bool eventLiftoff(LiftoffEvent theEvent)
        {
            Environment = Constants.ENVIRONMENT_NORMAL_SPACE;
            if (theEvent.playercontrolled)
            {
                Vehicle = Constants.VEHICLE_SHIP;
            }
            else
            {
                Vehicle = Constants.VEHICLE_SRV;
            }
            return true;
        }

        private bool eventMarket(MarketEvent theEvent)
        {
            if (allowMarketUpdate)
            {
                MarketInfoReader info = MarketInfoReader.FromFile();
                if (info != null && info.MarketID == theEvent.marketId 
                    && info.StarSystem == theEvent.system 
                    && info.StationName == theEvent.station)
                {
                    List<CommodityMarketQuote> quotes = new List<CommodityMarketQuote>();
                    foreach (MarketInfo item in info.Items)
                    {
                        CommodityMarketQuote quote = CommodityMarketQuote.FromMarketInfo(item);
                        if (quote != null)
                        {
                            quotes.Add(quote);
                        }
                    }

                    if (quotes != null && info.Items.Count == quotes.Count)
                    {
                        if (CurrentStation?.marketId == theEvent.marketId)
                        {
                            // Update the current station commodities
                            allowMarketUpdate = false;
                            CurrentStation.commodities = quotes;
                            CurrentStation.commoditiesupdatedat = (long)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;

                            // Update the current station information in our backend DB
                            Logging.Debug("Star system information updated from remote server; updating local copy");
                            StarSystemSqLiteRepository.Instance.SaveStarSystem(CurrentStarSystem);
                        }

                        // Post an update event for new market data
                        if (theEvent.fromLoad) { return true; } // Don't fire this event when loading pre-existing logs
                        Event @event = new MarketInformationUpdatedEvent(info.timeStamp, inHorizons, theEvent.system, theEvent.station, theEvent.marketId, quotes, null, null, null);
                        enqueueEvent(@event);
                    }
                    return true;
                }
            }
            return false;
        }

        private bool eventOutfitting(OutfittingEvent theEvent)
        {
            if (allowOutfittingUpdate)
            {
                OutfittingInfoReader info = OutfittingInfoReader.FromFile();
                if (info.Items != null && info.MarketID == theEvent.marketId 
                    && info.StarSystem == theEvent.system 
                    && info.StationName == theEvent.station 
                    && info.Horizons == Instance.inHorizons)
                {
                    List<EddiDataDefinitions.Module> modules = new List<EddiDataDefinitions.Module>();
                    foreach (OutfittingInfo item in info.Items)
                    {
                        EddiDataDefinitions.Module module = EddiDataDefinitions.Module.FromOutfittingInfo(item);
                        if (module != null)
                        {
                            modules.Add(module);
                        }
                    }

                    if (modules != null && info.Items.Count == modules.Count)
                    {
                        if (CurrentStation?.marketId == theEvent.marketId)
                        {
                            // Update the current station outfitting
                            allowOutfittingUpdate = false;
                            CurrentStation.outfitting = modules;
                            CurrentStation.outfittingupdatedat = (long)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;

                            // Update the current station information in our backend DB
                            Logging.Debug("Star system information updated from remote server; updating local copy");
                            StarSystemSqLiteRepository.Instance.SaveStarSystem(CurrentStarSystem);
                        }

                        // Post an update event for new outfitting data
                        if (theEvent.fromLoad) { return true; } // Don't fire this event when loading pre-existing logs
                        Event @event = new MarketInformationUpdatedEvent(info.timeStamp, inHorizons, theEvent.system, theEvent.station, theEvent.marketId, null, null, modules, null);
                        enqueueEvent(@event);
                    }
                    return true;
                }
            }
            return false;
        }

        private bool eventShipyard(ShipyardEvent theEvent)
        {
            if (allowShipyardUpdate)
            {
                ShipyardInfoReader info = ShipyardInfoReader.FromFile();
                if (info.PriceList != null && info.MarketID == theEvent.marketId 
                    && info.StarSystem == theEvent.system 
                    && info.StationName == theEvent.station 
                    && info.Horizons == Instance.inHorizons)
                {
                    List<Ship> ships = new List<Ship>();
                    foreach (ShipyardInfo item in info.PriceList)
                    {
                        Ship ship = Ship.FromShipyardInfo(item);
                        if (ship != null)
                        {
                            ships.Add(ship);
                        }
                    }

                    if (ships != null && info.PriceList.Count == ships.Count)
                    {
                        if (CurrentStation?.marketId == theEvent.marketId)
                        {
                            // Update the current station shipyard
                            allowShipyardUpdate = false;
                            CurrentStation.shipyard = ships;
                            CurrentStation.shipyardupdatedat = (long)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;

                            // Update the current station information in our backend DB
                            Logging.Debug("Star system information updated from remote server; updating local copy");
                            StarSystemSqLiteRepository.Instance.SaveStarSystem(CurrentStarSystem);
                        }

                        // Post an update event for new shipyard data
                        if (theEvent.fromLoad) { return true; } // Don't fire this event when loading pre-existing logs
                        Event @event = new MarketInformationUpdatedEvent(info.timeStamp, inHorizons, theEvent.system, theEvent.station, theEvent.marketId, null, null, null, ships);
                        enqueueEvent(@event);
                    }
                    return true;
                }
            }
            return false;
        }

        private void updateCurrentSystem(string name)
        {
            if (name == null)
            {
                return;
            }
            if (CurrentStarSystem == null || CurrentStarSystem.systemname != name)
            {
                if (CurrentStarSystem != null && CurrentStarSystem.systemname != name)
                {
                    // We have changed system so update the old one as to when we left
                    StarSystemSqLiteRepository.Instance.LeaveStarSystem(CurrentStarSystem);
                }
                LastStarSystem = CurrentStarSystem;
                if (NextStarSystem?.systemname == name)
                {
                    CurrentStarSystem = NextStarSystem;
                    NextStarSystem = null;
                }
                else
                {
                    CurrentStarSystem = StarSystemSqLiteRepository.Instance.GetOrCreateStarSystem(name);
                }
                setSystemDistanceFromHome(CurrentStarSystem);
                setSystemDistanceFromDestination(CurrentStarSystem?.systemname);
            }
        }

        private void updateCurrentStellarBody(string bodyName, string systemName, long? systemAddress = null)
        {
            // Make sure our system information is up to date
            if (CurrentStarSystem == null || CurrentStarSystem.systemname != systemName)
            {
                updateCurrentSystem(systemName);
            }
            // Update the body 
            if (CurrentStarSystem != null)
            {
                Body body = CurrentStarSystem.bodies?.Find(s => s.bodyname == bodyName);
                if (body == null)
                {
                    // We may be near a ring. For rings, we want to select the parent body
                    List<Body> ringedBodies = CurrentStarSystem.bodies?
                        .Where(b => b?.rings?.Count > 0).ToList();
                    foreach (Body ringedBody in ringedBodies)
                    {
                        Ring ring = ringedBody.rings.FirstOrDefault(r => r.name == bodyName);
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

            // We are in the ship
            Vehicle = Constants.VEHICLE_SHIP;

            // Remove information about the current station and stellar body 
            CurrentStation = null;
            CurrentStellarBody = null;

            // Set the destination system as the current star system
            updateCurrentSystem(@event.system);

            // Save a copy of this event for later reference
            LastFSDEngagedEvent = @event;

            return true;
        }

        private bool eventFSDTarget(FSDTargetEvent @event)
        {
            // Set and prepare data about the next star system
            NextStarSystem = StarSystemSqLiteRepository.Instance.GetOrFetchStarSystem(@event.system);
            return true;
        }

        private bool eventFileHeader(FileHeaderEvent @event)
        {
            // Test whether we're in beta by checking the filename, version described by the header, 
            // and certain version / build combinations
            inBeta =
                (
                    @event.filename.Contains("Beta") ||
                    @event.version.Contains("Beta") ||
                    (
                        @event.version.Contains("2.2") &&
                        (
                            @event.build.Contains("r121645/r0") ||
                            @event.build.Contains("r129516/r0")
                        )
                    )
                );
            Logging.Info(inBeta ? "On beta" : "On live");
            EliteConfiguration config = EliteConfiguration.FromFile();
            config.Beta = inBeta;
            config.ToFile();

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
                Faction squadronFaction = theEvent.factions.Find(f => (bool)f.presences.
                    Find(p => p.systemName == CurrentStarSystem.systemname)?.squadronhomesystem || f.squadronfaction);
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
            if (CurrentStellarBody == null && !string.IsNullOrEmpty(theEvent.star))
            {
                CurrentStellarBody = new Body()
                {
                    bodyname = theEvent.star,
                    bodyType = BodyType.FromEDName("Star"),
                    stellarclass = LastFSDEngagedEvent?.stellarclass,
                };
                CurrentStarSystem.AddOrUpdateBody(CurrentStellarBody);
            }

            // (When pledged) Powerplay information
            CurrentStarSystem.Power = theEvent.Power;
            CurrentStarSystem.powerState = theEvent.powerState;

            // Update to most recent information
            CurrentStarSystem.visitLog.Add(theEvent.timestamp);
            CurrentStarSystem.updatedat = (long)theEvent.timestamp.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
            StarSystemSqLiteRepository.Instance.SaveStarSystem(CurrentStarSystem);

            setSystemDistanceFromHome(CurrentStarSystem);
            setCommanderTitle();

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

            // No longer in 'station instance'
            CurrentStation = null;

            // We are in the ship
            Vehicle = Constants.VEHICLE_SHIP;

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
            return true;
        }

        private bool eventCrewJoined(CrewJoinedEvent theEvent)
        {
            inCrew = true;
            Logging.Info("Entering multicrew session");
            return true;
        }

        private bool eventCrewLeft(CrewLeftEvent theEvent)
        {
            inCrew = false;
            Logging.Info("Leaving multicrew session");
            return true;
        }

        private bool eventCommanderContinued(CommanderContinuedEvent theEvent)
        {
            // If we see this it means that we aren't in CQC
            inCQC = false;

            // Set our commander name
            if (Cmdr.name == null)
            {
                Cmdr.name = theEvent.commander;
            }

            // Set game version
            inHorizons = theEvent.horizons;
            EliteConfiguration config = EliteConfiguration.FromFile();
            config.Horizons = inHorizons;
            config.ToFile();

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

        private bool eventCombatPromotion(CombatPromotionEvent theEvent)
        {
            // There is a bug with the journal where it reports superpower increases in rank as combat increases
            // Hence we check to see if this is a real event by comparing our known combat rating to the promoted rating
            if ((Cmdr == null || Cmdr.combatrating == null) || theEvent.rating != Cmdr.combatrating.localizedName)
            {
                // Real event. Capture commander ratings and add them to the commander object
                if (Cmdr != null)
                {
                    Cmdr.combatrating = CombatRating.FromName(theEvent.rating);
                }
                return true;
            }
            else
            {
                // False event
                return false;
            }
        }

        private bool eventTradePromotion(TradePromotionEvent theEvent)
        {
            // Capture commander ratings and add them to the commander object
            if (Cmdr != null)
            {
                Cmdr.traderating = TradeRating.FromName(theEvent.rating);
            }
            return true;
        }

        private bool eventExplorationPromotion(ExplorationPromotionEvent theEvent)
        {
            // Capture commander ratings and add them to the commander object
            if (Cmdr != null)
            {
                Cmdr.explorationrating = ExplorationRating.FromName(theEvent.rating);
            }
            return true;
        }

        private bool eventFederationPromotion(FederationPromotionEvent theEvent)
        {
            // Capture commander ratings and add them to the commander object
            if (Cmdr != null)
            {
                Cmdr.federationrating = FederationRating.FromName(theEvent.rank);
            }
            return true;
        }

        private bool eventEmpirePromotion(EmpirePromotionEvent theEvent)
        {
            // Capture commander ratings and add them to the commander object
            if (Cmdr != null)
            {
                Cmdr.empirerating = EmpireRating.FromName(theEvent.rank);
            }
            return true;
        }

        private bool eventSquadronStartup(SquadronStartupEvent theEvent)
        {
            SquadronRank rank = SquadronRank.FromRank(theEvent.rank + 1);

            // Update the configuration file
            EDDIConfiguration configuration = EDDIConfiguration.FromFile();
            configuration.SquadronName = theEvent.name;
            configuration.SquadronRank = rank;
            configuration.ToFile();

            // Update the squadron UI data
            Instance.MainWindow?.Dispatcher?.Invoke(new Action(() =>
            {
                Instance.MainWindow.eddiSquadronNameText.Text = theEvent.name;
                Instance.MainWindow.squadronRankDropDown.SelectedItem = rank.localizedName;
            }));

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
            EDDIConfiguration configuration = EDDIConfiguration.FromFile();

            switch (theEvent.status)
            {
                case "created":
                    {
                        SquadronRank rank = SquadronRank.FromRank(1);

                        // Update the configuration file
                        configuration.SquadronName = theEvent.name;
                        configuration.SquadronRank = rank;

                        // Update the squadron UI data
                        Instance.MainWindow?.Dispatcher?.Invoke(new Action(() =>
                        {
                            Instance.MainWindow.eddiSquadronNameText.Text = theEvent.name;
                            Instance.MainWindow.squadronRankDropDown.SelectedItem = rank.localizedName;
                            configuration = Instance.MainWindow.resetSquadronRank(configuration);
                        }));

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
                        Instance.MainWindow?.Dispatcher?.Invoke(new Action(() =>
                        {
                            Instance.MainWindow.eddiSquadronNameText.Text = theEvent.name;
                        }));

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
                        Instance.MainWindow?.Dispatcher?.Invoke(new Action(() =>
                        {
                            Instance.MainWindow.eddiSquadronNameText.Text = string.Empty;
                            Instance.MainWindow.eddiSquadronIDText.Text = string.Empty;
                            configuration = Instance.MainWindow.resetSquadronRank(configuration);
                        }));

                        // Update the commander object, if it exists
                        if (Cmdr != null)
                        {
                            Cmdr.squadronname = null;
                        }
                        break;
                    }
            }
            configuration.ToFile();
            return true;
        }

        private bool eventSquadronRank(SquadronRankEvent theEvent)
        {
            SquadronRank rank = SquadronRank.FromRank(theEvent.newrank + 1);

            // Update the configuration file
            EDDIConfiguration configuration = EDDIConfiguration.FromFile();
            configuration.SquadronName = theEvent.name;
            configuration.SquadronRank = rank;
            configuration.ToFile();

            // Update the squadron UI data
            Instance.MainWindow?.Dispatcher?.Invoke(new Action(() =>
            {
                Instance.MainWindow.eddiSquadronNameText.Text = theEvent.name;
                Instance.MainWindow.squadronRankDropDown.SelectedItem = rank.localizedName;
            }));

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
            // In CQC we don't want to report anything, so set our CQC flag
            inCQC = true;
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
            if (CurrentStarSystem == null) { return false; }

            Body star = CurrentStarSystem?.bodies?.Find(s => s.bodyname == theEvent.bodyname);
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
            if (CurrentStarSystem == null) { return false; }

            // Add this body if it hasn't been previously added to our database, but don't
            // replace prior data which isn't re-obtainable from this event. 
            // (e.g. alreadydiscovered, scanned, alreadymapped, mapped, mappedEfficiently, etc.)
            Body body = CurrentStarSystem?.bodies?.Find(s => s.bodyname == theEvent.bodyname);
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
            if (CurrentStarSystem != null)
            {
                Body body = CurrentStarSystem.bodies.Find(b => b.bodyname == theEvent.bodyName);
                if (body?.mapped is null && !(theEvent.body is null))
                {
                    CurrentStarSystem.AddOrUpdateBody(theEvent.body);
                    StarSystemSqLiteRepository.Instance.SaveStarSystem(CurrentStarSystem);
                    updateCurrentStellarBody(theEvent.bodyName, CurrentStarSystem?.systemname, CurrentStarSystem?.systemAddress);
                }
            }
            return true;
        }

        private bool eventRingMapped(RingMappedEvent theEvent)
        {
            updateCurrentStellarBody(theEvent.ringname, CurrentStarSystem?.systemname, CurrentStarSystem?.systemAddress);
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
                    // Save a timestamp when the API refreshes, so that we can compare whether events are more or less recent
                    ApiTimeStamp = DateTime.UtcNow;

                    long profileTime = (long)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
                    Profile profile = CompanionAppService.Instance.Profile();
                    if (profile != null)
                    {
                        // Use the profile as primary information for our commander and shipyard
                        Cmdr = profile.Cmdr;

                        // Reinstate information not obtained from the Companion API (gender settings)
                        EDDIConfiguration configuration = EDDIConfiguration.FromFile();
                        if (configuration != null)
                        {
                            Cmdr.gender = configuration.Gender;
                            Cmdr.squadronname = configuration.SquadronName;
                            Cmdr.squadronid = configuration.SquadronID;
                            Cmdr.squadronrank = configuration.SquadronRank;
                            Cmdr.squadronallegiance = configuration.SquadronAllegiance;
                            Cmdr.squadronpower = configuration.SquadronPower;
                            Cmdr.squadronfaction = configuration.SquadronFaction;
                        }

                        bool updatedCurrentStarSystem = false;

                        // Only set the current star system if it is not present, otherwise we leave it to events
                        if (CurrentStarSystem == null)
                        {
                            CurrentStarSystem = profile?.CurrentStarSystem;
                            setSystemDistanceFromHome(CurrentStarSystem);

                            if (profile.docked && profile.CurrentStarSystem?.systemname == CurrentStarSystem.systemname && CurrentStarSystem.stations != null)
                            {
                                CurrentStation = CurrentStarSystem.stations.FirstOrDefault(s => s.name == profile.LastStation.name);
                                if (CurrentStation != null)
                                {
                                    // Only set the current station if it is not present, otherwise we leave it to events
                                    Logging.Debug("Set current station to " + CurrentStation.name);
                                    CurrentStation.updatedat = profileTime;
                                    updatedCurrentStarSystem = true;
                                }
                            }
                        }

                        if (refreshStation && CurrentStation != null && Environment == Constants.ENVIRONMENT_DOCKED)
                        {
                            // Refresh station data
                            profileUpdateNeeded = true;
                            profileStationRequired = CurrentStation.name;
                            Thread updateThread = new Thread(() => conditionallyRefreshProfile())
                            {
                                IsBackground = true
                            };
                            updateThread.Start();
                        }

                        setCommanderTitle();

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
                                Dictionary<string, object> data = new Dictionary<string, object>
                                {
                                    { "message", ex.Message },
                                    { "stacktrace", ex.StackTrace },
                                    { "profile", JsonConvert.SerializeObject(profile) }
                                };
                                Logging.Error("Monitor " + monitor.MonitorName() + " failed to handle profile.", data);
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

        private void setSystemDistanceFromHome(StarSystem system)
        {
            if (HomeStarSystem != null && HomeStarSystem.x != null && system.x != null)
            {
                system.distancefromhome = (decimal)Math.Round(Math.Sqrt(Math.Pow((double)(system.x - HomeStarSystem.x), 2)
                    + Math.Pow((double)(system.y - HomeStarSystem.y), 2)
                    + Math.Pow((double)(system.z - HomeStarSystem.z), 2)), 2);
                Logging.Debug("Distance from home is " + system.distancefromhome);
            }
        }

        public decimal getSystemDistanceFromDestination(string system)
        {
            decimal distance = 0;
            if (system != null)
            {
                StarSystem starSystem = StarSystemSqLiteRepository.Instance.GetOrFetchStarSystem(system, true);

                if (DestinationStarSystem?.x != null && starSystem?.x != null)
                {
                    distance = (decimal)Math.Round(Math.Sqrt(Math.Pow((double)(starSystem.x - DestinationStarSystem.x), 2)
                        + Math.Pow((double)(starSystem.y - DestinationStarSystem.y), 2)
                        + Math.Pow((double)(starSystem.z - DestinationStarSystem.z), 2)), 2);
                }
            }
            return distance;
        }

        public void setSystemDistanceFromDestination(string system)
        {
            DestinationDistanceLy = getSystemDistanceFromDestination(system);
        }

        /// <summary>Work out the title for the commander in the current system</summary>
        private const int minEmpireRankForTitle = 3;
        private const int minFederationRankForTitle = 1;
        private void setCommanderTitle()
        {
            if (Cmdr != null)
            {
                Cmdr.title = Properties.EddiResources.Commander;
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
            DirectoryInfo dir = new DirectoryInfo(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
            List<EDDIMonitor> monitors = new List<EDDIMonitor>();
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
                                Logging.Debug("Instantiating monitor plugin at " + file.FullName);
                                EDDIMonitor monitor = type.InvokeMember(null,
                                                           BindingFlags.CreateInstance,
                                                           null, null, null) as EDDIMonitor;
                                monitors.Add(monitor);
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
                    string msg = string.Format(Properties.EddiResources.problem_load_monitor_file, dir.FullName);
                    Logging.Error(msg, flex);
                    SpeechService.Instance.Say(null, msg, 0);
                }
                catch (Exception ex)
                {
                    string msg = string.Format(Properties.EddiResources.problem_load_monitor, $"{file.Name}.\n{ex.Message} {ex.InnerException?.Message ?? ""}");
                    Logging.Error(msg, ex);
                    SpeechService.Instance.Say(null, msg, 0);
                }
            }
            return monitors;
        }

        /// <summary>
        /// Find all responders
        /// </summary>
        public List<EDDIResponder> findResponders()
        {
            DirectoryInfo dir = new DirectoryInfo(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
            List<EDDIResponder> responders = new List<EDDIResponder>();
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
                                responders.Add(responder);
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
            return responders;
        }

        private bool profileUpdateNeeded = false;
        private string profileStationRequired = null;

        /// <summary>
        /// Update the profile when requested, ensuring that we meet the condition in the updated profile
        /// </summary>
        private void conditionallyRefreshProfile()
        {
            int maxTries = 6;

            while (running && maxTries > 0 && CompanionAppService.Instance.CurrentState == CompanionAppService.State.Authorized)
            {
                try
                {
                    Logging.Debug("Starting conditional profile fetch");

                    // See if we need to fetch the profile
                    if (profileUpdateNeeded)
                    {
                        // See if we still need this particular update
                        if (profileStationRequired != null && (CurrentStation?.name != profileStationRequired))
                        {
                            Logging.Debug("No longer at requested station; giving up on update");
                            profileUpdateNeeded = false;
                            profileStationRequired = null;
                            break;
                        }

                        // Make sure we know where we are
                        if (CurrentStarSystem.systemname.Length < 0)
                        {
                            break;
                        }

                        // We do need to fetch an updated profile; do so
                        Profile profile = CompanionAppService.Instance?.Profile();
                        if (profile != null)
                        {
                            // If we're docked, the lastStation information is located within the lastSystem identified by the profile
                            if (profile.docked && Environment == Constants.ENVIRONMENT_DOCKED)
                            {
                                ApiTimeStamp = DateTime.UtcNow;
                                long profileTime = (long)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;

                                Logging.Debug("Fetching station profile");
                                Profile stationProfile = CompanionAppService.Instance.Station(CurrentStarSystem.systemname);

                                // Post an update event
                                Event @event = new MarketInformationUpdatedEvent(DateTime.UtcNow, inHorizons, stationProfile.CurrentStarSystem.systemname, stationProfile.LastStation.name, stationProfile.LastStation.marketId, stationProfile.LastStation.commodities, stationProfile.LastStation.prohibited, stationProfile.LastStation.outfitting, stationProfile.LastStation.shipyard);
                                enqueueEvent(@event);

                                // See if we need to update our current station
                                Logging.Debug("profileStationRequired is " + profileStationRequired + ", profile station is " + stationProfile.LastStation.name);

                                if (profileStationRequired != null && profileStationRequired == stationProfile.LastStation.name)
                                {
                                    // We have the required station information
                                    Logging.Debug("Current station matches profile information; updating info");
                                    CurrentStation.commodities = stationProfile.LastStation.commodities;
                                    CurrentStation.economyShares = stationProfile.LastStation.economyShares;
                                    CurrentStation.prohibited = stationProfile.LastStation.prohibited;
                                    CurrentStation.commoditiesupdatedat = profileTime;
                                    CurrentStation.outfitting = stationProfile.LastStation.outfitting;
                                    CurrentStation.shipyard = stationProfile.LastStation.shipyard;
                                    CurrentStation.updatedat = profileTime;

                                    // Update the current station information in our backend DB
                                    Logging.Debug("Star system information updated from remote server; updating local copy");
                                    StarSystemSqLiteRepository.Instance.SaveStarSystem(CurrentStarSystem);

                                    profileUpdateNeeded = false;
                                    allowMarketUpdate = false;
                                    allowOutfittingUpdate = false;
                                    allowShipyardUpdate = false;

                                    break;
                                }
                            }
                        }

                        // No luck; sleep and try again
                        Thread.Sleep(15000);
                    }
                }
                catch (Exception ex)
                {
                    Logging.Error("Exception obtaining profile", ex);
                }
                finally
                {
                    maxTries--;
                }
            }

            if (maxTries == 0)
            {
                Logging.Info("Maximum attempts reached; giving up on request");
            }

            // Clear the update info
            profileUpdateNeeded = false;
            profileStationRequired = null;
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

        public void updateDestinationSystemStation(EDDIConfiguration configuration)
        {
            updateDestinationSystem(configuration.DestinationSystem);
            updateDestinationStation(configuration.DestinationStation);
        }

        public void updateDestinationSystem(string destinationSystem)
        {
            EDDIConfiguration configuration = EDDIConfiguration.FromFile();
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
            configuration.ToFile();
        }

        public void updateDestinationStation(string destinationStation)
        {
            EDDIConfiguration configuration = EDDIConfiguration.FromFile();
            if (destinationStation != null && DestinationStarSystem?.stations != null)
            {
                string destinationStationName = destinationStation.Trim();
                Station station = DestinationStarSystem.stations.FirstOrDefault(s => s.name == destinationStationName);
                if (station != null)
                {
                    if (station.name != DestinationStation?.name)
                    {
                        Logging.Debug("Destination station is " + station.name);
                        DestinationStation = station;
                    }
                }
            }
            else
            {
                DestinationStation = null;
            }
            configuration.DestinationStation = destinationStation;
            configuration.ToFile();
        }

        public void updateHomeSystemStation(EDDIConfiguration configuration)
        {
            updateHomeSystem(configuration);
            updateHomeStation(configuration);
            configuration.ToFile();
        }

        public EDDIConfiguration updateHomeSystem(EDDIConfiguration configuration)
        {
            Logging.Verbose = configuration.Debug;
            if (configuration.HomeSystem != null)
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
            Logging.Verbose = configuration.Debug;
            if (HomeStarSystem?.stations != null && configuration.HomeStation != null)
            {
                string homeStationName = configuration.HomeStation.Trim();
                foreach (Station station in HomeStarSystem.stations)
                {
                    if (station.name == homeStationName)
                    {
                        HomeStation = station;
                        Logging.Debug("Home station is " + HomeStation.name);
                        break;
                    }
                }
            }
            return configuration;
        }

        public EDDIConfiguration updateSquadronSystem(EDDIConfiguration configuration)
        {
            Logging.Verbose = configuration.Debug;
            if (configuration.SquadronSystem != null)
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
                EDDIConfiguration configuration = EDDIConfiguration.FromFile();

                //Update the squadron faction, if changed
                if (configuration.SquadronFaction == null || configuration.SquadronFaction != faction.name)
                {
                    configuration.SquadronFaction = faction.name;

                    Instance.MainWindow?.Dispatcher?.Invoke(new Action(() =>
                    {
                        Instance.MainWindow.squadronFactionDropDown.SelectedItem = faction.name;
                    }));

                    Cmdr.squadronfaction = faction.name;
                }

                // Update system, allegiance, & power when in squadron home system
                if ((bool)faction.presences.FirstOrDefault(p => p.systemName == systemName)?.squadronhomesystem)
                {
                    // Update the squadron system data, if changed
                    string system = CurrentStarSystem.systemname;
                    if (configuration.SquadronSystem == null || configuration.SquadronSystem != system)
                    {
                        configuration.SquadronSystem = system;

                        Instance.MainWindow?.Dispatcher?.Invoke(new Action(() =>
                        {
                            Instance.MainWindow.squadronSystemDropDown.Text = system;
                            Instance.MainWindow.ConfigureSquadronFactionOptions(configuration);
                        }));

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

                            Instance.MainWindow?.Dispatcher?.Invoke(new Action(() =>
                            {
                                Instance.MainWindow.squadronPowerDropDown.SelectedItem = power.localizedName;
                                Instance.MainWindow.ConfigureSquadronPowerOptions(configuration);
                            }));
                        }
                    }
                }
                configuration.ToFile();
            }
        }
    }
}
