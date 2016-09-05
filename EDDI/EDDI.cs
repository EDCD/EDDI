using EliteDangerousCompanionAppService;
using EliteDangerousDataDefinitions;
using EliteDangerousDataProviderService;
using EliteDangerousJournalMonitor;
using EliteDangerousNetLogMonitor;
using EliteDangerousSpeechService;
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
    public delegate void OnEventHandler(string eventName);

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
                            instance = new Eddi();
                        }
                    }
                }
                return instance;
            }
        }

        public event OnEventHandler EventHandler;
        protected virtual void OnEvent(string eventName)
        {
            EventHandler?.Invoke(eventName);
        }

        private CompanionAppService appService;

        // Information obtained from the companion app service
        public Commander Cmdr { get; private set; }
        public Ship Ship { get; private set; }
        public List<Ship> StoredShips { get; private set; }
        public Station LastStation { get; private set; }
        public List<Module> Outfitting { get; private set; }

        // Services made available from EDDI
        public StarMapService starMapService { get; private set; }
        public SpeechService speechService { get; private set;  }
        public IEDDIStarSystemRepository starSystemRepository { get; private set; }

        // Information obtained from the configuration
        public StarSystem HomeStarSystem { get; private set; }
        public Station HomeStation { get; private set; }
        public decimal? Insurance { get; private set; }

        // Information obtained from the log watcher
        public string Environment { get; private set; }
        public StarSystem CurrentStarSystem { get; private set; }
        public StarSystem LastStarSystem { get; private set; }

        private Thread logWatcherThread;

        // Scripts
        private Dictionary<String, EventScript> eventScripts;

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

                // Set up our local star system repository
                starSystemRepository = new EDDIStarSystemSqLiteRepository();

                // Set up the EDDI configuration
                EDDIConfiguration eddiConfiguration = EDDIConfiguration.FromFile();
                if (eddiConfiguration.HomeSystem != null && eddiConfiguration.HomeSystem.Trim().Length > 0)
                {
                    EDDIStarSystem HomeStarSystemData = starSystemRepository.GetEDDIStarSystem(eddiConfiguration.HomeSystem.Trim());
                    if (HomeStarSystemData == null)
                    {
                        // We have no record of this system; set it up
                        HomeStarSystemData = new EDDIStarSystem();
                        HomeStarSystemData.Name = eddiConfiguration.HomeSystem.Trim();
                        HomeStarSystemData.StarSystem = DataProviderService.GetSystemData(eddiConfiguration.HomeSystem.Trim(), null, null, null);
                        HomeStarSystemData.LastVisit = DateTime.Now;
                        HomeStarSystemData.StarSystemLastUpdated = HomeStarSystemData.LastVisit;
                        HomeStarSystemData.TotalVisits = 1;
                        starSystemRepository.SaveEDDIStarSystem(HomeStarSystemData);
                    }
                    HomeStarSystem = HomeStarSystemData.StarSystem;

                    if (eddiConfiguration.HomeStation != null && eddiConfiguration.HomeStation.Trim().Length > 0)
                    {
                        string homeStationName = eddiConfiguration.HomeStation.Trim();
                        foreach (Station station in HomeStarSystem.Stations)
                        {
                            if (station.Name == homeStationName)
                            {
                                HomeStation = station;
                                break;
                            }
                        }
                    }
                }
                Logging.Verbose = eddiConfiguration.Debug;
                Insurance = eddiConfiguration.Insurance;

                // Event scripts are stored in a list; make them a dictionary for ease of lookup
                eventScripts = new Dictionary<string, EventScript>();
                if (eddiConfiguration.EventScripts != null)
                {
                    foreach (EventScript eventScript in eddiConfiguration.EventScripts)
                    {
                        eventScripts.Add(eventScript.EventName, eventScript);
                    }
                }

                // Set up the app service
                appService = new CompanionAppService();
                if (appService.CurrentState == CompanionAppService.State.READY)
                {
                    // Carry out initial population of profile
                    refreshProfile();
                }
                if (Cmdr != null && Cmdr.Name != null)
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
                    else if (Cmdr.Name != null)
                    {
                        commanderName = Cmdr.Name;
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

                speechService = new SpeechService(SpeechServiceConfiguration.FromFile());

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
                }
                else
                {
                    Logging.Info("EDDI netlog monitor is disabled");
                }

                Logging.Info("EDDI " + EDDI_VERSION + " initialised");
                Start();
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
                JournalMonitor monitor = new JournalMonitor(configuration, (result) => journalEntryHandler(result));
                monitor.start();
            }
        }

        public void Start()
        {
            EventHandler += new OnEventHandler(EventPosted);
        }

        private void EventPosted(String eventName)
        {
            EventScript script;
            eventScripts.TryGetValue(eventName, out script);
            if (script != null && script.Enabled)
            {
                ScriptResolver resolver = new ScriptResolver();
                Dictionary<string, Cottle.Value> dict = new Dictionary<string, Cottle.Value>();
                dict["shipname"] = Ship.PhoneticName == null ? Ship.Name == null ? "Your ship" : Ship.Name : Ship.PhoneticName;
                string result = resolver.resolve(script.Value, dict);
                speechService.Say(null, null, result);
            }
        }

        public void Stop()
        {
            if (logWatcherThread != null)
            {
                logWatcherThread.Abort();
                logWatcherThread = null;
            }

            if (speechService != null)
            {
                speechService.ShutdownSpeech();
            }
            Logging.Info("EDDI " + EDDI_VERSION + " shutting down");
        }

        void journalEntryHandler(JournalEntry entry)
        {
            Logging.Debug("Handling event " + JsonConvert.SerializeObject(entry));
            switch (entry.type)
            {
                case "Location":
                    eventJumped(entry);
                    break;
                case "Entered supercruise":
                    eventEnteredSupercruise(entry);
                    break;
                case "Entered normal space":
                    eventEnteredNormalSpace(entry);
                    break;
                case "Fine incurred":
                    eventFineIncurred(entry);
                    break;
                default:
                    speechService.Say(Cmdr, Ship, "Unknown event " + entry.type);
                    break;
            }
        }

        void eventDocked()
        {
        }

        void eventUndocked()
        {
        }

        void eventFineIncurred(JournalEntry entry)
        {

        }

        void eventJumped(JournalEntry entry)
        {
            if (CurrentStarSystem == null || CurrentStarSystem.Name != entry.stringData["System name"])
            {
                LastStarSystem = CurrentStarSystem;
                CurrentStarSystem = DataProviderService.GetSystemData(entry.stringData["System name"], entry.decimalData["System x"], entry.decimalData["System y"], entry.decimalData["System z"]);
                // After jump we are always in supercruise
                Environment = ENVIRONMENT_SUPERCRUISE;
                speechService.Say(Cmdr, Ship, "Jumped to system " + Translations.StarSystem(entry.stringData["System name"]));
                OnEvent("Jumped");
            }
        }

        void eventEnteredSupercruise(JournalEntry entry)
        {
            if (Environment != ENVIRONMENT_SUPERCRUISE)
            {
                Environment = ENVIRONMENT_SUPERCRUISE;
                OnEvent("Entered supercruise");
            }
        }

        void eventEnteredNormalSpace(JournalEntry entry)
        {
            if (Environment != ENVIRONMENT_NORMAL_SPACE)
            {
                Environment = ENVIRONMENT_NORMAL_SPACE;
                OnEvent("Entered normal space");
            }
        }

        private void refreshProfile()
        {
            if (appService != null)
            {
                Profile profile = appService.Profile();
                Cmdr = profile == null ? null : profile.Cmdr;
                Ship = profile == null ? null : profile.Ship;
                StoredShips = profile == null ? null : profile.StoredShips;
                CurrentStarSystem = profile == null ? null : profile.CurrentStarSystem;
                // TODO last station string to station
                //LastStation = profile.LastStation;
                Outfitting = profile == null ? null : profile.Outfitting;
            }
        }
    }
}
