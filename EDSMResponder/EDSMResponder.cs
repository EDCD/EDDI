using Eddi;
using EddiDataProviderService;
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
        private List<string> ignoredEvents = new List<string>();

        public string ResponderName()
        {
            return "EDSM responder";
        }

        public string LocalizedResponderName()
        {
            return Properties.EDSMResources.name;
        }

        public string ResponderVersion()
        {
            return "1.0.0";
        }

        public string ResponderDescription()
        {
            return Properties.EDSMResources.desc;
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
            starMapService = new StarMapService();
            if (ignoredEvents == null)
            {
                ignoredEvents = starMapService?.getIgnoredEvents();
            }

            if (starMapService != null && updateThread == null)
            {
                // Spin off a thread to download & sync flight logs & system comments from EDSM in the background 
                updateThread = new Thread(() => DataProviderService.syncFromStarMapService())
                {
                    IsBackground = true,
                    Name = "EDSM updater"
                };
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
                /// for the event and send the event with transient info to EDSM
                string eventData = null;
                try
                {
                    eventData = prepareEventData(theEvent);
                }
                catch (System.Exception ex)
                {
                    Logging.Error("Failed to prepare event meta-data for submittal to EDSM", ex);
                }
                if (eventData != null && !EDDI.Instance.ShouldUseTestEndpoints())
                {
                    starMapService.sendEvent(eventData);
                }
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
                case "ShipyardBuy":
                    {
                        eventObject.Add("_shipId", null);
                        break;
                    }
                case "SetUserShipName":
                case "ShipyardSwap":
                case "Loadout":
                    {
                        eventObject.TryGetValue("ShipID", out object shipIdVal);
                        if (shipIdVal != null)
                        {
                            eventObject.Add("_shipId", (int)(long)shipIdVal);
                        }
                        break;
                    }
                case "Undocked":
                    {
                        eventObject.Add("_marketId", null);
                        eventObject.Add("_stationName", null);
                        break;
                    }
                case "Location":
                case "FSDJump":
                case "Docked":
                    {
                        if (eventObject.ContainsKey("StarSystem"))
                        {
                            eventObject.Add("_systemName", JsonParsing.getString(eventObject, "StarSystem"));
                        }
                        if (eventObject.ContainsKey("SystemAddress"))
                        {
                            long? systemAddress = JsonParsing.getOptionalLong(eventObject, "SystemAddress");
                            // Some events are bugged and return a SystemAddress of 1, regardles of the system we are in.
                            // We need to ignore data that matches this pattern.
                            systemAddress = (systemAddress > 1 ? systemAddress : null);
                            if (systemAddress != null)
                            {
                                eventObject.Add("_systemAddress", systemAddress);
                            }
                        }
                        if (eventObject.ContainsKey("StarPos"))
                        {
                            eventObject.TryGetValue("StarPos", out object starpos);
                            if (starpos != null)
                            {
                                eventObject.Add("_systemCoordinates", starpos);
                            }
                        }
                        if (eventObject.ContainsKey("MarketID"))
                        {
                            eventObject.Add("_marketId", JsonParsing.getOptionalLong(eventObject, "MarketID"));
                        }
                        if (eventObject.ContainsKey("StationName"))
                        {
                            eventObject.Add("_stationName", JsonParsing.getString(eventObject, "StationName"));
                        }
                        break;
                    }
            }

            // Supplement with metadata from the tracked game state, as applicable
            if (EDDI.Instance.CurrentStarSystem != null)
            {
                if (!eventObject.ContainsKey("_systemAddress"))
                {
                    eventObject.Add("_systemAddress", EDDI.Instance.CurrentStarSystem.systemAddress);
                } 
                if (!eventObject.ContainsKey("_systemName"))
                {
                    eventObject.Add("_systemName", EDDI.Instance.CurrentStarSystem.name);
                }
                if (!eventObject.ContainsKey("_systemCoordinates"))
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
                if (!eventObject.ContainsKey("_marketId"))
                {
                    eventObject.Add("_marketId", EDDI.Instance.CurrentStation.marketId);
                } 
                if (!eventObject.ContainsKey("_stationName"))
                {
                    eventObject.Add("_stationName", EDDI.Instance.CurrentStation.name);
                }
            }
            if (EDDI.Instance.CurrentShip != null && !eventObject.ContainsKey("_shipId"))
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
