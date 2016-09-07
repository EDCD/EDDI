using EliteDangerousCompanionAppService;
using EliteDangerousDataProviderService;
using EliteDangerousDataDefinitions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.Concurrent;
using EliteDangerousNetLogMonitor;
using System.Threading;
using System.Diagnostics;
using EliteDangerousStarMapService;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using EliteDangerousSpeechService;
using Utilities;
using EliteDangerousJournalMonitor;
using EDDI;

namespace EDDIVAPlugin
{
    public class VoiceAttackPlugin
    {
        private static Eddi eddi;

        public static string VA_DisplayName()
        {
            return "EDDI " + Eddi.EDDI_VERSION;
        }

        public static string VA_DisplayInfo()
        {
            return "Elite: Dangerous Data Interface\r\nVersion " + Eddi.EDDI_VERSION;
        }

        public static Guid VA_Id()
        {
            return new Guid("{4AD8E3A4-CEFA-4558-B503-1CC9B99A07C1}");
        }

        public static void VA_Init1(ref Dictionary<string, object> state, ref Dictionary<string, Int16?> shortIntValues, ref Dictionary<string, string> textValues, ref Dictionary<string, int?> intValues, ref Dictionary<string, decimal?> decimalValues, ref Dictionary<string, Boolean?> booleanValues, ref Dictionary<string, DateTime?> dateTimeValues, ref Dictionary<string, object> extendedValues)
        {
            eddi = Eddi.Instance;
            if (eddi.HomeStarSystem != null)
            {
                setStarSystemValues(eddi.Cmdr, eddi.HomeStarSystem, "Home system", ref state, ref shortIntValues, ref textValues, ref intValues, ref decimalValues, ref booleanValues, ref dateTimeValues, ref extendedValues);
            }

            if (eddi.HomeStation != null)
            {
                setString(ref textValues, "Home station", eddi.HomeStation.Name);

            }

            if (eddi.Insurance != null)
            {
                setDecimal(ref decimalValues, "Insurance", eddi.Insurance);
            }

            setString(ref textValues, "Environment", eddi.Environment);

            // If (Cmdr != null && Cmdr.Name != null)
            setString(ref textValues, "EDDI plugin profile status", "Enabled");

            //if (!initialised)
            //{
            //    lock (initLock)
            //    {
            //        if (!initialised)
            //        {
            //            try
            //            {
            //                Logging.Info("EDDI " + PLUGIN_VERSION + " starting");

            //                // Set up and/or open our database
            //                String dataDir = Environment.GetEnvironmentVariable("AppData") + "\\EDDI";
            //                System.IO.Directory.CreateDirectory(dataDir);

            //                // Set up our local star system repository
            //                starSystemRepository = new EDDIStarSystemSqLiteRepository();

            //                // Set up the EDDI configuration
            //                EDDIConfiguration eddiConfiguration = EDDIConfiguration.FromFile();
            //                if (eddiConfiguration.HomeSystem != null && eddiConfiguration.HomeSystem.Trim().Length > 0)
            //                {
            //                    EDDIStarSystem HomeStarSystemData = starSystemRepository.GetEDDIStarSystem(eddiConfiguration.HomeSystem.Trim());
            //                    if (HomeStarSystemData == null)
            //                    {
            //                        // We have no record of this system; set it up
            //                        HomeStarSystemData = new EDDIStarSystem();
            //                        HomeStarSystemData.Name = eddiConfiguration.HomeSystem.Trim();
            //                        HomeStarSystemData.StarSystem = DataProviderService.GetSystemData(eddiConfiguration.HomeSystem.Trim(), null, null ,null);
            //                        HomeStarSystemData.LastVisit = DateTime.Now;
            //                        HomeStarSystemData.StarSystemLastUpdated = HomeStarSystemData.LastVisit;
            //                        HomeStarSystemData.TotalVisits = 1;
            //                        starSystemRepository.SaveEDDIStarSystem(HomeStarSystemData);
            //                    }
            //                    HomeStarSystem = HomeStarSystemData.StarSystem;
            //                }


            //                Logging.Verbose = eddiConfiguration.Debug;

            //                // Set up the app service
            //                appService = new CompanionAppService();
            //                if (appService.CurrentState == CompanionAppService.State.READY)
            //                {
            //                    // Carry out initial population of profile
            //                    InvokeUpdateProfile(ref state, ref shortIntValues, ref textValues, ref intValues, ref decimalValues, ref booleanValues, ref dateTimeValues, ref extendedValues);
            //                }
            //                if (Cmdr != null && Cmdr.Name != null)
            //                {
            //                    Logging.Info("EDDI access to the companion app is enabled");
            //                }
            //                else
            //                {
            //                    // If InvokeUpdatePlugin failed then it will have have left an error message, but this once we ignore it
            //                    setPluginStatus(ref textValues, "Operational", null, null);
            //                    setString(ref textValues, "EDDI plugin profile status", "Disabled");
            //                    Logging.Info("EDDI access to the companion app is disabled");
            //                    // We create a commander anyway, as data such as starsystem uses it
            //                    Cmdr = new Commander();
            //                }

            //                // Set up the star map service
            //                StarMapConfiguration starMapCredentials = StarMapConfiguration.FromFile();
            //                if (starMapCredentials != null && starMapCredentials.apiKey != null)
            //                {
            //                    // Commander name might come from star map credentials or the companion app's profile
            //                    string commanderName = null;
            //                    if (starMapCredentials.commanderName != null)
            //                    {
            //                        commanderName = starMapCredentials.commanderName;
            //                    }
            //                    else if (Cmdr.Name != null)
            //                    {
            //                        commanderName = Cmdr.Name;
            //                    }
            //                    if (commanderName != null)
            //                    {
            //                        starMapService = new StarMapService(starMapCredentials.apiKey, commanderName);
            //                        setString(ref textValues, "EDDI plugin EDSM status", "Enabled");
            //                        Logging.Info("EDDI access to EDSM is enabled");
            //                    }
            //                }
            //                if (starMapService == null)
            //                {
            //                    setString(ref textValues, "EDDI plugin EDSM status", "Disabled");
            //                    Logging.Info("EDDI access to EDSM is disabled");
            //                }

            //                setString(ref textValues, "EDDI version", PLUGIN_VERSION);

            //                speechService = new SpeechService(SpeechServiceConfiguration.FromFile());

            //                InvokeNewSystem(ref state, ref shortIntValues, ref textValues, ref intValues, ref decimalValues, ref booleanValues, ref dateTimeValues, ref extendedValues);
            //                CurrentEnvironment = ENVIRONMENT_NORMAL_SPACE;

            //                // Set up log monitor
            //                NetLogConfiguration netLogConfiguration = NetLogConfiguration.FromFile();
            //                if (netLogConfiguration != null && netLogConfiguration.path != null)
            //                {
            //                    logWatcherThread = new Thread(() => StartLogMonitor(netLogConfiguration));
            //                    logWatcherThread.IsBackground = true;
            //                    logWatcherThread.Name = "EDDI netlog watcher";
            //                    logWatcherThread.Start();
            //                    setString(ref textValues, "EDDI plugin NetLog status", "Enabled");
            //                    Logging.Info("EDDI netlog monitor is enabled for " + netLogConfiguration.path);
            //                }
            //                else
            //                {
            //                    setString(ref textValues, "EDDI plugin NetLog status", "Disabled");
            //                    Logging.Info("EDDI netlog monitor is disabled");
            //                }

            //                setPluginStatus(ref textValues, "Operational", null, null);

            //                initialised = true;
            //                Logging.Info("EDDI " + PLUGIN_VERSION + " initialised");
            //            }
            //            catch (Exception ex)
            //            {
            //                setPluginStatus(ref textValues, "Failed", "Failed to initialise", ex);
            //            }
            //        }
            //    }
            //}
        }

        public static void VA_Exit1(ref Dictionary<string, object> state)
        {
            eddi.Stop();
        }

