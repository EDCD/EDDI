using EddiCompanionAppService;
using EddiDataDefinitions;
using EddiDataProviderService;
using EddiEvents;
using EddiSpeechService;
using EddiStarMapService;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using Utilities;

namespace Eddi
{
    // Notifications delegate
    public delegate void OnEventHandler(Event theEvent);

    /// <summary>
    /// Eddi is the controller for all EDDI operations.  Its job is to retain the state of the objects such as the commander, the current system, etc.
    /// and keep them up-to-date with changes that occur.  It also passes on messages to responders to handle as required.
    /// </summary>
    public class EDDI
    {
        private static EDDI instance;

        private static bool started;

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

        // Information obtained from the log watcher
        public string Environment { get; private set; }
        public StarSystem CurrentStarSystem { get; private set; }
        public StarSystem LastStarSystem { get; private set; }

        private EDDI()
        {
            try
            {
                Logging.Info(Constants.EDDI_NAME + " " + Constants.EDDI_VERSION + " starting");

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
                else
                {
                    // We don't have the companion API available, create dummy entries for the commander
                    Cmdr = new Commander();
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

                // Check for an update
                string response;
                try
                {
                    response = Net.DownloadString("http://api.eddp.co/version");
                    if (Versioning.Compare(response, Constants.EDDI_VERSION) == 1)
                    {
                        SpeechService.Instance.Say(null, "EDDI version " + response.Replace(".", " point ") + " is now available.", false);
                    }
                }
                catch
                {
                    SpeechService.Instance.Say(null, "There was a problem connecting to external data services; some features may not work fully", false);
                }

                Logging.Info(Constants.EDDI_NAME + " " + Constants.EDDI_VERSION + " initialised");
            }
            catch (Exception ex)
            {
                Logging.Error("Failed to initialise: " + ex.ToString());
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
                        Thread monitorThread = new Thread(() => monitor.Start());
                        monitorThread.IsBackground = true;
                        monitorThread.Name = monitor.MonitorName();
                        Logging.Info("Starting " + monitor.MonitorName());
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
                            EventHandler += new OnEventHandler(responder.Handle);
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
            if (started)
            {
                foreach (EDDIResponder responder in responders)
                {
                    responder.Stop();
                }
                foreach (EDDIMonitor monitor in monitors)
                {
                    monitor.Stop();
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

            Logging.Info(Constants.EDDI_NAME + " " + Constants.EDDI_VERSION + " stopped");
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

        public void eventHandler(Event journalEvent)
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
            else if (journalEvent is EnteredSupercruiseEvent)
            {
                passEvent = eventEnteredSupercruise((EnteredSupercruiseEvent)journalEvent);
            }
            else if (journalEvent is EnteredNormalSpaceEvent)
            {
                passEvent = eventEnteredNormalSpace((EnteredNormalSpaceEvent)journalEvent);
            }
            else if (journalEvent is ShipPurchasedEvent)
            {
                // TODO
            }
            else if (journalEvent is ShipSwappedEvent)
            {
                // TODO
            }
            else if (journalEvent is ShipSoldEvent)
            {
                // TODO
            }
            // Additional processing is over, send to the event responders if required
            if (passEvent)
            {
                OnEvent(journalEvent);
            }
        }

        private bool eventDocked(DockedEvent theEvent)
        {
            updateCurrentSystem(theEvent.system);
            return true;
        }

        private bool eventUndocked(UndockedEvent theEvent)
        {
            return true;
        }

        private bool eventLocation(LocationEvent theEvent)
        {
            updateCurrentSystem(theEvent.system);
            if (CurrentStarSystem.x == null)
            {
                // Star system is missing co-ordinates to take them from the event
                CurrentStarSystem.x = theEvent.x;
                CurrentStarSystem.y = theEvent.y;
                CurrentStarSystem.z = theEvent.z;
                StarSystemSqLiteRepository.Instance.SaveStarSystem(CurrentStarSystem);
            }
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
                updateCurrentSystem(theEvent.system);
                if (CurrentStarSystem.x == null)
                {
                    // Star system is missing co-ordinates to take them from the event
                    CurrentStarSystem.x = theEvent.x;
                    CurrentStarSystem.y = theEvent.y;
                    CurrentStarSystem.z = theEvent.z;
                }
                CurrentStarSystem.visits++;
                CurrentStarSystem.lastvisit = DateTime.Now;
                StarSystemSqLiteRepository.Instance.SaveStarSystem(CurrentStarSystem);
                // After jump we are always in supercruise
                Environment = Constants.ENVIRONMENT_SUPERCRUISE;
                setCommanderTitle();
            }
            return passEvent;
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
                StarSystemSqLiteRepository.Instance.SaveStarSystem(CurrentStarSystem);
                // After jump we are always in supercruise
                Environment = Constants.ENVIRONMENT_SUPERCRUISE;
                setCommanderTitle();
            }
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

        /// <summary>Obtain information from the companion API and use it to refresh our own data</summary>
        public void refreshProfile()
        {
            if (CompanionAppService.Instance != null)
            {
                Profile profile = CompanionAppService.Instance.Profile();
                Cmdr = profile == null ? new Commander() : profile.Cmdr;
                Ship = profile == null ? null : profile.Ship;
                StoredShips = profile == null ? null : profile.StoredShips;
                // We only set the current star system if it is not present, otherwise we leave it to events
                if (CurrentStarSystem == null)
                {
                    CurrentStarSystem = profile == null ? null : profile.CurrentStarSystem;
                    setSystemDistanceFromHome(CurrentStarSystem);
                }
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
