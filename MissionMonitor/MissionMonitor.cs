using Eddi;
using EddiDataDefinitions;
using EddiDataProviderService;
using EddiEvents;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Data;
using System.Windows.Controls;
using System.Windows.Threading;
using Utilities;

namespace EddiMissionMonitor
{
    /**
     * Monitor missions for the commander
     */
    public class MissionMonitor : EDDIMonitor
    {
        // Keep track of status
        private bool running;

        // Observable collection for us to handle changes
        public ObservableCollection<Mission> missions { get; private set; }

        private DateTime updateDat;
        public int missionsCount;
        public int? missionWarning;
        public string missionsRouteList;
        public decimal missionsRouteDistance;

        private static readonly object missionsLock = new object();
        public event EventHandler MissionUpdatedEvent;

        public string MonitorName()
        {
            return "Mission monitor";
        }

        public string LocalizedMonitorName()
        {
            return Properties.MissionMonitor.mission_monitor_name;
        }

        public string MonitorVersion()
        {
            return "1.0.0";
        }

        public string MonitorDescription()
        {
            return Properties.MissionMonitor.mission_monitor_desc;
        }

        public bool IsRequired()
        {
            return true;
        }

        public MissionMonitor()
        {
            missions = new ObservableCollection<Mission>();
            BindingOperations.CollectionRegistering += Missions_CollectionRegistering;
            initializeMissionMonitor();
        }

        public void initializeMissionMonitor(MissionMonitorConfiguration configuration = null)
        {
            readMissions(configuration);
            Logging.Info("Initialised " + MonitorName() + " " + MonitorVersion());
        }

        private void Missions_CollectionRegistering(object sender, CollectionRegisteringEventArgs e)
        {
            if (Application.Current != null)
            {
                // Synchronize this collection between threads
                BindingOperations.EnableCollectionSynchronization(missions, missionsLock);
            }
            else
            {
                // If started from VoiceAttack, the dispatcher is on a different thread. Invoke synchronization there.
                Dispatcher.CurrentDispatcher.Invoke(() => { BindingOperations.EnableCollectionSynchronization(missions, missionsLock); });
            }
        }
        public bool NeedsStart()
        {
            return true;
        }

        public void Start()
        {
            _start();
        }

        public void Stop()
        {
            running = false;
        }

        public void Reload()
        {
            readMissions();
            Logging.Info("Reloaded " + MonitorName() + " " + MonitorVersion());

        }

        public void _start()
        {
            running = true;

            while (running)
            {
                List<Mission> missionsList;
                lock (missionsLock)
                {
                    missionsList = missions.ToList();
                }
                if (missionsList != null)
                {
                    foreach (Mission mission in missionsList)
                    {
                        if (mission.expiry != null && mission.statusEDName != "Failed")
                        {
                            TimeSpan span = (DateTime)mission.expiry - DateTime.UtcNow;
                            if (span.Days > 6)
                            {
                                int weeks = Decimal.ToInt32(span.Days / 7);
                                int days = span.Days - weeks * 7;
                                mission.timeremaining = weeks.ToString() + "W " + days.ToString() + "D ";
                            }
                            else
                            {
                                mission.timeremaining = span.Days.ToString() + "D ";
                            }
                            mission.timeremaining += span.Hours.ToString() + "H " + span.Minutes.ToString() + "MIN";

                            if (mission.expiry < DateTime.UtcNow)
                            {
                                EDDI.Instance.enqueueEvent(new MissionExpiredEvent(DateTime.UtcNow, mission.missionid, mission.name));
                            }
                            else if (mission.expiry < DateTime.UtcNow.AddMinutes(missionWarning ?? 60))
                            {
                                if (!mission.expiring)
                                {
                                    mission.expiring = true;
                                    EDDI.Instance.enqueueEvent(new MissionWarningEvent(DateTime.UtcNow, mission.missionid, mission.name, (int)span.TotalMinutes));
                                }
                            }
                            else if (mission.expiring)
                            {
                                mission.expiring = false;
                            }
                        }
                        else
                        {
                            mission.timeremaining = String.Empty;
                        }
                    }
                }
                Thread.Sleep(5000);
            }
        }

        public UserControl ConfigurationTabItem()
        {
            return new ConfigurationWindow();
        }

        public void EnableConfigBinding(MainWindow configWindow)
        {
            configWindow.Dispatcher.Invoke(() => { BindingOperations.EnableCollectionSynchronization(missions, missionsLock); });
        }

        public void DisableConfigBinding(MainWindow configWindow)
        {
            configWindow.Dispatcher.Invoke(() => { BindingOperations.DisableCollectionSynchronization(missions); });
        }

        public void HandleProfile(JObject profile)
        {
        }

        public void PostHandle(Event @event)
        {
        }

