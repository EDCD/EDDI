using EddiCompanionAppService;
using EddiDataDefinitions;
using EddiDataProviderService;
using EddiEvents;
using EddiSpeechService;
using EddiStarMapService;
using Exceptionless;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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

        private static bool started;

        private static bool running = true;

        public bool inCQC { get; private set; } = false;

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

        public List<EDDIMonitor> monitors = new List<EDDIMonitor>();
        // Each monitor runs in its own thread
        private List<Thread> monitorThreads = new List<Thread>();

        public List<EDDIResponder> responders = new List<EDDIResponder>();
        private List<EDDIResponder> activeResponders = new List<EDDIResponder>();

        // Information obtained from the companion app service
        public Commander Cmdr { get; private set; }
        public Ship Ship { get; private set; }
        public List<Ship> Shipyard { get; private set; }
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

        // Information from the remote server
        public InstanceInfo Server { get; private set; }

        // Session state
        public Dictionary<string, object> State = new Dictionary<string, object>();

        private EDDI()
        {
            try
            {
                Logging.Info(Constants.EDDI_NAME + " " + Constants.EDDI_VERSION + " starting");

                // Exception handling
                ExceptionlessClient.Default.Startup("vJW9HtWB2NHiQb7AwVQsBQM6hjWN1sKzHf5PCpW1");
                ExceptionlessClient.Default.Configuration.SetVersion(Constants.EDDI_VERSION);

                // Start by fetching information from the update server, and handling appropriately
                Server = UpdateServer();

                // Ensure that our primary data structures have something in them.  This allows them to be updated from any source
                Cmdr = new Commander();
                Ship = new Ship();
                Shipyard = new List<Ship>();

                // Set up the EDDI configuration
                EDDIConfiguration configuration = EDDIConfiguration.FromFile();
                Logging.Verbose = configuration.Debug;
                if (configuration.HomeSystem != null && configuration.HomeSystem.Trim().Length > 0)
                {
                    HomeStarSystem = StarSystemSqLiteRepository.Instance.GetOrCreateStarSystem(configuration.HomeSystem.Trim());
                    if (HomeStarSystem != null)
                    {
                        Logging.Debug("Home star system is " + HomeStarSystem.name);
                        if (configuration.HomeStation != null && configuration.HomeStation.Trim().Length > 0)
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
                    }
                }

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

                // Set up monitors and responders
                monitors = findMonitors();
                responders = findResponders();

                Logging.Info(Constants.EDDI_NAME + " " + Constants.EDDI_VERSION + " initialised");
            }
            catch (Exception ex)
            {
                Logging.Error("Failed to initialise", ex);
            }
        }

        /// <summary>
        /// Obtain and check the information from the update server
        /// </summary>
        private static InstanceInfo UpdateServer()
        {
            try
            {
                ServerInfo updateServerInfo = ServerInfo.FromServer("http://api.eddp.co/");
                if (updateServerInfo == null)
                {
                    Logging.Warn("Failed to contact update server");
                }
                else
                {
                    InstanceInfo info = Constants.EDDI_VERSION.Contains("b") ? updateServerInfo.beta : updateServerInfo.production;
                    if (Versioning.Compare(info.minversion, Constants.EDDI_VERSION) == -1)
                    {
                        // We are too old to run
                        Logging.Info("EDDI requires an update.  Please download the latest version at " + info.url);
                        SpeechService.Instance.Say(null, "EDDI requires an update.", true);
                        System.Environment.Exit(1);
                    }

                    if (Versioning.Compare(info.version, Constants.EDDI_VERSION) == 1)
                    {
                        // There is an update available
                        SpeechService.Instance.Say(null, "EDDI version " + info.version.Replace(".", " point ") + " is now available.", false);
                    }

                    if (info.motd != null)
                    {
                        // There is a message
                        SpeechService.Instance.Say(null, info.motd, false);
                    }
                    return info;
                }
            }
            catch (Exception ex)
            {
                SpeechService.Instance.Say(null, "There was a problem connecting to external data services; some features may be temporarily unavailable", false);
                Logging.Warn("Failed to access api.eddp.co", ex);
            }
            return null;
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
                        Thread monitorThread = new Thread(() => keepAlive(monitor.MonitorName(), monitor.Start));
                        monitorThread.IsBackground = true;
                        Logging.Info("Starting keepalive for " + monitor.MonitorName());
                        monitorThread.Start();
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

            Logging.Info(Constants.EDDI_NAME + " " + Constants.EDDI_VERSION + " stopped");
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

        public void eventHandler(Event journalEvent)
        {
            if (journalEvent != null)
            {
                try
                {
                    Logging.Debug("Handling event " + JsonConvert.SerializeObject(journalEvent));
                    // We have some additional processing to do for a number of events
                    bool passEvent = true;
                    if (journalEvent is JumpingEvent)
                    {
                        passEvent = eventJumping((JumpingEvent)journalEvent);
                    }
                    else if (journalEvent is JumpedEvent)
                    {
                        passEvent = eventJumped((JumpedEvent)journalEvent);
                    }
                    else if (journalEvent is DockedEvent)
                    {
                        passEvent = eventDocked((DockedEvent)journalEvent);
                    }
                    else if (journalEvent is UndockedEvent)
                    {
                        passEvent = eventUndocked((UndockedEvent)journalEvent);
                    }
                    else if (journalEvent is LocationEvent)
                    {
                        passEvent = eventLocation((LocationEvent)journalEvent);
                    }
                    else if (journalEvent is EnteredSupercruiseEvent)
                    {
                        passEvent = eventEnteredSupercruise((EnteredSupercruiseEvent)journalEvent);
                    }
                    else if (journalEvent is EnteredNormalSpaceEvent)
                    {
                        passEvent = eventEnteredNormalSpace((EnteredNormalSpaceEvent)journalEvent);
                    }
                    else if (journalEvent is ShipDeliveredEvent)
                    {
                        passEvent = eventShipDeliveredEvent((ShipDeliveredEvent)journalEvent);
                    }
                    else if (journalEvent is ShipSwappedEvent)
                    {
                        passEvent = eventShipSwappedEvent((ShipSwappedEvent)journalEvent);
                    }
                    else if (journalEvent is ShipSoldEvent)
                    {
                        passEvent = eventShipSoldEvent((ShipSoldEvent)journalEvent);
                    }
                    else if (journalEvent is CommanderContinuedEvent)
                    {
                        passEvent = eventCommanderContinuedEvent((CommanderContinuedEvent)journalEvent);
                    }
                    else if (journalEvent is CombatPromotionEvent)
                    {
                        passEvent = eventCombatPromotionEvent((CombatPromotionEvent)journalEvent);
                    }
                    else if (journalEvent is EnteredCQCEvent)
                    {
                        passEvent = eventEnteredCQCEvent((EnteredCQCEvent)journalEvent);
                    }
                    // Additional processing is over, send to the event responders if required
                    if (passEvent)
                    {
                        OnEvent(journalEvent);
                    }
                }
                catch (Exception ex)
                {
                    Logging.Error("Failed to handle event " + JsonConvert.SerializeObject(journalEvent), ex);
                }
            }
        }

        private void OnEvent(Event @event)
        {
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
        }

        private bool eventLocation(LocationEvent theEvent)
        {
            updateCurrentSystem(theEvent.system);
            // Always update the current system with the current co-ordinates, just in case things have changed
            CurrentStarSystem.x = theEvent.x;
            CurrentStarSystem.y = theEvent.y;
            CurrentStarSystem.z = theEvent.z;

            if (theEvent.docked == true)
            {
                // In this case body === station

                if (CurrentStation != null && CurrentStation.name == theEvent.body)
                {
                    // We are already at this station; nothing to do
                    Logging.Debug("Already at station " + theEvent.body);
                    return false;
                }
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

                // Now call refreshProfile() to obtain the outfitting and commodity information
                refreshProfile();
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

            CurrentStation = station;

            // Now call refreshProfile() to obtain the outfitting and commodity information
            refreshProfile();

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
                LastStarSystem = CurrentStarSystem;
                CurrentStarSystem = StarSystemSqLiteRepository.Instance.GetOrCreateStarSystem(name);
                setSystemDistanceFromHome(CurrentStarSystem);
            }
        }

        private bool eventJumping(JumpingEvent theEvent)
        {
            bool passEvent;
            Logging.Debug("Jumping to " + theEvent.system);
            if (CurrentStarSystem == null || CurrentStarSystem.name != theEvent.system)
            {
                // New system
                passEvent = true;
                updateCurrentSystem(theEvent.system);
                // The information in the event is more up-to-date than the information we obtain from external sources, so update it here
                CurrentStarSystem.x = theEvent.x;
                CurrentStarSystem.y = theEvent.y;
                CurrentStarSystem.z = theEvent.z;
                CurrentStarSystem.visits++;
                CurrentStarSystem.lastvisit = DateTime.Now;
                StarSystemSqLiteRepository.Instance.SaveStarSystem(CurrentStarSystem);
                setCommanderTitle();
            }
            else
            {
                // Restatement of current system
                passEvent = false;
            }

            // Whilst jumping we are in witch space
            Environment = Constants.ENVIRONMENT_WITCH_SPACE;

            return passEvent;
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
                CurrentStarSystem.allegiance = theEvent.allegiance;
                CurrentStarSystem.faction = theEvent.faction;
                CurrentStarSystem.primaryeconomy = theEvent.economy;
                CurrentStarSystem.government = theEvent.government;
                CurrentStarSystem.security = theEvent.security;
                CurrentStarSystem.updatedat = (long)theEvent.timestamp.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;

                CurrentStarSystem.visits++;
                CurrentStarSystem.lastvisit = DateTime.Now;
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
                CurrentStarSystem.allegiance = theEvent.allegiance;
                CurrentStarSystem.faction = theEvent.faction;
                CurrentStarSystem.primaryeconomy = theEvent.economy;
                CurrentStarSystem.government = theEvent.government;
                CurrentStarSystem.security = theEvent.security;

                CurrentStarSystem.visits++;
                CurrentStarSystem.lastvisit = DateTime.Now;
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
            if (Environment == null || Environment != Constants.ENVIRONMENT_SUPERCRUISE)
            {
                Environment = Constants.ENVIRONMENT_SUPERCRUISE;
                updateCurrentSystem(theEvent.system);
                return true;
            }
            return false;
        }

        private bool eventEnteredNormalSpace(EnteredNormalSpaceEvent theEvent)
        {
            if (Environment == null || Environment != Constants.ENVIRONMENT_NORMAL_SPACE)
            {
                Environment = Constants.ENVIRONMENT_NORMAL_SPACE;
                updateCurrentSystem(theEvent.system);
                return true;
            }
            return false;
        }

        private bool eventShipDeliveredEvent(ShipDeliveredEvent theEvent)
        {
            refreshProfile();
            SetShip(theEvent.Ship);
            return true;
        }

        private bool eventShipSwappedEvent(ShipSwappedEvent theEvent)
        {
            SetShip(theEvent.Ship);
            return true;
        }

        private bool eventShipSoldEvent(ShipSoldEvent theEvent)
        {
            // Need to update shipyard
            refreshProfile();
            return true;
        }

        private bool eventCommanderContinuedEvent(CommanderContinuedEvent theEvent)
        {
            // If we see this it means that we aren't in CQC
            inCQC = false;

            SetShip(theEvent.Ship);

            if (Cmdr.name == null)
            {
                Cmdr.name = theEvent.commander;
            }

            return true;
        }

        private bool eventCombatPromotionEvent(CombatPromotionEvent theEvent)
        {
            // There is a bug with the journal where it reports superpower increases in rank as combat increases
            // Hence we check to see if this is a real event by comparing our known combat rating to the promoted rating

            return theEvent.rating != Cmdr.combatrating.name;
        }

        private bool eventEnteredCQCEvent(EnteredCQCEvent theEvent)
        {
            // In CQC we don't want to report anything, so set our CQC flag
            inCQC = true;
            return true;
        }

        /// <summary>Obtain information from the companion API and use it to refresh our own data</summary>
        public void refreshProfile()
        {
            if (CompanionAppService.Instance != null && CompanionAppService.Instance.CurrentState == CompanionAppService.State.READY)
            {
                try
                {
                    long profileTime = (long)DateTime.Now.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
                    Profile profile = CompanionAppService.Instance.Profile();
                    if (profile != null)
                    {
                        // Use the profile as primary information for our commander and shipyard
                        Cmdr = profile.Cmdr;
                        Shipyard = profile.Shipyard;

                        // Only use the ship information if we agree that this is the correct ship to use
                        if (Ship.model == null || profile.Ship.LocalId == Ship.LocalId)
                        {
                            SetShip(profile.Ship);
                        }

                        bool updatedCurrentStarSystem = false;

                        // Only set the current star system if it is not present, otherwise we leave it to events
                        if (CurrentStarSystem == null)
                        {
                            CurrentStarSystem = profile == null ? null : profile.CurrentStarSystem;
                            setSystemDistanceFromHome(CurrentStarSystem);

                            // We don't know if we are docked or not at this point.  Fill in the data if we can, and
                            // let later systems worry about removing it if it's decided that we aren't docked
                            if (profile.LastStation.systemname == CurrentStarSystem.name)
                            {
                                CurrentStation = CurrentStarSystem.stations.FirstOrDefault(s => s.name == profile.LastStation.name);
                                CurrentStation.updatedat = profileTime;
                                updatedCurrentStarSystem = true;
                            }
                        }
                        else
                        {
                            if (CurrentStation != null && CurrentStation.systemname == profile.LastStation.systemname && CurrentStation.name == profile.LastStation.name)
                            {
                                // Match for our expected station with the information returned from the profile
                                Logging.Debug("Current station matches profile information; updating info");

                                // Update the outfitting, commodities and shipyard with the data obtained from the profile
                                CurrentStation.outfitting = profile.LastStation.outfitting;
                                CurrentStation.updatedat = profileTime;
                                CurrentStation.commodities = profile.LastStation.commodities;
                                CurrentStation.commoditiesupdatedat = profileTime;
                                CurrentStation.shipyard = profile.LastStation.shipyard;
                                updatedCurrentStarSystem = true;
                            }
                            else
                            {
                                Logging.Debug("Current station does not match profile information; ignoring");
                            }
                        }

                        setCommanderTitle();

                        if (updatedCurrentStarSystem)
                        {
                            Logging.Debug("Star system information updated from remote server; updating local copy");
                            StarSystemSqLiteRepository.Instance.SaveStarSystem(CurrentStarSystem);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logging.Error("Exception obtaining profile", ex);
                }
            }
        }

        private void SetShip(Ship ship)
        {
            if (ship == null)
            {
                Logging.Warn("Refusing to set ship to null");
            }
            else
            {
                Logging.Debug("Set ship to " + JsonConvert.SerializeObject(ship));
                Ship = ship;
            }
        }

        private void setSystemDistanceFromHome(StarSystem system)
        {
            Logging.Info("HomeStarSystem is " + (HomeStarSystem == null ? null : HomeStarSystem.name));
            if (HomeStarSystem != null && HomeStarSystem.x != null && system.x != null)
            {
                system.distancefromhome = (decimal)Math.Round(Math.Sqrt(Math.Pow((double)(system.x - HomeStarSystem.x), 2)
                                                                      + Math.Pow((double)(system.y - HomeStarSystem.y), 2)
                                                                      + Math.Pow((double)(system.z - HomeStarSystem.z), 2)), 2);
                Logging.Info("Distance from home is " + system.distancefromhome);
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
                    string msg = "Failed to load monitor.  Please ensure that " + dir.FullName + " is not on a network share, or itself shared";
                    Logging.Error(msg, flex);
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
    }
}
