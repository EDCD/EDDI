using Eddi;
using EddiCompanionAppService;
using EddiDataDefinitions;
using EddiDataProviderService;
using EddiEvents;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Controls;
using Utilities;

namespace EDDNResponder
{
    /// <summary>
    /// A responder for EDDI to provide information to EDDN.
    /// </summary>
    public class EDDNResponder : EDDIResponder
    {
        // We keep track of the starsystem information locally
        public string systemName { get; private set; } = null;
        public long? systemAddress { get; private set; } = null;
        public decimal? systemX { get; private set; } = null;
        public decimal? systemY { get; private set; } = null;
        public decimal? systemZ { get; private set; } = null;
        public string stationName { get; private set; } = null;
        public long? marketId { get; private set; } = null;

        private StarSystemRepository starSystemRepository;

        public string ResponderName()
        {
            return "EDDN responder";
        }

        public string LocalizedResponderName()
        {
            return EddiEddnResponder.Properties.EddnResources.name;
        }

        public string ResponderVersion()
        {
            return "1.0.0";
        }

        public string ResponderDescription()
        {
            return EddiEddnResponder.Properties.EddnResources.desc;
        }

        public EDDNResponder() : this(EddiDataProviderService.StarSystemSqLiteRepository.Instance)
        {}

        public EDDNResponder(StarSystemRepository starSystemRepository)
        {
            this.starSystemRepository = starSystemRepository;
            Logging.Info("Initialised " + ResponderName() + " " + ResponderVersion());
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

            Logging.Debug("Received event " + JsonConvert.SerializeObject(theEvent));

            if (theEvent is LocationEvent)
            {
                handleLocationEvent((LocationEvent)theEvent);
            }
            if (theEvent is JumpedEvent)
            {
                handleJumpedEvent((JumpedEvent)theEvent);
            }

            if (theEvent is DockedEvent)
            {
                handleDockedEvent((DockedEvent)theEvent);
            }

            if (theEvent is MarketInformationUpdatedEvent)
            {
                handleMarketInformationUpdatedEvent((MarketInformationUpdatedEvent)theEvent);
            }

            if (theEvent is JumpedEvent || theEvent is DockedEvent || theEvent is BodyScannedEvent || theEvent is StarScannedEvent)
            {
                handleRawEvent(theEvent);
            }
        }

        public bool Start()
        {
            return true;
        }

        public void Stop()
        {
        }

        public void Reload()
        {
        }

        private bool ShouldUseTestEndpoints()
        {
#if DEBUG
            return true;
#else
            // use test endpoints if the game is in beta or EDDI is not release candidate or final
            return EDDI.Instance.inBeta || (Constants.EDDI_VERSION.phase < Utilities.Version.TestPhase.rc);
#endif
        }

        private void handleLocationEvent(LocationEvent @event)
        {
            // Set all of the information available from the event
            systemName = @event.system;
            systemAddress = @event.systemAddress;
            systemX = @event.x;
            systemY = @event.y;
            systemZ = @event.z;
            stationName = @event.station;
            marketId = @event.marketId;
        }

        private void handleJumpedEvent(JumpedEvent @event)
        {
            // Set all of the information available from the event
            systemName = @event.system;
            systemAddress = @event.systemAddress;
            systemX = @event.x;
            systemY = @event.y;
            systemZ = @event.z;
            stationName = null;
            marketId = null;
        }

        private void handleRawEvent(Event theEvent)
        {
            IDictionary<string, object> data = Deserializtion.DeserializeData(theEvent.raw);
            // Need to strip a number of entries
            data.Remove("CockpitBreach");
            data.Remove("BoostUsed");
            data.Remove("FuelLevel");
            data.Remove("FuelUsed");
            data.Remove("JumpDist");
            data.Remove("Wanted");

            // Need to remove any keys ending with _Localised
            data = data.Where(x => !x.Key.EndsWith("_Localised")).ToDictionary(x => x.Key, x => x.Value);

            // Can only proceed if we know our current system

            // Need to add StarSystem to scan events - can only do so if we have the data
            if (theEvent is BeltScannedEvent || theEvent is StarScannedEvent || theEvent is BodyScannedEvent)
            {
                if (systemName == null || systemAddress == null || systemX == null || systemY == null || systemZ == null)
                {
                    Logging.Debug("Missing current starsystem information, cannot send message to EDDN");
                    return;
                }
                data.Add("StarSystem", systemName);
            }

            // Need to add StarPos to all events that don't already have them
            if (!data.ContainsKey("StarPos"))
            {
                if (systemName == null || systemAddress == null || systemX == null || systemY == null || systemZ == null)
                {
                    Logging.Debug("Missing current starsystem information, cannot send message to EDDN");
                    return;
                }
                IList<decimal> starpos = new List<decimal>
                {
                    systemX.Value,
                    systemY.Value,
                    systemZ.Value
                };
                data.Add("StarPos", starpos);
            }

            EDDNBody body = new EDDNBody
            {
                header = generateHeader(),
                schemaRef = "https://eddn.edcd.io/schemas/journal/1" + (ShouldUseTestEndpoints() ? "/test" : ""),
                message = data
            };

            sendMessage(body);
        }