        public void PreHandle(Event @event)
        {
            Logging.Debug("Received pre-event " + JsonConvert.SerializeObject(@event));

            // Handle the events that we care about
            if (@event is DataScannedEvent)
            {
                //
                handleDataScannedEvent((DataScannedEvent)@event);
            }
            else if (@event is PassengersEvent)
            {
                //
                handlePassengersEvent((PassengersEvent)@event);
            }
            else if (@event is MissionsEvent)
            {
                //
                handleMissionsEvent((MissionsEvent)@event);
            }
            else if (@event is CommunityGoalEvent)
            {
                //
                handleCommunityGoalEvent((CommunityGoalEvent)@event);
            }
            else if (@event is CargoDepotEvent)
            {
                //
                handleCargoDepotEvent((CargoDepotEvent)@event);
            }
            else if (@event is MissionAbandonedEvent)
            {
                //
                handleMissionAbandonedEvent((MissionAbandonedEvent)@event);
            }
            else if (@event is MissionAcceptedEvent)
            {
                //
                handleMissionAcceptedEvent((MissionAcceptedEvent)@event);
            }
            else if (@event is MissionCompletedEvent)
            {
                //
                handleMissionCompletedEvent((MissionCompletedEvent)@event);
            }
            else if (@event is MissionExpiredEvent)
            {
                //
                handleMissionExpiredEvent((MissionExpiredEvent)@event);
            }
            else if (@event is MissionFailedEvent)
            {
                //
                handleMissionFailedEvent((MissionFailedEvent)@event);
            }
            else if (@event is MissionRedirectedEvent)
            {
                //
                handleMissionRedirectedEvent((MissionRedirectedEvent)@event);
            }
        }

        private void handleDataScannedEvent(DataScannedEvent @event)
        {
            if (@event.timestamp > updateDat)
            {
                updateDat = @event.timestamp;
                if (_handleDataScannedEvent(@event))
                {
                    writeMissions();
                }
            }
        }

        public bool _handleDataScannedEvent(DataScannedEvent @event)
        {
            bool update = false;
            string datalinktypeEDName = DataScan.FromName(@event.datalinktype).edname;
            if (datalinktypeEDName == "TouristBeacon")
            {
                foreach (Mission mission in missions.ToList())
                {
                    string type = mission.typeEDName.ToLowerInvariant();
                    switch (type)
                    {
                        case "sightseeing":
                            {
                                DestinationSystem system = mission.destinationsystems
                                    .FirstOrDefault(s => s.name == EDDI.Instance?.CurrentStarSystem?.name);
                                if (system != null)
                                {
                                    system.visited = true;
                                    if (mission.destinationsystems.Where(s => s.visited == false).Count() > 0)
                                    {
                                        // Set destination system to next in chain & trigger a 'Mission redirected' event
                                        string destinationsystem = mission.destinationsystems?
                                            .FirstOrDefault(s => s.visited == false).name;
                                        EDDI.Instance.enqueueEvent(new MissionRedirectedEvent(DateTime.Now, mission.missionid, mission.name, null, null, destinationsystem, EDDI.Instance?.CurrentStarSystem?.name));
                                    }
                                    update = true;
                                }
                            }
                            break;
                    }
                    if (update)
                    {
                        break;
                    }
                }
            }
            return update;
        }

        private void handleMissionsEvent(MissionsEvent @event)
        {
            if (@event.timestamp > updateDat)
            {
                updateDat = @event.timestamp;
                if (_handleMissionsEvent(@event))
                {
                    writeMissions();
                }
            }
        }

        public bool _handleMissionsEvent(MissionsEvent @event)
        {
            bool update = false;
            foreach (Mission mission in @event.missions)
            {
                Mission missionEntry = missions.FirstOrDefault(m => m.missionid == mission.missionid);

                // If the mission exists in the log, update status
                if (missionEntry != null)
                {
                    switch (mission.statusEDName)
                    {
                        case "Active":
                            {
                                if (missionEntry.statusEDName == "Failed" && mission.expiry > missionEntry.expiry)
                                {
                                    missionEntry.expiry = mission.expiry;
                                    missionEntry.statusDef = MissionStatus.FromEDName("Active");
                                    update = true;
                                }

                                if (missionEntry.statusEDName == "Active" && missionEntry.destinationsystem == missionEntry.originsystem)
                                {
                                    switch (missionEntry.typeEDName)
                                    {
                                        case "assassinate":
                                        case "disable":
                                        case "hack":
                                        case "longdistanceexpedition":
                                        case "passengervip":
                                        case "piracy":
                                        case "rescue":
                                        case "salvage":
                                        case "scan":
                                        case "sightseeing":
                                            {
                                                missionEntry.statusDef = MissionStatus.FromEDName("Complete");
                                                update = true;
                                            }
                                            break;
                                    }
                                }
                            }
                            break;
                        case "Failed":
                            {
                                if (missionEntry.statusDef.edname != "Failed")
                                {
                                    missionEntry.statusDef = MissionStatus.FromEDName("Failed");
                                    update = true;
                                }
                            }
                            break;
                    }

                    //If placeholder from 'Passengers' event, add 'Missions' parameters
                    if (missionEntry.name.Contains("None"))
                    {
                        missionEntry.name = mission.name;
                        missionEntry.typeDef = MissionType.FromEDName(mission.name.Split('_').ElementAt(1));
                        missionEntry.expiry = mission.expiry;
                        update = true;
                    }
                }

                // Add missions to mission log
                else
                {
                    AddMission(mission);
                    update = true;
                }
            }

            // Remove strays from the mission log
            foreach (Mission missionEntry in missions.ToList())
            {
                Mission mission = @event.missions.FirstOrDefault(m => m.missionid == missionEntry.missionid);
                if (mission == null)
                {
                    // Strip out the stray from the mission log
                    RemoveMissionWithMissionId(missionEntry.missionid);
                    update = true;
                }
            }
            return update;
        }

