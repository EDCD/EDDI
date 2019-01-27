using EddiCompanionAppService;
using EddiDataDefinitions;
using EddiDataProviderService;
using EddiEvents;
using EddiSpeechService;
using EddiStarMapService;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Text;
using System.Threading;
using System.Windows;
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
        private List<EDDIMonitor> activeMonitors = new List<EDDIMonitor>();

        public List<EDDIResponder> responders = new List<EDDIResponder>();
        private List<EDDIResponder> activeResponders = new List<EDDIResponder>();

        // Information obtained from the companion app service
        public Commander Cmdr { get; private set; }
        public DateTime ApiTimeStamp { get; private set; }

        // Services made available from EDDI
        public StarMapService starMapService { get; private set; }

        // Information obtained from the configuration
        public StarSystem HomeStarSystem { get; private set; }
        public Station HomeStation { get; private set; }
        public StarSystem SquadronStarSystem { get; private set; }

        // Information obtained from the player journal
        public string Environment { get; private set; }
        public StarSystem CurrentStarSystem { get; private set; }
        public StarSystem LastStarSystem { get; private set; }
        public Station CurrentStation { get; private set; }
        public Body CurrentStellarBody { get; private set; }
        public DateTime JournalTimeStamp { get; set; } = DateTime.MinValue;

        // Current vehicle of player
        public string Vehicle { get; private set; } = Constants.VEHICLE_SHIP;
        public Ship CurrentShip { get; set; }

        // Information from the last jump we initiated (for reference)
        public FSDEngagedEvent LastFSDEngagedEvent { get; private set; }

        // Our main window, made accessible via the applicable EDDI Instance
        public MainWindow MainWindow { get; internal set; }

        public ObservableConcurrentDictionary<string, object> State = new ObservableConcurrentDictionary<string, object>();

        /// <summary>
        ///  Special case - trigger our first location event regardless of if it matches our current location
        /// </summary>
        private bool firstLocation = true;

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

                // Set up the EDDI configuration
                EDDIConfiguration configuration = EDDIConfiguration.FromFile();
                updateHomeSystemStation(configuration);
                updateSquadronSystem(configuration, true);

                if (running)
                {
                    // Set up monitors and responders
                    monitors = findMonitors();
                    responders = findResponders();

                    // Set up the app service
                    if (CompanionAppService.Instance.CurrentState == CompanionAppService.State.Authorized)
                    {
                        // Carry out initial population of profile
                        try
                        {
                            refreshProfile();
                        }
                        catch (Exception ex)
                        {
                            Logging.Debug("Failed to obtain profile: " + ex);
                        }
                    }

                    Cmdr.gender = configuration.Gender;
                    Cmdr.squadronname = configuration.SquadronName;
                    Cmdr.squadronid = configuration.SquadronID;
                    Cmdr.squadronrank = configuration.SquadronRank;
                    Cmdr.squadronallegiance = configuration.SquadronAllegiance;
                    Cmdr.squadronpower = configuration.SquadronPower;
                    Cmdr.squadronfaction = configuration.SquadronFaction;
                    if (Cmdr.name != null)
                    {
                        Logging.Info("EDDI access to the companion app is enabled");
                    }
                    else
                    {
                        // If InvokeUpdatePlugin failed then it will have have left an error message, but this once we ignore it
                        Logging.Info("EDDI access to the companion app is disabled");
                    }

                    // Pass our commander name to the StarMapService (if it has been set via the Frontier API) and initialize the StarMapService
                    if (Cmdr != null && Cmdr.name != null)
                    {
                        StarMapService.commanderName = Cmdr.name;
                    }
                    starMapService = new StarMapService();
                }
                else
                {
                    Logging.Info("Mandatory upgrade required! EDDI initializing in safe mode until upgrade is completed.");
                }

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
                            SpeechService.Instance.Say(null, message, false);
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
                            SpeechService.Instance.Say(null, message, false);
                        }
                        UpgradeAvailable = true;
                        UpgradeLocation = info.url;
                        UpgradeVersion = info.version;
                    }
                }
            }
            catch (Exception ex)
            {
                SpeechService.Instance.Say(null, Properties.EddiResources.update_server_unreachable, false);
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
                    SpeechService.Instance.Say(null, Properties.EddiResources.downloading_upgrade, true);
                    string updateFile = Net.DownloadFile(UpgradeLocation, @"EDDI-update.exe");
                    if (updateFile == null)
                    {
                        SpeechService.Instance.Say(null, Properties.EddiResources.download_failed, true);
                    }
                    else
                    {
                        // Inno setup will attempt to restart this application so register it
                        NativeMethods.RegisterApplicationRestart(null, RestartFlags.NONE);

                        Logging.Info("Downloaded update to " + updateFile);
                        Logging.Info("Path is " + Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
                        File.SetAttributes(updateFile, FileAttributes.Normal);
                        SpeechService.Instance.Say(null, Properties.EddiResources.starting_upgrade, true);
                        Logging.Info("Starting upgrade.");

                        Process.Start(updateFile, @"/closeapplications /restartapplications /silent /log /nocancel /noicon /dir=""" + Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"""");
                    }
                }
            }
            catch (Exception ex)
            {
                SpeechService.Instance.Say(null, Properties.EddiResources.upgrade_failed, true);
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
                        if (monitor.NeedsStart())
                        {
                            Thread monitorThread = new Thread(() => keepAlive(monitor.MonitorName(), monitor.Start))
                            {
                                IsBackground = true
                            };
                            Logging.Info("Starting keepalive for " + monitor.MonitorName());
                            monitorThread.Start();
                        }
                        activeMonitors.Add(monitor);
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
                    responder.Stop();
                    activeResponders.Remove(responder);
                }
                foreach (EDDIMonitor monitor in monitors)
                {
                    monitor.Stop();
                    activeMonitors.Remove(monitor);
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

        /// <summary>
        /// Obtain a named responder
        /// </summary>
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

        /// <summary>
        /// Disable a named responder for this session.  This does not update the on-disk status of the responder
        /// </summary>
        public void DisableResponder(string name)
        {
            EDDIResponder responder = ObtainResponder(name);
            if (responder != null)
            {
                responder.Stop();
                activeResponders.Remove(responder);
            }
        }

        /// <summary>
        /// Enable a named responder for this session.  This does not update the on-disk status of the responder
        /// </summary>
        public void EnableResponder(string name)
        {
            EDDIResponder responder = ObtainResponder(name);
            if (responder != null)
            {
                if (!activeResponders.Contains(responder))
                {
                    responder.Start();
                    activeResponders.Add(responder);
                }
            }
        }

        /// <summary>
        /// Disable a named monitor for this session.  This does not update the on-disk status of the responder
        /// </summary>
        public void DisableMonitor(string name)
        {
            EDDIMonitor monitor = ObtainMonitor(name);
            if (monitor != null)
            {
                if (activeMonitors.Contains(monitor))
                {
                    monitor.Stop();
                    activeMonitors.Remove(monitor);
                }
            }
        }

        /// <summary>
        /// Enable a named monitor for this session.  This does not update the on-disk status of the responder
        /// </summary>
        public void EnableMonitor(string name)
        {
            EDDIMonitor monitor = monitors.FirstOrDefault(m => m.MonitorName() == name);
            if (monitor != null)
            {
                if (!activeMonitors.Contains(monitor))
                {
                    if (monitor.NeedsStart())
                    {
                        Thread monitorThread = new Thread(() => keepAlive(monitor.MonitorName(), monitor.Start))
                        {
                            IsBackground = true
                        };
                        Logging.Info("Starting keepalive for " + monitor.MonitorName());
                        monitorThread.Start();
                    }
                    activeMonitors.Add(monitor);
                }
            }
        }

        /// <summary>
        /// Reload a specific monitor or responder
        /// </summary>
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
                    Logging.Warn(name + " stopping after too many failures");
                }
            }
            catch (ThreadAbortException)
            {
                Logging.Debug("Thread aborted");
            }
            catch (Exception ex)
            {
                Logging.Warn("keepAlive failed", ex);
            }
        }

        public void eventHandler(Event @event)
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
                    else if (@event is JumpedEvent)
                    {
                        passEvent = eventJumped((JumpedEvent)@event);
                    }
                    else if (@event is DockedEvent)
                    {
                        passEvent = eventDocked((DockedEvent)@event);
                    }
                    else if (@event is UndockedEvent)
                    {
                        passEvent = eventUndocked((UndockedEvent)@event);
                    }
                    else if (@event is LocationEvent)
                    {
                        passEvent = eventLocation((LocationEvent)@event);
                    }
                    else if (@event is FSDEngagedEvent)
                    {
                        passEvent = eventFSDEngaged((FSDEngagedEvent)@event);
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
                    else if (@event is BeltScannedEvent)
                    {
                        passEvent = eventBeltScanned((BeltScannedEvent)@event);
                    }
                    else if (@event is StarScannedEvent)
                    {
                        passEvent = eventStarScanned((StarScannedEvent)@event);
                    }
                    else if (@event is BodyScannedEvent)
                    {
                        passEvent = eventBodyScanned((BodyScannedEvent)@event);
                    }
                    else if (@event is VehicleDestroyedEvent)
                    {
                        passEvent = eventVehicleDestroyed((VehicleDestroyedEvent)@event);
                    }
                    else if (@event is StatusEvent)
                    {
                        passEvent = eventStatus((StatusEvent)@event);
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

                    // Additional processing is over, send to the event responders if required
                    if (passEvent)
                    {
                        OnEvent(@event);
                    }
                }
                catch (Exception ex)
                {
                    Logging.Error("EDDI core failed to handle event " + JsonConvert.SerializeObject(@event), ex);

                    // Even if an error occurs, we still need to pass the raw data 
                    // to the EDDN responder to maintain it's integrity.
                    Instance.ObtainResponder("EDDN responder").Handle(@event);
                }
            }
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

        private void OnEvent(Event @event)
        {
            // We send the event to all monitors to ensure that their info is up-to-date
            // This is synchronous
            foreach (EDDIMonitor monitor in activeMonitors)
            {
                try
                {
                    monitor.PreHandle(@event);

                }
                catch (Exception ex)
                {
                    Logging.Error("Monitor failed to handle event " + JsonConvert.SerializeObject(@event), ex);
                }
            }

            // Now we pass the data to the responders
            // This is asynchronous
            foreach (EDDIResponder responder in activeResponders)
            {
                try
                {
                    Thread responderThread = new Thread(() =>
                    {
                        try
                        {
                            responder.Handle(@event);
                        }
                        catch (Exception ex)
                        {
                            Logging.Error("Responder failed to handle event " + JsonConvert.SerializeObject(@event), ex);
                        }
                    })
                    {
                        Name = responder.ResponderName(),
                        IsBackground = true
                    };
                    responderThread.Start();
                }
                catch (ThreadAbortException tax)
                {
                    Thread.ResetAbort();
                    Logging.Error(JsonConvert.SerializeObject(@event), tax);
                }
                catch (Exception ex)
                {
                    Logging.Error(JsonConvert.SerializeObject(@event), ex);
                }
            }

            // We also pass the event to all active monitors in case they have follow-on work
            foreach (EDDIMonitor monitor in activeMonitors)
            {
                try
                {
                    Thread monitorThread = new Thread(() =>
                    {
                        try
                        {
                            monitor.PostHandle(@event);
                        }
                        catch (Exception ex)
                        {
                            Logging.Warn("Monitor failed", ex);
                        }
                    })
                    {
                        IsBackground = true
                    };
                    monitorThread.Start();
                }
                catch (ThreadAbortException tax)
                {
                    Thread.ResetAbort();
                    Logging.Error(JsonConvert.SerializeObject(@event), tax);
                }
                catch (Exception ex)
                {
                    Logging.Error(JsonConvert.SerializeObject(@event), ex);
                }
            }
        }

        private bool eventLocation(LocationEvent theEvent)
        {
            Logging.Info("Location StarSystem: " + theEvent.system);

            updateCurrentSystem(theEvent.system);
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

                // Faction data
                Faction controllingFaction = new Faction
                {
                    name = theEvent.faction,
                    Government = theEvent.Government,
                    Allegiance = theEvent.Allegiance,
                    FactionState = theEvent.factions?.FirstOrDefault(f => f.name == theEvent.faction)?.FactionState,
                };
                CurrentStarSystem.Faction = controllingFaction;
            }

            // Update squadron data, if available
            if (theEvent.factions != null)
            {
                updateSquadronData(theEvent.factions);
            }

            if (theEvent.docked == true || theEvent.bodytype.ToLowerInvariant() == "station")
            {
                // In this case body === station and our body information is invalid
                CurrentStellarBody = null;

                // Force first location update even if it matches with 'firstLocation' bool
                if (!firstLocation && (CurrentStation != null && CurrentStation.name == theEvent.station))
                {
                    // We are already at this station; nothing to do
                    Logging.Debug("Already at station " + theEvent.station);
                    return false;
                }
                firstLocation = false;

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
                }

                // Our data source may not include the market id or system address
                station.marketId = theEvent.marketId;
                station.systemAddress = theEvent.systemAddress;

                // Information from the event might be more current than that from our data source so use it in preference
                Faction controllingFaction = new Faction
                {
                    name = theEvent.faction,
                    Government = theEvent.Government,
                    Allegiance = theEvent.Allegiance
                };
                station.Faction = controllingFaction;

                CurrentStation = station;

                // Kick off the profile refresh if the companion API is available
                if (CompanionAppService.Instance.CurrentState == CompanionAppService.State.Authorized)
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
            }
            else if (theEvent.body != null)
            {
                // If we are not at a station then our station information is invalid 
                CurrentStation = null;

                // Force first location update even if it matches with 'firstLocation' bool 
                if (!firstLocation && (CurrentStellarBody != null && CurrentStellarBody.name == theEvent.body))
                {
                    // We are already at this body; nothing to do 
                    Logging.Debug("Already at body " + theEvent.body);
                    return false;
                }
                firstLocation = false;

                // Update the body 
                Logging.Debug("Now at body " + theEvent.body);
                Body body = CurrentStarSystem.bodies.Find(s => s.name == theEvent.body);
                if (body == null)
                {
                    // This body is unknown to us, might not be in our data source or we might not have connectivity.  Use a placeholder 
                    body = new Body
                    {
                        name = theEvent.body,
                        systemname = theEvent.system,
                        systemAddress = theEvent.systemAddress
                    };
                }

                CurrentStellarBody = body;
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

            if (CurrentStation != null && CurrentStation.name == theEvent.station)
            {
                // We are already at this station; nothing to do
                Logging.Debug("Already at station " + theEvent.station);
                return false;
            }

            // We are in the ship
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
            }

            // Not all stations in our database will have a system address or market id, so we set them here
            station.systemAddress = theEvent.systemAddress;
            station.marketId = theEvent.marketId;

            // Information from the event might be more current than our data source so use it in preference

            Faction controllingFaction = new Faction
            {
                name = theEvent.faction,
                Allegiance = theEvent.Allegiance,
                FactionState = theEvent.factionState,
                Government = theEvent.Government
            };
            station.Faction = controllingFaction;
            station.stationServices = theEvent.stationServices;
            station.economyShares = theEvent.economyShares;

            CurrentStation = station;
            CurrentStellarBody = null;

            // Kick off the profile refresh if the companion API is available
            if (CompanionAppService.Instance.CurrentState == CompanionAppService.State.Authorized)
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
            else
            {
                // Kick off a dummy that triggers a market refresh after a couple of seconds
                if (theEvent.fromLoad) { return true; } // Don't fire this event when loading pre-existing logs
                Thread updateThread = new Thread(() => dummyRefreshMarketData())
                {
                    IsBackground = true
                };
                updateThread.Start();
            }

            return true;
        }

        private bool eventUndocked(UndockedEvent theEvent)
        {
            // Call refreshProfile() to ensure that our ship is up-to-date
            refreshProfile();

            return true;
        }

        private bool eventMarket(MarketEvent theEvent)
        {
            if (allowMarketUpdate && CurrentStation != null && CurrentStation.marketId == theEvent.marketId)
            {
                MarketInfoReader info = MarketInfoReader.FromFile();
                if (info != null)
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
                        // Update the current station commodities
                        allowMarketUpdate = false;
                        CurrentStation.commodities = quotes;
                        CurrentStation.commoditiesupdatedat = (long)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;

                        // Update the current station information in our backend DB
                        Logging.Debug("Star system information updated from remote server; updating local copy");
                        StarSystemSqLiteRepository.Instance.SaveStarSystem(CurrentStarSystem);

                        // Post an update event for new market data
                        if (theEvent.fromLoad) { return true; } // Don't fire this event when loading pre-existing logs
                        Event @event = new MarketInformationUpdatedEvent(DateTime.UtcNow, "market");
                        eventHandler(@event);
                    }
                    return true;
                }
            }
            return false;
        }

        private bool eventOutfitting(OutfittingEvent theEvent)
        {
            if (allowOutfittingUpdate && CurrentStation != null && CurrentStation.marketId == theEvent.marketId)
            {
                OutfittingInfoReader info = OutfittingInfoReader.FromFile();
                if (info.Items != null)
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
                        // Update the current station outfitting
                        allowOutfittingUpdate = false;
                        CurrentStation.outfitting = modules;
                        CurrentStation.outfittingupdatedat = (long)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;

                        // Update the current station information in our backend DB
                        Logging.Debug("Star system information updated from remote server; updating local copy");
                        StarSystemSqLiteRepository.Instance.SaveStarSystem(CurrentStarSystem);

                        // Post an update event for new outfitting data
                        if (theEvent.fromLoad) { return true; } // Don't fire this event when loading pre-existing logs
                        Event @event = new MarketInformationUpdatedEvent(DateTime.UtcNow, "outfitting");
                        eventHandler(@event);
                    }
                    return true;
                }
            }
            return false;
        }

        private bool eventShipyard(ShipyardEvent theEvent)
        {
            if (allowShipyardUpdate && CurrentStation != null && CurrentStation.marketId == theEvent.marketId)
            {
                ShipyardInfoReader info = ShipyardInfoReader.FromFile();
                if (info.PriceList != null)
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
                        // Update the current station shipyard
                        allowShipyardUpdate = false;
                        CurrentStation.shipyard = ships;
                        CurrentStation.shipyardupdatedat = (long)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;

                        // Update the current station information in our backend DB
                        Logging.Debug("Star system information updated from remote server; updating local copy");
                        StarSystemSqLiteRepository.Instance.SaveStarSystem(CurrentStarSystem);

                        // Post an update event for new shipyard data
                        if (theEvent.fromLoad) { return true; } // Don't fire this event when loading pre-existing logs
                        Event @event = new MarketInformationUpdatedEvent(DateTime.UtcNow, "shipyard");
                        eventHandler(@event);
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
            if (CurrentStarSystem == null || CurrentStarSystem.name != name)
            {
                if (CurrentStarSystem != null && CurrentStarSystem.name != name)
                {
                    // We have changed system so update the old one as to when we left
                    StarSystemSqLiteRepository.Instance.LeaveStarSystem(CurrentStarSystem);
                }
                LastStarSystem = CurrentStarSystem;
                CurrentStarSystem = StarSystemSqLiteRepository.Instance.GetOrCreateStarSystem(name);
                setSystemDistanceFromHome(CurrentStarSystem);
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

            // Save a copy of this event for reference
            LastFSDEngagedEvent = @event;

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
            if (CurrentStarSystem == null || CurrentStarSystem.name != theEvent.system)
            {
                // The 'StartJump' event must have been missed
                updateCurrentSystem(theEvent.system);
            }

            passEvent = true;
            CurrentStarSystem.systemAddress = theEvent.systemAddress;
            CurrentStarSystem.x = theEvent.x;
            CurrentStarSystem.y = theEvent.y;
            CurrentStarSystem.z = theEvent.z;
            CurrentStellarBody = CurrentStarSystem.bodies.FirstOrDefault(b => b.name == theEvent.star);

            Faction controllingFaction = new Faction
            {
                name = theEvent.faction,
                Allegiance = theEvent.Allegiance,
                Government = theEvent.Government,
                FactionState = theEvent.factionState,
            };
            CurrentStarSystem.Faction = controllingFaction;

            // Update squadron data, if available
            if (theEvent.factions != null)
            {
                updateSquadronData(theEvent.factions);
            }

            CurrentStarSystem.Economies = new List<Economy> { theEvent.Economy, theEvent.Economy2 };
            CurrentStarSystem.securityLevel = theEvent.securityLevel;
            if (theEvent.population != null)
            {
                CurrentStarSystem.population = theEvent.population;
            }

            if (CurrentStarSystem.lastvisit < theEvent.timestamp)
            {
                CurrentStarSystem.lastvisit = theEvent.timestamp;
                CurrentStarSystem.visits++;
            }

            // Update to most recent information
            CurrentStarSystem.updatedat = (long)theEvent.timestamp.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
            StarSystemSqLiteRepository.Instance.SaveStarSystem(CurrentStarSystem);

            setSystemDistanceFromHome(CurrentStarSystem);
            setCommanderTitle();

            // After jump has completed we are always in supercruise
            Environment = Constants.ENVIRONMENT_SUPERCRUISE;

            // If we don't have any information about bodies in the system yet, create a basic main star from current and saved event data
            if (CurrentStellarBody == null)
            {
                CurrentStellarBody = new Body()
                {
                    name = theEvent.star,
                    Type = BodyType.FromEDName("Star"),
                    stellarclass = LastFSDEngagedEvent?.stellarclass,
                    mainstar = true,
                    distance = 0, 
                };
                CurrentStarSystem.bodies.Add(CurrentStellarBody);
            }

            return passEvent;
        }

        private bool eventEnteredSupercruise(EnteredSupercruiseEvent theEvent)
        {
            Environment = Constants.ENVIRONMENT_SUPERCRUISE;
            updateCurrentSystem(theEvent.system);

            // We are in the ship
            Vehicle = Constants.VEHICLE_SHIP;

            return true;
        }

        private bool eventEnteredNormalSpace(EnteredNormalSpaceEvent theEvent)
        {
            // We won't update CurrentStation with this event, as doing so triggers false / premature updates from the Frontier API
            CurrentStation = null;

            Environment = Constants.ENVIRONMENT_NORMAL_SPACE;

            if (theEvent.bodytype.ToLowerInvariant() == "station")
            {
                // In this case body == station 
            }
            else if (theEvent.body != null)
            {
                // Update the body 
                Body body = CurrentStarSystem?.bodies?.Find(s => s.name == theEvent.body);
                if (body == null)
                {
                    // This body is unknown to us, might not be in EDDB or we might not have connectivity.  Use a placeholder 
                    body = new Body
                    {
                        name = theEvent.body,
                        systemname = theEvent.system
                    };
                }
                CurrentStellarBody = body;
            }
            updateCurrentSystem(theEvent.system);
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

        private bool eventSquadronStatus(SquadronStatusEvent theEvent)
        {
            // Update the configuration file
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

        private bool eventStatus(StatusEvent theEvent)
        {
            if (Environment != Constants.ENVIRONMENT_WITCH_SPACE)
            {
                if (theEvent.status.supercruise)
                {
                    Environment = Constants.ENVIRONMENT_SUPERCRUISE;
                }
                else
                {
                    Environment = Constants.ENVIRONMENT_NORMAL_SPACE;
                }
            }
            Vehicle = theEvent.status.vehicle;
            return true;
        }

        private bool eventNearSurface(NearSurfaceEvent theEvent)
        {
            // We won't update CurrentStation with this event, as doing so triggers false / premature updates from the Frontier API
            CurrentStation = null;

            if (theEvent.approaching_surface)
            {
                // Update the body 
                Body body = CurrentStarSystem?.bodies?.Find(s => s.name == theEvent.body);
                if (body == null)
                {
                    // This body is unknown to us, might not be in our data source or we might not have connectivity.  Use a placeholder 
                    body = new Body
                    {
                        name = theEvent.body,
                        systemname = theEvent.system
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
            updateCurrentSystem(theEvent.system);
            return true;
        }

        private bool eventBeltScanned(BeltScannedEvent theEvent)
        {
            // We just scanned a star.  We can only proceed if we know our current star system
            if (CurrentStarSystem != null)
            {
                Body belt = CurrentStarSystem.bodies?.FirstOrDefault(b => b.name == theEvent.name);
                if (belt == null)
                {
                    Logging.Debug("Scanned belt " + theEvent.name + " is new - creating");
                    // A new item - set it up
                    belt = new Body
                    {
                        EDDBID = -1,
                        Type = BodyType.FromEDName("Star"),
                        name = theEvent.name,
                        systemname = CurrentStarSystem?.name,
                        systemAddress = CurrentStarSystem?.systemAddress
                    };
                    CurrentStarSystem.bodies?.Add(belt);
                }

                // Update with the information we have

                belt.distance = (long?)theEvent.distancefromarrival;

                CurrentStarSystem.bodies?.Add(belt);
            }
            return CurrentStarSystem != null;
        }

        private bool eventStarScanned(StarScannedEvent theEvent)
        {
            // We just scanned a star.  We can only proceed if we know our current star system
            if (CurrentStarSystem != null)
            {
                Body star = CurrentStarSystem.bodies?.FirstOrDefault(b => b.name == theEvent.name);
                if (star == null)
                {
                    Logging.Debug("Scanned star " + theEvent.name + " is new - creating");
                    // A new item - set it up
                    star = new Body
                    {
                        EDDBID = -1,
                        Type = BodyType.FromEDName("Star"),
                        name = theEvent.name,
                        systemname = CurrentStarSystem?.name,
                        systemAddress = CurrentStarSystem?.systemAddress
                    };
                    CurrentStarSystem.bodies?.Add(star);
                }
                // Our data source may not include system address, so we include it here.
                star.systemAddress = CurrentStarSystem?.systemAddress;

                // Update with the information we have
                star.age = theEvent.age;
                star.distance = (long?)theEvent.distancefromarrival;
                star.luminosityclass = theEvent.luminosityclass;
                star.temperature = (long?)theEvent.temperature;
                star.stellarclass = theEvent.stellarclass;
                star.solarmass = theEvent.solarmass;
                star.solarradius = theEvent.solarradius;
                star.rings = theEvent.rings;

                star.setStellarExtras();

                CurrentStarSystem.bodies?.Add(star);
                Logging.Debug("Saving data for scanned star " + theEvent.name);
                StarSystemSqLiteRepository.Instance.SaveStarSystem(CurrentStarSystem);
            }
            return CurrentStarSystem != null;
        }

        private bool eventBodyScanned(BodyScannedEvent theEvent)
        {
            // We just scanned a body.  We can only proceed if we know our current star system
            if (CurrentStarSystem != null)
            {
                Body body = CurrentStarSystem.bodies.FirstOrDefault(b => b.name == theEvent.name);
                if (body == null)
                {
                    Logging.Debug("Scanned body " + theEvent.name + " is new - creating");
                    // A new body - set it up
                    body = new Body
                    {
                        EDDBID = -1,
                        Type = BodyType.FromEDName("Planet"),
                        name = theEvent.name,
                        systemname = CurrentStarSystem.name,
                        systemAddress = CurrentStarSystem?.systemAddress
                    };
                    CurrentStarSystem.bodies.Add(body);
                }
                // Our data source may not include system address, so we include it here.
                body.systemAddress = CurrentStarSystem?.systemAddress;

                // Update with the information we have
                body.distance = (long?)theEvent.distancefromarrival;
                body.landable = theEvent.landable;
                body.tidallylocked = theEvent.tidallylocked;
                body.temperature = (long?)theEvent.temperature;
                body.periapsis = theEvent.periapsis;
                body.atmosphereclass = theEvent.atmosphereclass;
                body.atmospherecompositions = theEvent.atmospherecomposition;
                body.solidcompositions = theEvent.solidcomposition;
                body.gravity = theEvent.gravity;
                body.eccentricity = theEvent.eccentricity;
                body.inclination = theEvent.orbitalinclination;
                body.orbitalperiod = theEvent.orbitalperiod;
                body.rotationalperiod = theEvent.rotationperiod;
                body.semimajoraxis = theEvent.semimajoraxis;
                body.pressure = theEvent.pressure;
                body.terraformState = theEvent.terraformState;
                body.planetClass = theEvent.planetClass;
                body.volcanism = theEvent.volcanism;
                body.materials = new List<MaterialPresence>();
                foreach (MaterialPresence presence in theEvent.materials)
                {
                    body.materials.Add(new MaterialPresence(presence.definition, presence.percentage));
                }
                body.reserveLevel = ReserveLevel.FromEDName(theEvent.reserves);
                body.rings = theEvent.rings;

                Logging.Debug("Saving data for scanned body " + theEvent.name);
                StarSystemSqLiteRepository.Instance.SaveStarSystem(CurrentStarSystem);
            }

            return CurrentStarSystem != null;
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

                            // We don't know if we are docked or not at this point.  Fill in the data if we can, and
                            // let later systems worry about removing it if it's decided that we aren't docked
                            if (profile.LastStation != null && profile.LastStation.systemname == CurrentStarSystem.name && CurrentStarSystem.stations != null)
                            {
                                CurrentStation = CurrentStarSystem.stations.FirstOrDefault(s => s.name == profile.LastStation.name);
                                if (CurrentStation != null)
                                {
                                    Logging.Debug("Set current station to " + CurrentStation.name);
                                    CurrentStation.updatedat = profileTime;
                                    updatedCurrentStarSystem = true;
                                }
                            }
                        }

                        if (refreshStation && CurrentStation != null)
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
                                Logging.Error(JsonConvert.SerializeObject(profile), tax);
                                success = false;
                            }
                            catch (Exception ex)
                            {
                                Logging.Error(JsonConvert.SerializeObject(profile), ex);
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
                    SpeechService.Instance.Say(null, msg, false);
                }
                catch (Exception ex)
                {
                    string msg = string.Format(Properties.EddiResources.problem_load_monitor, $"{file.Name}.\n{ex.Message} {ex.InnerException?.Message ?? ""}");
                    Logging.Error(msg, ex);
                    SpeechService.Instance.Say(null, msg, false);
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
                        if (profileStationRequired != null && (CurrentStation == null || CurrentStation.name != profileStationRequired))
                        {
                            Logging.Debug("No longer at requested station; giving up on update");
                            profileUpdateNeeded = false;
                            profileStationRequired = null;
                            break;
                        }

                        // Make sure we know where we are
                        if (CurrentStarSystem.name.Length < 0)
                        {
                            break;
                        }

                        // We do need to fetch an updated profile; do so
                        ApiTimeStamp = DateTime.UtcNow;
                        long profileTime = (long)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
                        Logging.Debug("Fetching station profile");
                        Profile profile = CompanionAppService.Instance.Station(CurrentStarSystem.name);

                        // See if it is up-to-date regarding our requirements
                        Logging.Debug("profileStationRequired is " + profileStationRequired + ", profile station is " + profile.LastStation.name);

                        if (profileStationRequired != null && profileStationRequired == profile.LastStation.name)
                        {
                            // We have the required station information
                            Logging.Debug("Current station matches profile information; updating info");
                            CurrentStation.commodities = profile.LastStation.commodities;
                            CurrentStation.economyShares = profile.LastStation.economyShares;
                            CurrentStation.prohibited = profile.LastStation.prohibited;
                            CurrentStation.commoditiesupdatedat = profileTime;
                            CurrentStation.outfitting = profile.LastStation.outfitting;
                            CurrentStation.shipyard = profile.LastStation.shipyard;
                            CurrentStation.updatedat = profileTime;

                            // Update the current station information in our backend DB
                            Logging.Debug("Star system information updated from remote server; updating local copy");
                            StarSystemSqLiteRepository.Instance.SaveStarSystem(CurrentStarSystem);

                            // Post an update event
                            Event @event = new MarketInformationUpdatedEvent(DateTime.UtcNow, "profile");
                            eventHandler(@event);

                            profileUpdateNeeded = false;
                            allowMarketUpdate = false;
                            allowOutfittingUpdate = false;
                            allowShipyardUpdate = false;

                            break;
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

        // If we have no access to the companion API but need to trigger a market update then we can call this method
        private void dummyRefreshMarketData()
        {
            Thread.Sleep(2000);
            Event @event = new MarketInformationUpdatedEvent(DateTime.UtcNow, "profile");
            eventHandler(@event);
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

        public void updateHomeSystemStation(EDDIConfiguration configuration)
        {
            updateHomeSystem(configuration, true);
            updateHomeStation(configuration);
            configuration.ToFile();
        }

        public EDDIConfiguration updateHomeSystem(EDDIConfiguration configuration, bool refresh = false)
        {
            Logging.Verbose = configuration.Debug;
            configuration.validHomeSystem = false;
            if (configuration.HomeSystem != null && configuration.HomeSystem.Trim().Length > 2)
            {
                StarSystem system = StarSystemSqLiteRepository.Instance.GetOrFetchStarSystem(configuration.HomeSystem.Trim(), refresh);

                //Ignore null & empty systems
                if (system != null && system.bodies?.Count > 0)
                {
                    if (refresh || system.name != HomeStarSystem?.name)
                    {
                        HomeStarSystem = system;
                        Logging.Debug("Home star system is " + HomeStarSystem.name);
                        configuration.HomeSystem = system.name;
                    }
                    configuration.validHomeSystem = HomeStarSystem.bodies.Count > 0 || HomeStarSystem.stations.Count > 0 || HomeStarSystem.population > 0;
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
            configuration.validHomeStation = false;
            if (HomeStarSystem?.stations != null && configuration.HomeStation != null)
            {
                string homeStationName = configuration.HomeStation.Trim();
                foreach (Station station in HomeStarSystem.stations)
                {
                    if (station.name == homeStationName)
                    {
                        HomeStation = station;
                        Logging.Debug("Home station is " + HomeStation.name);
                        configuration.validHomeStation = true;
                        break;
                    }
                }
            }
            return configuration;
        }

        public EDDIConfiguration updateSquadronSystem(EDDIConfiguration configuration, bool refresh = false)
        {
            Logging.Verbose = configuration.Debug;
            configuration.validSquadronSystem = false;
            if (configuration.SquadronSystem != null && configuration.SquadronSystem.Trim().Length > 2)
            {
                StarSystem system = StarSystemSqLiteRepository.Instance.GetOrFetchStarSystem(configuration.SquadronSystem.Trim(), true);

                //Ignore null & empty systems
                if (system != null && system?.bodies.Count > 0)
                {
                    if (refresh || system.name != SquadronStarSystem?.name)
                    {
                        SquadronStarSystem = system;
                        if (SquadronStarSystem?.factions != null)
                        {
                            Logging.Debug("Squadron star system is " + SquadronStarSystem.name);
                            configuration.SquadronSystem = system.name;
                            configuration.validSquadronSystem = SquadronStarSystem.factions.Count() > 0;
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

        public void updateSquadronData(List<Faction> factions)
        {
            // Check if current system is inhabited by or HQ for squadron faction
            Faction faction = factions.FirstOrDefault(f => f.squadronhomesystem || f.squadronfaction);
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
                if (faction.squadronhomesystem)
                {
                    // Update the squadron system data, if changed
                    string system = CurrentStarSystem.name;
                    if (configuration.SquadronSystem == null || configuration.SquadronSystem != system)
                    {
                        configuration.SquadronSystem = system;

                        Instance.MainWindow?.Dispatcher?.Invoke(new Action(() =>
                        {
                            Instance.MainWindow.eddiSquadronSystemText.Text = system;
                            Instance.MainWindow.ConfigureSquadronFactionOptions(configuration);
                        }));

                        configuration = updateSquadronSystem(configuration, true);
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