        public static void VA_Invoke1(String context, ref Dictionary<string, object> state, ref Dictionary<string, Int16?> shortIntValues, ref Dictionary<string, string> textValues, ref Dictionary<string, int?> intValues, ref Dictionary<string, decimal?> decimalValues, ref Dictionary<string, Boolean?> booleanValues, ref Dictionary<string, DateTime?> dateTimeValues, ref Dictionary<string, object> extendedValues)
        {
            switch (context)
            {
                case "coriolis":
                    InvokeCoriolis(ref state, ref shortIntValues, ref textValues, ref intValues, ref decimalValues, ref booleanValues, ref dateTimeValues, ref extendedValues);
                    return;
                case "eddbsystem":
                    InvokeEDDBSystem(ref state, ref shortIntValues, ref textValues, ref intValues, ref decimalValues, ref booleanValues, ref dateTimeValues, ref extendedValues);
                    return;
                case "eddbstation":
                    InvokeEDDBStation(ref state, ref shortIntValues, ref textValues, ref intValues, ref decimalValues, ref booleanValues, ref dateTimeValues, ref extendedValues);
                    return;
                case "profile":
                    InvokeUpdateProfile(ref state, ref shortIntValues, ref textValues, ref intValues, ref decimalValues, ref booleanValues, ref dateTimeValues, ref extendedValues);
                    return;
                case "system":
                    InvokeNewSystem(ref state, ref shortIntValues, ref textValues, ref intValues, ref decimalValues, ref booleanValues, ref dateTimeValues, ref extendedValues);
                    return;
                case "log watcher":
                    InvokeJournalWatcher(ref state, ref shortIntValues, ref textValues, ref intValues, ref decimalValues, ref booleanValues, ref dateTimeValues, ref extendedValues);
                    return;
                case "say":
                    InvokeSay(ref state, ref shortIntValues, ref textValues, ref intValues, ref decimalValues, ref booleanValues, ref dateTimeValues, ref extendedValues);
                    return;
                case "transmit":
                    InvokeTransmit(ref state, ref shortIntValues, ref textValues, ref intValues, ref decimalValues, ref booleanValues, ref dateTimeValues, ref extendedValues);
                    return;
                case "receive":
                    InvokeReceive(ref state, ref shortIntValues, ref textValues, ref intValues, ref decimalValues, ref booleanValues, ref dateTimeValues, ref extendedValues);
                    return;
                case "generate callsign":
                    InvokeGenerateCallsign(ref state, ref shortIntValues, ref textValues, ref intValues, ref decimalValues, ref booleanValues, ref dateTimeValues, ref extendedValues);
                    return;
                case "system distance":
                    InvokeStarMapSystemDistance(ref state, ref shortIntValues, ref textValues, ref intValues, ref decimalValues, ref booleanValues, ref dateTimeValues, ref extendedValues);
                    return;
                case "system note":
                    InvokeStarMapSystemComment(ref state, ref shortIntValues, ref textValues, ref intValues, ref decimalValues, ref booleanValues, ref dateTimeValues, ref extendedValues);
                    return;
                default:
                    if (context.ToLower().StartsWith("event:"))
                    {
                        // Inject an event
                        string data = context.Replace("event: ", "");
                        JObject eventData = JObject.Parse(data);
                        //LogQueue.Add(eventData);
                    }
                    return;
            }
        }

        public static void InvokeJournalWatcher(ref Dictionary<string, object> state, ref Dictionary<string, Int16?> shortIntValues, ref Dictionary<string, string> textValues, ref Dictionary<string, int?> intValues, ref Dictionary<string, decimal?> decimalValues, ref Dictionary<string, Boolean?> booleanValues, ref Dictionary<string, DateTime?> dateTimeValues, ref Dictionary<string, object> extendedValues)
        {
            //bool somethingToReport = false;
            //while (!somethingToReport)
            //{
            //    try
            //    {
            //        Logging.Debug("Queue has " + eddi.EventsOutstanding() + " entries");
            //        JournalEntry entry = eddi.GetNextEvent();
            //        Logging.Debug("Entry is " + JsonConvert.SerializeObject(entry));
            //        if (entry.refetchProfile)
            //        {
            //            // Whatever has happened needs us to refetch the profile
            //            InvokeUpdateProfile(ref state, ref shortIntValues, ref textValues, ref intValues, ref decimalValues, ref booleanValues, ref dateTimeValues, ref extendedValues);
            //        }

            //        bool discardEvent = false;
            //        // Handle any type-specific items
            //        switch ((string)entry.type)
            //        {
            //            case "Docked":
            //                InvokeUpdateStation(ref state, ref shortIntValues, ref textValues, ref intValues, ref decimalValues, ref booleanValues, ref dateTimeValues, ref extendedValues);
            //                break;
            //            case "Jumped":
            //                if (Cmdr.StarSystem == entry.stringData["starsystem"])
            //                {
            //                    discardEvent = true;
            //                }
            //                else
            //                {
            //                    Cmdr.StarSystem = entry.stringData["starsystem"];
            //                    //Cmdr.StarSystemX = (decimal?)entry.x;
            //                    //Cmdr.StarSystemY = (decimal?)entry.y;
            //                    //Cmdr.StarSystemZ = (decimal?)entry.z;
            //                    InvokeNewSystem(ref state, ref shortIntValues, ref textValues, ref intValues, ref decimalValues, ref booleanValues, ref dateTimeValues, ref extendedValues);
            //                    // Whenever we jump system we always come out in supercruise
            //                    eddi.Environment = Eddi.ENVIRONMENT_SUPERCRUISE;
            //                    setString(ref textValues, "Environment", eddi.Environment);
            //                }
            //                break;
            //            case "Entered supercruise":
            //                if (eddi.Environment == Eddi.ENVIRONMENT_SUPERCRUISE)
            //                {
            //                    // Already in supercruise
            //                    discardEvent = true;
            //                }
            //                else
            //                {
            //                    eddi.Environment = Eddi.ENVIRONMENT_SUPERCRUISE;
            //                }
            //                break;
            //            case "Left supercruise":
            //                if (eddi.Environment == Eddi.ENVIRONMENT_NORMAL_SPACE)
            //                {
            //                    // Already in normal space
            //                    discardEvent = true;
            //                }
            //                else
            //                {
            //                    eddi.Environment = Eddi.ENVIRONMENT_NORMAL_SPACE;
            //                }
            //                break;
            //        }

            //        if (discardEvent)
            //        {
            //            // This event is a duplicate or extraneous; ignore it
            //            continue;
            //        }
            //        somethingToReport = true;

            //        // At this point our internal data should be up-to-date

            //        setString(ref textValues, "EDDI event", entry.type);
            //        foreach (var key in entry.stringData.Keys)
            //        {
            //            setString(ref textValues, entry.type + " " + key.ToLower(), entry.stringData[key]);
            //        }
            //        foreach (var key in entry.intData.Keys)
            //        {
            //            setInt(ref intValues, entry.type + " " + key.ToLower(), entry.intData[key]);
            //        }
            //        foreach (var key in entry.boolData.Keys)
            //        {
            //            setBoolean(ref booleanValues, entry.type + " " + key.ToLower(), entry.boolData[key]);
            //        }
            //        foreach (var key in entry.datetimeData.Keys)
            //        {
            //            setDateTime(ref dateTimeValues, entry.type + " " + key.ToLower(), entry.datetimeData[key]);
            //        }
            //        foreach (var key in entry.decimalData.Keys)
            //        {
            //            setDecimal(ref decimalValues, entry.type + " " + key.ToLower(), entry.decimalData[key]);
            //        }
            //    }
            //    catch (Exception ex)
            //    {
            //        Logging.Warn("Error occurred: " + ex);
            //        setPluginStatus(ref textValues, "Failed", "Failed to process journal entry", ex);
            //    }
            //}
        }