        private void handlePassengersEvent(PassengersEvent @event)
        {
            if (@event.timestamp > updateDat)
            {
                updateDat = @event.timestamp;
                _handlePassengersEvent(@event);
                writeMissions();
            }
        }

        public void _handlePassengersEvent(PassengersEvent @event)
        {
            Mission mission = new Mission();
            foreach (Passenger passenger in @event.passengers)
            {
                mission = missions.FirstOrDefault(m => m.missionid == passenger.missionid);
                if (mission != null)
                {
                    mission.passengertypeEDName = passenger.type;
                    mission.passengervips = passenger.vip;
                    mission.passengerwanted = passenger.wanted;
                    mission.amount = passenger.amount;
                }
                else
                {
                    // Dummy mission to populate 'Passengers' parameters
                    // 'Missions' event will populate 'name', 'status', 'type' & 'expiry'
                    MissionStatus status = MissionStatus.FromEDName("Active");
                    mission = new Mission(passenger.missionid, "Mission_None", DateTime.Now.AddDays(1), status)
                    {
                        passengertypeEDName = passenger.type,
                        passengervips = passenger.vip,
                        passengerwanted = passenger.wanted,
                        amount = passenger.amount
                    };
                    AddMission(mission);
                }
            }
        }

        private void handleCommunityGoalEvent(CommunityGoalEvent @event)
        {
            if (@event.timestamp > updateDat)
            {
                updateDat = @event.timestamp;
                _handleCommunityGoalEvent(@event);
                writeMissions();
            }
        }

        public void _handleCommunityGoalEvent(CommunityGoalEvent @event)
        {
            Mission mission = new Mission();
            for (int i = 0; i < @event.cgid.Count(); i++)
            {
                mission = missions.FirstOrDefault(m => m.missionid == @event.cgid[i]);
                if (mission == null)
                {
                    MissionStatus status = MissionStatus.FromEDName("Active");
                    mission = new Mission(@event.cgid[i], "MISSION_CommunityGoal", DateTime.Now.AddSeconds(@event.expiry[i]), status)
                    {
                        localisedname = @event.name[i],
                        originstation = @event.station[i]
                    };

                }
                else
                {
                    if (mission.expiry == null)
                    {
                        mission.expiry = DateTime.Now.AddSeconds(@event.expiry[i]);
                        mission.originstation = @event.station[i];
                    }
                }
            }
        }

        private void handleCargoDepotEvent(CargoDepotEvent @event)
        {
            if (@event.timestamp > updateDat)
            {
                _handleCargoDepotEvent(@event);
                updateDat = @event.timestamp;
                writeMissions();
            }
        }

        public void _handleCargoDepotEvent(CargoDepotEvent @event)
        {
            if (@event.missionid != null)
            {
                Mission mission = missions.FirstOrDefault(m => m.missionid == @event.missionid);
                int amountRemaining = @event.totaltodeliver - @event.delivered;
                if (@event.updatetype == "Collect")
                {
                    if (mission == null)
                    {
                        // Add shared mission not previously instantiated
                        MissionStatus status = MissionStatus.FromEDName("Active");
                        mission = new Mission(@event.missionid ?? 0, "MISSION_DeliveryWing", null, status, true)
                        {
                            amount = @event.totaltodeliver,
                            commodity = @event.commodity,
                            originsystem = EDDI.Instance?.CurrentStarSystem?.name,
                            originstation = EDDI.Instance?.CurrentStation?.name,
                            wing = true,
                            originreturn = false
                        };
                        AddMission(mission);
                    }
                    else if (mission.shared)
                    {
                        // Update shared mission previously instantiated
                        mission.commodity = @event.commodity;
                        mission.originsystem = EDDI.Instance?.CurrentStarSystem?.name;
                        mission.originstation = EDDI.Instance?.CurrentStation?.name;
                    }
                }
                else // Update type is 'WingUpdate' or 'Deliver'
                {
                    if (mission == null)
                    {
                        if (amountRemaining > 0)
                        {
                            // If requirements not yet satisfied, add shared mission not previously instantiated
                            MissionStatus status = MissionStatus.FromEDName("Active");
                            string type = @event.startmarketid == 0 ? "MISSION_CollectWing" : "MISSION_DeliveryWing";
                            mission = new Mission(@event.missionid ?? 0, type, null, status, true)
                            {
                                amount = @event.totaltodeliver,
                                commodity = @event.commodity,
                                originsystem = @event.startmarketid == 0 && @event.updatetype == "Deliver" ? EDDI.Instance?.CurrentStarSystem?.name : null,
                                originstation = @event.startmarketid == 0 && @event.updatetype == "Deliver" ? EDDI.Instance?.CurrentStarSystem?.name : null,
                                wing = true,
                                originreturn = @event.startmarketid == 0
                            };
                            AddMission(mission);
                        }
                    }
                    else if (mission.shared)
                    {
                        if (amountRemaining > 0)
                        {
                            // If requirements not yet satisfied, update shared mission previously instantiated
                            if (@event.updatetype == "Deliver")
                            {
                                mission.commodity = @event.commodity;
                                mission.originsystem = EDDI.Instance?.CurrentStarSystem?.name;
                                mission.originstation = EDDI.Instance?.CurrentStation?.name;
                            }
                        }
                        else
                        {
                            // Otherwise, remove shared mission
                            RemoveMission(mission);
                        }
                    }
                    else if (amountRemaining == 0)
                    {
                        // Update 'owned' mission status to 'Complete'
                        MissionStatus status = MissionStatus.FromEDName("Complete");
                        mission.statusDef = status;
                    }
                }
            }
        }