        private void handleDockedEvent(DockedEvent theEvent)
        {
            // Set all of the information available from the event
            systemName = theEvent.system;
            systemAddress = theEvent.systemAddress;
            stationName = theEvent.station;
            marketId = theEvent.marketId;

            if (eventConfirmCoordinates(theEvent.system, theEvent.systemAddress))
            {
                // When we dock we have access to commodity and outfitting information
                sendCommodityInformation();
                sendOutfittingInformation();
                sendShipyardInformation();
            }
        }

        private void handleMarketInformationUpdatedEvent(MarketInformationUpdatedEvent theEvent)
        {
            // This event is triggered by an update to the profile via the Frontier API
            // Check to make sure the marketId from the acquired profile matches our current station's marketId before continuing
            if (eventStationMatches(marketId))
            {
                // When we dock we have access to commodity and outfitting information
                sendCommodityInformation();
                sendOutfittingInformation();
                sendShipyardInformation();
            }
        }

        private void sendCommodityInformation()
        {
            if (EDDI.Instance.CurrentStation != null && EDDI.Instance.CurrentStation.commodities != null && EDDI.Instance.CurrentStation.commodities.Count > 0)
            {
                List<EDDNEconomy> eddnEconomies = prepareEconomyInformation();
                List<EDDNCommodity> eddnCommodities = prepareCommodityInformation();

                // Only send the message if we have commodities
                if (eddnCommodities.Count > 0)
                {
                    IDictionary<string, object> data = new Dictionary<string, object>
                    {
                        { "timestamp", DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ") },
                        { "systemName", systemName },
                        { "stationName", stationName },
                        { "marketId", marketId }
                    };
                    if (eddnEconomies.Count > 0)
                    {
                        data.Add("economies", eddnEconomies);
                    }
                    data.Add("commodities", eddnCommodities);
                    if (EDDI.Instance.CurrentStation.prohibited?.Count > 0 && EDDI.Instance.CurrentStation.name == stationName)
                    {
                        data.Add("prohibited", EDDI.Instance.CurrentStation.prohibited);
                    }

                    EDDNBody body = new EDDNBody
                    {
                        header = generateHeader(),
                        schemaRef = "https://eddn.edcd.io/schemas/commodity/3" + (ShouldUseTestEndpoints() ? "/test" : ""),
                        message = data
                    };

                    Logging.Debug("EDDN message is: " + JsonConvert.SerializeObject(body));
                    sendMessage(body);
                }
            }
        }

        private static List<EDDNEconomy> prepareEconomyInformation()
        {
            List<EDDNEconomy> eddnEconomies = new List<EDDNEconomy>();
            if (EDDI.Instance.CurrentStation.economiesShares != null)
            {
                foreach (EconomyShare economy in EDDI.Instance.CurrentStation.economiesShares)
                {
                    EDDNEconomy eddnEconomy = new EDDNEconomy(economy);
                    eddnEconomies.Add(eddnEconomy);
                }
            }

            return eddnEconomies;
        }

        private static List<EDDNCommodity> prepareCommodityInformation()
        {
            List<EDDNCommodity> eddnCommodities = new List<EDDNCommodity>();
            foreach (CommodityMarketQuote quote in EDDI.Instance.CurrentStation.commodities)
            {
                if (quote.definition == null)
                {
                    continue;
                }
                if (!quote.fromFDev)
                {
                    // We only want data from the Frontier API (or market.json)
                    // Data from 3rd parties (EDDB, EDSM, EDDP, etc.) is not acceptable.
                    continue;
                }
                if (quote.avgprice == 0)
                {
                    // Check that the average price is greater than zero.
                    continue;
                }
                if (quote.definition.category == CommodityCategory.NonMarketable)
                {
                    // Include only marketable commodities.
                    continue;
                }
                EDDNCommodity eddnCommodity = new EDDNCommodity(quote);
                eddnCommodities.Add(eddnCommodity);
            };
            return eddnCommodities;
        }

        private void sendOutfittingInformation()
        {
            if (EDDI.Instance.CurrentStation != null && EDDI.Instance.CurrentStation.outfitting != null)
            {
                List<string> eddnModules = new List<string>();
                foreach (Module module in EDDI.Instance.CurrentStation.outfitting)
                {
                    if ((!module.IsPowerPlay())
                        && (module.EDName.StartsWith("Int_") || module.EDName.StartsWith("Hpt_") || module.EDName.Contains("_Armour_"))
                        && (!(module.EDName == "Int_PlanetApproachSuite")))
                    {
                        eddnModules.Add(module.EDName);
                    }
                }

                // Only send the message if we have modules
                if (eddnModules.Count > 0)
                {
                    IDictionary<string, object> data = new Dictionary<string, object>
                    {
                        { "timestamp", DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ") },
                        { "systemName", systemName },
                        { "stationName", stationName },
                        { "marketId", marketId },
                        { "modules", eddnModules }
                    };

                    EDDNBody body = new EDDNBody
                    {
                        header = generateHeader(),
                        schemaRef = "https://eddn.edcd.io/schemas/outfitting/2" + (ShouldUseTestEndpoints() ? "/test" : ""),
                        message = data
                    };

                    sendMessage(body);
                }
            }
        }

        private void sendShipyardInformation()
        {
            if (EDDI.Instance.CurrentStation != null && EDDI.Instance.CurrentStation.shipyard != null)
            {
                List<string> eddnShips = new List<string>();
                foreach (Ship ship in EDDI.Instance.CurrentStation.shipyard)
                {
                        eddnShips.Add(ship.EDName);
                }

                // Only send the message if we have ships
                if (eddnShips.Count > 0)
                {
                    IDictionary<string, object> data = new Dictionary<string, object>
                    {
                        { "timestamp", DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ") },
                        { "systemName", systemName },
                        { "stationName", stationName },
                        { "marketId", marketId },
                        { "ships", eddnShips }
                    };

                    EDDNBody body = new EDDNBody
                    {
                        header = generateHeader(),
                        schemaRef = "https://eddn.edcd.io/schemas/shipyard/2" + (ShouldUseTestEndpoints() ? "/test" : ""),
                        message = data
                    };

                    sendMessage(body);
                }
            }
        }


        private static string generateUploaderId()
        {
            // Uploader ID is a hash of the commander's name
            //System.Security.Cryptography.SHA256Managed crypt = new System.Security.Cryptography.SHA256Managed();
            //StringBuilder hash = new StringBuilder();
            //string uploader = (EDDI.Instance.Cmdr == null ? "commander" : EDDI.Instance.Cmdr.name);
            //byte[] crypto = crypt.ComputeHash(Encoding.UTF8.GetBytes(uploader), 0, Encoding.UTF8.GetByteCount(uploader));
            //foreach (byte theByte in crypto)
            //{
            //    hash.Append(theByte.ToString("x2"));
            //}
            //return hash.ToString();
            return EDDI.Instance.Cmdr == null || EDDI.Instance.Cmdr.name == null ? "Unknown commander" : EDDI.Instance.Cmdr.name;
        }

        private static EDDNHeader generateHeader()
        {
            EDDNHeader header = new EDDNHeader
            {
                softwareName = Constants.EDDI_NAME,
                softwareVersion = Constants.EDDI_VERSION.ToString(),
                uploaderID = generateUploaderId()
            };
            return header;
        }

        private static void sendMessage(EDDNBody body)
        {
            var client = new RestClient("https://eddn.edcd.io:4430/");
            var request = new RestRequest("upload/", Method.POST);
            request.AddParameter("application/json", JsonConvert.SerializeObject(body), ParameterType.RequestBody);

            Logging.Debug("Sending " + JsonConvert.SerializeObject(body));

            Thread thread = new Thread(() =>
            {
                try
                {
                    IRestResponse response = client.Execute(request);
                    var content = response.Content; // raw content as string
                    Logging.Debug("Response content is " + content);
                }
                catch (ThreadAbortException)
                {
                    Logging.Debug("Thread aborted");
                }
                catch (Exception ex)
                {
                    Logging.Warn("Failed to send error to EDDN", ex);
                }
            })
            {
                Name = "EDDN message",
                IsBackground = true
            };
            thread.Start();
        }

        public UserControl ConfigurationTabItem()
        {
            return null;
        }

        public bool eventConfirmCoordinates(string eventSystem, long? eventSystemAddress = null)
        {
            StarSystem system = starSystemRepository.GetStarSystem(eventSystem);
            if (system != null)
            {
                if ((eventSystemAddress != null && system.systemAddress != null) ? eventSystemAddress == system.systemAddress : true)
                {
                    // The `Docked` event doesn't provide system coordinates, so we use coordinates from our saved star systems
                    // to confirm the coordinates in memory. If the saved star system has a system address, we use that to confirm our lookup too.
                    if (systemX == system.x && systemY == system.y && systemZ == system.z)
                    {
                        return true;
                    }
                }
            }

            // Set values to null if data isn't available. If system coordinates are null, data shall not be sent to EDDN.
            systemX = null;
            systemY = null;
            systemZ = null;
            return false;
        }

        public bool eventStationMatches(long? eventMarketId)
        {
            Profile profile = CompanionAppService.Instance.Profile();
            if (profile != null)
            {
                if (profile.LastStation?.marketId == eventMarketId && (bool?)profile.json["commander"]["docked"] == true)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
