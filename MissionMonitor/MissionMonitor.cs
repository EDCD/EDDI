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
        // Observable collection for us to handle changes
        public ObservableCollection<Mission> missions { get; private set; }

        public int missioncount;
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
            // We don't actively do anything, just listen to events
            return false;
        }

        public void Start()
        {
        }

        public void Stop()
        {
        }

        public void Reload()
        {
            readMissions();
            Logging.Info("Reloaded " + MonitorName() + " " + MonitorVersion());

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
				    missionEntry.status = mission.status;
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
            string type = @event.name.Split('_').ElementAtOrDefault(1).ToLowerInvariant();
            switch (type)
            {
                case "altruism":
                case "collect":
                case "delivery":
                case "mining":
                case "piracy":
                case "rescue":
                case "salvage":
                case "smuggle":
                    {

                    }
                    break;
            }
        }

        private void handleMissionCompletedEvent(MissionCompletedEvent @event)
        {
            if (@event.commodityDefinition != null || @event.commodityrewards != null)
            {
                _handleMissionCompletedEvent(@event);
                writeMissions();
            }
        }

        public void _handleMissionCompletedEvent(MissionCompletedEvent @event)
        {

        }

        private void handleMissionFailedEvent(MissionFailedEvent @event)
        {
            _handleMissionFailedEvent(@event);
            writeMissions();
        }

        public void _handleMissionFailedEvent(MissionFailedEvent @event)
        {

        }

        private void handleMissionRedirectedEvent(MissionRedirectedEvent @event)
        {
            _handleMissionRedirectedEvent(@event);
            writeMissions();
        }

        public void _handleMissionRedirectedEvent(MissionRedirectedEvent @event)
        {

        }

        public IDictionary<string, object> GetVariables()
        {
            IDictionary<string, object> variables = new Dictionary<string, object>
            {
                ["missions"] = new List<Mission>(missions)
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
                configuration.missioncount = missioncount;
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

                // Build a new missions log
                List<Mission> newMissions = new List<Mission>();

                // Start with the materials we have in the log
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