        private void handleMissionAbandonedEvent(MissionAbandonedEvent @event)
        {
            if (@event.timestamp > updateDat)
            {
                updateDat = @event.timestamp;
                if (_handleMissionAbandonedEvent(@event))
                {
                    writeMissions();
                }
            }
        }

        public bool _handleMissionAbandonedEvent(MissionAbandonedEvent @event)
        {
            bool update = false;
            if (@event.missionid != null)
            {
                Mission mission = missions.FirstOrDefault(m => m.missionid == @event.missionid);
                if (mission != null)
                {
                    RemoveMissionWithMissionId(@event.missionid ?? 0);
                    update = true;
                }
            }
            return update;
        }

        private void handleMissionAcceptedEvent(MissionAcceptedEvent @event)
        {
            if (@event.timestamp > updateDat)
            {
                updateDat = @event.timestamp;
                if (_handleMissionAcceptedEvent(@event))
                {
                    writeMissions();
                }
            }
        }

        public bool _handleMissionAcceptedEvent(MissionAcceptedEvent @event)
        {
            bool update = false;

            // Protect against duplicates and empty strings
            bool exists = missions.Any(m => m.missionid == @event.missionid);
            if (!exists && !string.IsNullOrEmpty(@event.name))
            {
                MissionStatus status = MissionStatus.FromEDName("Active");
                Mission mission = new Mission(@event.missionid ?? 0, @event.name, @event.expiry, status)
                {
                    // Common parameters
                    localisedname = @event.localisedname,
                    amount = @event.amount ?? 0,
                    influence = @event.influence,
                    reputation = @event.reputation,
                    reward = @event.reward ?? 0,
                    wing = @event.wing,
                    communal = @event.communal,

                    // Get the minor faction stuff
                    faction = @event.faction,
                    factionstate = FactionState.FromEDName("None").localizedName,

                    // Set mission origin to to the current system & station
                    originsystem = @event.communal ? @event.destinationsystem : EDDI.Instance?.CurrentStarSystem?.name,
                    originstation = @event.communal ? null : EDDI.Instance?.CurrentStation?.name,

                    // Missions with commodities
                    commodity = @event.commodity,

                    // Missions with targets
                    targetTypeEDName = @event.targettype?.Split('_').ElementAtOrDefault(2),
                    target = @event.target,
                    targetfaction = @event.targetfaction,

                    // Missions with passengers
                    passengertypeEDName = @event.passengertype,
                    passengervips = @event.passengervips,
                    passengerwanted = @event.passengerwanted
                };

                // Get the faction state (Boom, Bust, Civil War, etc), if available
                for (int i = 2; i < mission.name.Split('_').Count(); i++)
                {
                    string element = mission.name.Split('_')
                        .ElementAtOrDefault(i)?
                        .ToLowerInvariant();

                    // Might be a faction state
                    FactionState factionState = FactionState
                        .AllOfThem
                        .Find(s => s.edname.ToLowerInvariant() == element);
                    if (factionState != null)
                    {
                        mission.factionstate = factionState.localizedName;
                        break;
                    }
                }

                // Missions with multiple destinations
                if (@event.destinationsystem != null && @event.destinationsystem.Contains("$MISSIONUTIL_MULTIPLE"))
                {
                    // If 'chained' mission, get the destination systems
                    string[] systems = @event.destinationsystem
                        .Replace("$MISSIONUTIL_MULTIPLE_INNER_SEPARATOR;", "#")
                        .Replace("$MISSIONUTIL_MULTIPLE_FINAL_SEPARATOR;", "#")
                        .Split('#');

                    foreach (string system in systems)
                    {
                        mission.destinationsystems.Add(new DestinationSystem(system));
                    }

                    // Load the first destination system.
                    mission.destinationsystem = mission.destinationsystems.ElementAtOrDefault(0).name;
                }
                else
                {
                    // Populate destination system and station, depending on mission type
                    string type = mission.typeEDName.ToLowerInvariant();
                    switch (type)
                    {
                        case "altruism":
                        case "altruismcredits":
                            {
                                mission.destinationsystem = mission.originsystem;
                                mission.destinationstation = mission.originstation;
                            }
                            break;
                        default:
                            {
                                mission.destinationsystem = @event.destinationsystem;
                                mission.destinationstation = @event.destinationstation;
                            }
                            break;
                    }
                }
                AddMission(mission);
                update = true;
            }
            return update;
        }

        private void handleMissionCompletedEvent(MissionCompletedEvent @event)
        {
            if (@event.timestamp > updateDat)
            {
                updateDat = @event.timestamp;
                if (_handleMissionCompletedEvent(@event))
                {
                    writeMissions();
                }
            }
        }

