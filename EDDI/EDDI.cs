using EliteDangerousCompanionAppService;
using EliteDangerousDataDefinitions;
using EliteDangerousDataProviderService;
using EliteDangerousEvents;
using EliteDangerousJournalMonitor;
using EliteDangerousNetLogMonitor;
using EliteDangerousStarMapService;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Utilities;

namespace EDDI
{
    // Notifications delegate
    public delegate void OnEventHandler(Event theEvent);

    /// <summary>
    /// Eddi is the controller for all EDDI operations.  Its job is to retain the state of the objects such as the commander, the current system, etc.
    /// and keep them up-to-date with changes that occur.  It also passes on messages to responders to handle as required.
    /// </summary>
    public class Eddi
    {
        public static readonly string EDDI_VERSION = "2.0.0b1";

        private static Eddi instance;

        private static readonly object instanceLock = new object();
        public static Eddi Instance
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
                            instance = new Eddi();
                        }
                    }
                }
                return instance;
            }
        }

        public event OnEventHandler EventHandler;
        protected virtual void OnEvent(Event theEvent)
        {
            EventHandler?.Invoke(theEvent);
        }

        private List<EDDIResponder> responders = new List<EDDIResponder>();

        private CompanionAppService appService;

        // Information obtained from the companion app service
        public Commander Cmdr { get; private set; }
        public Ship Ship { get; private set; }
        public List<Ship> StoredShips { get; private set; }
        public Station LastStation { get; private set; }
        public List<Module> Outfitting { get; private set; }

        // Services made available from EDDI
        public StarMapService starMapService { get; private set; }

        // Information obtained from the configuration
        public StarSystem HomeStarSystem { get; private set; }
        public Station HomeStation { get; private set; }
        public decimal? Insurance { get; private set; }

        // Information obtained from the log watcher
        public string Environment { get; private set; }
        public StarSystem CurrentStarSystem { get; private set; }
        public StarSystem LastStarSystem { get; private set; }

        private Thread logWatcherThread;
        private Thread journalWatcherThread;

        public EDDIConfiguration configuration { get; private set; }

        public static readonly string ENVIRONMENT_SUPERCRUISE = "Supercruise";
        public static readonly string ENVIRONMENT_NORMAL_SPACE = "Normal space";

        private Eddi()
        {
            try
            {
                Logging.Info("EDDI " + EDDI_VERSION + " starting");

                // Set up and/or open our database
                String dataDir = System.Environment.GetEnvironmentVariable("AppData") + "\\EDDI";
                System.IO.Directory.CreateDirectory(dataDir);

                // Set up the EDDI configuration
                configuration = EDDIConfiguration.FromFile();
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
                                if (station.Name == homeStationName)
                                {
                                    HomeStation = station;
                                    Logging.Debug("Home station is " + HomeStation.Name);
                                    break;

                                }
                            }
                        }
                    }
                }
                Insurance = configuration.Insurance;

                // Set up the app service
                appService = new CompanionAppService();
                if (appService.CurrentState == CompanionAppService.State.READY)
                {
                    // Carry out initial population of profile
                    refreshProfile();
                }
                if (Cmdr != null && Cmdr.name != null)
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
                    else if (Cmdr.name != null)
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
                Environment = ENVIRONMENT_NORMAL_SPACE;

                // Set up log monitor
                NetLogConfiguration netLogConfiguration = NetLogConfiguration.FromFile();
                if (netLogConfiguration != null && netLogConfiguration.path != null)
                {
                    logWatcherThread = new Thread(() => StartLogMonitor(netLogConfiguration));
                    logWatcherThread.IsBackground = true;
                    logWatcherThread.Name = "EDDI netlog watcher";
                    logWatcherThread.Start();
                    Logging.Info("EDDI netlog monitor is enabled for " + netLogConfiguration.path);

                    journalWatcherThread = new Thread(() => StartJournalMonitor(netLogConfiguration));
                    journalWatcherThread.IsBackground = true;
                    journalWatcherThread.Name = "EDDI journal watcher";
                    journalWatcherThread.Start();
                    Logging.Info("EDDI journal monitor is enabled");
                }
                else
                {
                    Logging.Info("EDDI netlog monitor is disabled");
                }

                Logging.Info("EDDI " + EDDI_VERSION + " initialised");
            }
            catch (Exception ex)
            {
                Logging.Error("Failed to initialise: " + ex);
            }
        }

        private void StartLogMonitor(NetLogConfiguration configuration)
        {
            if (configuration != null)
            {
                NetLogMonitor netLogMonitor = new NetLogMonitor(configuration, (result) => eventHandler(result));
                netLogMonitor.start();
            }
        }

        private void StartJournalMonitor(NetLogConfiguration configuration)
        {
            if (configuration != null)
            {
                JournalMonitor journalMonitor = new JournalMonitor(configuration, (result) => eventHandler(result));
                journalMonitor.start();
            }
        }

        public void Start()
        {
            responders.Add(new SpeechResponder());
            responders.Add(new EDSMResponder());

            foreach (EDDIResponder responder in responders)
            {
                EventHandler += new OnEventHandler(responder.Handle);
                responder.Start();
            }
        }

        public void Stop()
        {
            foreach (EDDIResponder responder in responders)
            {
                responder.Stop();
            }
            if (logWatcherThread != null)
            {
                logWatcherThread.Abort();
                logWatcherThread = null;
            }
            if (journalWatcherThread != null)
            {
                journalWatcherThread.Abort();
                journalWatcherThread = null;
            }

            Logging.Info("EDDI " + EDDI_VERSION + " shutting down");
        }

        /// <summary>
        /// Add a responder to the list of active responders.  This starts the responder.
        /// </summary>
        /// <param name="responder"></param>
        public void AddResponder(EDDIResponder responder)
        {
            responders.Add(responder);
            EventHandler += new OnEventHandler(responder.Handle);
            responder.Start();
        }

        void eventHandler(Event journalEvent)
        {
            Logging.Debug("Handling event " + JsonConvert.SerializeObject(journalEvent));
            // We have some additional processing to do for a number of events
            if (journalEvent is JumpedEvent)
            {
                eventJumped((JumpedEvent)journalEvent);
            }
            else if (journalEvent is DockedEvent)
            {
                eventDocked((DockedEvent)journalEvent);
            }
            else if (journalEvent is UndockedEvent)
            {
                eventUndocked((UndockedEvent)journalEvent);
            }
            else if (journalEvent is EnteredSupercruiseEvent)
            {
                eventEnteredSupercruise((EnteredSupercruiseEvent)journalEvent);
            }
            else if (journalEvent is EnteredNormalSpaceEvent)
            {
                eventEnteredNormalSpace((EnteredNormalSpaceEvent)journalEvent);
            }
            // Additional processing is over, send to the event responders
            OnEvent(journalEvent);
        }

        void eventDocked(DockedEvent theEvent)
        {
        }

        void eventUndocked(UndockedEvent theEvent)
        {
        }

        void eventJumped(JumpedEvent theEvent)
        {
            Logging.Debug("Jumped to " + theEvent.system);
            if (CurrentStarSystem == null || CurrentStarSystem.name != theEvent.system)
            {
                LastStarSystem = CurrentStarSystem;
                CurrentStarSystem = StarSystemSqLiteRepository.Instance.GetOrCreateStarSystem(theEvent.system);
                Logging.Error("***********************************************1 - " + CurrentStarSystem);
                if (CurrentStarSystem.x == null)
                {
                    // Star system is missing co-ordinates to take them from the event
                    CurrentStarSystem.x = theEvent.x;
                    CurrentStarSystem.y = theEvent.y;
                    CurrentStarSystem.z = theEvent.z;
                }
                CurrentStarSystem.visits++;
                Logging.Error("***********************************************2 - " + CurrentStarSystem.visits);
                CurrentStarSystem.lastvisit = DateTime.Now;
                StarSystemSqLiteRepository.Instance.SaveStarSystem(CurrentStarSystem);
                Logging.Debug("Number of visits to this system is now " + CurrentStarSystem.visits);
                // After jump we are always in supercruise
                Environment = ENVIRONMENT_SUPERCRUISE;
                setCommanderTitle();
            }
        }

        void eventEnteredSupercruise(EnteredSupercruiseEvent theEvent)
        {
            Environment = ENVIRONMENT_SUPERCRUISE;
        }

        void eventEnteredNormalSpace(EnteredNormalSpaceEvent theEvent)
        {
            Environment = ENVIRONMENT_SUPERCRUISE;
        }

        /// <summary>Obtain information from the copmanion API and use it to refresh our own data</summary>
        public void refreshProfile()
        {
            if (appService != null)
            {
                Profile profile = appService.Profile();
                Cmdr = profile == null ? null : profile.Cmdr;
                Ship = profile == null ? null : profile.Ship;
                StoredShips = profile == null ? null : profile.StoredShips;
                CurrentStarSystem = profile == null ? null : profile.CurrentStarSystem;
                setCommanderTitle();
                // TODO last station string to station
                //LastStation = profile.LastStation;
                Outfitting = profile == null ? null : profile.Outfitting;
            }
        }

        /// <summary>Work out the title for the commander in the current system</summary>
        private static int minEmpireRatingForTitle = 3;
        private static int minFederationRatingForTitle = 1;
        private void setCommanderTitle()
        {
            if (Cmdr != null)
            {
                Cmdr.title = "Commander";
                if (CurrentStarSystem != null)
                {
                    if (CurrentStarSystem.allegiance == "Federation" && Cmdr.federationrating > minFederationRatingForTitle)
                    {
                        Cmdr.title = Cmdr.federationrank;
                    }
                    else if (CurrentStarSystem.allegiance == "Empire" && Cmdr.empirerating > minEmpireRatingForTitle)
                    {
                        Cmdr.title = Cmdr.empirerank;
                    }
                }
            }
        }
    }
}
