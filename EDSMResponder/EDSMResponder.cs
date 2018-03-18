using Eddi;
using EddiDataDefinitions;
using EddiEvents;
using EddiShipMonitor;
using EddiStarMapService;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Controls;
using Utilities;

namespace EddiEdsmResponder
{
    public class EDSMResponder : EDDIResponder
    {
        private StarMapService starMapService;
        private string system;
        private Thread updateThread;
        private List<string> ignoredEvents;

        public string ResponderName()
        {
            return "EDSM responder";
        }

        public string ResponderVersion()
        {
            return "1.0.0";
        }

        public string ResponderDescription()
        {
            return "Send details of your travels to EDSM.  EDSM is a third-party tool that provides information on the locations of star systems and keeps a log of the star systems you have visited.  It uses the data provided to crowd-source a map of the galaxy";
        }

        public EDSMResponder()
        {
            Logging.Info("Initialised " + ResponderName() + " " + ResponderVersion());
        }

        public bool Start()
        {
            Reload();

            return starMapService != null;
        }

        public void Stop()
        {
            updateThread?.Abort();
            updateThread = null;
            starMapService = null;
        }

        public void Reload()
        {
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
                else if (EDDI.Instance.Cmdr != null)
                {
                    commanderName = EDDI.Instance.Cmdr.name;
                }
                if (commanderName != null)
                {
                    starMapService = new StarMapService(starMapCredentials.apiKey, commanderName);
                }
                if (ignoredEvents == null)
                {
                    ignoredEvents = starMapService.getIgnoredEvents();
                }
            }

            if (starMapService != null && updateThread == null)
            {
                // Spin off a thread to download & sync EDSM flight logs & system comments in the background
                updateThread = new Thread(() => starMapService.Sync(starMapCredentials.lastSync));
                updateThread.IsBackground = true;
                updateThread.Name = "EDSM updater";
                updateThread.Start();
            }
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

            if (starMapService != null)
            {
                if (!ignoredEvents.Contains(theEvent.type))
                {
                    // Prep transient game state info
                    List<decimal?> coordinates = new List<decimal?>()
                    {
                        EDDI.Instance.CurrentStarSystem.x,
                        EDDI.Instance.CurrentStarSystem.y,
                        EDDI.Instance.CurrentStarSystem.z
                    };
                    Dictionary<string, object> transientData = new Dictionary<string, object>()
                    {
                        { "_systemName", EDDI.Instance.CurrentStarSystem.name },
                        { "_systemCoordinates", coordinates },
                        { "_stationName", EDDI.Instance.CurrentStation.name },
                        { "_shipId", EDDI.Instance.CurrentShip.LocalId },

                        // We don't collect this info yet
                        { "_systemAddress", null },
                        { "_marketId", null },
                    };

                    // Unpackage and add transient game state info
                    IDictionary<string, object> eventObject = Deserializtion.DeserializeData(theEvent.raw);
                    eventObject.Add("transient", transientData);

                    // Repackage and send the event
                    string eventData = JsonConvert.SerializeObject(eventObject);
                    starMapService.sendEvent(eventData);
                }
            }
        }

        public UserControl ConfigurationTabItem()
        {
            return new ConfigurationWindow();
        }
    }
}
