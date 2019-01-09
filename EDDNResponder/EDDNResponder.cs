using Eddi;
using EddiCompanionAppService;
using EddiDataDefinitions;
using EddiDataProviderService;
using EddiEvents;
using EddiSpeechService; // Used in release but not in debug.
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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

        public bool invalidState { get; private set; }

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

            if (theEvent is MarketInformationUpdatedEvent)
            {
                handleMarketInformationUpdatedEvent((MarketInformationUpdatedEvent)theEvent);
            }
            else
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

        private void handleRawEvent(Event theEvent)
        {
            IDictionary<string, object> data = Deserializtion.DeserializeData(theEvent.raw);
            string edType = JsonParsing.getString(data, "event");

            if (edType == "FSDTarget" || edType == "StartJump")
            {
                // FSDTarget events describing the system we are targetting rather than the system we are in.
                // Scan events can register after StartJump and before we actually leave the originating system.
                // These must be ignored.
                return;
            }
            else if (edType == "Location" || edType == "FSDJump")
            {
                // We always start fresh from Location and FSDJump events
                invalidState = false;
                ClearLocation();
            }

            // Except as noted above, always attempt to obtain available location data from the active event 
            GetLocationData(data);

            // Confirm the data in memory is as accurate as possible
            if (edType == "Docked" || edType == "Scan")
            {
                CheckLocationData(data);
            }

            if (LocationIsSet())
            {
                if (edType == "Docked" && systemName != null && stationName != null && marketId != null)
                {
                    // Send station data from the CAPI servers
                    sendCommodityInformation();
                    sendOutfittingInformation();
                    sendShipyardInformation();
                }

                if (edType == "Location" || edType == "FSDJump" || edType == "Docked" || edType == "Scan")
                {
                    StripPersonalData(data);
                    EnrichLocationData(edType, data);

                    if (data != null)
                    {
                        SendToEDDN(data);
                    }
                }
            }
        }

        private void ClearLocation()
        {
            systemName = null;
            systemAddress = null;
            systemX = null;
            systemY = null;
            systemZ = null;
            stationName = null;
            marketId = null;
        }

        private void GetLocationData(IDictionary<string, object> data)
        {
            try
            {
                systemName = JsonParsing.getString(data, "StarSystem") ?? systemName;

                // Some events are bugged and return a SystemAddress of 1, regardles of the system we are in.
                // We need to ignore data that matches this pattern.
                long? SystemAddress = JsonParsing.getOptionalLong(data, "SystemAddress");
                systemAddress = (SystemAddress > 1 ? SystemAddress : systemAddress);

                data.TryGetValue("StarPos", out object starpos);
                if (starpos != null)
                {
                    List<object> starPos = (List<object>)starpos;
                    systemX = Math.Round(JsonParsing.getDecimal("X", starPos[0]) * 32M) / 32M;
                    systemY = Math.Round(JsonParsing.getDecimal("Y", starPos[1]) * 32M) / 32M;
                    systemZ = Math.Round(JsonParsing.getDecimal("Z", starPos[2]) * 32M) / 32M;
                }

                marketId = JsonParsing.getOptionalLong(data, "MarketID") ?? marketId;
                stationName = JsonParsing.getString(data, "StationName") ?? stationName;
            }
            catch (Exception ex)
            {
                Logging.Error("Failed to parse EDDN location data from journal entry line: " + JsonConvert.SerializeObject(data), ". Exception: " + ex.Message + ". " + ex.StackTrace);
            }
        }

        private void CheckLocationData(IDictionary<string, object> data)
        {
            // Can only send journal data if we know our current location data is correct
            // If any location data is null, data shall not be sent to EDDN.
            if (LocationIsSet())
            {
                // The `Docked` event doesn't provide system coordinates, and the `Scan`event doesn't provide any system location data.
                // The EDDN journal schema requires that we enrich the journal event data with coordinates and system name (and system address if possible).
                if (data.ContainsKey("BodyName") && !data.ContainsKey("SystemName"))
                {
                    // Apply heuristics to weed out mismatched systems and bodies
                    ConfirmScan(JsonParsing.getString(data, "BodyName"));
                }
                if (!data.ContainsKey("SystemAddress") || !data.ContainsKey("StarPos"))
                {
                    // Out of an overabundance of caution, we do not use data from our saved star systems to enrich the data we send to EDDN, 
                    // but we do use it as an independent check to make sure our system address and coordinates are accurate
                    ConfirmAddressAndCoordinates(systemName);
                }

                // Logging.Warn("TimeStamp: " + ((DateTime)data["timestamp"]).ToUniversalTime().ToString() + ", " + "SystemName: " + systemName + ", " + "SystemAddress: " + systemAddress + ", " + "StarPos: " + systemX + ", " + systemY + ", " + systemZ + ", " + "Event: " + (string)data["event"]);

                if (LocationIsSet())
                {
                    invalidState = false;
                }
                else if (!invalidState)
                {
                    invalidState = true;
#if DEBUG
#else
                    Logging.Warn("The EDDN responder is in an invalid state and is unable to send messages.", JsonConvert.SerializeObject(this) + " Event: " + JsonConvert.SerializeObject(data));
                    SpeechService.Instance.Say(null, EddiEddnResponder.Properties.EddnResources.errPosition, true);
#endif
                }
            }
        }

        private bool LocationIsSet()
        {
            return systemName != null && systemAddress != null && systemX != null && systemY != null && systemZ != null;
        }

        private IDictionary<string, object> StripPersonalData(IDictionary<string, object> data)
        {
            // Need to strip a number of entries
            data.Remove("ActiveFine");
            data.Remove("CockpitBreach");
            data.Remove("BoostUsed");
            data.Remove("FuelLevel");
            data.Remove("FuelUsed");
            data.Remove("BoostUsed");
            data.Remove("JumpDist");
            data.Remove("Wanted");

            data.TryGetValue("Factions", out object factionsVal);
            if (factionsVal != null)
            {
                var factions = (List<object>)factionsVal;
                foreach (object faction in factions)
                {
                    ((IDictionary<string, object>)faction).Remove("MyReputation");
                    ((IDictionary<string, object>)faction).Remove("SquadronFaction");
                    ((IDictionary<string, object>)faction).Remove("HappiestSystem");
                    ((IDictionary<string, object>)faction).Remove("HomeSystem");
                }
            }

            // Need to remove any keys ending with _Localised
            data = data.Where(x => !x.Key.EndsWith("_Localised")).ToDictionary(x => x.Key, x => x.Value);
            return data;
        }

        private IDictionary<string, object> EnrichLocationData(string edType, IDictionary<string, object> data)
        {
            if (!data.ContainsKey("StarSystem"))
            {
                data.Add("StarSystem", systemName);
            }
            if (!data.ContainsKey("SystemAddress"))
            {
                data.Add("SystemAddress", systemAddress);
            }
            if (!data.ContainsKey("StarPos"))
            {
                IList<decimal> starpos = new List<decimal>
                {
                    systemX.Value,
                    systemY.Value,
                    systemZ.Value
                };
                data.Add("StarPos", starpos);
            }
            return data;
        }

        private static void SendToEDDN(IDictionary<string, object> data)
        {
            EDDNBody body = new EDDNBody
            {
                header = generateHeader(),
                schemaRef = "https://eddn.edcd.io/schemas/journal/1" + (EDDI.Instance.ShouldUseTestEndpoints() ? "/test" : ""),
                message = data
            };

            sendMessage(body);
        }

        private void handleMarketInformationUpdatedEvent(MarketInformationUpdatedEvent theEvent)
        {
            // This event is triggered by an update to the profile via the Frontier API
            // Check to make sure the marketId from the acquired profile matches our current station's marketId before continuing
            if (eventStationMatches(marketId, theEvent.update))
            {
                // When we dock we have access to commodity and outfitting information
                switch (theEvent.update)
                {
                    case "market":
                        {
                            sendCommodityInformation();
                        }
                        break;
                    case "outfitting":
                        {
                            sendOutfittingInformation();
                        }
                        break;
                    case "profile":
                        {
                            sendCommodityInformation();
                            sendOutfittingInformation();
                            sendShipyardInformation();
                        }
                        break;
                    case "shipyard":
                        {
                            sendShipyardInformation();
                        }
                        break;
                }
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
                        schemaRef = "https://eddn.edcd.io/schemas/commodity/3" + (EDDI.Instance.ShouldUseTestEndpoints() ? "/test" : ""),
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
            if (EDDI.Instance.CurrentStation.economyShares != null)
            {
                foreach (EconomyShare economy in EDDI.Instance.CurrentStation.economyShares)
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
                        schemaRef = "https://eddn.edcd.io/schemas/outfitting/2" + (EDDI.Instance.ShouldUseTestEndpoints() ? "/test" : ""),
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
                        schemaRef = "https://eddn.edcd.io/schemas/shipyard/2" + (EDDI.Instance.ShouldUseTestEndpoints() ? "/test" : ""),
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

        private bool ConfirmAddressAndCoordinates(string systemName)
        {
            if (systemName != null)
            {
                StarSystem system = starSystemRepository.GetOrCreateStarSystem(systemName);
                if (system != null)
                {
                    if (systemAddress != system.systemAddress)
                    {
                        systemAddress = null;
                    }
                    if (systemX != system.x || systemY != system.y || systemZ != system.z)
                    {
                        systemX = null;
                        systemY = null;
                        systemZ = null;
                    }
                }
                else
                {
                    ClearLocation();
                }
            }
            return systemAddress != null && systemX != null && systemY != null && systemZ != null;
        }

        private bool ConfirmScan(string bodyName)
        {
            if (bodyName != null && systemName != null)
            {
                if (bodyName.StartsWith(systemName))
                {
                    // If the system name is a subset of the body name, we're probably in the right place.
                    return true;
                }
                else
                {
                    // If the body doesn't start with the system name, it should also 
                    // not match a naming pattern for a procedurally generated name.
                    // If it does, it's (probably) in the wrong place.
                    Regex isProcGenName = new Regex(@"[A-Z][A-Z]-[A-Z] [a-h][0-9]");
                    if (!isProcGenName.IsMatch(bodyName))
                    {
                        return true;
                    }
                }
            }
            // Set values to null if data can't be confirmed. 
            ClearLocation();
            return false;
        }

        private bool eventStationMatches(long? eventMarketId, string update)
        {
            if (update == "profile")
            {
                Profile profile = CompanionAppService.Instance?.Profile();
                if (profile != null)
                {
                    if (profile.LastStation?.marketId == eventMarketId && (bool?)profile.json["commander"]["docked"] == true)
                    {
                        return true;
                    }
                }
            }
            else if (EDDI.Instance?.CurrentStation?.marketId == marketId)
            {
                return true;
            }
            return false;
        }
    }
}
