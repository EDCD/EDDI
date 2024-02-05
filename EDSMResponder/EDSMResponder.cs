using EddiConfigService;
using EddiCore;
using EddiDataProviderService;
using EddiEvents;
using EddiStarMapService;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using Utilities;

namespace EddiEdsmResponder
{
    public class EDSMResponder : IEddiResponder
    {
        private Task updateTask;
        private CancellationTokenSource updateThreadCancellationTokenSource;
        private List<string> ignoredEvents = new List<string>();
        private readonly IEdsmService edsmService;
        private readonly DataProviderService dataProviderService;

        // This responder currently requires game version 4.0 or later.
        private static readonly System.Version minGameVersion = new System.Version(4, 0);

        public string ResponderName()
        {
            return Properties.EDSMResources.ResourceManager.GetString( "name", CultureInfo.InvariantCulture );
        }

        public string LocalizedResponderName()
        {
            return Properties.EDSMResources.name;
        }

        public string ResponderDescription()
        {
            return Properties.EDSMResources.desc;
        }

        public EDSMResponder() : this(new StarMapService(null, true))
        { }

        public EDSMResponder(IEdsmService edsmService)
        {
            this.edsmService = edsmService;
            dataProviderService = new DataProviderService(edsmService);
            Logging.Info($"Initialized {ResponderName()}");
        }

        public bool Start()
        {
            Reload();
            edsmService?.StartJournalSync();
            return edsmService != null;
        }

        public void Stop()
        {
            edsmService?.StopJournalSync();
            // Stop flight log synchronization
            updateThreadCancellationTokenSource?.Cancel();
            updateTask = null;
        }

        public void Reload()
        {
            // Set up the star map service
            if (ignoredEvents == null)
            {
                ignoredEvents = edsmService?.getIgnoredEvents();
            }

            if (edsmService != null)
            {
                // Renew our credentials for the EDSM API
                StarMapService.inGameCommanderName = EDDI.Instance.Cmdr?.name;
                edsmService.SetEdsmCredentials();

                if ( updateTask == null && edsmService.EdsmCredentialsSet() )
                {
                    // Spin off a task to download & sync flight logs & system comments from EDSM in the background 
                    updateThreadCancellationTokenSource = new CancellationTokenSource();
                    updateTask = new Task( () =>
                    {
                        try
                        {
                            dataProviderService
                                .syncFromStarMapService( ConfigService.Instance.edsmConfiguration?.lastFlightLogSync );
                        }
                        catch ( TaskCanceledException )
                        {
                            // Nothing to do here
                        }
                    }, updateThreadCancellationTokenSource.Token );
                    updateTask.Start();
                }
            }
        }

        public void Handle(Event theEvent)
        {
            if (EDDI.Instance.inTelepresence)
            {
                // We don't do anything whilst in CQC
                return;
            }

            if (EDDI.Instance.gameIsBeta)
            {
                // We don't send data whilst in beta
                return;
            }

            if (EDDI.Instance.GameVersion is null || EDDI.Instance.GameVersion < minGameVersion)
            {
                // We don't sent data whilst running a lower game version than the minimum required by EDSM
                return;
            }

            if (edsmService != null)
            {
                /// Retrieve applicable transient game state info (metadata) 
                /// for the event and send the event with transient info to EDSM
                IDictionary<string, object> eventObject = null;
                try
                {
                    eventObject = prepareEventData(theEvent);
                }
                catch (System.Exception ex)
                {
                    Logging.Error("Failed to prepare event meta-data for submittal to EDSM", ex);
                }
                if (eventObject != null && !EDDI.Instance.gameIsBeta)
                {
                    edsmService.EnqueueEvent(eventObject);
                }
            }
        }

        private IDictionary<string, object> prepareEventData(Event theEvent)
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
            var currentStarSystem = EDDI.Instance.CurrentStarSystem?.Copy();
            if (currentStarSystem != null)
            {
                if (!eventObject.ContainsKey("_systemAddress"))
                {
                    eventObject.Add("_systemAddress", currentStarSystem.systemAddress);
                }

                if (!eventObject.ContainsKey("_systemName"))
                {
                    eventObject.Add("_systemName", currentStarSystem.systemname);
                }

                if (!eventObject.ContainsKey("_systemCoordinates"))
                {
                    List<decimal?> _coordinates = new List<decimal?>
                    {
                        currentStarSystem.x,
                        currentStarSystem.y,
                        currentStarSystem.z
                    };
                    eventObject.Add("_systemCoordinates", _coordinates);
                }
            }

            var currentStation = EDDI.Instance.CurrentStation?.Copy();
            if (currentStation != null)
            {
                if (!eventObject.ContainsKey("_marketId"))
                {
                    eventObject.Add("_marketId", currentStation.marketId);
                }

                if (!eventObject.ContainsKey("_stationName"))
                {
                    eventObject.Add("_stationName", currentStation.name);
                }
            }

            var currentShip = EDDI.Instance.CurrentShip?.Copy();
            if (currentShip != null && !eventObject.ContainsKey("_shipId"))
            {
                eventObject.Add("_shipId", currentShip.LocalId);
            }

            return eventObject;
        }

        public UserControl ConfigurationTabItem()
        {
            return new ConfigurationWindow(this);
        }
    }
}
