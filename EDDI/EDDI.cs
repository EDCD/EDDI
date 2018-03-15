using EddiCompanionAppService;
using EddiDataDefinitions;
using EddiDataProviderService;
using EddiEvents;
using EddiSpeechService;
using EddiStarMapService;
using Exceptionless;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Text;
using System.Threading;
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

        private static bool running = true;

        public bool inCQC { get; private set; } = false;

        public bool inCrew { get; private set; } = false;

        public bool inBeta { get; private set; } = false;

        static EDDI()
        {
            // Set up our app directory
            Directory.CreateDirectory(Constants.DATA_DIR);

            // Use invariant culture to ensure that we use . rather than , for our separator when writing out decimals
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
            CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;
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
        // Each monitor runs in its own thread
        private List<Thread> monitorThreads = new List<Thread>();

        public List<EDDIResponder> responders = new List<EDDIResponder>();
        private List<EDDIResponder> activeResponders = new List<EDDIResponder>();

        // Information obtained from the companion app service
        public Commander Cmdr { get; private set; }
        public DateTime ApiTimeStamp { get; private set; }
        //public ObservableCollection<Ship> Shipyard { get; private set; } = new ObservableCollection<Ship>();
        public Station CurrentStation { get; private set; }

        // Services made available from EDDI
        public StarMapService starMapService { get; private set; }

        // Information obtained from the configuration
        public StarSystem HomeStarSystem { get; private set; }
        public Station HomeStation { get; private set; }

        // Information obtained from the log watcher
        public string Environment { get; private set; }
        public StarSystem CurrentStarSystem { get; private set; }
        public StarSystem LastStarSystem { get; private set; }

        // Information obtained from the player journal
        public DateTime JournalTimeStamp { get; set; } = DateTime.MinValue;

        // Current vehicle of player
        public string Vehicle { get; private set; } = Constants.VEHICLE_SHIP;
        public Ship CurrentShip { get; set; }

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

                // Exception handling
                ExceptionlessClient.Default.Startup("vJW9HtWB2NHiQb7AwVQsBQM6hjWN1sKzHf5PCpW1");
                ExceptionlessClient.Default.Configuration.SetVersion(Constants.EDDI_VERSION);

                // Start by fetching information from the update server, and handling appropriately
                CheckUpgrade();
                if (UpgradeRequired)
                {
                    // We are too old to continue; don't
                    running = false;
                    return;
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

                // Set up monitors and responders
                monitors = findMonitors();
                responders = findResponders();

                // Set up the app service
                if (CompanionAppService.Instance.CurrentState == CompanionAppService.State.READY)
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

                Cmdr.insurance = configuration.Insurance;
                Cmdr.gender = configuration.Gender;
                if (Cmdr.name != null)
                {
                    Logging.Info("EDDI access to the companion app is enabled");
                }
                else
                {
                    // If InvokeUpdatePlugin failed then it will have have left an error message, but this once we ignore it
                    Logging.Info("EDDI access to the companion app is disabled");
                }

                // Set up the star map service
                StarMapConfiguration starMapCredentials = StarMapConfiguration.FromFile();
                if (starMapCredentials != null && starMapCredentials.apiKey != null)
                {
                    // Commander name might come from star map credentials or the companion app's profile
                    string commanderName = null;
                    if (starMapCredentials.commanderName != null)
                    {
                        commanderName = starMapCredentials.commanderName;
                    }
                    else if (Cmdr != null && Cmdr.name != null)
                    {
                        commanderName = Cmdr.name;
                    }
                    if (commanderName != null)
                    {
                        starMapService = new StarMapService(starMapCredentials.apiKey, commanderName);
                        Logging.Info("EDDI access to EDSM is enabled");
                    }

                }
                if (starMapService == null)
                {
                    Logging.Info("EDDI access to EDSM is disabled");
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
                    Motd = info.motd;
                    if (updateServerInfo.productionbuilds != null)
                    {
                        ProductionBuilds = updateServerInfo.productionbuilds;
                    }

                    if (Versioning.Compare(info.minversion, Constants.EDDI_VERSION) == 1)
                    {
                        // There is a mandatory update available
                        if (!FromVA)
                        {
                            SpeechService.Instance.Say(null, "Mandatory Eddi upgrade to " + info.version.Replace(".", " point ") + " is required.", false);
                        }
                        UpgradeRequired = true;
                        UpgradeLocation = info.url;
                        UpgradeVersion = info.version;
                        return;
                    }

                    if (Versioning.Compare(info.version, Constants.EDDI_VERSION) == 1)
                    {
                        // There is an update available
                        if (!FromVA)
                        {
                            SpeechService.Instance.Say(null, "Eddi version " + info.version.Replace(".", " point ") + " is now available.", false);
                        }
                        UpgradeAvailable = true;
                        UpgradeLocation = info.url;
                        UpgradeVersion = info.version;
                    }
                }
            }
            catch (Exception ex)
            {
                SpeechService.Instance.Say(null, "I could not connect to my update server. If this recurs, please check the forum to see if it is a known problem.", false);
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
                    SpeechService.Instance.Say(null, "Downloading upgrade.", true);
                    string updateFile = Net.DownloadFile(UpgradeLocation, @"EDDI-update.exe");
                    if (updateFile == null)
                    {
                        SpeechService.Instance.Say(null, "Download failed.  Please try again later.", true);
                    }
                    else
                    {
                        // Inno setup will attempt to restart this application so register it
                        NativeMethods.RegisterApplicationRestart(null, RestartFlags.NONE);

                        Logging.Info("Downloaded update to " + updateFile);
                        Logging.Info("Path is " + Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
                        File.SetAttributes(updateFile, FileAttributes.Normal);
                        SpeechService.Instance.Say(null, "Starting upgrade.", true);
                        Logging.Info("Starting upgrade.");

                        Process.Start(updateFile, @"/closeapplications /restartapplications /silent /log /nocancel /noicon /dir=""" + Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"""");
                    }
                }
            }
            catch (Exception ex)
            {
                SpeechService.Instance.Say(null, "Upgrade failed.  Please try again later.", true);
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
                    bool enabled;
                    if (!configuration.Plugins.TryGetValue(monitor.MonitorName(), out enabled))
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
                    }
                }

                foreach (EDDIResponder responder in responders)
                {
                    bool enabled;
                    if (!configuration.Plugins.TryGetValue(responder.ResponderName(), out enabled))
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
                }
            }

            Logging.Info(Constants.EDDI_NAME + " " + Constants.EDDI_VERSION + " stopped");

            ExceptionlessClient.Default.Shutdown();

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
        public EDDIMonitor ObtainMonitor(string name)
        {
            foreach (EDDIMonitor monitor in monitors)
            {
                if (monitor.MonitorName() == name)
                {
                    return monitor;
                }
            }
            return null;
        }

        /// <summary>
        /// Obtain a named responder
        /// </summary>
        public EDDIResponder ObtainResponder(string name)
        {
            foreach (EDDIResponder responder in responders)
            {
                if (responder.ResponderName() == name)
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

            Logging.Info(Constants.EDDI_NAME + " " + Constants.EDDI_VERSION + " stopped");
        }

        /// <summary>
        /// Keep a thread alive, restarting it as required
        /// </summary>
        private void keepAlive(string name, Action start)
        {
            try
            {
                int failureCount = 0;
                while (running && failureCount < 5)
                {
                    try
                    {
                        Thread monitorThread = new Thread(() => start());
                        monitorThread.Name = name;
                        monitorThread.IsBackground = true;
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
                    // Additional processing is over, send to the event responders if required
                    if (passEvent)
                    {
                        OnEvent(@event);
                    }
                }
                catch (Exception ex)
                {
                    Logging.Error("Failed to handle event " + JsonConvert.SerializeObject(@event), ex);
                }
            }
        }

        private void OnEvent(Event @event)
        {
            // We send the event to all monitors to ensure that their info is up-to-date
            // This is synchronous
            foreach (EDDIMonitor monitor in monitors)
            {
                try
                {
                    monitor.PreHandle(@event);

                }
                catch (Exception ex)
                {
                    Logging.Error(JsonConvert.SerializeObject(@event), ex);
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
                            Logging.Warn("Responder failed", ex);
                        }
                    });
                    responderThread.Name = responder.ResponderName();
                    responderThread.IsBackground = true;
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

            // We also pass the event to all monitors in case they have follow-on work
            foreach (EDDIMonitor monitor in monitors)
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
            updateCurrentSystem(theEvent.system);
            // Always update the current system with the current co-ordinates, just in case things have changed
            CurrentStarSystem.x = theEvent.x;
            CurrentStarSystem.y = theEvent.y;
            CurrentStarSystem.z = theEvent.z;
            setSystemDistanceFromHome(CurrentStarSystem);

            // Update the system population from the journal
            if (theEvent.population != null)
            {
                CurrentStarSystem.population = theEvent.population;
            }

            if (theEvent.docked == true)
            {
                // In this case body === station

                // Force first location update even if it matches with 'firstLocation' bool
                if (!firstLocation && (CurrentStation != null && CurrentStation.name == theEvent.body))
                {
                    // We are already at this station; nothing to do
                    Logging.Debug("Already at station " + theEvent.body);
                    return false;
                }
                firstLocation = false;

                // Update the station
                Logging.Debug("Now at station " + theEvent.body);
                Station station = CurrentStarSystem.stations.Find(s => s.name == theEvent.body);
                if (station == null)
                {
                    // This station is unknown to us, might not be in EDDB or we might not have connectivity.  Use a placeholder
                    station = new Station();
                    station.name = theEvent.body;
                    station.systemname = theEvent.system;
                }

                // Information from the event might be more current than that from EDDB so use it in preference
                station.faction = theEvent.faction;
                station.government = theEvent.government;
                station.allegiance = theEvent.allegiance;

                CurrentStation = station;

                // Kick off the profile refresh if the companion API is available
                if (CompanionAppService.Instance.CurrentState == CompanionAppService.State.READY)
                {
                    // Refresh station data
                    profileUpdateNeeded = true;
                    profileStationRequired = CurrentStation.name;
                    Thread updateThread = new Thread(() => conditionallyRefreshProfile());
                    updateThread.IsBackground = true;
                    updateThread.Start();
                }
            }
            else
            {
                // If we are not docked then our station information is invalid
                CurrentStation = null;
            }

            return true;
        }

        private bool eventDocked(DockedEvent theEvent)
        {
            updateCurrentSystem(theEvent.system);

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
                // This station is unknown to us, might not be in EDDB or we might not have connectivity.  Use a placeholder
                station = new Station();
                station.name = theEvent.station;
                station.systemname = theEvent.system;
            }

            // Information from the event might be more current than that from EDDB so use it in preference
            station.state = theEvent.factionstate;
            station.faction = theEvent.faction;
            station.government = theEvent.government;

            if (theEvent.stationservices != null)
            {
                foreach (var service in theEvent.stationservices)
                {
                    if (service == "Refuel")
                    {
                        station.hasrefuel = true;
                    }
                    else if (service == "Rearm")
                    {
                        station.hasrearm = true;
                    }
                    else if (service == "Repair")
                    {
                        station.hasrepair = true;
                    }
                    else if (service == "Outfitting")
                    {
                        station.hasoutfitting = true;
                    }
                    else if (service == "Shipyard")
                    {
                        station.hasshipyard = true;
                    }
                    else if (service == "Commodities")
                    {
                        station.hasmarket = true;
                    }
                    else if (service == "BlackMarket")
                    {
                        station.hasblackmarket = true;
                    }
                }
            }

            CurrentStation = station;

            // Kick off the profile refresh if the companion API is available
            if (CompanionAppService.Instance.CurrentState == CompanionAppService.State.READY)
            {
                // Refresh station data
                profileUpdateNeeded = true;
                profileStationRequired = CurrentStation.name;
                Thread updateThread = new Thread(() => conditionallyRefreshProfile());
                updateThread.IsBackground = true;
                updateThread.Start();
            }
            else
            {
                // Kick off a dummy that triggers a market refresh after a couple of seconds
                Thread updateThread = new Thread(() => dummyRefreshMarketData());
                updateThread.IsBackground = true;
                updateThread.Start();
            }

            return true;
        }

        private bool eventUndocked(UndockedEvent theEvent)
        {
            // Call refreshProfile() to ensure that our ship is up-to-date
            refreshProfile();

            // Remove information about the station
            CurrentStation = null;

            return true;
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
            Logging.Debug("Jumped to " + theEvent.system);
            if (CurrentStarSystem == null || CurrentStarSystem.name != theEvent.system)
            {
                // New system
                passEvent = true;
                updateCurrentSystem(theEvent.system);
                // The information in the event is more up-to-date than the information we obtain from external sources, so update it here
                CurrentStarSystem.x = theEvent.x;
                CurrentStarSystem.y = theEvent.y;
                CurrentStarSystem.z = theEvent.z;
                setSystemDistanceFromHome(CurrentStarSystem);
                CurrentStarSystem.allegiance = theEvent.allegiance;
                CurrentStarSystem.faction = theEvent.faction;
                CurrentStarSystem.primaryeconomy = theEvent.economy;
                CurrentStarSystem.government = theEvent.government;
                CurrentStarSystem.security = theEvent.security;
                CurrentStarSystem.updatedat = (long)theEvent.timestamp.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
                if (theEvent.population != null)
                {
                    CurrentStarSystem.population = theEvent.population;
                }

                CurrentStarSystem.visits++;
                // We don't update lastvisit because we do that when we leave
                StarSystemSqLiteRepository.Instance.SaveStarSystem(CurrentStarSystem);
                setCommanderTitle();
            }
            else if (CurrentStarSystem.name == theEvent.system && Environment == Constants.ENVIRONMENT_SUPERCRUISE)
            {
                // Restatement of current system
                passEvent = false;
            }
            else if (CurrentStarSystem.name == theEvent.system && Environment == Constants.ENVIRONMENT_WITCH_SPACE)
            {
                passEvent = true;

                // Jumped event following a Jumping event, so most information is up-to-date but we should pass this anyway for
                // plugin triggers

                // The information in the event is more up-to-date than the information we obtain from external sources, so update it here
                CurrentStarSystem.allegiance = theEvent.allegiance;
                CurrentStarSystem.faction = theEvent.faction;
                CurrentStarSystem.primaryeconomy = theEvent.economy;
                CurrentStarSystem.government = theEvent.government;
                CurrentStarSystem.security = theEvent.security;
                CurrentStarSystem.updatedat = (long)theEvent.timestamp.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
                StarSystemSqLiteRepository.Instance.SaveStarSystem(CurrentStarSystem);
                setCommanderTitle();
            }
            else
            {
                passEvent = true;
                updateCurrentSystem(theEvent.system);

                // The information in the event is more up-to-date than the information we obtain from external sources, so update it here
                CurrentStarSystem.x = theEvent.x;
                CurrentStarSystem.y = theEvent.y;
                CurrentStarSystem.z = theEvent.z;
                setSystemDistanceFromHome(CurrentStarSystem);
                CurrentStarSystem.allegiance = theEvent.allegiance;
                CurrentStarSystem.faction = theEvent.faction;
                CurrentStarSystem.primaryeconomy = theEvent.economy;
                CurrentStarSystem.government = theEvent.government;
                CurrentStarSystem.security = theEvent.security;

                CurrentStarSystem.visits++;
                // We don't update lastvisit because we do that when we leave
                CurrentStarSystem.updatedat = (long)theEvent.timestamp.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
                StarSystemSqLiteRepository.Instance.SaveStarSystem(CurrentStarSystem);
                setCommanderTitle();
            }

            // After jump has completed we are always in supercruise
            Environment = Constants.ENVIRONMENT_SUPERCRUISE;

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
            Environment = Constants.ENVIRONMENT_NORMAL_SPACE;
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
            if ((Cmdr == null || Cmdr.combatrating == null) || theEvent.rating != Cmdr.combatrating.name)
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
            if (theEvent.status.supercruise == true)
            {
                Environment = Constants.ENVIRONMENT_SUPERCRUISE;
            }
            Vehicle = theEvent.status.vehicle;
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
                    belt = new Body();
                    belt.EDDBID = -1;
                    belt.type = "Star";
                    belt.name = theEvent.name;
                    belt.systemname = CurrentStarSystem?.name;
                    CurrentStarSystem.bodies?.Add(belt);
                }

                // Update with the information we have

                belt.distance = (long?)theEvent.distancefromarrival;

                CurrentStarSystem.bodies?.Add(belt);
                Logging.Debug("Saving data for scanned belt " + theEvent.name);
                StarSystemSqLiteRepository.Instance.SaveStarSystem(CurrentStarSystem);
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
                    star = new Body();
                    star.EDDBID = -1;
                    star.type = "Star";
                    star.name = theEvent.name;
                    star.systemname = CurrentStarSystem?.name;
                    CurrentStarSystem.bodies?.Add(star);
                }

                // Update with the information we have
                star.age = theEvent.age;
                star.distance = (long?)theEvent.distancefromarrival;
                star.luminosityclass = theEvent.luminosityclass;
                star.temperature = (long?)theEvent.temperature;
                star.mainstar = theEvent.distancefromarrival == 0 ? true : false;
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
                    body = new Body();
                    body.EDDBID = -1;
                    body.type = "Planet";
                    body.name = theEvent.name;
                    body.systemname = CurrentStarSystem.name;
                    CurrentStarSystem.bodies.Add(body);
                }

                // Update with the information we have
                body.distance = (long?)theEvent.distancefromarrival;
                body.landable = theEvent.landable;
                body.tidallylocked = theEvent.tidallylocked;
                body.temperature = (long?)theEvent.temperature;
                body.periapsis = theEvent.periapsis;
                body.atmosphere = theEvent.atmosphere;
                body.gravity = theEvent.gravity;
                body.eccentricity = theEvent.eccentricity;
                body.inclination = theEvent.orbitalinclination;
                body.orbitalperiod = Math.Round(theEvent.orbitalperiod / 86400, 2);
                body.rotationalperiod = Math.Round(theEvent.rotationperiod / 86400, 2);
                body.semimajoraxis = theEvent.semimajoraxis;
                body.pressure = theEvent.pressure;
                switch (theEvent.terraformstate)
                {
                    case "terrraformable":
                    case "terraformable":
                        body.terraformstate = "Terraformable";
                        break;
                    case "terraforming":
                        body.terraformstate = "Terraforming";
                        break;
                    case "Terraformed":
                        body.terraformstate = "Terraformed";
                        break;
                    default:
                        body.terraformstate = "Not terraformable";
                        break;
                }
                body.terraformstate = theEvent.terraformstate;
                body.planettype = theEvent.bodyclass;
                body.volcanism = theEvent.volcanism;
                body.materials = new List<MaterialPresence>();
                foreach (MaterialPresence presence in theEvent.materials)
                {
                    body.materials.Add(new MaterialPresence(presence.definition, presence.percentage));
                }
                body.reserves = theEvent.reserves;
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
            if (CompanionAppService.Instance?.CurrentState == CompanionAppService.State.READY)
            {
                try
                {
                    // Save a timestamp when the API refreshes, so that we can compare whether events are more or less recent
                    ApiTimeStamp = DateTime.UtcNow;

                    long profileTime = (long)DateTime.Now.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
                    Profile profile = CompanionAppService.Instance.Profile();
                    if (profile != null)
                    {
                        // Use the profile as primary information for our commander and shipyard
                        Cmdr = profile.Cmdr;

                        // Reinstate information not obtained from the Companion API (insurance & gender settings)
                        EDDIConfiguration configuration = EDDIConfiguration.FromFile();
                        if (configuration != null)
                        {
                            Cmdr.insurance = configuration.Insurance;
                            Cmdr.gender = configuration.Gender;
                        }

                        bool updatedCurrentStarSystem = false;

                        // Only set the current star system if it is not present, otherwise we leave it to events
                        if (CurrentStarSystem == null)
                        {
                            CurrentStarSystem = profile == null ? null : profile.CurrentStarSystem;
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
                            Thread updateThread = new Thread(() => conditionallyRefreshProfile());
                            updateThread.IsBackground = true;
                            updateThread.Start();
                        }

                        setCommanderTitle();

                        if (updatedCurrentStarSystem)
                        {
                            Logging.Debug("Star system information updated from remote server; updating local copy");
                            StarSystemSqLiteRepository.Instance.SaveStarSystem(CurrentStarSystem);
                        }

                        foreach (EDDIMonitor monitor in monitors)
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
                                });
                                monitorThread.Name = monitor.MonitorName();
                                monitorThread.IsBackground = true;
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
        private static int minEmpireRankForTitle = 3;
        private static int minFederationRankForTitle = 1;
        private void setCommanderTitle()
        {
            if (Cmdr != null)
            {
                Cmdr.title = "Commander";
                if (CurrentStarSystem != null)
                {
                    if (CurrentStarSystem.allegiance == "Federation" && Cmdr.federationrating != null && Cmdr.federationrating.rank > minFederationRankForTitle)
                    {
                        Cmdr.title = Cmdr.federationrating.name;
                    }
                    else if (CurrentStarSystem.allegiance == "Empire" && Cmdr.empirerating != null && Cmdr.empirerating.rank > minEmpireRankForTitle)
                    {
                        Cmdr.title = Cmdr.empirerating.name;
                    }
                }
            }
        }

        /// <summary>
        /// Find all monitors
        /// </summary>
        private List<EDDIMonitor> findMonitors()
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
                        FileNotFoundException exFileNotFound = exSub as FileNotFoundException;
                        if (exFileNotFound != null)
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
                    string msg = "Failed to load monitor. Please ensure that " + dir.FullName + " is not on a network share, or itself shared";
                    Logging.Error(msg, flex);
                    SpeechService.Instance.Say(null, msg, false);
                }
                catch (Exception ex)
                {
                    string msg = $"Failed to load monitor: {file.Name}.\n{ex.Message} {ex.InnerException?.Message ?? ""}";
                    Logging.Error(msg, ex);
                    SpeechService.Instance.Say(null, msg, false);
                }
            }
            return monitors;
        }

        /// <summary>
        /// Find all responders
        /// </summary>
        private List<EDDIResponder> findResponders()
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
                        FileNotFoundException exFileNotFound = exSub as FileNotFoundException;
                        if (exFileNotFound != null)
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

            while (running && maxTries > 0 && CompanionAppService.Instance.CurrentState == CompanionAppService.State.READY)
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
                        long profileTime = (long)DateTime.Now.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
                        Logging.Debug("Fetching station profile");
                        Profile profile = CompanionAppService.Instance.Station(CurrentStarSystem.name);

                        // See if it is up-to-date regarding our requirements
                        Logging.Debug("profileStationRequired is " + profileStationRequired + ", profile station is " + profile.LastStation.name);

                        if (profileStationRequired != null && profileStationRequired == profile.LastStation.name)
                        {
                            // We have the required station information
                            Logging.Debug("Current station matches profile information; updating info");
                            CurrentStation.commodities = profile.LastStation.commodities;
                            CurrentStation.economies = profile.LastStation.economies;
                            CurrentStation.prohibited = profile.LastStation.prohibited;
                            CurrentStation.commoditiesupdatedat = profileTime;
                            CurrentStation.outfitting = profile.LastStation.outfitting;
                            CurrentStation.shipyard = profile.LastStation.shipyard;
                            CurrentStation.updatedat = profileTime;

                            // Update the current station information in our backend DB
                            Logging.Debug("Star system information updated from remote server; updating local copy");
                            StarSystemSqLiteRepository.Instance.SaveStarSystem(CurrentStarSystem);

                            // Post an update event
                            Event @event = new MarketInformationUpdatedEvent(DateTime.Now);
                            eventHandler(@event);

                            profileUpdateNeeded = false;
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
            Event @event = new MarketInformationUpdatedEvent(DateTime.Now);
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
            updateHomeSystem(configuration);
            updateHomeStation(configuration);
            configuration.ToFile();
        }

        public EDDIConfiguration updateHomeSystem(EDDIConfiguration configuration)
        {
            Logging.Verbose = configuration.Debug;
            if (configuration.HomeSystem != null && configuration.HomeSystem.Trim().Length > 0)
            {
                HomeStarSystem = StarSystemSqLiteRepository.Instance.GetOrCreateStarSystem(configuration.HomeSystem.Trim());
                if (HomeStarSystem != null)
                {
                    Logging.Debug("Home star system is " + HomeStarSystem.name);
                    configuration.validSystem = HomeStarSystem.bodies.Count > 0;
                }
            }
            return configuration;
        }

        public EDDIConfiguration updateHomeStation(EDDIConfiguration configuration)
        {
            Logging.Verbose = configuration.Debug;
            configuration.validStation = false;
            if (configuration.HomeStation != null && configuration.HomeStation.Trim().Length > 0)
            {
                string homeStationName = configuration.HomeStation.Trim();
                foreach (Station station in HomeStarSystem.stations)
                {
                    if (station.name == homeStationName)
                    {
                        HomeStation = station;
                        Logging.Debug("Home station is " + HomeStation.name);
                        configuration.validStation = true;
                        break;
                    }
                }
            }
            return configuration;
        }
    }
}