        public static void InvokeLogWatcher(ref Dictionary<string, object> state, ref Dictionary<string, Int16?> shortIntValues, ref Dictionary<string, string> textValues, ref Dictionary<string, int?> intValues, ref Dictionary<string, decimal?> decimalValues, ref Dictionary<string, Boolean?> booleanValues, ref Dictionary<string, DateTime?> dateTimeValues, ref Dictionary<string, object> extendedValues)
        {
            //bool somethingToReport = false;
            //while (!somethingToReport)
            //{
            //    try
            //    {
            //        Logging.Debug("Queue has " + LogQueue.Count + " entries");
            //        dynamic entry = LogQueue.Take();
            //        Logging.Debug("Entry is " + entry);
            //        switch ((string)entry.type)
            //        {
            //            case "Location": // Change of location
            //                Logging.Debug("Handling change of location");
            //                if (Cmdr != null)
            //                {
            //                    string newEnvironment;
            //                    // Tidy up the environment string
            //                    switch ((string)entry.environment)
            //                    {
            //                        case "Supercruise":
            //                            newEnvironment = ENVIRONMENT_SUPERCRUISE;
            //                            break;
            //                        case "NormalFlight":
            //                            newEnvironment = ENVIRONMENT_NORMAL_SPACE;
            //                            break;
            //                        default:
            //                            newEnvironment = (string)entry.environment;
            //                            break;
            //                    }

            //                    if ((string)entry.starsystem != Cmdr.StarSystem)
            //                    {
            //                        Logging.Debug("System changed from " + Cmdr.StarSystem + " to " + entry.starsystem);
            //                        // Change of system
            //                        setString(ref textValues, "EDDI event", "System change");
            //                        Cmdr.StarSystem = (string)entry.starsystem;
            //                        Cmdr.StarSystemX = (decimal?)entry.x;
            //                        Cmdr.StarSystemY = (decimal?)entry.y;
            //                        Cmdr.StarSystemZ = (decimal?)entry.z;

            //                        // Need to fetch new starsystem information
            //                        InvokeNewSystem(ref state, ref shortIntValues, ref textValues, ref intValues, ref decimalValues, ref booleanValues, ref dateTimeValues, ref extendedValues);
            //                        Logging.Debug("Obtained new system data");
            //                        CurrentEnvironment = ENVIRONMENT_SUPERCRUISE;
            //                        setString(ref textValues, "Environment", CurrentEnvironment); // Whenever we jump system we always come out in supercruise
            //                        somethingToReport = true;
            //                    }
            //                    else if (newEnvironment != CurrentEnvironment)
            //                    {
            //                        Logging.Debug("Environment changed from " + CurrentEnvironment + " to " + newEnvironment);
            //                        // Change of environment
            //                        setString(ref textValues, "EDDI event", "Environment change");
            //                        CurrentEnvironment = newEnvironment;
            //                        setString(ref textValues, "Environment", CurrentEnvironment);
            //                        somethingToReport = true;
            //                    }
            //                }
            //                break;
            //            case "Ship docked": // Ship docked
            //                Logging.Debug("Handling docking of ship");
            //                setString(ref textValues, "EDDI event", "Ship docked");
            //                // Need to refetch profile information
            //                InvokeUpdateProfile(ref state, ref shortIntValues, ref textValues, ref intValues, ref decimalValues, ref booleanValues, ref dateTimeValues, ref extendedValues);
            //                InvokeUpdateStation(ref state, ref shortIntValues, ref textValues, ref intValues, ref decimalValues, ref booleanValues, ref dateTimeValues, ref extendedValues);
            //                somethingToReport = true;
            //                break;
            //            case "Ship change": // New or swapped ship
            //                Logging.Debug("Handling change of ship");
            //                setString(ref textValues, "EDDI event", "Ship change");
            //                // Need to refetch profile information
            //                InvokeUpdateProfile(ref state, ref shortIntValues, ref textValues, ref intValues, ref decimalValues, ref booleanValues, ref dateTimeValues, ref extendedValues);
            //                somethingToReport = true;
            //                break;
            //            default:
            //                setPluginStatus(ref textValues, "Failed", "Unknown log entry " + entry.type, null);
            //                break;
            //        }
            //        if (somethingToReport)
            //        {
            //            setString(ref textValues, "EDDI raw event", entry.ToString());
            //        }
            //    }
            //    catch (Exception ex)
            //    {
            //        Logging.Warn("Error occurred: " + ex);
            //        setPluginStatus(ref textValues, "Failed", "Failed to obtain log entry", ex);
            //    }
            //}
        }

        public static void InvokeEDDBSystem(ref Dictionary<string, object> state, ref Dictionary<string, Int16?> shortIntValues, ref Dictionary<string, string> textValues, ref Dictionary<string, int?> intValues, ref Dictionary<string, decimal?> decimalValues, ref Dictionary<string, Boolean?> booleanValues, ref Dictionary<string, DateTime?> dateTimeValues, ref Dictionary<string, object> extendedValues)
        {
            //Logging.Debug("Entered");
            //try
            //{
            //    if (Cmdr == null || Cmdr.Ship == null || Cmdr.Ship.Model == null)
            //    {
            //        // Refetch the profile to set our system
            //        InvokeUpdateProfile(ref state, ref shortIntValues, ref textValues, ref intValues, ref decimalValues, ref booleanValues, ref dateTimeValues, ref extendedValues);
            //        if (Cmdr == null || Cmdr.Ship == null || Cmdr.Ship.Model == null)
            //        {
            //            // Still no luck; assume an error of some sort has been logged by InvokeUpdateProfile()
            //            Logging.Debug("Cannot obtain profile information; leaving");
            //            return;
            //        }
            //    }

            //    if (eddi.CurrentStarSystem == null)
            //    {
            //        // Missing current star system information
            //        Logging.Debug("No information on current system");
            //        return;
            //    }
            //    string systemUri = "https://eddb.io/system/" + eddi.CurrentStarSystem.EDDBID;

            //    Logging.Debug("Starting process with uri " + systemUri);

            //    Process.Start(systemUri);

            //    setPluginStatus(ref textValues, "Operational", null, null);
            //}
            //catch (Exception e)
            //{
            //    setPluginStatus(ref textValues, "Failed", "Failed to send system data to EDDB", e);
            //}
            //Logging.Debug("Leaving");
        }

        public static void InvokeEDDBStation(ref Dictionary<string, object> state, ref Dictionary<string, Int16?> shortIntValues, ref Dictionary<string, string> textValues, ref Dictionary<string, int?> intValues, ref Dictionary<string, decimal?> decimalValues, ref Dictionary<string, Boolean?> booleanValues, ref Dictionary<string, DateTime?> dateTimeValues, ref Dictionary<string, object> extendedValues)
        {
            //Logging.Debug("Entered");
            //try
            //{
            //    if (Cmdr == null || Cmdr.Ship == null || Cmdr.Ship.Model == null)
            //    {
            //        // Refetch the profile to set our station
            //        InvokeUpdateProfile(ref state, ref shortIntValues, ref textValues, ref intValues, ref decimalValues, ref booleanValues, ref dateTimeValues, ref extendedValues);
            //        if (Cmdr == null || Cmdr.Ship == null || Cmdr.Ship.Model == null)
            //        {
            //            // Still no luck; assume an error of some sort has been logged by InvokeUpdateProfile()
            //            Logging.Debug("Cannot obtain profile information; leaving");
            //            return;
            //        }
            //    }

            //    if (eddi.CurrentStarSystem == null || Cmdr.LastStation == null)
            //    {
            //        // Missing current star system information
            //        Logging.Debug("No information on current station");
            //        return;
            //    }
            //    Logging.Debug("Current star system is " + JsonConvert.SerializeObject(eddi.CurrentStarSystem));
            //    Logging.Debug("Attempting to find station " + Cmdr.LastStation);
            //    Station thisStation = eddi.CurrentStarSystem.Stations.SingleOrDefault(s => s.Name == Cmdr.LastStation);
            //    if (thisStation == null)
            //    {
            //        // Missing current star system information
            //        Logging.Debug("No information on current station");
            //        return;
            //    }
            //    string stationUri = "https://eddb.io/station/" + thisStation.EDDBID;

            //    Logging.Debug("Starting process with uri " + stationUri);

            //    Process.Start(stationUri);

            //    setPluginStatus(ref textValues, "Operational", null, null);
            //}
            //catch (Exception e)
            //{
            //    setPluginStatus(ref textValues, "Failed", "Failed to send station data to EDDB", e);
            //}
            //Logging.Debug("Leaving");
        }

        public static void InvokeCoriolis(ref Dictionary<string, object> state, ref Dictionary<string, Int16?> shortIntValues, ref Dictionary<string, string> textValues, ref Dictionary<string, int?> intValues, ref Dictionary<string, decimal?> decimalValues, ref Dictionary<string, Boolean?> booleanValues, ref Dictionary<string, DateTime?> dateTimeValues, ref Dictionary<string, object> extendedValues)
        {
            //Logging.Debug("Entered");
            //try
            //{
            //    if (Cmdr == null || Cmdr.Ship == null || Cmdr.Ship.Model == null)
            //    {
            //        // Refetch the profile to set our ship
            //        InvokeUpdateProfile(ref state, ref shortIntValues, ref textValues, ref intValues, ref decimalValues, ref booleanValues, ref dateTimeValues, ref extendedValues);
            //        if (Cmdr == null || Cmdr.Ship == null || Cmdr.Ship.Model == null)
            //        {
            //            // Still no luck; assume an error of some sort has been logged by InvokeUpdateProfile()
            //            Logging.Debug("Cannot obtain profile information; leaving");
            //            return;
            //        }
            //    }

            //    string shipUri = Coriolis.ShipUri(Cmdr.Ship);

            //    Logging.Debug("Starting process with uri " + shipUri);

            //    Process.Start(shipUri);

            //    setPluginStatus(ref textValues, "Operational", null, null);
            //}
            //catch (Exception e)
            //{
            //    setPluginStatus(ref textValues, "Failed", "Failed to send ship data to coriolis", e);
            //}
            //Logging.Debug("Leaving");
        }