        public bool _handleMissionCompletedEvent(MissionCompletedEvent @event)
        {
            bool update = false;
            if (@event.missionid != null)
            {
                Mission mission = missions.FirstOrDefault(m => m.missionid == @event.missionid);
                if (mission != null)
                {
                    RemoveMissionWithMissionId(@event.missionid ?? 0);
                    update = true;
                }
            }
            return update;
        }

        private void handleMissionExpiredEvent(MissionExpiredEvent @event)
        {
            // 'Expired' is a non-journal event and not subject to 'LogLoad'
            updateDat = @event.timestamp;
            if (_handleMissionExpiredEvent(@event))
            {
                writeMissions();
            }
        }

        public bool _handleMissionExpiredEvent(MissionExpiredEvent @event)
        {
            bool update = false;
            if (@event.missionid != null)
            {
                Mission mission = missions.FirstOrDefault(m => m.missionid == @event.missionid);
                if (mission != null)
                {
                    mission.statusDef = MissionStatus.FromEDName("Failed");
                    update = true;
                }
            }
            return update;
        }

        private void handleMissionFailedEvent(MissionFailedEvent @event)
        {
            if (@event.timestamp > updateDat)
            {
                updateDat = @event.timestamp;
                if (_handleMissionFailedEvent(@event))
                {
                    writeMissions();
                }
            }
        }

        public bool _handleMissionFailedEvent(MissionFailedEvent @event)
        {
            bool update = false;
            if (@event.missionid != null)
            {
                Mission mission = missions.FirstOrDefault(m => m.missionid == @event.missionid);
                if (mission != null)
                {
                    RemoveMissionWithMissionId(@event.missionid ?? 0);
                    update = true;
                }
            }
            return update;
        }

        private void handleMissionRedirectedEvent(MissionRedirectedEvent @event)
        {
            if (@event.timestamp > updateDat)
            {
                updateDat = @event.timestamp;
                if (_handleMissionRedirectedEvent(@event))
                {
                    writeMissions();
                }
            }
        }

        public bool _handleMissionRedirectedEvent(MissionRedirectedEvent @event)
        {
            bool update = false;
            if (@event.missionid != null)
            {
                Mission mission = missions.FirstOrDefault(m => m.missionid == @event.missionid);
                if (mission != null)
                {
                    mission.destinationsystem = @event.newdestinationsystem;
                    mission.destinationstation = @event.newdestinationstation;

                    if (mission.originreturn && mission.originsystem == @event.newdestinationsystem
                        && mission.originstation == @event.newdestinationstation)
                    {
                        mission.statusDef = MissionStatus.FromEDName("Complete");
                    }
                    update = true;
                }
            }
            return update;
        }

        public IDictionary<string, object> GetVariables()
        {
            IDictionary<string, object> variables = new Dictionary<string, object>
            {
                ["missions"] = new List<Mission>(missions),
                ["missionsCount"] = missionsCount,
                ["missionWarning"] = missionWarning,
                ["routeList"] = missionsRouteList,
                ["routeDistance"] = missionsRouteDistance
            };
            return variables;
        }

        public void writeMissions()
        {
            lock (missionsLock)
            {
                // Write cargo configuration with current inventory
                missionsCount = missions.Where(m => !m.shared && !m.communal).Count();
                MissionMonitorConfiguration configuration = new MissionMonitorConfiguration
                {
                    updatedat = updateDat,
                    missions = missions,
                    missionsCount = missionsCount,
                    missionWarning = missionWarning,
                    missionsRouteList = missionsRouteList,
                    missionsRouteDistance = missionsRouteDistance

                };
                configuration.ToFile();
            }
            // Make sure the UI is up to date
            RaiseOnUIThread(MissionUpdatedEvent, missions);
        }

        private void readMissions(MissionMonitorConfiguration configuration = null)
        {
            lock (missionsLock)
            {
                // Obtain current missions inventory from configuration
                configuration = configuration ?? MissionMonitorConfiguration.FromFile();
                missionsCount = configuration.missionsCount;
                missionWarning = configuration.missionWarning ?? 60;
                missionsRouteList = configuration.missionsRouteList;
                missionsRouteDistance = configuration.missionsRouteDistance;
                updateDat = configuration.updatedat;

                // Build a new missions log
                List<Mission> newMissions = new List<Mission>();

                // Start with the missions we have in the log
                foreach (Mission mission in configuration.missions)
                {
                    newMissions.Add(mission);
                }

                // Now order the list by mission id
                newMissions = newMissions.OrderBy(m => m.missionid).ToList();

                // Update the missions log 
                missions.Clear();
                foreach (Mission mission in newMissions)
                {
                    missions.Add(mission);
                }
            }
        }

        public Mission GetMissionWithMissionId(long missionid)
        {
            return missions.FirstOrDefault(m => m.missionid == missionid);
        }

        public List<long> GetSystemMissionIds(string system)
        {
            List<long> missionids = new List<long>();       // List of mission IDs for the system

            // Get mission IDs associated with the system
            foreach (Mission mission in missions.Where(m => m.destinationsystem == system
                || (m.originreturn && m.originsystem == system)).ToList())
            {
                missionids.Add(mission.missionid);
            }
            return missionids;
        }

