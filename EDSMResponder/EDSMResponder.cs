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
                    // Retrieve applicable transient game state info (metadata) for the event
                    Dictionary<string, object> transientData = GetTransientData(theEvent);

                    // Unpackage the event, add transient game state info as applicable, then repackage and send the event
                    string eventData;
                    IDictionary<string, object> eventObject;
                    if (transientData.Count > 0)
                    {
                        eventObject = Deserializtion.DeserializeData(theEvent.raw);
                        eventObject.Add("transient", transientData);
                        eventData = JsonConvert.SerializeObject(eventObject);
                    }
                    else
                    {
                        eventData = theEvent.raw;
                    }
                    starMapService.sendEvent(eventData);
                }
            }
        }

        private static Dictionary<string, object> GetTransientData(Event theEvent)
        {
            // Prep transient game state info (metadata) per https://www.edsm.net/en/api-journal-v1.
            Dictionary<string, object> transientData = new Dictionary<string, object>();

            // Add metadata from events
            switch (theEvent.type)
            {
                case "LoadGame":
                    {
                        transientData.Add("_systemAddress", null);
                        transientData.Add("_systemName", null);
                        transientData.Add("_systemCoordinates", null);
                        transientData.Add("_marketId", null);
                        transientData.Add("_stationName", null);
                        break;
                    }
                case "SetUserShipName":
                    {
                        ShipRenamedEvent shipRenamedEvent = (ShipRenamedEvent)theEvent;
                        transientData.Add("_shipId", shipRenamedEvent.shipid);
                        break;
                    }
                case "ShipyardBuy":
                    {
                        transientData.Add("_shipId", null);
                        break;
                    }
                case "ShipyardSwap":
                    {
                        ShipSwappedEvent shipSwappedEvent = (ShipSwappedEvent)theEvent;
                        transientData.Add("_shipId", shipSwappedEvent.shipid);
                        break;
                    }
                case "Loadout":
                    {
                        ShipLoadoutEvent shipLoadoutEvent = (ShipLoadoutEvent)theEvent;
                        transientData.Add("_shipId", shipLoadoutEvent.shipid);
                        break;
                    }
                case "Undocked":
                    {
                        transientData.Add("_marketId", null);
                        transientData.Add("_stationName", null);
                        break;
                    }
                case "Location":
                    {
                        LocationEvent locationEvent = (LocationEvent)theEvent;
                        transientData.Add("_systemAddress", null); // We don't collect this info yet
                        transientData.Add("_systemName", locationEvent.system);
                        List<decimal?> _systemCoordinates = new List<decimal?>
                        {
                            locationEvent.x,
                            locationEvent.y,
                            locationEvent.z
                        };
                        transientData.Add("_systemCoordinates", _systemCoordinates);
                        transientData.Add("_marketId", null); // We don't collect this info yet
                        transientData.Add("_stationName", locationEvent.station);
                        break;
                    }
                case "FSDJump":
                    {
                        JumpedEvent jumpedEvent = (JumpedEvent)theEvent;
                        transientData.Add("_systemAddress", null); // We don't collect this info yet
                        transientData.Add("_systemName", jumpedEvent.system);
                        List<decimal?> _systemCoordinates = new List<decimal?>
                        {
                            jumpedEvent.x,
                            jumpedEvent.y,
                            jumpedEvent.z
                        };
                        transientData.Add("_systemCoordinates", _systemCoordinates);
                        break;
                    }
                case "Docked":
                    {
                        DockedEvent dockedEvent = (DockedEvent)theEvent;
                        transientData.Add("_systemAddress", null); // We don't collect this info yet
                        transientData.Add("_systemName", dockedEvent.system);
                        transientData.Add("_systemCoordinates", null);
                        transientData.Add("_marketId", null); // We don't collect this info yet
                        transientData.Add("_stationName", dockedEvent.station);
                        break;
                    }
            }

            // Supplement with metadata from the tracked game state, as applicable
            if (!transientData.ContainsKey("_systemAddress")) { transientData.Add("_systemAddress", null); } // We don't collect this info yet
            if (!transientData.ContainsKey("_systemName")) { transientData.Add("_systemName", EDDI.Instance.CurrentStarSystem.name); }
            List<decimal?> _coordinates = new List<decimal?>
            {
                EDDI.Instance.CurrentStarSystem.x,
                EDDI.Instance.CurrentStarSystem.y,
                EDDI.Instance.CurrentStarSystem.z
            };
            if (!transientData.ContainsKey("_systemCoordinates")) { transientData.Add("_systemCoordinates", _coordinates); }
            if (!transientData.ContainsKey("_marketId")) { transientData.Add("_marketId", null); } // We don't collect this info yet
            if (!transientData.ContainsKey("_stationName")) { transientData.Add("_stationName", EDDI.Instance.CurrentStation.name); }
            if (!transientData.ContainsKey("_shipId")) { transientData.Add("_shipId", EDDI.Instance.CurrentShip.LocalId); }

            return transientData;
        }

        public UserControl ConfigurationTabItem()
        {
            return new ConfigurationWindow();
        }
    }
}
