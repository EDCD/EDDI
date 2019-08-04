using Eddi;
using EddiDataProviderService;
using EddiEvents;
using EddiInaraService;
using EddiMissionMonitor;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using Utilities;

namespace EddiInaraResponder
{
    public class InaraResponder : EDDIResponder
    {
        private Thread updateThread;
        private bool bgSyncRunning;
        public string ResponderName()
        {
            return "Inara responder";
        }

        public string LocalizedResponderName()
        {
            return Properties.InaraResources.name;
        }

        public string ResponderVersion()
        {
            return "1.0.0";
        }

        public string ResponderDescription()
        {
            return Properties.InaraResources.desc;
        }

        public InaraResponder()
        {
            Logging.Info("Initialised " + ResponderName() + " " + ResponderVersion());
        }

        public bool Start()
        {
            // Set up an event handler to send any pending events when the application exits.
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(OnApplicationExit);

            Reload();
            return InaraService.Instance != null;
        }

        public void Reload()
        {
            Stop();
            InaraService.Reload();
            try
            {
                updateThread = new Thread(() => BackgroundSync())
                {
                    Name = "Inara sync",
                    IsBackground = true
                };
                updateThread.Start();
            }
            catch (ThreadAbortException tax)
            {
                Thread.ResetAbort();
                Logging.Debug("Thread aborted", tax);
            }
        }

        private void BackgroundSync()
        {
            bgSyncRunning = true;
            while (bgSyncRunning)
            {
                InaraService.Instance.SendQueuedAPIEventsAsync(EDDI.Instance.ShouldUseTestEndpoints());
                Thread.Sleep(120000);
            }
        }

        public UserControl ConfigurationTabItem()
        {
            return new ConfigurationWindow();
        }

        public void Stop()
        {
            bgSyncRunning = false;
            updateThread?.Abort();
            updateThread = null;
        }

        public void Handle(Event theEvent)
        {
            if (EDDI.Instance.inCQC)
            {
                // We don't do anything whilst in CQC
                return;
            }

            if (EDDI.Instance.inCrew)
            {
                // We don't do anything whilst in multicrew
                return;
            }

            if (EDDI.Instance.inBeta)
            {
                // We don't send data whilst in beta
                return;
            }

            if (!(theEvent is null))
            {
                try
                {
                    Logging.Debug("Handling event " + JsonConvert.SerializeObject(theEvent));

                    // These events will start or restart our instance of InaraService
                    if (theEvent is CommanderLoadingEvent commanderLoadingEvent)
                    {
                        handleCommanderLoadingEvent(commanderLoadingEvent);
                    }
                    else if (theEvent is CommanderStartedEvent commanderStartedEvent)
                    {
                        handleCommanderStartedEvent(commanderStartedEvent);
                    }
                    else if (InaraService.Instance != null)
                    {
                        if (theEvent is CommanderContinuedEvent commanderContinuedEvent)
                        {
                            handleCommanderContinuedEvent(commanderContinuedEvent);
                        }
                        else if (theEvent is MissionCompletedEvent missionCompletedEvent)
                        {
                            handleMissionCompletedEvent(missionCompletedEvent);
                        }
                        else if (theEvent is StatisticsEvent statisticsEvent)
                        {
                            handleStatisticsEvent(statisticsEvent);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Dictionary<string, object> data = new Dictionary<string, object>
                        {
                            { "event", JsonConvert.SerializeObject(theEvent) },
                            { "exception", ex.Message },
                            { "stacktrace", ex.StackTrace }
                        };
                    Logging.Error("Failed to handle event " + theEvent.type, data);
                }
            }
        }

        private void handleStatisticsEvent(StatisticsEvent @event)
        {
            // Send the commanders game statistics to Inara
            // Prepare and send the raw event, less the event name and timestamp. 
            IDictionary<string, object> statsData = Deserializtion.DeserializeData(@event.raw);
            statsData.Remove("timestamp");
            statsData.Remove("event");
            InaraService.Instance.EnqueueAPIEvent(new setCommanderGameStatistics(@event.timestamp, JsonConvert.SerializeObject(statsData)));
        }

        private void handleCommanderContinuedEvent(CommanderContinuedEvent @event)
        {
            // Send the commander's current credits and loans to Inara
            InaraService.Instance.EnqueueAPIEvent(new setCommanderCredits(@event.timestamp, @event.credits, @event.loan));
        }

        private void handleCommanderStartedEvent(CommanderStartedEvent @event)
        {
            // Start or restart the Inara service
            InaraConfiguration inaraConfiguration = InaraConfiguration.FromFile();
            inaraConfiguration.commanderName = @event.name;
            inaraConfiguration.commanderFrontierID = @event.frontierID;
            inaraConfiguration.ToFile();
            if (inaraConfiguration.commanderFrontierID != InaraService.Instance.commanderFrontierID)
            {
                InaraService.Reload();
            }
        }

        private void handleCommanderLoadingEvent(CommanderLoadingEvent @event)
        {
            // Start or restart the Inara service
            InaraConfiguration inaraConfiguration = InaraConfiguration.FromFile();
            inaraConfiguration.commanderName = @event.name;
            inaraConfiguration.commanderFrontierID = @event.frontierID;
            inaraConfiguration.ToFile();
            if (inaraConfiguration.commanderFrontierID != InaraService.Instance.commanderFrontierID)
            {
                InaraService.Reload();
            }
        }

        private void handleMissionCompletedEvent(MissionCompletedEvent @event)
        {
            // Send aquired permits to Inara
            if (@event.permitsawarded.Count > 0)
            {
                foreach (string systemName in @event.permitsawarded)
                {
                    if (string.IsNullOrEmpty(systemName)) { continue; }
                    InaraService.Instance.EnqueueAPIEvent(new addCommanderPermit(@event.timestamp, systemName));
                }
            }
        }

        void OnApplicationExit(object sender, EventArgs e)
        {
            InaraService.Instance.SendQueuedAPIEventsAsync(EDDI.Instance.ShouldUseTestEndpoints());
        }
    }
}