        private void AddMission(Mission mission)
        {
            if (mission == null)
            {
                return;
            }

            lock (missionsLock)
            {
                missions.Add(mission);
            }
        }

        private void RemoveMission(Mission mission)
        {
            RemoveMissionWithMissionId(mission.missionid);
        }

        private void RemoveMissionWithMissionId(long missionid)
        {
            lock (missionsLock)
            {
                for (int i = 0; i < missions.Count; i++)
                {
                    if (missions[i].missionid == missionid)
                    {
                        missions.RemoveAt(i);
                        break;
                    }
                }
            }
        }

        public void CancelRoute()
        {
            // Clear route and destination variables
            missionsRouteList = null;
            missionsRouteDistance = 0;

            EDDI.Instance.enqueueEvent(new RouteDetailsEvent(DateTime.Now, "cancel", null, missionsRouteList, 0, 0, missionsRouteDistance, null));
        }

        public string GetMissionsRoute(string homeSystem = null)
        {
            string nextSystem = null;
            decimal nextDistance = 0;
            int routeCount = 0;

            List<string> systems = new List<string>();      // List of eligible mission destintaion systems
            List<long> missionids = new List<long>();       // List of mission IDs for the next system

            if (missions.Count > 0)
            {
                // Add current star system first
                string currentSystem = EDDI.Instance?.CurrentStarSystem?.name;
                systems.Add(currentSystem);

                // Add origin systems for 'return to origin' missions to the 'systems' list
                foreach (Mission mission in missions.Where(m => m.statusEDName != "Failed").ToList())
                {
                    if (mission.originreturn && !systems.Contains(mission.originsystem))
                    {
                        systems.Add(mission.originsystem);
                    }
                }

                // Add destination systems for applicable mission types to the 'systems' list
                foreach (Mission mission in missions.Where(m => m.statusEDName == "Active").ToList())
                {
                    string type = mission.typeEDName.ToLowerInvariant();
                    switch (type)
                    {
                        case "assassinate":
                        case "courier":
                        case "delivery":
                        case "disable":
                        case "hack":
                        case "massacre":
                        case "passengerbulk":
                        case "passengervip":
                        case "rescue":
                        case "salvage":
                        case "scan":
                        case "sightseeing":
                        case "smuggle":
                            {
                                if (!(mission.destinationsystems?.Any() ?? false))
                                {
                                    if (!systems.Contains(mission.destinationsystem))
                                    {
                                        systems.Add(mission.destinationsystem);
                                    }
                                }
                                else
                                {
                                    foreach (DestinationSystem system in mission.destinationsystems)
                                    {
                                        if (!systems.Contains(system.name))
                                        {
                                            systems.Add(system.name);
                                        }
                                    }
                                }
                            }
                            break;
                    }
                }

                // Calculate the missions route using the 'Repetitive Nearest Neighbor' Algorithim (RNNA)
                if (CalculateRNNA(systems, homeSystem))
                {
                    nextSystem = missionsRouteList?.Split('_')[0];
                    nextDistance = CalculateDistance(currentSystem, nextSystem);
                    routeCount = missionsRouteList.Split('_').Count();

                    Logging.Debug("Calculated Route Selected = " + missionsRouteList + ", Total Distance = " + missionsRouteDistance);
                    writeMissions();

                    // Get mission IDs for 'next' system
                    missionids = GetSystemMissionIds(nextSystem);

                    // Set destination variables
                    UpdateDestinationData(nextSystem, null, nextDistance);
                }
                else
                {
                    Logging.Debug("Unable to meet missions route calculation criteria");
                }
            }
            EDDI.Instance.enqueueEvent(new RouteDetailsEvent(DateTime.Now, "route", nextSystem, missionsRouteList, routeCount, nextDistance, missionsRouteDistance, missionids));
            return nextSystem;
        }