        public static void InvokeUpdateProfile(ref Dictionary<string, object> state, ref Dictionary<string, Int16?> shortIntValues, ref Dictionary<string, string> textValues, ref Dictionary<string, int?> intValues, ref Dictionary<string, decimal?> decimalValues, ref Dictionary<string, Boolean?> booleanValues, ref Dictionary<string, DateTime?> dateTimeValues, ref Dictionary<string, object> extendedValues)
        {
            //Logging.Debug("Entered.  App service is " + (eddi.appService == null ? "disabled" : "enabled"));
            //if (eddi.appService != null)
            //{
            try
            {
                //        // Obtain the command profile
                //        Cmdr = eddi.appService.Profile();

                Logging.Debug("Commander is " + JsonConvert.SerializeObject(eddi.Cmdr));

                //
                // Commander data
                //
                setCommanderValues(eddi.Cmdr, ref state, ref shortIntValues, ref textValues, ref intValues, ref decimalValues, ref booleanValues, ref dateTimeValues, ref extendedValues);

                setShipValues(eddi.Ship, "Ship", ref state, ref shortIntValues, ref textValues, ref intValues, ref decimalValues, ref booleanValues, ref dateTimeValues, ref extendedValues);


                //
                // Stored ships data
                //
                int currentStoredShip = 1;
                foreach (Ship StoredShip in eddi.StoredShips)
                {
                    setShipValues(StoredShip, "Stored ship " + currentStoredShip, ref state, ref shortIntValues, ref textValues, ref intValues, ref decimalValues, ref booleanValues, ref dateTimeValues, ref extendedValues);

                    currentStoredShip++;
                }

                setInt(ref intValues, "Stored ships", eddi.StoredShips.Count);
                // We also clear out any ships that have been sold since the last run.  We don't know
                // how many there are so just clear out the succeeding 10 slots and hope that the commander
                // hasn't gone on a selling spree
                for (int i = 0; i < 10; i++)
                {
                    string varBase = "Stored ship " + (currentStoredShip + i);
                    setString(ref textValues, varBase + " model", null);
                    setString(ref textValues, varBase + " system", null);
                    setString(ref textValues, varBase + " station", null);
                    setString(ref textValues, varBase + " callsign", null);
                    setString(ref textValues, varBase + " callsign (spoken)", null);
                    setString(ref textValues, varBase + " name", null);
                    setString(ref textValues, varBase + " role", null);
                    setDecimal(ref decimalValues, varBase + " distance", null);
                }

                // Last station
                if (eddi.LastStation != null)
                {
                    //setStationValues(eddi.LastStation, ref state, ref shortIntValues, ref textValues, ref intValues, ref decimalValues, ref booleanValues, ref dateTimeValues, ref extendedValues);
                }
                setString(ref textValues, "Last station name", eddi.LastStation.Name);

                if (Logging.Verbose)
                {
                    Logging.Debug("Resultant shortint values " + JsonConvert.SerializeObject(shortIntValues));
                    Logging.Debug("Resultant text values " + JsonConvert.SerializeObject(textValues));
                    Logging.Debug("Resultant int values " + JsonConvert.SerializeObject(intValues));
                    Logging.Debug("Resultant decimal values " + JsonConvert.SerializeObject(decimalValues));
                    Logging.Debug(" Resultant boolean values " + JsonConvert.SerializeObject(booleanValues));
                    Logging.Debug("Resultant datetime values " + JsonConvert.SerializeObject(dateTimeValues));
                }

                setPluginStatus(ref textValues, "Operational", null, null);
                setString(ref textValues, "EDDI plugin profile status", "Enabled");
            }
            catch (Exception ex)
            {
                setPluginStatus(ref textValues, "Failed", "Failed to access profile", ex);
                setString(ref textValues, "EDDI plugin profile status", "Disabled");
            }
            //}
        }


        /// <summary>Find a module in outfitting that matches our existing module and provide its price</summary>
        private static void SetShipModuleValues(string name, Module module, ref Dictionary<string, string> textValues, ref Dictionary<string, int?> intValues, ref Dictionary<string, decimal?> decimalValues)
        {
            setString(ref textValues, name, module == null ? null : module.Name);
            setInt(ref intValues, name + " class", module == null ? (int?)null : module.Class);
            setString(ref textValues, name + " grade", module == null ? null : module.Grade);
            setDecimal(ref decimalValues, name + " health", module == null ? (decimal?)null : module.Health);
            setDecimal(ref decimalValues, name + " cost", module == null ? (decimal?)null : (decimal)module.Cost);
            setDecimal(ref decimalValues, name + " value", module == null ? (decimal?)null : (decimal)module.Value);
            if (module != null && module.Cost < module.Value)
            {
                decimal discount = Math.Round((1 - (((decimal)module.Cost) / ((decimal)module.Value))) * 100, 1);
                setDecimal(ref decimalValues, name + " discount", discount > 0.01M ? discount : (decimal?)null);
            }
            else
            {
                setDecimal(ref decimalValues, name + " discount", null);
            }
        }

        /// <summary>Find a module in outfitting that matches our existing module and provide its price</summary>
        private static void SetShipModuleOutfittingValues(string name, Module existing, List<Module> outfittingModules, ref Dictionary<string, string> textValues, ref Dictionary<string, decimal?> decimalValues)
        {
            if (existing != null)
            {
                foreach (Module Module in outfittingModules)
                {
                    if (existing.EDDBID == Module.EDDBID)
                    {
                        // Found it
                        setDecimal(ref decimalValues, name + " station cost", (decimal?)Module.Cost);
                        if (Module.Cost < existing.Cost)
                        {
                            // And it's cheaper
                            setDecimal(ref decimalValues, name + " station discount", existing.Cost - Module.Cost);
                            setString(ref textValues, name + " station discount (spoken)", humanize(existing.Cost - Module.Cost));
                        }
                        return;
                    }
                }
            }
            // Not found so remove any existing
            setDecimal(ref decimalValues, "Ship " + name + " station cost", (decimal?)null);
            setDecimal(ref decimalValues, "Ship " + name + " station discount", (decimal?)null);
            setString(ref textValues, "Ship " + name + " station discount (spoken)", (string)null);
        }


