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
                        else if (theEvent is CommanderProgressEvent commanderProgressEvent)
                        {
                            handleCommanderProgressEvent(commanderProgressEvent);
                        }
                        else if (theEvent is CommanderRatingsEvent commanderRatingsEvent)
                        {
                            handleCommanderRatingsEvent(commanderRatingsEvent);
                        }
                        else if (theEvent is EngineerProgressedEvent engineerProgressedEvent)
                        {
                            handleEngineerProgressedEvent(engineerProgressedEvent);
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

        private void handleCommanderProgressEvent(CommanderProgressEvent @event)
        {
            // Pilots federation/Navy rank name as are in the journals (["combat", "trade", "explore", "cqc", "federation", "empire"]) 
            // Rank progress (range: [0..1], which corresponds to 0% - 100%) (In the journal, these are given out of 100)
            List<Dictionary<string, object>> rankData = new List<Dictionary<string, object>>()
            {
                new Dictionary<string, object>()
                {
                    { "rankName", "combat" },
                    { "rankProgress", @event.combat / 100 }
                },
                new Dictionary<string, object>()
                {
                    { "rankName", "trade" },
                    { "rankProgress", @event.trade / 100 }
                },
                new Dictionary<string, object>()
                {
                    { "rankName", "explore" },
                    { "rankProgress", @event.exploration / 100 }
                },
                new Dictionary<string, object>()
                {
                    { "rankName", "empire" },
                    { "rankProgress", @event.empire / 100 }
                },
                new Dictionary<string, object>()
                {
                    { "rankName", "federation" },
                    { "rankProgress", @event.federation / 100 }
                },
                new Dictionary<string, object>()
                {
                    { "rankName", "cqc" },
                    { "rankProgress", @event.cqc / 100 }
                }
            };
            InaraService.Instance.EnqueueAPIEvent(new setCommanderRankPilot(@event.timestamp, rankData));
        }

        private void handleCommanderRatingsEvent(CommanderRatingsEvent @event)
        {
            // Pilots federation/Navy rank name as are in the journals (["combat", "trade", "explore", "cqc", "federation", "empire"]) 
            // Rank value (range [0..8] for Pilots federation ranks, range [0..14] for Navy ranks)
            List<Dictionary<string, object>> rankData = new List<Dictionary<string, object>>()
            {
                new Dictionary<string, object>()
                {
                    { "rankName", "combat" },
                    { "rankValue", @event.combat.rank }
                },
                new Dictionary<string, object>()
                {
                    { "rankName", "trade" },
                    { "rankValue", @event.trade.rank }
                },
                new Dictionary<string, object>()
                {
                    { "rankName", "explore" },
                    { "rankValue", @event.exploration.rank }
                },
                new Dictionary<string, object>()
                {
                    { "rankName", "empire" },
                    { "rankValue", @event.empire.rank }
                },
                new Dictionary<string, object>()
                {
                    { "rankName", "federation" },
                    { "rankValue", @event.federation.rank }
                },
                new Dictionary<string, object>()
                {
                    { "rankName", "cqc" },
                    { "rankValue", @event.cqc.rank }
                }
            };
            InaraService.Instance.EnqueueAPIEvent(new setCommanderRankPilot(@event.timestamp, rankData));
        }

        private void handleEngineerProgressedEvent(EngineerProgressedEvent @event)
        {
            // Send engineer rank progress to Inara
            IDictionary<string, object> data = Deserializtion.DeserializeData(@event.raw);
            data.TryGetValue("Engineers", out object val);
            if (val != null)
            {
                // This is a startup entry. 
                List<Dictionary<string, object>> eventData = new List<Dictionary<string, object>>();
                List<object> engineers = (List<object>)val;
                foreach (IDictionary<string, object> engineerData in engineers)
                {
                    Dictionary<string, object> engineer = parseEngineerInara(engineerData);

                    eventData.Add(engineer);
                }
                InaraService.Instance.EnqueueAPIEvent(new setCommanderRankEngineer(@event.timestamp, eventData));
            }
            else
            {
                // This is a progress entry.
                Dictionary<string, object> eventData = parseEngineerInara(data);
                InaraService.Instance.EnqueueAPIEvent(new setCommanderRankEngineer(@event.timestamp, eventData));
            }
        }

        private static Dictionary<string, object> parseEngineerInara(IDictionary<string, object> engineerData)
        {
            Dictionary<string, object> engineer = new Dictionary<string, object>()
            {
                { "engineerName", JsonParsing.getString(engineerData, "Engineer") },
                { "rankStage", JsonParsing.getString(engineerData, "Progress") }
            };
            int? rank = JsonParsing.getOptionalInt(engineerData, "Rank");
            if (!(rank is null))
            {
                engineer.Add("rankValue", rank);
            }
            return engineer;
        }

        private void handleStatisticsEvent(StatisticsEvent @event)
        {
            // Send the commanders game statistics to Inara
            // Prepare and send the raw event, less the event name and timestamp. 
            IDictionary<string, object> data = Deserializtion.DeserializeData(@event.raw);
            data.Remove("timestamp");
            data.Remove("event");
            InaraService.Instance.EnqueueAPIEvent(new setCommanderGameStatistics(@event.timestamp, JsonConvert.SerializeObject(data)));
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