        public bool CalculateRNNA(List<string> systems, string homeSystem)
        {
            // Clear route list & distance
            missionsRouteList = null;
            missionsRouteDistance = 0;
            bool found = false;

            int numSystems = systems.Count();
            if (numSystems > 1)
            {
                List<string> bestRoute = new List<string>();
                decimal bestDistance = 0;

                // Pre-load all system distances
                if (homeSystem != null)
                {
                    systems.Add(homeSystem);
                }
                List<StarSystem> starsystems = DataProviderService.GetSystemsData(systems.ToArray(), true, false, false, false, false);
                decimal[][] distMatrix = new decimal[systems.Count][];
                for (int i = 0; i < systems.Count; i++)
                {
                    distMatrix[i] = new decimal[systems.Count];
                }
                for (int i = 0; i < systems.Count - 1; i++)
                {
                    StarSystem curr = starsystems.Find(s => s.name == systems[i]);
                    for (int j = i + 1; j < systems.Count; j++)
                    {
                        StarSystem dest = starsystems.Find(s => s.name == systems[j]);
                        decimal distance = CalculateDistance(curr, dest);
                        distMatrix[i][j] = distance;
                        distMatrix[j][i] = distance;
                    }
                }

                // Repetitive Nearest Neighbor Algorithm (RNNA)
                // Iterate through all possible routes by changing the starting system
                for (int i = 0; i < numSystems; i++)
                {
                    // If starting system is a destination for a 'return to origin' mission, then not a viable route
                    if (DestinationOriginReturn(systems[i])) { continue; }

                    List<string> route = new List<string>();
                    decimal totalDistance = 0;
                    int currIndex = i;

                    // Repeat until all systems (except starting system) are in the route
                    while (route.Count() < numSystems - 1)
                    {
                        SortedList<decimal, int> nearestList = new SortedList<decimal, int>();

                        // Iterate through the remaining systems to find nearest neighbor
                        for (int j = 1; j < numSystems; j++)
                        {
                            // Wrap around the list
                            int destIndex = i + j < numSystems ? i + j : i + j - numSystems;
                            if (homeSystem != null && destIndex == 0) { destIndex = numSystems; }

                            // Check if destination system previously added to the route
                            if (route.IndexOf(systems[destIndex]) == -1)
                            {
                                nearestList.Add(distMatrix[currIndex][destIndex], destIndex);
                            }
                        }
                        // Set the 'Nearest' system as the new 'current' system
                        currIndex = nearestList.Values.FirstOrDefault();

                        // Add 'nearest' system to the route list and add its distance to total distance traveled
                        route.Add(systems[currIndex]);
                        totalDistance += nearestList.Keys.FirstOrDefault();
                    }

                    // Add 'starting system' to complete the route & add its distance to total distance traveled
                    int startIndex = homeSystem != null && i == 0 ? numSystems : i;
                    route.Add(systems[startIndex]);
                    if (currIndex == numSystems) { currIndex = 0; }
                    totalDistance += distMatrix[currIndex][startIndex];
                    Logging.Debug("Build Route Iteration #" + i + " - Route = " + string.Join("_", route) + ", Total Distance = " + totalDistance);

                    // Use this route if total distance traveled is less than previous iterations
                    if (bestDistance == 0 || totalDistance < bestDistance)
                    {
                        bestRoute.Clear();
                        int homeIndex = route.IndexOf(systems[homeSystem != null ? numSystems : 0]);
                        if (homeIndex < route.Count - 1)
                        {
                            // Rotate list to place homesystem at the end
                            bestRoute = route.Skip(homeIndex + 1)
                                .Concat(route.Take(homeIndex + 1))
                                .ToList();
                        }
                        else
                        {
                            bestRoute = route.ToList();
                        }
                        bestDistance = totalDistance;
                    }
                }

                if (bestRoute.Count == numSystems)
                {
                    missionsRouteList = string.Join("_", bestRoute);
                    missionsRouteDistance = bestDistance;
                    found = true;
                }
            }
            return found;
        }

        public string SetNextRoute()
        {
            string nextSystem = missionsRouteList?.Split('_')[0];
            decimal nextDistance = 0;
            int count = 0;
            List<long> missionids = new List<long>();       // List of mission IDs for the next system

            if (nextSystem != null)
            {
                StarSystem curr = EDDI.Instance?.CurrentStarSystem;
                StarSystem dest = StarSystemSqLiteRepository.Instance.GetOrFetchStarSystem(nextSystem, true);

                if (dest != null && nextSystem != curr.name)
                {
                    nextDistance = CalculateDistance(curr, dest);
                }
                count = missionsRouteList.Split('_').Count();

                // Get mission IDs for 'next' system
                missionids = GetSystemMissionIds(nextSystem);

                // Set destination variables
                UpdateDestinationData(nextSystem, null, nextDistance);
            }
            EDDI.Instance.enqueueEvent(new RouteDetailsEvent(DateTime.Now, "next", nextSystem, missionsRouteList, count, nextDistance, missionsRouteDistance, missionids));
            return nextSystem;
        }

        public string UpdateRoute(string updateSystem = null)
        {
            bool update;
            string nextSystem = null;
            decimal nextDistance = 0;
            List<long> missionids = new List<long>();       // List of mission IDs for the next system
            string currentSystem = EDDI.Instance?.CurrentStarSystem?.name;
            List<string> route = missionsRouteList?.Split('_').ToList() ?? new List<string>();

            if (route.Count == 0) { update = false; }
            else if (updateSystem == null)
            {
                updateSystem = route[0];

                // Determine if the 'update' system in the missions route list is the current system & has no pending missions
                update = currentSystem == updateSystem ? !SystemPendingMissions(updateSystem) : false;
            }
            else { update = route.Contains(updateSystem); }

            // Remove 'update' system from the missions route list
            if (update)
            {
                if (RemoveSystemFromRoute(updateSystem))
                {
                    nextSystem = missionsRouteList?.Split('_')[0];
                    if (nextSystem != null)
                    {
                        nextDistance = CalculateDistance(currentSystem, nextSystem);

                        // Get mission IDs for 'next' system
                        missionids = GetSystemMissionIds(nextSystem);
                    }
                    Logging.Debug("Route Updated = " + missionsRouteList + ", Total Distance = " + missionsRouteDistance);
                    writeMissions();

                    // Set destination variables
                    UpdateDestinationData(nextSystem, null, nextDistance);
                }
            }
            EDDI.Instance.enqueueEvent(new RouteDetailsEvent(DateTime.Now, "update", nextSystem, missionsRouteList, route.Count, nextDistance, missionsRouteDistance, missionids));
            return nextSystem;
        }