        public static void InvokeUpdateStation(ref Dictionary<string, object> state, ref Dictionary<string, Int16?> shortIntValues, ref Dictionary<string, string> textValues, ref Dictionary<string, int?> intValues, ref Dictionary<string, decimal?> decimalValues, ref Dictionary<string, Boolean?> booleanValues, ref Dictionary<string, DateTime?> dateTimeValues, ref Dictionary<string, object> extendedValues)
        {
            //Logging.Debug("Entered");
            //try
            //{
            //    Boolean hasData = false;
            //    if (Cmdr != null && Cmdr.LastStation != null)
            //    {
            //        if (eddi.CurrentStarSystem != null)
            //        {
            //            Station currentStation = eddi.CurrentStarSystem.Stations.SingleOrDefault(s => s.Name == Cmdr.LastStation);
            //            if (currentStation != null)
            //            {
            //                hasData = true;


            //                // Station information
            //                setDecimal(ref decimalValues, "Last station distance from star", currentStation.DistanceFromStar);
            //                setString(ref textValues, "Last station government", currentStation.Government);
            //                setString(ref textValues, "Last station allegiance", currentStation.Allegiance);
            //                setString(ref textValues, "Last station faction", currentStation.Faction);
            //                setString(ref textValues, "Last station state", currentStation.State);
            //                if (currentStation.Economies != null)
            //                {
            //                    if (currentStation.Economies.Count > 0)
            //                    {
            //                        setString(ref textValues, "Last station primary economy", currentStation.Economies[0]);
            //                    }
            //                    if (currentStation.Economies.Count > 1)
            //                    {
            //                        setString(ref textValues, "Last station secondary economy", currentStation.Economies[1]);
            //                    }
            //                    if (currentStation.Economies.Count > 2)
            //                    {
            //                        setString(ref textValues, "Last station tertiary economy", currentStation.Economies[2]);
            //                    }
            //                }

            //                // Services
            //                setBoolean(ref booleanValues, "Last station has refuel", currentStation.HasRefuel);
            //                setBoolean(ref booleanValues, "Last station has repair", currentStation.HasRepair);
            //                setBoolean(ref booleanValues, "Last station has rearm", currentStation.HasRearm);
            //                setBoolean(ref booleanValues, "Last station has market", currentStation.HasMarket);
            //                setBoolean(ref booleanValues, "Last station has black market", currentStation.HasBlackMarket);
            //                setBoolean(ref booleanValues, "Last station has outfitting", currentStation.HasOutfitting);
            //                setBoolean(ref booleanValues, "Last station has shipyard", currentStation.HasShipyard);
            //            }
            //        }
            //    }
            //    if (!hasData)
            //    {
            //        // We don't have any data so remove any info that we might have in history
            //        setDecimal(ref decimalValues, "Last station distance from star", null);
            //        setString(ref textValues, "Last station government", null);
            //        setString(ref textValues, "Last station allegiance", null);
            //        setString(ref textValues, "Last station faction", null);
            //        setString(ref textValues, "Last station state", null);
            //        setString(ref textValues, "Last station primary economy", null);
            //        setString(ref textValues, "Last station secondary economy", null);
            //        setString(ref textValues, "Last station tertiary economy", null);
            //        setBoolean(ref booleanValues, "Last station has refuel", null);
            //        setBoolean(ref booleanValues, "Last station has repair", null);
            //        setBoolean(ref booleanValues, "Last station has rearm", null);
            //        setBoolean(ref booleanValues, "Last station has market", null);
            //        setBoolean(ref booleanValues, "Last station has black market", null);
            //        setBoolean(ref booleanValues, "Last station has outfitting", null);
            //        setBoolean(ref booleanValues, "Last station has shipyard", null);
            //    }
            //    setPluginStatus(ref textValues, "Operational", null, null);
            //}
            //catch (Exception e)
            //{
            //    setPluginStatus(ref textValues, "Failed", "Failed to obtain station data", e);
            //}
            //Logging.Debug("Leaving");
        }

        public static void InvokeNewSystem(ref Dictionary<string, object> state, ref Dictionary<string, Int16?> shortIntValues, ref Dictionary<string, string> textValues, ref Dictionary<string, int?> intValues, ref Dictionary<string, decimal?> decimalValues, ref Dictionary<string, Boolean?> booleanValues, ref Dictionary<string, DateTime?> dateTimeValues, ref Dictionary<string, object> extendedValues)
        {
            //Logging.Debug("Entered");
            //try
            //{
            //    if (Cmdr == null)
            //    {
            //        Logging.Debug("Cmdr is NULL - attempting to refetch");
            //        // Refetch the profile to set our system
            //        InvokeUpdateProfile(ref state, ref shortIntValues, ref textValues, ref intValues, ref decimalValues, ref booleanValues, ref dateTimeValues, ref extendedValues);
            //        if (Cmdr == null)
            //        {
            //            // Still no luck; assume an error of some sort has been logged by InvokeUpdateProfile()
            //            Logging.Debug("Cmdr remained NULL - giving up");
            //            return;
            //        }
            //    }

            //    Logging.Debug("CurrentStarSystem is " + (eddi.CurrentStarSystem == null ? "<null>" : JsonConvert.SerializeObject(eddi.CurrentStarSystem)));

            //    if (Cmdr.StarSystem == null)
            //    {
            //        // No information available
            //        Logging.Debug("No starsystem data available");
            //        return;
            //    }

            //    bool RecordUpdated = false;
            //    if (eddi == null || || eddi.CurrentStarSystem == null || Cmdr.StarSystem != eddi.CurrentStarSystem.Name)
            //    {
            //        if (initialised)
            //        {
            //            Logging.Debug("Starsystem has changed");
            //        }

            //        // The star system has changed or we're in init; obtain the data ready for setting the VA values
            //        EDDIStarSystem CurrentStarSystemData = starSystemRepository.GetEDDIStarSystem(Cmdr.StarSystem);
            //        Logging.Debug("CurrentStarSystemData is " + (CurrentStarSystemData == null ? "<null>" : JsonConvert.SerializeObject(CurrentStarSystemData)));
            //        if (CurrentStarSystemData == null)
            //        {
            //            Logging.Debug("Creating new starsystemdata");
            //            // We have no record of this system; set it up
            //            CurrentStarSystemData = new EDDIStarSystem();
            //            CurrentStarSystemData.Name = Cmdr.StarSystem;
            //            CurrentStarSystemData.StarSystem = DataProviderService.GetSystemData(Cmdr.StarSystem, Cmdr.StarSystemX, Cmdr.StarSystemY, Cmdr.StarSystemZ);
            //            CurrentStarSystemData.LastVisit = DateTime.Now;
            //            CurrentStarSystemData.StarSystemLastUpdated = CurrentStarSystemData.LastVisit;
            //            CurrentStarSystemData.TotalVisits = 1;
            //            RecordUpdated = true;
            //        }
            //        else
            //        {
            //            Logging.Debug("Checking existing starsystemdata");
            //            if (CurrentStarSystemData.StarSystem == null || (DateTime.Now - CurrentStarSystemData.StarSystemLastUpdated).TotalHours > 12)
            //            {
            //                Logging.Debug("Refreshing stale or missing data");
            //                // Data is stale; refresh it
            //                CurrentStarSystemData.StarSystem = DataProviderService.GetSystemData(CurrentStarSystemData.Name, Cmdr.StarSystemX, Cmdr.StarSystemY, Cmdr.StarSystemZ);
            //                CurrentStarSystemData.StarSystemLastUpdated = CurrentStarSystemData.LastVisit;
            //                RecordUpdated = true;
            //            }
            //            // Only update if we have moved (as opposed to reinitialising here)
            //            if (initialised)
            //            {
            //                Logging.Debug("Updating visit information");
            //                CurrentStarSystemData.PreviousVisit = CurrentStarSystemData.LastVisit;
            //                CurrentStarSystemData.LastVisit = DateTime.Now;
            //                CurrentStarSystemData.TotalVisits++;
            //                RecordUpdated = true;
            //            }
            //        }
            //        Logging.Debug("CurrentStarSystemData is now " + (CurrentStarSystemData == null ? "<null>" : JsonConvert.SerializeObject(CurrentStarSystemData)));
            //        if (RecordUpdated)
            //        {
            //            Logging.Debug("Storing updated starsystemdata");
            //            eddi.starSystemRepository.SaveEDDIStarSystem(CurrentStarSystemData);
            //        }
            //        setString(ref textValues, "System comment", CurrentStarSystemData.Comment);

            //        StarSystem ThisStarSystem = CurrentStarSystemData.StarSystem;
            //        LastStarSystem = CurrentStarSystem;
            //        CurrentStarSystem = ThisStarSystem;

            //        Logging.Debug("CurrentStarSystem is now " + (eddi.CurrentStarSystem == null ? "<null>" : JsonConvert.SerializeObject(eddi.CurrentStarSystem)));
            //        Logging.Debug("LastStarSystem is now " + (eddi.LastStarSystem == null ? "<null>" : JsonConvert.SerializeObject(eddi.LastStarSystem)));

            //        setStarSystemValues(eddi.Cmdr, eddi.CurrentStarSystem, "System", ref state, ref shortIntValues, ref textValues, ref intValues, ref decimalValues, ref booleanValues, ref dateTimeValues, ref extendedValues);

            //        setInt(ref intValues, "System  visits", CurrentStarSystemData.TotalVisits);
            //        setDateTime(ref dateTimeValues, "System  previous visit", CurrentStarSystemData.PreviousVisit);
            //        setInt(ref intValues, "System  minutes since previous visit", CurrentStarSystemData.PreviousVisit == null ? (int?)null : (int)(DateTime.Now - (DateTime)CurrentStarSystemData.PreviousVisit).TotalMinutes);

            //        setStarSystemValues(eddi.Cmdr, eddi.LastStarSystem, "Last system", ref state, ref shortIntValues, ref textValues, ref intValues, ref decimalValues, ref booleanValues, ref dateTimeValues, ref extendedValues);


            //        Logging.Debug(" Setting last jump");
            //        if (eddi.LastStarSystem.X != null && eddi.CurrentStarSystem.X != null)
            //        {
            //            setDecimal(ref decimalValues, "Last jump", (decimal)Math.Round(Math.Sqrt(Math.Pow((double)(eddi.CurrentStarSystem.X - eddi.LastStarSystem.X), 2) + Math.Pow((double)(eddi.CurrentStarSystem.Y - eddi.LastStarSystem.Y), 2) + Math.Pow((double)(eddi.CurrentStarSystem.Z - eddi.LastStarSystem.Z), 2)), 2));
            //        }
            //        Logging.Debug("Set last jump");

            //    }

            //    if (Logging.Verbose)
            //    {
            //        Logging.Debug("Resultant shortint values " + JsonConvert.SerializeObject(shortIntValues));
            //        Logging.Debug("Resultant text values " + JsonConvert.SerializeObject(textValues));
            //        Logging.Debug("Resultant int values " + JsonConvert.SerializeObject(intValues));
            //        Logging.Debug("Resultant decimal values " + JsonConvert.SerializeObject(decimalValues));
            //        Logging.Debug("Resultant boolean values " + JsonConvert.SerializeObject(booleanValues));
            //        Logging.Debug("Resultant datetime values " + JsonConvert.SerializeObject(dateTimeValues));
            //    }

            //    setPluginStatus(ref textValues, "Operational", null, null);
            //}
            //catch (Exception e)
            //{
            //    setPluginStatus(ref textValues, "Failed", "Failed to obtain system data", e);
            //}
        }

