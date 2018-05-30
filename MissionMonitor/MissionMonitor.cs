using Eddi;
using EddiDataDefinitions;
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

        public int missioncount;
        public int? warning;

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
                foreach (Mission mission in missions)
                {
                    if (mission.statusEDName == "Active")
                    {
                        TimeSpan span = mission.expiry.ToLocalTime() - DateTime.Now;
                        mission.timeremaining = span.Days.ToString() + "D " + span.Hours.ToString() + "H " + span.Minutes.ToString() + "MIN";

                        if (mission.expiry.ToLocalTime() < DateTime.Now)
                        {
                            EDDI.Instance.eventHandler(new MissionExpiredEvent(DateTime.Now, mission.missionid, mission.name));
                        }
                        else if (mission.expiry.ToLocalTime() < DateTime.Now.AddMinutes(-warning ?? 60))
                        {
                            EDDI.Instance.eventHandler(new MissionWarningEvent(DateTime.Now, mission.missionid, mission.name));
                        }
                    }
                    else
                    {
                        mission.timeremaining = String.Empty;
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
            Logging.Debug("Received event " + JsonConvert.SerializeObject(@event));

            // Handle the events that we care about
            if (@event is MissionsEvent)
            {
			    //
                handleMissionsEvent((MissionsEvent)@event);
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
            else if (@event is MissionFailedEvent)
            {
                //
                handleMissionFailedEvent((MissionFailedEvent)@event);
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

        private void handleMissionsEvent(MissionsEvent @event)
        {
            _handleMissionsEvent(@event);
            writeMissions();
        }

        public void _handleMissionsEvent(MissionsEvent @event)
        {
            foreach (Mission mission in @event.missions)
			{
			    // Add missions to mission log
                Mission missionEntry = missions.FirstOrDefault(m => m.missionid == mission.missionid);
                if (missionEntry != null)
                {
				    missionEntry.statusDef = mission.statusDef;
				}
				else
				{
				    AddMission(mission);
				}
			}
			// Remove strays from the mission log
			foreach (Mission missionEntry in missions.ToList())
			{
			    Mission mission = @event.missions.FirstOrDefault(m => m.missionid == missionEntry.missionid);
				if (mission == null)
				{
                    // Strip out the stray from the mission log
                    _RemoveMissionWithMissionId(missionEntry.missionid);
				}
			}
        }

        private void handleMissionAbandonedEvent(MissionAbandonedEvent @event)
        {
            _handleMissionAbandonedEvent(@event);
            writeMissions();
        }

        public void _handleMissionAbandonedEvent(MissionAbandonedEvent @event)
        {
            if (@event.missionid != null)
            {
                Mission mission = missions.FirstOrDefault(m => m.missionid == @event.missionid);
                if (mission != null)
                {
                    _RemoveMissionWithMissionId(@event.missionid ?? 0);
                }
            }
        }

        private void handleMissionAcceptedEvent(MissionAcceptedEvent @event)
        {
            if (@event.name != null)
            {
                _handleMissionAcceptedEvent(@event);
                writeMissions();
            }
        }

        public void _handleMissionAcceptedEvent(MissionAcceptedEvent @event)
        {
            if (@event.missionid != null)
            {
                MissionStatus status = MissionStatus.FromEDName("Active");
                Mission mission = new Mission(@event.missionid ?? 0, @event.name, @event.expiry ?? DateTime.MaxValue, status);
                string type = mission.typeEDName.ToLowerInvariant();

                // Common parameters
                mission.amount = @event.amount ?? 0;
                mission.influence = @event.influence;
                mission.reputation = @event.reputation;
                mission.reward = @event.reward ?? 0;
                mission.wing = @event.wing;

                // Mission faction parameters
                mission.faction = @event.faction;
                string state = mission.name.Split('_').ElementAt(2).ToLowerInvariant() ?? String.Empty;
                mission.factionstate = SystemState.FromEDName(state)?.localizedName ?? String.Empty;
                if (mission.factionstate == String.Empty)
                {
                    state = mission.name.Split('_').ElementAt(3).ToLowerInvariant() ?? String.Empty;
                    mission.factionstate = SystemState.FromEDName(state)?.localizedName ?? String.Empty;
                }

                // Mission legality
                mission.legal = mission.name.Split('_').ElementAt(2).ToLowerInvariant() == "illegal" ? false : true;

                // Set mission origin to to the current system & station
                mission.originsystem = EDDI.Instance?.CurrentStarSystem?.name;
                mission.originstation = EDDI.Instance?.CurrentStation?.name;

                // Mission returns to origin
                switch (type)
                {
                    case "altruism":
                    case "altruismcredits":
                    case "assassinate":
                    case "collect":
                    case "disable":
                    case "longdistanceexpedition":
                    case "massacre":
                    case "mining":
                    case "piracy":
                    case "sightseeing":
                        {
                            mission.originreturn = true;
                        }
                        break;
                    default:
                        {
                            mission.originreturn = false;
                        }
                        break;
                }

                // Missions with commodities
                mission.commodity = @event.commodity;

                // Missions with destinations
                if (@event.destinationsystem.Contains("$MISSIONUTIL_MULTIPLE"))
                {
                    // If 'chained' mission, get the first destination system. 'Mission redirected' should take care of the rest
                    mission.destinationsystem = @event.destinationsystem
                        .Substring(0, @event.destinationsystem.IndexOf("$MISSIONUTIL_MULTIPLE_FINAL"))
                        .Replace("$MISSIONUTIL_MULTIPLE_INNER_SEPARATOR;", "#")
                        .Replace("$FINAL_SEPARATOR;", "#")
                        .Split('#')
                        .ElementAt(0);
                    mission.destinationstation = String.Empty;
                }
                else
                {
                    // Populate destination system and station, depending on mission type
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

                // Missions with targets
                mission.target = @event.target;
                mission.targettype = @event.targettype;
                mission.targetfaction = @event.targetfaction;

                // Missions with passengers
                mission.passengertypeEDName = @event.passengertype;
                mission.passengervips = @event.passengervips;
                mission.passengerwanted = @event.passengerwanted;

                AddMission(mission);
            }
        }

        private void handleMissionCompletedEvent(MissionCompletedEvent @event)
        {

            _handleMissionCompletedEvent(@event);
            writeMissions();

        }

        public void _handleMissionCompletedEvent(MissionCompletedEvent @event)
        {
            if (@event.missionid != null)
            {
                Mission mission = missions.FirstOrDefault(m => m.missionid == @event.missionid);
                if (mission != null)
                {
                    _RemoveMissionWithMissionId(@event.missionid ?? 0);
                }
            }
        }

        private void handleMissionExpiredEvent(MissionExpiredEvent @event)
        {
            _handleMissionExpiredEvent(@event);
            writeMissions();
        }

        public void _handleMissionExpiredEvent(MissionExpiredEvent @event)
        {
            if (@event.missionid != null)
            {
                Mission mission = missions.FirstOrDefault(m => m.missionid == @event.missionid);
                if (mission != null)
                {
                    mission.statusDef = MissionStatus.FromEDName("Failed");
                }
            }
        }

        private void handleMissionFailedEvent(MissionFailedEvent @event)
        {
            _handleMissionFailedEvent(@event);
            writeMissions();
        }

        public void _handleMissionFailedEvent(MissionFailedEvent @event)
        {
            if (@event.missionid != null)
            {
                Mission mission = missions.FirstOrDefault(m => m.missionid == @event.missionid);
                if (mission != null)
                {
                    _RemoveMissionWithMissionId(@event.missionid ?? 0);
                }
            }
        }

        private void handleMissionRedirectedEvent(MissionRedirectedEvent @event)
        {
            _handleMissionRedirectedEvent(@event);
            writeMissions();
        }

        public void _handleMissionRedirectedEvent(MissionRedirectedEvent @event)
        {
            if (@event.missionid != null)
            {
                Mission mission = missions.FirstOrDefault(m => m.missionid == @event.missionid);
                if (mission != null)
                {
                    mission.destinationsystem = @event.newdestinationsystem;
                    mission.destinationstation = @event.newdestinationstation;

                    string type = mission.typeEDName.ToLowerInvariant();
                    switch (type)
                    {
                        case "collect":
                        case "mining":
                        case "piracy":
                        case "rescue":
                        case "salvage":
                            {
                                if (@event.newdestinationsystem == mission.originsystem
                                    && @event.newdestinationstation == mission.originstation)
                                {
                                    mission.statusDef = MissionStatus.FromEDName("Complete");
                                }
                            }
                            break;
                    }
                }
            }
        }

        public IDictionary<string, object> GetVariables()
        {
            IDictionary<string, object> variables = new Dictionary<string, object>
            {
                ["missions"] = new List<Mission>(missions),
                ["missioncount"] = missioncount,
                ["missionwarning"] = warning
            };
            return variables;
        }

        public void writeMissions()
        {
            lock (missionsLock)
            {
                // Write cargo configuration with current inventory
                MissionMonitorConfiguration configuration = new MissionMonitorConfiguration();

                configuration.missions = missions;
                missioncount = missions.Count;
                configuration.missioncount = missioncount;
                configuration.warning = warning;
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
                missioncount = configuration.missioncount;
                warning = configuration.warning ?? 60;

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
            writeMissions();
        }

        private void RemoveMission(Mission mission)
        {
            _RemoveMissionWithMissionId(mission.missionid);
        }

        private void _RemoveMissionWithMissionId(long missionid)
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
            writeMissions();
        }

        private Mission GetMissionWithMissionId(long missionid)
        {
            return missions.FirstOrDefault(m => m.missionid == missionid);
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
