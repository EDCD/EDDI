using Eddi;
using EddiConfigService;
using EddiDataDefinitions;
using EddiDataProviderService;
using EddiStarMapService;
using EddiEvents;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
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

        private MissionMonitorConfiguration missionsConfig = new MissionMonitorConfiguration();

        private DateTime updateDat;
        public int goalsCount;
        public int missionsCount;
        public int? missionWarning;

        private readonly IEdsmService edsmService;
        private readonly DataProviderService dataProviderService;

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

        public string MonitorDescription()
        {
            return Properties.MissionMonitor.mission_monitor_desc;
        }

        public bool IsRequired()
        {
            return true;
        }

        public MissionMonitor() : this(null)
        { }

        public MissionMonitor(IEdsmService edsmService)
        {
            this.edsmService = edsmService ?? new StarMapService();
            dataProviderService = new DataProviderService(edsmService);
            missions = new ObservableCollection<Mission>();
            BindingOperations.CollectionRegistering += Missions_CollectionRegistering;
            initializeMissionMonitor();
        }

        public void initializeMissionMonitor(MissionMonitorConfiguration configuration = null)
        {
            readMissions();
            Logging.Info($"Initialized {MonitorName()}");
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
            Logging.Info($"Reloaded {MonitorName()}");

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
                            // Check for mission types which have no expiry after requiremensts completed
                            string type = mission.typeEDName.ToLowerInvariant();
                            if (mission.statusEDName == "Claim")
                            {
                                mission.timeremaining = String.Empty;
                                continue;
                            }

                            // Build the 'time remaining' notification
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

                            // Generate 'Expired' and 'Warning' events when conditions met
                            if (mission.expiry < DateTime.UtcNow)
                            {
                                EDDI.Instance.enqueueEvent(new MissionExpiredEvent(DateTime.UtcNow, mission.missionid, mission.name));
                            }
                            else if (mission.expiry < DateTime.UtcNow.AddMinutes(missionWarning ?? Constants.missionWarningDefault))
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
            if (@event is MissionAbandonedEvent)
            {
                //
                handleMissionAbandonedEvent((MissionAbandonedEvent)@event);
            }
            else if (@event is MissionFailedEvent)
            {
                //
                handleMissionFailedEvent((MissionFailedEvent)@event);
            }
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
                                    .FirstOrDefault(s => s.name == EDDI.Instance?.CurrentStarSystem?.systemname);
                                if (system != null)
                                {
                                    system.visited = true;
                                    if (mission.destinationsystems.Where(s => s.visited == false).Count() > 0)
                                    {
                                        // Set destination system to next in chain & trigger a 'Mission redirected' event
                                        string destinationsystem = mission.destinationsystems?
                                            .FirstOrDefault(s => s.visited == false).name;
                                        EDDI.Instance.enqueueEvent(new MissionRedirectedEvent(DateTime.Now, mission.missionid, mission.name, null, null, destinationsystem, EDDI.Instance?.CurrentStarSystem?.systemname));
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
                                if (missionEntry.statusEDName == "Failed")
                                {
                                    if (mission.expiry > missionEntry.expiry)
                                    {
                                        // Fix status if erroneously reported as failed
                                        missionEntry.expiry = mission.expiry;
                                        missionEntry.statusDef = MissionStatus.FromEDName("Active");
                                        update = true;
                                    }
                                }
                                else if (missionEntry.statusEDName == "Active")
                                {
                                    // Update status on a missed 'redirect'
                                    update = UpdateRedirectStatus(missionEntry);
                                }
                            }
                            break;
                        default:
                            {
                                if (missionEntry.statusDef != mission.statusDef)
                                {
                                    missionEntry.statusDef = mission.statusDef;
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
                    // Starter zone missions have no consistent 'accepted' or 'completed' events, so exclude them from the mission log
                    if (mission.typeEDName != "StartZone")
                    {
                        AddMission(mission);
                        update = true;
                    }
                }
            }

            // Remove strays from the mission log
            foreach (Mission missionEntry in missions.ToList())
            {
                // Ensure Community Goals remain in the mission log
                if (!missionEntry.communal)
                {
                    Mission mission = @event.missions.FirstOrDefault(m => m.missionid == missionEntry.missionid);
                    if (mission == null || mission.name.Contains("StartZone"))
                    {
                        // Strip out the stray and 'StartZone' missions from the log
                        RemoveMissionWithMissionId(missionEntry.missionid);
                        update = true;
                    }
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
                    mission = new Mission(passenger.missionid, "Mission_None", DateTime.UtcNow.AddDays(1), status)
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
                            originsystem = EDDI.Instance?.CurrentStarSystem?.systemname,
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
                        mission.originsystem = EDDI.Instance?.CurrentStarSystem?.systemname;
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
                                originsystem = @event.startmarketid == 0 && @event.updatetype == "Deliver" ? EDDI.Instance?.CurrentStarSystem?.systemname : null,
                                originstation = @event.startmarketid == 0 && @event.updatetype == "Deliver" ? EDDI.Instance?.CurrentStarSystem?.systemname : null,
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
                                mission.originsystem = EDDI.Instance?.CurrentStarSystem?.systemname;
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
            bool exists = missions?.Any(m => m.missionid == @event.missionid) ?? false;
            bool valid = !string.IsNullOrEmpty(@event.name) && !@event.name.Contains("StartZone");
            if (!exists && valid)
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
                    originsystem = @event.communal ? @event.destinationsystem : EDDI.Instance?.CurrentStarSystem?.systemname,
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
                    update = UpdateRedirectStatus(mission);
                }
            }
            return update;
        }

        public IDictionary<string, object> GetVariables()
        {
            IDictionary<string, object> variables = new Dictionary<string, object>
            {
                ["goalsCount"] = goalsCount,
                ["missions"] = new List<Mission>(missions),
                ["missionsCount"] = missionsCount,
                ["missionWarning"] = missionWarning
            };
            return variables;
        }

        public void writeMissions()
        {
            missionsCount = missions.Where(m => !m.shared && !m.communal).Count();
            lock (missionsLock)
            {
                // Write bookmarks configuration with current list
                missionsConfig = ConfigService.Instance.missionMonitorConfiguration;
                missionsConfig.missions = missions;
                missionsConfig.missionsCount = missionsCount;
                missionsConfig.missionWarning = missionWarning;
                missionsConfig.updatedat = updateDat;
                ConfigService.Instance.missionMonitorConfiguration = missionsConfig;
            }
            // Make sure the UI is up to date
            RaiseOnUIThread(MissionUpdatedEvent, missions);
        }

        private void readMissions(MissionMonitorConfiguration configuration = null)
        {
            lock (missionsLock)
            {
                // Obtain current missions log from configuration
                missionsConfig = ConfigService.Instance.missionMonitorConfiguration;
                missionsCount = missionsConfig.missionsCount;
                missionWarning = missionsConfig.missionWarning ?? Constants.missionWarningDefault;
                updateDat = missionsConfig.updatedat;

                // Build a new missions log
                List<Mission> newMissions = new List<Mission>();

                // Start with the missions we have in the log
                foreach (Mission mission in missionsConfig.missions)
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

            if (system != null)
            {
                // Get mission IDs associated with the system
                foreach (Mission mission in missions.Where(m => m.destinationsystem == system
                    || (m.originreturn && m.originsystem == system)).ToList())
                {
                    missionids.Add(mission.missionid);
                }
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

        public decimal CalculateDistance(string currentSystem, string destinationSystem)
        {
            StarSystem curr = StarSystemSqLiteRepository.Instance.GetOrFetchStarSystem(currentSystem, true);
            StarSystem dest = StarSystemSqLiteRepository.Instance.GetOrFetchStarSystem(destinationSystem, true);
            return CalculateDistance(curr, dest);
        }

        public decimal CalculateDistance(StarSystem curr, StarSystem dest)
        {
            double square(double x) => x * x;
            decimal distance = 0;
            if (curr?.x != null && dest?.x != null)
            {
                distance = (decimal)Math.Round(Math.Sqrt(square((double)(curr.x - dest.x))
                            + square((double)(curr.y - dest.y))
                            + square((double)(curr.z - dest.z))), 2);
            }
            return distance;
        }

        public bool UpdateRedirectStatus(Mission mission)
        {
            if (mission.originreturn && mission.originsystem == mission.destinationsystem
                && mission.originstation == mission.destinationstation)
            {
                string type = mission.typeEDName.ToLowerInvariant();
                switch (type)
                {
                    case "assassinate":
                    case "assassinatewing":
                    case "disable":
                    case "disablewing":
                    case "massacre":
                    case "massacrethargoid":
                    case "massacrewing":
                        {
                            if (mission.statusEDName != "Claim")
                            {
                                mission.statusDef = MissionStatus.FromEDName("Claim");
                                return true;
                            }
                        }
                        break;
                    case "hack":
                    case "longdistanceexpedition":
                    case "passengervip":
                    case "piracy":
                    case "rescue":
                    case "salvage":
                    case "scan":
                    case "sightseeing":
                        {
                            if (mission.statusEDName != "Complete")
                            {
                                mission.statusDef = MissionStatus.FromEDName("Complete");
                                return true;
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