        private bool RemoveSystemFromRoute(string system)
        {
            List<string> route = missionsRouteList?.Split('_').ToList();
            if (route.Count == 0) { return false; }

            int index = route.IndexOf(system);
            if (index > -1)
            {
                // Do not remove the 'home' system unless last in list
                if (route.Count > 1 && index == route.Count - 1) { return false; }

                route.RemoveAt(index);
                if (route.Count > 0)
                {
                    // If other than 'next' system removed, recalculate the route
                    if (route.Count > 2 && index > 0)
                    {
                        // Use copy to keep the original intact.
                        List<string> systems = new List<string>(route);

                        // Build systems list
                        string homeSystem = systems.Last();
                        systems.RemoveAt(systems.Count - 1);
                        systems.Insert(0, EDDI.Instance?.CurrentStarSystem?.name);

                        if (CalculateRNNA(systems, homeSystem)) { return true; }
                    }
                    missionsRouteList = string.Join("_", route);
                    missionsRouteDistance = CalculateRouteDistance();
                }
                else
                {
                    missionsRouteList = null;
                    missionsRouteDistance = 0;
                }
                return true;
            }
            return false;
        }

        public decimal CalculateDistance(string currentSystem, string destinationSystem)
        {
            StarSystem curr = StarSystemSqLiteRepository.Instance.GetOrFetchStarSystem(currentSystem, true);
            StarSystem dest = StarSystemSqLiteRepository.Instance.GetOrFetchStarSystem(destinationSystem, true);
            return CalculateDistance(curr, dest);
        }

        public decimal CalculateDistance(StarSystem curr, StarSystem dest)
        {
            decimal distance = -1;
            if (curr != null && dest != null)
            {
                distance = (decimal)Math.Round(Math.Sqrt(Math.Pow((double)(curr.x - dest.x), 2)
                    + Math.Pow((double)(curr.y - dest.y), 2)
                    + Math.Pow((double)(curr.z - dest.z), 2)), 2);

            }
            return distance;
        }

        private decimal CalculateRouteDistance()
        {
            List<string> route = missionsRouteList?.Split('_').ToList();
            decimal distance = 0;

            if (route.Count > 0)
            {
                StarSystem curr = EDDI.Instance?.CurrentStarSystem;

                // Get all the route coordinates from EDSM in one request
                List<StarSystem> starsystems = DataProviderService.GetSystemsData(route.ToArray(), true, false, false, false, false);

                // Get distance to the next system
                StarSystem dest = starsystems.Find(s => s.name == route[0]);
                distance = CalculateDistance(curr, dest);

                // Calculate remaining route distance
                for (int i = 0; i < route.Count() - 1; i++)
                {
                    curr = starsystems.Find(s => s.name == route[i]);
                    dest = starsystems.Find(s => s.name == route[i + 1]);
                    distance += CalculateDistance(curr, dest);
                }
            }
            return distance;
        }

        private bool DestinationOriginReturn(string destination)
        {
            foreach (Mission mission in missions.Where(m => m.originreturn).ToList())
            {
                if (mission.destinationsystems == null)
                {
                    if (mission.destinationsystem == destination)
                    {
                        return true;
                    }
                }
                else
                {
                    DestinationSystem system = mission.destinationsystems.FirstOrDefault(ds => ds.name == destination);
                    if (system != null)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public void SetNavigationData(string system, string station, decimal distance)
        {
            missionsRouteList = system;
            missionsRouteDistance = distance;
            UpdateDestinationData(system, station, distance);
            writeMissions();
        }

        private bool SystemPendingMissions(string system)
        {
            foreach (Mission mission in missions.Where(m => m.statusEDName != "Fail").ToList())
            {
                string type = mission.typeEDName.ToLowerInvariant();
                switch (type)
                {
                    case "assassinate":
                    case "courier":
                    case "delivery":
                    case "disable":
                    case "hack":
                    case "massacre":
                    case "passengerbulk":
                    case "passengervip":
                    case "rescue":
                    case "salvage":
                    case "scan":
                    case "sightseeing":
                    case "smuggle":
                        {
                            // Check if the system is origin system for 'Active' and 'Complete' missions
                            if (mission.originsystem == system) { return true; }

                            // Check if the system is destination system for 'Active' missions
                            else if (mission.statusEDName == "Active")
                            {
                                if (mission.destinationsystems?.Any() ?? false)
                                {
                                    if (mission.destinationsystems.Where(d => d.name == system).Any()) { return true; }
                                }
                                else if (mission.destinationsystem == system) { return true; }
                            }
                        }
                        break;
                }
            }
            return false;
        }

        public void UpdateDestinationData(string system, string station, decimal distance)
        {
            EDDI.Instance.updateDestinationSystem(system);
            EDDI.Instance.DestinationDistanceLy = distance;
            EDDI.Instance.updateDestinationStation(station);
        }

        static void RaiseOnUIThread(EventHandler handler, object sender)
        {
            if (handler != null)
            {
                SynchronizationContext uiSyncContext = SynchronizationContext.Current ?? new SynchronizationContext();
                if (uiSyncContext == null)
                {
                    handler(sender, EventArgs.Empty);
                }
                else
                {
                    uiSyncContext.Send(delegate { handler(sender, EventArgs.Empty); }, null);
                }
            }
        }
    }
}
