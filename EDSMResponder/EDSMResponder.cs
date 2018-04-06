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
                /// Retrieve applicable transient game state info (metadata) 
                /// for the event and the event with transient info to EDSM
                string eventData = prepareEventData(theEvent);
                if (eventData == null)
                {
                    return;
                }
                starMapService.sendEvent(eventData);
            }
        }

        private string prepareEventData(Event theEvent)
        {
            // Prep transient game state info (metadata) per https://www.edsm.net/en/api-journal-v1.
            // Unpackage the event, add transient game state info as applicable, then repackage and send the event
            IDictionary<string, object> eventObject = Deserializtion.DeserializeData(theEvent.raw);
            string eventType = JsonParsing.getString(eventObject, "event");

            if (ignoredEvents.Contains(eventType) || theEvent.raw == null)
            {
                return null;
            }

            // Add metadata from events
            switch (eventType)
            {
                case "LoadGame":
                    {
                        eventObject.Add("_systemAddress", null);
                        eventObject.Add("_systemName", null);
                        eventObject.Add("_systemCoordinates", null);
                        eventObject.Add("_marketId", null);
                        eventObject.Add("_stationName", null);
                        break;
                    }
                case "SetUserShipName":
                    {
                        ShipRenamedEvent shipRenamedEvent = (ShipRenamedEvent)theEvent;
                        eventObject.Add("_shipId", shipRenamedEvent.shipid);
                        break;
                    }
                case "ShipyardBuy":
                    {
                        eventObject.Add("_shipId", null);
                        break;
                    }
                case "ShipyardSwap":
                    {
                        ShipSwappedEvent shipSwappedEvent = (ShipSwappedEvent)theEvent;
                        eventObject.Add("_shipId", shipSwappedEvent.shipid);
                        break;
                    }
                case "Loadout":
                    {
                        ShipLoadoutEvent shipLoadoutEvent = (ShipLoadoutEvent)theEvent;
                        eventObject.Add("_shipId", shipLoadoutEvent.shipid);
                        break;
                    }
                case "Undocked":
                    {
                        eventObject.Add("_marketId", null);
                        eventObject.Add("_stationName", null);
                        break;
                    }
                case "Location":
                    {
                        LocationEvent locationEvent = (LocationEvent)theEvent;
                        eventObject.Add("_systemAddress", null); // We don't collect this info yet
                        eventObject.Add("_systemName", locationEvent.system);
                        List<decimal?> _systemCoordinates = new List<decimal?>
                        {
                            locationEvent.x,
                            locationEvent.y,
                            locationEvent.z
                        };
                        eventObject.Add("_systemCoordinates", _systemCoordinates);
                        eventObject.Add("_marketId", null); // We don't collect this info yet
                        eventObject.Add("_stationName", locationEvent.station);
                        break;
                    }
                case "FSDJump":
                    {
                        JumpedEvent jumpedEvent = (JumpedEvent)theEvent;
                        eventObject.Add("_systemAddress", null); // We don't collect this info yet
                        eventObject.Add("_systemName", jumpedEvent.system);
                        List<decimal?> _systemCoordinates = new List<decimal?>
                        {
                            jumpedEvent.x,
                            jumpedEvent.y,
                            jumpedEvent.z
                        };
                        eventObject.Add("_systemCoordinates", _systemCoordinates);
                        break;
                    }
                case "Docked":
                    {
                        DockedEvent dockedEvent = (DockedEvent)theEvent;
                        eventObject.Add("_systemAddress", null); // We don't collect this info yet
                        eventObject.Add("_systemName", dockedEvent.system);
                        eventObject.Add("_systemCoordinates", null);
                        eventObject.Add("_marketId", null); // We don't collect this info yet
                        eventObject.Add("_stationName", dockedEvent.station);
                        break;
                    }
            }

            // Supplement with metadata from the tracked game state, as applicable
            if (EDDI.Instance.CurrentStarSystem != null)
            {
                if (!eventObject.ContainsKey("_systemAddress") && !eventObject.ContainsKey("SystemAddress"))
                {
                    eventObject.Add("_systemAddress", null); // We don't collect this info yet
                } 
                if (!eventObject.ContainsKey("_systemName") && !eventObject.ContainsKey("SystemName"))
                {
                    eventObject.Add("_systemName", EDDI.Instance.CurrentStarSystem.name);
                }
                if (!eventObject.ContainsKey("_systemCoordinates") && !eventObject.ContainsKey("StarPos"))
                {
                    List<decimal?> _coordinates = new List<decimal?>
                    {
                    EDDI.Instance.CurrentStarSystem.x,
                    EDDI.Instance.CurrentStarSystem.y,
                    EDDI.Instance.CurrentStarSystem.z
                    };
                    eventObject.Add("_systemCoordinates", _coordinates);
                }

            }
            if (EDDI.Instance.CurrentStation != null)
            {
                if (!eventObject.ContainsKey("_marketId") && !eventObject.ContainsKey("MarketID"))
                {
                    eventObject.Add("_marketId", null); // We don't collect this info yet
                } 
                if (!eventObject.ContainsKey("_stationName") && !eventObject.ContainsKey("StationName"))
                {
                    eventObject.Add("_stationName", EDDI.Instance.CurrentStation.name);
                }
            }
            if (EDDI.Instance.CurrentShip != null && !eventObject.ContainsKey("ShipId") && !eventObject.ContainsKey("_shipId"))
            {
                eventObject.Add("_shipId", EDDI.Instance.CurrentShip.LocalId);
            }

            return JsonConvert.SerializeObject(eventObject).Normalize();
        }

        public UserControl ConfigurationTabItem()
        {
            return new ConfigurationWindow();
        }
    }
}