        private static void setCommanderValues(Commander cmdr, ref Dictionary<string, object> state, ref Dictionary<string, Int16?> shortIntValues, ref Dictionary<string, string> textValues, ref Dictionary<string, int?> intValues, ref Dictionary<string, decimal?> decimalValues, ref Dictionary<string, Boolean?> booleanValues, ref Dictionary<string, DateTime?> dateTimeValues, ref Dictionary<string, object> extendedValues)
        {
            Logging.Debug("Setting commander information");
            setString(ref textValues, "Name", cmdr == null ? null : cmdr.name);
            setInt(ref intValues, "Combat rating", cmdr == null ? (int?)null : cmdr.combatrating);
            setString(ref textValues, "Combat rank", cmdr == null ? null : cmdr.combatrank);
            setInt(ref intValues, "Trade rating", cmdr == null ? (int?)null : cmdr.traderating);
            setString(ref textValues, "Trade rank", cmdr == null ? null : cmdr.traderank);
            setInt(ref intValues, "Explore rating", cmdr == null ? (int?)null : cmdr.explorationrating);
            setString(ref textValues, "Explore rank", cmdr == null ? null : cmdr.explorationrank);
            setInt(ref intValues, "Empire rating", cmdr == null ? (int?)null : cmdr.empirerating);
            setString(ref textValues, "Empire rank", cmdr == null ? null : cmdr.empirerank);
            setInt(ref intValues, "Federation rating", cmdr == null ? (int?)null : cmdr.federationrating);
            setString(ref textValues, "Federation rank", cmdr == null ? null : cmdr.federationrank);
            setDecimal(ref decimalValues, "Credits", cmdr == null ? (decimal?)null : cmdr.credits);
            setString(ref textValues, "Credits (spoken)", cmdr == null ? null : humanize(cmdr.credits));
            setDecimal(ref decimalValues, "Debt", cmdr == null ? (decimal?)null : cmdr.debt);
            setString(ref textValues, "Debt (spoken)", cmdr == null ? null : humanize(cmdr.debt));
            Logging.Debug("Set commander information");
        }

        private static void setShipValues(Ship ship, string prefix, ref Dictionary<string, object> state, ref Dictionary<string, Int16?> shortIntValues, ref Dictionary<string, string> textValues, ref Dictionary<string, int?> intValues, ref Dictionary<string, decimal?> decimalValues, ref Dictionary<string, Boolean?> booleanValues, ref Dictionary<string, DateTime?> dateTimeValues, ref Dictionary<string, object> extendedValues)
        {
            Logging.Debug("Setting ship information");
            //
            // Ship data
            //
            setString(ref textValues, prefix + " model", ship == null ? null : ship.model);
            setString(ref textValues, prefix + " model (spoken)", ship == null ? null : Translations.ShipModel(ship.model));
            setString(ref textValues, prefix + " callsign", ship == null ? null : ship.callsign);
            setString(ref textValues, prefix + " callsign (spoken)", ship == null ? null : Translations.CallSign(ship.callsign));
            setString(ref textValues, prefix + " name", ship == null ? null : ship.name);
            setString(ref textValues, prefix + " role", ship == null ? null : ship.role.ToString());
            setString(ref textValues, prefix + " size", ship == null ? null : ship.size.ToString());
            setDecimal(ref decimalValues, prefix + " value", ship == null ? (decimal?)null : ship.value);
            setString(ref textValues, prefix + " value (spoken)", ship == null ? null : humanize(ship.value));
            setDecimal(ref decimalValues, prefix + " health", ship == null ? (decimal?)null : ship.Health);
            setInt(ref intValues, prefix + " cargo capacity", ship == null ? (int?)null : ship.cargocapacity);
            setInt(ref intValues, prefix + " cargo carried", ship == null ? (int?)null : ship.cargocarried);
            // Add number of limpets carried
            int limpets = 0;
            foreach (Cargo cargo in ship.cargo)
            {
                if (cargo.Commodity.Name == "Limpet")
                {
                    limpets += cargo.Quantity;
                }
            }
            setInt(ref intValues, prefix + " limpets carried", limpets);

            SetShipModuleValues(prefix + " bulkheads", ship == null ? null : ship.Bulkheads, ref textValues, ref intValues, ref decimalValues);
            SetShipModuleOutfittingValues(prefix + " bulkheads", ship == null ? null : ship.Bulkheads, eddi.Outfitting, ref textValues, ref decimalValues);
            SetShipModuleValues(prefix + " power plant", ship == null ? null : ship.PowerPlant, ref textValues, ref intValues, ref decimalValues);
            SetShipModuleOutfittingValues(prefix + " power plant", ship == null ? null : ship.PowerPlant, eddi.Outfitting, ref textValues, ref decimalValues);
            SetShipModuleValues(prefix + " thrusters", ship == null ? null : ship.Thrusters, ref textValues, ref intValues, ref decimalValues);
            SetShipModuleOutfittingValues(prefix + " thrusters", ship == null ? null : ship.Thrusters, eddi.Outfitting, ref textValues, ref decimalValues);
            SetShipModuleValues(prefix + " frame shift drive", ship == null ? null : ship.FrameShiftDrive, ref textValues, ref intValues, ref decimalValues);
            SetShipModuleOutfittingValues(prefix + " frame shift drive", ship == null ? null : ship.FrameShiftDrive, eddi.Outfitting, ref textValues, ref decimalValues);
            SetShipModuleValues(prefix + " life support", ship == null ? null : ship.LifeSupport, ref textValues, ref intValues, ref decimalValues);
            SetShipModuleOutfittingValues(prefix + " life support", ship == null ? null : ship.LifeSupport, eddi.Outfitting, ref textValues, ref decimalValues);
            SetShipModuleValues(prefix + " power distributor", ship == null ? null : ship.PowerDistributor, ref textValues, ref intValues, ref decimalValues);
            SetShipModuleOutfittingValues(prefix + " power distributor", ship == null ? null : ship.PowerDistributor, eddi.Outfitting, ref textValues, ref decimalValues);
            SetShipModuleValues(prefix + " sensors", ship == null ? null : ship.Sensors, ref textValues, ref intValues, ref decimalValues);
            SetShipModuleOutfittingValues(prefix + " sensors", ship == null ? null : ship.Sensors, eddi.Outfitting, ref textValues, ref decimalValues);
            SetShipModuleValues(prefix + " fuel tank", ship == null ? null : ship.FuelTank, ref textValues, ref intValues, ref decimalValues);
            SetShipModuleOutfittingValues(prefix + " fuel tank", ship == null ? null : ship.FuelTank, eddi.Outfitting, ref textValues, ref decimalValues);

            // Hardpoints
            if (ship != null)
            {
                int numTinyHardpoints = 0;
                int numSmallHardpoints = 0;
                int numMediumHardpoints = 0;
                int numLargeHardpoints = 0;
                int numHugeHardpoints = 0;
                foreach (Hardpoint Hardpoint in ship.Hardpoints)
                {
                    string baseHardpointName = prefix;
                    switch (Hardpoint.Size)
                    {
                        case 0:
                            baseHardpointName = " tiny hardpoint " + ++numTinyHardpoints;
                            break;
                        case 1:
                            baseHardpointName = " small hardpoint " + ++numSmallHardpoints;
                            break;
                        case 2:
                            baseHardpointName = " medium hardpoint " + ++numMediumHardpoints;
                            break;
                        case 3:
                            baseHardpointName = " large hardpoint " + ++numLargeHardpoints;
                            break;
                        case 4:
                            baseHardpointName = " huge hardpoint " + ++numHugeHardpoints;
                            break;
                    }

                    setBoolean(ref booleanValues, baseHardpointName + " occupied", Hardpoint.Module != null);
                    SetShipModuleValues(baseHardpointName + " module", Hardpoint.Module, ref textValues, ref intValues, ref decimalValues);
                    SetShipModuleOutfittingValues(baseHardpointName + " module", Hardpoint.Module, eddi.Outfitting, ref textValues, ref decimalValues);
                }

                setInt(ref intValues, prefix + " hardpoints", numSmallHardpoints + numMediumHardpoints + numLargeHardpoints + numHugeHardpoints);
                setInt(ref intValues, prefix + " utility slots", numTinyHardpoints);
                // Compartments
                int curCompartment = 0;
                foreach (Compartment Compartment in ship.Compartments)
                {
                    string baseCompartmentName = prefix + " compartment " + ++curCompartment;
                    setInt(ref intValues, baseCompartmentName + " size", Compartment.Size);
                    setBoolean(ref booleanValues, baseCompartmentName + " occupied", Compartment.Module != null);
                    SetShipModuleValues(baseCompartmentName + " module", Compartment.Module, ref textValues, ref intValues, ref decimalValues);
                    SetShipModuleOutfittingValues(baseCompartmentName + " module", Compartment.Module, eddi.Outfitting, ref textValues, ref decimalValues);
                }
                setInt(ref intValues, prefix + " compartments", curCompartment);
            }


            // Fetch the star system in which the ship is stored
            StarSystem StoredShipStarSystem = eddi.starSystemRepository.GetOrCreateStarSystem(ship.StarSystem);

            // Have to grab a local copy of our star system as CurrentStarSystem might not have been initialised yet
            StarSystem ThisStarSystem = eddi.starSystemRepository.GetStarSystem(eddi.CurrentStarSystem.name);

            // Work out the distance to the system where the ship is stored if we can
            if (ThisStarSystem != null && ThisStarSystem.x != null && StoredShipStarSystem.x != null)
            {
                decimal distance = (decimal)Math.Round(Math.Sqrt(Math.Pow((double)(ThisStarSystem.x - StoredShipStarSystem.x), 2)
                    + Math.Pow((double)(ThisStarSystem.y - StoredShipStarSystem.y), 2)
                    + Math.Pow((double)(ThisStarSystem.z - StoredShipStarSystem.z), 2)), 2);
                setDecimal(ref decimalValues, prefix + " distance", distance);
            }
            else
            {
                // We don't know how far away the ship is
                setDecimal(ref decimalValues, prefix + " distance", (decimal?)null);
            }


            Logging.Debug("Set ship information");
        }

