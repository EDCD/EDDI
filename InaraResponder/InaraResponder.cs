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
            Reload();
            return InaraService.Instance != null;
        }

        public void Reload()
        {
            Stop();
            InaraService.Reload();
            updateThread = new Thread(() => BackgroundSync())
            {
                Name = "Inara sync",
                IsBackground = true
            };
            updateThread.Start();
        }

        private void BackgroundSync()
        {
            while (InaraService.Instance != null)
            {
                Thread.Sleep(60000);
                InaraService.Instance.SendQueuedAPIEventsAsync(EDDI.Instance.ShouldUseTestEndpoints());
            }
        }

        public UserControl ConfigurationTabItem()
        {
            return new ConfigurationWindow();
        }

        public void Stop()
        {
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
                        if (theEvent is MissionCompletedEvent missionCompletedEvent)
                        {
                            handleMissionCompletedEvent(missionCompletedEvent);
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

        private void handleCommanderStartedEvent(CommanderStartedEvent @event)
        {
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

            }
            throw new NotImplementedException();
        }
    }
}
