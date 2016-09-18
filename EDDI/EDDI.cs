using EliteDangerousCompanionAppService;
using EliteDangerousDataDefinitions;
using EliteDangerousDataProviderService;
using EliteDangerousEvents;
using EliteDangerousStarMapService;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
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

        public List<EDDIMonitor> monitors = new List<EDDIMonitor>();
        // Each monitor runs in its own thread
        public List<Thread> monitorThreads = new List<Thread>();

        public List<EDDIResponder> responders = new List<EDDIResponder>();

        // Information obtained from the companion app service
        public Commander Cmdr { get; private set; }
        public Ship Ship { get; private set; }
        public List<Ship> StoredShips { get; private set; }
        public Station LastStation { get; private set; }

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

        public EDDIConfiguration configuration { get; private set; }

        public static readonly string ENVIRONMENT_SUPERCRUISE = "Supercruise";
        public static readonly string ENVIRONMENT_NORMAL_SPACE = "Normal space";

        private Eddi()
        {
            try
            {
                // Set up our app directory
                Directory.CreateDirectory(Constants.DATA_DIR);

                // Use en-US as our default culture to ensure that we use . rather than , for our separator when writing out decimals
                CultureInfo defaultCulture = new CultureInfo("en-US");
                CultureInfo.DefaultThreadCurrentCulture = defaultCulture;
                CultureInfo.DefaultThreadCurrentUICulture = defaultCulture;

                Logging.Info(Constants.EDDI_NAME + " " + Constants.EDDI_VERSION + " starting");

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
                Insurance = configuration.Insurance;

                // Set up the app service
                if (CompanionAppService.Instance.CurrentState == CompanionAppService.State.READY)
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

                // Set up monitors and responders
                monitors = loadMonitors();
                responders = loadResponders();

                Logging.Info(Constants.EDDI_NAME + " " + Constants.EDDI_VERSION + " initialised");
            }
            catch (Exception ex)
            {
                Logging.Error("Failed to initialise: " + ex);
            }
        }

        public void Start()
        {
            foreach (EDDIMonitor monitor in monitors)
            {
                Thread monitorThread = new Thread(() => monitor.Start());
                monitorThread.IsBackground = true;
                monitorThread.Name = monitor.MonitorName();
                Logging.Info("Starting " + monitor.MonitorName());
                monitorThread.Start();
            }

            foreach (EDDIResponder responder in responders)
            {
                bool responderStarted = responder.Start();
                if (responderStarted)
                {
                    EventHandler += new OnEventHandler(responder.Handle);
                    Logging.Info("Started " + responder.ResponderName());
                }
                else
                {
                    Logging.Warn("Failed to start " + responder.ResponderName());
                }
            }
        }

        public void Stop()
        {
            foreach (EDDIResponder responder in responders)
            {
                responder.Stop();
            }
            foreach (EDDIMonitor monitor in monitors)
            {
                monitor.Stop();
            }

            Logging.Info(Constants.EDDI_NAME + " " + Constants.EDDI_VERSION + " shutting down");
        }

        public void eventHandler(Event journalEvent)
        {
            Logging.Debug("Handling event " + JsonConvert.SerializeObject(journalEvent));
            // We have some additional processing to do for a number of events
            bool passEvent = true;
            if (journalEvent is JumpedEvent)
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
            else if (journalEvent is EnteredSupercruiseEvent)
            {
                passEvent = eventEnteredSupercruise((EnteredSupercruiseEvent)journalEvent);
            }
            else if (journalEvent is EnteredNormalSpaceEvent)
            {
                passEvent = eventEnteredNormalSpace((EnteredNormalSpaceEvent)journalEvent);
            }
            // Additional processing is over, send to the event responders if required
            if (passEvent)
            {
                OnEvent(journalEvent);
            }
        }

        private bool eventDocked(DockedEvent theEvent)
        {
            return true;
        }

        private bool eventUndocked(UndockedEvent theEvent)
        {
            return true;
        }

        private bool eventJumped(JumpedEvent theEvent)
        {
            bool passEvent;
            Logging.Debug("Jumped to " + theEvent.system);
            if (CurrentStarSystem == null)
            {
                // Initialisation; don't pass the event along
                passEvent = false;
            }
            else if (CurrentStarSystem.name == theEvent.system)
            {
                // Restatement of current system
                passEvent = false;
            }
            else
            {
                passEvent = true;
                LastStarSystem = CurrentStarSystem;
                CurrentStarSystem = StarSystemSqLiteRepository.Instance.GetOrCreateStarSystem(theEvent.system);
                if (CurrentStarSystem.x == null)
                {
                    // Star system is missing co-ordinates to take them from the event
                    CurrentStarSystem.x = theEvent.x;
                    CurrentStarSystem.y = theEvent.y;
                    CurrentStarSystem.z = theEvent.z;
                }
                CurrentStarSystem.visits++;
                CurrentStarSystem.lastvisit = DateTime.Now;
                setSystemDistanceFromHome(CurrentStarSystem);
                StarSystemSqLiteRepository.Instance.SaveStarSystem(CurrentStarSystem);
                // After jump we are always in supercruise
                Environment = ENVIRONMENT_SUPERCRUISE;
                setCommanderTitle();
            }
            return passEvent;
        }

        private bool eventEnteredSupercruise(EnteredSupercruiseEvent theEvent)
        {
            if (Environment == null || Environment != ENVIRONMENT_SUPERCRUISE)
            {
                Environment = ENVIRONMENT_SUPERCRUISE;
                return true;
            }
            return false;
        }

        private bool eventEnteredNormalSpace(EnteredNormalSpaceEvent theEvent)
        {
            if (Environment == null || Environment != ENVIRONMENT_NORMAL_SPACE)
            {
                Environment = ENVIRONMENT_NORMAL_SPACE;
                return true;
            }
            return false;
        }

        /// <summary>Obtain information from the copmanion API and use it to refresh our own data</summary>
        public void refreshProfile()
        {
            if (CompanionAppService.Instance != null)
            {
                Profile profile = CompanionAppService.Instance.Profile();
                Cmdr = profile == null ? null : profile.Cmdr;
                Ship = profile == null ? null : profile.Ship;
                StoredShips = profile == null ? null : profile.StoredShips;
                CurrentStarSystem = profile == null ? null : profile.CurrentStarSystem;
                setSystemDistanceFromHome(CurrentStarSystem);
                LastStation = profile == null ? null : profile.LastStation;
                setCommanderTitle();
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
                    if (CurrentStarSystem.allegiance == "Federation" && Cmdr.federationrating.rank > minFederationRankForTitle)
                    {
                        Cmdr.title = Cmdr.federationrating.name;
                    }
                    else if (CurrentStarSystem.allegiance == "Empire" && Cmdr.empirerating.rank > minEmpireRankForTitle)
                    {
                        Cmdr.title = Cmdr.empirerating.name;
                    }
                }
            }
        }

        /// <summary>
        /// Find all monitors
        /// </summary>
        private List<EDDIMonitor> loadMonitors()
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
                                Logging.Debug("Instantiating plugin at " + file.FullName);
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
            return monitors;
        }

        /// <summary>
        /// Find all responders
        /// </summary>
        private List<EDDIResponder> loadResponders()
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
                                Logging.Debug("Instantiating plugin at " + file.FullName);
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