        private static void setStarSystemValues(Commander cmdr, StarSystem system, string prefix, ref Dictionary<string, object> state, ref Dictionary<string, Int16?> shortIntValues, ref Dictionary<string, string> textValues, ref Dictionary<string, int?> intValues, ref Dictionary<string, decimal?> decimalValues, ref Dictionary<string, Boolean?> booleanValues, ref Dictionary<string, DateTime?> dateTimeValues, ref Dictionary<string, object> extendedValues)
        {
            Logging.Debug("Setting system information (" + prefix + ")");
            setString(ref textValues, prefix + " name", system == null ? null : system.name);
            setString(ref textValues, prefix + " name (spoken)", system == null ? null : Translations.StarSystem(system.name));
            setDecimal(ref decimalValues, prefix + " population", system == null ? null : (decimal?)system.population);
            setString(ref textValues, prefix + " population (spoken)", system == null ? null : humanize(system.population));
            setString(ref textValues, prefix + " allegiance", system == null ? null : system.allegiance);
            setString(ref textValues, prefix + " government", system == null ? null : system.government);
            setString(ref textValues, prefix + " faction", system == null ? null : system.faction);
            setString(ref textValues, prefix + " primary economy", system == null ? null : system.primaryeconomy);
            setString(ref textValues, prefix + " state", system == null ? null : system.state);
            setString(ref textValues, prefix + " security", system == null ? null : system.security);
            setString(ref textValues, prefix + " power", system == null ? null : system.power);
            setString(ref textValues, prefix + " power (spoken)", Translations.Power(eddi.CurrentStarSystem.power));
            setString(ref textValues, prefix + " power state", system == null ? null : system.powerState);
            setDecimal(ref decimalValues, prefix + " X", system == null ? null : system.x);
            setDecimal(ref decimalValues, prefix + " Y", system == null ? null : system.y);
            setDecimal(ref decimalValues, prefix + " Z", system == null ? null : system.z);

            if (eddi.HomeStarSystem != null && eddi.HomeStarSystem.x != null && system.x != null)
            {
                setDecimal(ref decimalValues, prefix + " distance from home", (decimal)Math.Round(Math.Sqrt(Math.Pow((double)(system.x - eddi.HomeStarSystem.x), 2) + Math.Pow((double)(system.y - eddi.HomeStarSystem.y), 2) + Math.Pow((double)(system.z - eddi.HomeStarSystem.z), 2)), 2));
            }

            //
            if (system != null)
            {
                foreach (Station Station in system.stations)
                {
                    setString(ref textValues, prefix + " station name", Station.Name);
                }
                setInt(ref intValues, prefix + " stations", system.stations.Count);
                setInt(ref intValues, prefix + " orbital stations", system.stations.Count(s => !s.IsPlanetary()));
                setInt(ref intValues, prefix + " starports", system.stations.Count(s => s.IsStarport()));
                setInt(ref intValues, prefix + " outposts", system.stations.Count(s => s.IsOutpost()));
                setInt(ref intValues, prefix + " planetary stations", system.stations.Count(s => s.IsPlanetary()));
                setInt(ref intValues, prefix + " planetary outposts", system.stations.Count(s => s.IsPlanetaryOutpost()));
                setInt(ref intValues, prefix + " planetary ports", system.stations.Count(s => s.IsPlanetaryPort()));
            }

            //// Allegiance-specific rank
            //string systemRank = "Commander";
            //if (cmdr.name != null) // using Name as a canary to see if the data is missing
            //{
            //    if (system.allegiance == "Federation" && cmdr.federationrating >= minFederationRatingForTitle)
            //    {
            //        systemRank = cmdr.federationrank;
            //    }
            //    else if (system.allegiance == "Empire" && cmdr.empirerating >= minEmpireRatingForTitle)
            //    {
            //        systemRank = cmdr.empirerank;
            //    }
            //}
            //setString(ref textValues, prefix + " rank", systemRank);

            Logging.Debug("Set system information (" + prefix + ")");
        }

        /// <summary>
        /// Say something inside the cockpit with text-to-speech
        /// </summary>
        public static void InvokeSay(ref Dictionary<string, object> state, ref Dictionary<string, Int16?> shortIntValues, ref Dictionary<string, string> textValues, ref Dictionary<string, int?> intValues, ref Dictionary<string, decimal?> decimalValues, ref Dictionary<string, Boolean?> booleanValues, ref Dictionary<string, DateTime?> dateTimeValues, ref Dictionary<string, object> extendedValues)
        {
            try
            {
                string script = null;
                foreach (string key in textValues.Keys)
                {
                    if (key.EndsWith(" script"))
                    {
                        script = textValues[key];
                        break;
                    }
                }
                if (script == null)
                {
                    return;
                }
                //eddi.speechService.Say(eddi.Cmdr, eddi.Ship, script);
            }
            catch (Exception e)
            {
                setPluginStatus(ref textValues, "Failed", "Failed to run internal speech system", e);
            }
        }

        /// <summary>
        /// Transmit something on the radio with text-to-speech
        /// </summary>
        public static void InvokeTransmit(ref Dictionary<string, object> state, ref Dictionary<string, Int16?> shortIntValues, ref Dictionary<string, string> textValues, ref Dictionary<string, int?> intValues, ref Dictionary<string, decimal?> decimalValues, ref Dictionary<string, Boolean?> booleanValues, ref Dictionary<string, DateTime?> dateTimeValues, ref Dictionary<string, object> extendedValues)
        {
            try
            {
                string script = null;
                foreach (string key in textValues.Keys)
                {
                    if (key.EndsWith(" script"))
                    {
                        script = textValues[key];
                        break;
                    }
                }
                if (script == null)
                {
                    return;
                }
                //eddi.speechService.Transmit(eddi.Cmdr, eddi.Ship, script);
            }
            catch (Exception e)
            {
                setPluginStatus(ref textValues, "Failed", "Failed to run internal speech system", e);
            }
        }

