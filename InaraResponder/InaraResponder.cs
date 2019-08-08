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
    // Documentation: https://inara.cz/inara-api-docs/

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
                        else if (theEvent is PowerplayEvent powerplayEvent)
                        {
                            handlePowerplayEvent(powerplayEvent);
                        }
                        else if (theEvent is PowerLeftEvent powerLeftEvent)
                        {
                            handlePowerLeftEvent(powerLeftEvent);
                        }
                        else if (theEvent is PowerJoinedEvent powerJoinedEvent)
                        {
                            handlePowerJoinedEvent(powerJoinedEvent);
                        }
                        else if (theEvent is CommanderReputationEvent commanderReputationEvent)
                        {
                            handleCommanderReputationEvent(commanderReputationEvent);
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

        private void handleCommanderReputationEvent(CommanderReputationEvent @event)
        {
            InaraService.Instance.EnqueueAPIEvent(new InaraAPIEvent(@event.timestamp, "setCommanderReputationMajorFaction", new List<Dictionary<string, object>>()
            {
                {
                    new Dictionary<string, object>()
                    {
                        { "majorfactionName", "empire" },
                        { "majorfactionReputation", @event.empire }
                    }
                },
                {
                    new Dictionary<string, object>()
                    {
                        { "majorfactionName", "federation" },
                        { "majorfactionReputation", @event.federation }
                    }
                },
                {
                    new Dictionary<string, object>()
                    {
                        { "majorfactionName", "independent" },
                        { "majorfactionReputation", @event.independent }
                    }
                },
                {
                    new Dictionary<string, object>()
                    {
                        { "majorfactionName", "alliance" },
                        { "majorfactionReputation", @event.alliance }
                    }
                }
            }));
        }

        private void handlePowerJoinedEvent(PowerJoinedEvent @event)
        {
            InaraService.Instance.EnqueueAPIEvent(new InaraAPIEvent(@event.timestamp, "setCommanderRankPower", new Dictionary<string, object>()
            {
                { "powerName", @event.Power.invariantName },
                { "rankValue", 1 }
            }));
        }

        private void handlePowerLeftEvent(PowerLeftEvent @event)
        {
            InaraService.Instance.EnqueueAPIEvent(new InaraAPIEvent(@event.timestamp, "setCommanderRankPower", new Dictionary<string, object>()
            {
                { "powerName", @event.Power.invariantName },
                { "rankValue", 0 }
            }));
        }

        private void handlePowerplayEvent(PowerplayEvent @event)
        {
            InaraService.Instance.EnqueueAPIEvent(new InaraAPIEvent(@event.timestamp, "setCommanderRankPower", new Dictionary<string, object>()
            {
                { "powerName", @event.Power.invariantName },
                { "rankValue", @event.rank }
            }));
        }

        private void handleCommanderProgressEvent(CommanderProgressEvent @event)
        {
            // Pilots federation/Navy rank name as are in the journals (["combat", "trade", "explore", "cqc", "federation", "empire"]) 
            // Rank progress (range: [0..1], which corresponds to 0% - 100%) (In the journal, these are given out of 100)
            List<Dictionary<string, object>> eventData = new List<Dictionary<string, object>>()
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
            InaraService.Instance.EnqueueAPIEvent(new InaraAPIEvent(@event.timestamp, "setCommanderRankPilot", eventData));
        }

        private void handleCommanderRatingsEvent(CommanderRatingsEvent @event)
        {
            // Pilots federation/Navy rank name as are in the journals (["combat", "trade", "explore", "cqc", "federation", "empire"]) 
            // Rank value (range [0..8] for Pilots federation ranks, range [0..14] for Navy ranks)
            List<Dictionary<string, object>> eventData = new List<Dictionary<string, object>>()
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
            InaraService.Instance.EnqueueAPIEvent(new InaraAPIEvent(@event.timestamp, "setCommanderRankPilot", eventData));
        }

        private void handleEngineerProgressedEvent(EngineerProgressedEvent @event)
        {
            // Send engineer rank progress to Inara
            IDictionary<string, object> data = Deserializtion.DeserializeData(@event.raw);
            data.TryGetValue("Engineers", out object val);
            if (val != null)
            {
                // This is a startup entry, containing data about all known engineers
                List<Dictionary<string, object>> eventData = new List<Dictionary<string, object>>();
                List<object> engineers = (List<object>)val;
                foreach (IDictionary<string, object> engineerData in engineers)
                {
                    Dictionary<string, object> engineer = parseEngineerInara(engineerData);

                    eventData.Add(engineer);
                }
                InaraService.Instance.EnqueueAPIEvent(new InaraAPIEvent(@event.timestamp, "setCommanderRankEngineer", eventData));
            }
            else
            {
                // This is a progress entry, containing data about a single engineer
                Dictionary<string, object> eventData = parseEngineerInara(data);
                InaraService.Instance.EnqueueAPIEvent(new InaraAPIEvent(@event.timestamp, "setCommanderRankEngineer", eventData));
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
            // Prepare and send the raw event, less the event name and timestamp. Please note that the statistics 
            // are always overridden as a whole, so any partial updates will cause erasing of the rest.
            IDictionary<string, object> data = Deserializtion.DeserializeData(@event.raw);
            data.Remove("timestamp");
            data.Remove("event");
            InaraService.Instance.EnqueueAPIEvent(new InaraAPIEvent(@event.timestamp, "setCommanderGameStatistics", (Dictionary<string, object>)data));
        }

        private void handleCommanderContinuedEvent(CommanderContinuedEvent @event)
        {
            // Sets current credits and loans. A record is added to the credits log (if the value differs).
            // Warning: Do NOT set credits/assets unless you are absolutely sure they are correct. 
            // The journals currently doesn't contain crew wage cuts, so credit gains are very probably off 
            // for most of the players. Also, please, do not send each minor credits change, as it will 
            // spam player's credits log with unusable data and they won't be most likely very happy about it. 
            // It may be good to set credits just on the session start, session end and on the big changes 
            // or in hourly intervals.
            InaraService.Instance.EnqueueAPIEvent(new InaraAPIEvent(@event.timestamp, "setCommanderCredits", new Dictionary<string, object>()
            {
                { "commanderCredits", @event.credits },
                { "commanderLoan", @event.loan }
            }));
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
            // Adds star system permit for the commander. You do not need to handle permits granted for the 
            // Pilots Federation or Navy rank promotion, but you should handle any other ways (like mission 
            // rewards).
            if (@event.permitsawarded.Count > 0)
            {
                Dictionary<string, object> eventData = new Dictionary<string, object>();
                foreach (string systemName in @event.permitsawarded)
                {
                    if (string.IsNullOrEmpty(systemName)) { continue; }
                    InaraService.Instance.EnqueueAPIEvent(new InaraAPIEvent(@event.timestamp, "addCommanderPermit", new Dictionary<string, object>() { { "starsystemName", systemName } }));
                }
            }
        }

        void OnApplicationExit(object sender, EventArgs e)
        {
            InaraService.Instance.SendQueuedAPIEventsAsync(EDDI.Instance.ShouldUseTestEndpoints());
        }
    }
}