        /// <summary>
        /// Receive something on the radio with text-to-speech
        /// </summary>
        public static void InvokeReceive(ref Dictionary<string, object> state, ref Dictionary<string, Int16?> shortIntValues, ref Dictionary<string, string> textValues, ref Dictionary<string, int?> intValues, ref Dictionary<string, decimal?> decimalValues, ref Dictionary<string, Boolean?> booleanValues, ref Dictionary<string, DateTime?> dateTimeValues, ref Dictionary<string, object> extendedValues)
        {
            try
            {
                string script = null;
                foreach (string key in textValues.Keys)
                {
                    if (key.EndsWith(" script"))
                    {
                        script = textValues[key];
                        break;
                    }
                }
                if (script == null)
                {
                    return;
                }
                //eddi.speechService.Receive(eddi.Cmdr, eddi.Ship, script);
            }
            catch (Exception e)
            {
                setPluginStatus(ref textValues, "Failed", "Failed to run internal speech system", e);
            }
        }

        /// <summary>
        /// Generate a callsign
        /// </summary>
        public static void InvokeGenerateCallsign(ref Dictionary<string, object> state, ref Dictionary<string, Int16?> shortIntValues, ref Dictionary<string, string> textValues, ref Dictionary<string, int?> intValues, ref Dictionary<string, decimal?> decimalValues, ref Dictionary<string, Boolean?> booleanValues, ref Dictionary<string, DateTime?> dateTimeValues, ref Dictionary<string, object> extendedValues)
        {
            try
            {
                string callsign = Ship.generateCallsign();
                setString(ref textValues, "EDDI generated callsign", callsign);
                setString(ref textValues, "EDDI generated callsign (spoken)", Translations.CallSign(callsign));
            }
            catch (Exception e)
            {
                setPluginStatus(ref textValues, "Failed", "Failed to generate callsign", e);
            }
        }

        /// <summary>
        /// Send a system distance to the starmap service
        /// </summary>
        public static void InvokeStarMapSystemDistance(ref Dictionary<string, object> state, ref Dictionary<string, Int16?> shortIntValues, ref Dictionary<string, string> textValues, ref Dictionary<string, int?> intValues, ref Dictionary<string, decimal?> decimalValues, ref Dictionary<string, Boolean?> booleanValues, ref Dictionary<string, DateTime?> dateTimeValues, ref Dictionary<string, object> extendedValues)
        {
            try
            {
                string system = null;
                foreach (string key in textValues.Keys)
                {
                    if (key == "EDDI remote system name")
                    {
                        system = textValues[key];
                    }
                }
                if (system == null)
                {
                    return;
                }

                decimal? distance = null;
                foreach (string key in decimalValues.Keys)
                {
                    if (key == "EDDI remote system distance")
                    {
                        distance = decimalValues[key];
                    }
                }
                if (distance == null)
                {
                    return;
                }

                if (eddi.Cmdr != null && eddi.starMapService != null)
                {
                    eddi.starMapService.sendStarMapDistance(eddi.CurrentStarSystem.name, system, (decimal)distance);
                }
            }
            catch (Exception e)
            {
                setPluginStatus(ref textValues, "Failed", "Failed to send system distance to EDSM", e);
            }
        }

        /// <summary>
        /// Send a comment to the starmap service and store locally
        /// </summary>
        public static void InvokeStarMapSystemComment(ref Dictionary<string, object> state, ref Dictionary<string, Int16?> shortIntValues, ref Dictionary<string, string> textValues, ref Dictionary<string, int?> intValues, ref Dictionary<string, decimal?> decimalValues, ref Dictionary<string, Boolean?> booleanValues, ref Dictionary<string, DateTime?> dateTimeValues, ref Dictionary<string, object> extendedValues)
        {
            try
            {
                string comment = null;
                foreach (string key in textValues.Keys)
                {
                    if (key == "EDDI system comment")
                    {
                        comment = textValues[key];
                    }
                }
                if (comment == null)
                {
                    return;
                }

                if (eddi.Cmdr != null)
                {
                    // Store locally
                    StarSystem here = eddi.starSystemRepository.GetOrCreateStarSystem(eddi.CurrentStarSystem.name);
                    here.comment = comment == "" ? null : comment;
                    eddi.starSystemRepository.SaveStarSystem(here);

                    if (eddi.starMapService != null)
                    {
                        // Store in EDSM
                        eddi.starMapService.sendStarMapComment(eddi.CurrentStarSystem.name, comment);
                    }
                }
            }
            catch (Exception e)
            {
                setPluginStatus(ref textValues, "Failed", "Failed to store system comment", e);
            }
        }

        private static void setInt(ref Dictionary<string, int?> values, string key, int? value)
        {
            if (values.ContainsKey(key))
                values[key] = value;
            else
                values.Add(key, value);
        }

        private static void setDecimal(ref Dictionary<string, decimal?> values, string key, decimal? value)
        {
            if (values.ContainsKey(key))
                values[key] = value;
            else
                values.Add(key, value);
        }

        private static void setBoolean(ref Dictionary<string, bool?> values, string key, bool? value)
        {
            if (values.ContainsKey(key))
                values[key] = value;
            else
                values.Add(key, value);
        }

        private static void setString(ref Dictionary<string, string> values, string key, string value)
        {
            if (values.ContainsKey(key))
                values[key] = value;
            else
                values.Add(key, value);
        }

        private static void setDateTime(ref Dictionary<string, DateTime?> values, string key, DateTime? value)
        {
            if (values.ContainsKey(key))
                values[key] = value;
            else
                values.Add(key, value);
        }

        private static void setPluginStatus(ref Dictionary<string, string> values, string status, string error, Exception exception)
        {
            setString(ref values, "EDDI status", status);
            if (status == "Operational")
            {
                setString(ref values, "EDDI error", null);
                setString(ref values, "EDDI exception", null);
            }
            else
            {
                Logging.Warn("EDDI error: " + error);
                Logging.Warn("EDDI exception: " + (exception == null ? "<null>" : exception.ToString()));
                setString(ref values, "EDDI error", error);
                setString(ref values, "EDDI exception", exception == null ? null : exception.ToString());
            }
        }

        public static string humanize(long? value)
        {
            if (value == null)
            {
                return null;
            }

            if (value == 0)
            {
                return "zero";
            }

            int number;
            int nextDigit;
            string order;
            int digits = (int)Math.Log10((double)value);
            if (digits < 3)
            {
                // Units
                number = (int)value;
                order = "";
                nextDigit = 0;
            }
            else if (digits < 6)
            {
                // Thousands
                number = (int)(value / 1000);
                order = "thousand";
                nextDigit = (((int)value) - (number * 1000)) / 100;
            }
            else if (digits < 9)
            {
                // Millions
                number = (int)(value / 1000000);
                order = "million";
                nextDigit = (((int)value) - (number * 1000000)) / 100000;
            }
            else
            {
                // Billions
                number = (int)(value / 1000000000);
                order = "billion";
                nextDigit = (int)(((long)value) - ((long)number * 1000000000)) / 100000000;
            }

            if (order == "")
            {
                return "" + number;
            }
            else
            {
                if (number > 60)
                {
                    if (nextDigit < 6)
                    {
                        return "Over " + number + " " + order;
                    }
                    else
                    {
                        return "Nearly " + (number + 1) + " " + order;
                    }
                }
                switch (nextDigit)
                {
                    case 0:
                        return "just over " + number + " " + order;
                    case 1:
                        return "over " + number + " " + order;
                    case 2:
                        return "well over " + number + " " + order;
                    case 3:
                        return "on the way to " + number + " and a half " + order;
                    case 4:
                        return "nearly " + number + " and a half " + order;
                    case 5:
                        return "around " + number + " and a half " + order;
                    case 6:
                        return "just over " + number + " and a half " + order;
                    case 7:
                        return "well over " + number + " and a half " + order;
                    case 8:
                        return "on the way to " + (number + 1) + " " + order;
                    case 9:
                        return "nearly " + (number + 1) + " " + order;
                    default:
                        return "around " + number + " " + order;
                }
            }

        }

        //// Debug method to allow manual updating of the system
        //public static void updateSystem(string system)
        //{
        //    if (eddi.Cmdr != null)
        //    {
        //        eddi.Cmdr.StarSystem = system;
        //    }
        //}
    }
}
