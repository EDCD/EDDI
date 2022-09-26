using EddiDataDefinitions;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading;
using System;
using System.Linq;
using Utilities;

namespace EddiCompanionAppService
{
    public partial class CompanionAppService
    {
        private const string MARKET_URL = "/market";
        private const string SHIPYARD_URL = "/shipyard";

        // We cache the market to avoid spamming the service
        private FrontierApiProfileStation cachedStation;
        private DateTime cachedStationExpires;

        // Set up an event handler for data changes
        public static event EventHandler StationUpdatedEvent;

        public FrontierApiProfileStation Station(bool forceRefresh = false)
        {
            if ((!forceRefresh) && cachedStationExpires > DateTime.UtcNow)
            {
                // return the cached version
                Logging.Debug("Returning cached profile");
                return cachedStation;
            }

            try
            {
                Logging.Debug("Getting station market data");
                string market = obtainData(ServerURL() + MARKET_URL, out DateTime marketTimestamp);
                market = "{\"lastStarport\":" + market + "}";
                JObject marketJson = JObject.Parse(market);
                var lastStation = MarketFromJson(marketTimestamp, JObject.FromObject(marketJson["lastStarport"]));
                
                if (TryGetOutfitting(lastStation, out List<OutfittingInfoItem> outfitting, out DateTime outfittingUpdatedAt))
                {
                    lastStation.outfitting = outfitting;
                    lastStation.outfittingupdatedat = outfittingUpdatedAt;
                }

                if (TryGetShipyard(lastStation, out List<ShipyardInfoItem> ships, out DateTime shipyardUpdatedAt))
                {
                    lastStation.ships = ships;
                    lastStation.shipyardupdatedat = shipyardUpdatedAt;
                }

                cachedStation = lastStation;
                StationUpdatedEvent?.Invoke(cachedStation, EventArgs.Empty);

                if (cachedStation != null)
                {
                    cachedStationExpires = DateTime.UtcNow.AddSeconds(30);
                    Logging.Debug("Station is " + JsonConvert.SerializeObject(cachedStation));
                }
            }
            catch (EliteDangerousCompanionAppException ex)
            {
                // not Logging.Error as Rollbar is getting spammed when the server is down
                Logging.Info(ex.Message);
            }

            return cachedStation;
        }

        public static FrontierApiProfileStation MarketFromJson(DateTime marketTimestamp, JObject marketJson)
        {
            FrontierApiProfileStation lastStation = null;
            try
            {
                string lastStarport = ((string)marketJson["name"])?.ReplaceEnd('+');
                long? marketId = (long?)marketJson["id"];
                lastStation = new FrontierApiProfileStation
                {
                    name = lastStarport,
                    marketId = marketId,
                    economyShares = EconomiesFromProfile(marketJson),
                    eddnCommodityMarketQuotes = CommodityQuotesFromProfile(marketJson),
                    prohibitedCommodities = ProhibitedCommoditiesFromProfile(marketJson),
                    commoditiesupdatedat = marketTimestamp,
                    json = marketJson
                };

                List<KeyValuePair<string, string>> stationServices = new List<KeyValuePair<string, string>>();
                foreach (var jToken in marketJson["services"])
                {
                    // These are key value pairs. The Key is the name of the service, the Value is its state.
                    var serviceJSON = (JProperty)jToken;
                    var service = new KeyValuePair<string, string>(serviceJSON.Name, serviceJSON.Value.ToString());
                    stationServices.Add(service);
                }
                lastStation.stationServices = stationServices;
            }
            catch (JsonException ex)
            {
                Logging.Error("Failed to parse companion station data", ex);
            }
            Logging.Debug("Station is " + JsonConvert.SerializeObject(lastStation));
            return lastStation;
        }

        private bool TryGetOutfitting(FrontierApiProfileStation lastStation, out List<OutfittingInfoItem> outfitting, out DateTime outfittingUpdatedAt)
        {
            outfitting = null;
            outfittingUpdatedAt = DateTime.MinValue;
            if (lastStation.stationServices.Exists(s => s.Key.ToLowerInvariant() == "outfitting"))
            {
                Logging.Debug("Getting station outfitting data");
                string outfittingData = obtainData(ServerURL() + SHIPYARD_URL, out DateTime outfittingTimestamp);
                outfittingData = "{\"lastStarport\":" + outfittingData + "}";
                JObject outfittingJson = JObject.Parse(outfittingData);
                outfitting = OutfittingFromProfile(outfittingJson);
                outfittingUpdatedAt = outfittingTimestamp;
            }
            return outfitting != null;
        }

        private bool TryGetShipyard (FrontierApiProfileStation lastStation, out List<ShipyardInfoItem> ships, out DateTime shipyardUpdatedAt)
        {
            ships = null;
            shipyardUpdatedAt = DateTime.MinValue;
            if (lastStation.stationServices.Exists(s => s.Key.ToLowerInvariant() == "shipyard"))
            {
                Logging.Debug("Getting station shipyard data");
                Thread.Sleep(5000);
                string shipyardData = obtainData(ServerURL() + SHIPYARD_URL, out DateTime shipyardTimestamp);
                shipyardData = "{\"lastStarport\":" + shipyardData + "}";
                JObject shipyardJson = JObject.Parse(shipyardData);
                ships = ShipyardFromProfile(shipyardJson);
                shipyardUpdatedAt = shipyardTimestamp;
            }
            return ships != null;
        }

        // Obtain the list of outfitting modules from the profile
        public static List<OutfittingInfoItem> OutfittingFromProfile(JObject json)
        {
            var edModules = new List<OutfittingInfoItem>();

            if (json != null && json["modules"] != null)
            {
                foreach (var jToken in json["modules"])
                {
                    var moduleJsonProperty = (JProperty)jToken;
                    JObject moduleJson = (JObject)moduleJsonProperty.Value;
                    // Not interested in paintjobs, decals, ...
                    string moduleCategory = (string)moduleJson["category"]; // need to convert from LINQ to string
                    switch (moduleCategory)
                    {
                        case "weapon":
                        case "module":
                        case "utility":
                            {
                                edModules.Add(JsonConvert.DeserializeObject<OutfittingInfoItem>(moduleJson.ToString()));
                            }
                            break;
                    }
                }
            }
            return edModules;
        }

        // Obtain the list of station economies from the profile
        public static List<FrontierApiEconomyShare> EconomiesFromProfile(dynamic json)
        {
            var economyShares = new List<FrontierApiEconomyShare>();

            if (json != null && json["economies"] != null)
            {
                foreach (dynamic economyJson in json["economies"])
                {
                    dynamic economy = economyJson.Value;
                    string name = ((string)economy["name"]).Replace("Agri", "Agriculture");
                    decimal proportion = (decimal)economy["proportion"];
                    economyShares.Add(new FrontierApiEconomyShare(name, proportion));
                }
            }
            economyShares = economyShares.OrderByDescending(x => x.proportion).ToList();
            Logging.Debug("Economies are " + JsonConvert.SerializeObject(economyShares));
            return economyShares;
        }

        // Obtain the list of prohibited commodities from the profile
        public static List<KeyValuePair<long, string>> ProhibitedCommoditiesFromProfile(dynamic json)
        {
            var edProhibitedCommodities = new List<KeyValuePair<long, string>>();
            if (json != null && json["prohibited"] != null)
            {
                foreach (JProperty prohibitedJSON in json["prohibited"])
                {
                    var prohibitedCommodity = new KeyValuePair<long, string>(long.Parse(prohibitedJSON.Name), prohibitedJSON.Value.ToString());
                    edProhibitedCommodities.Add(prohibitedCommodity);
                }
            }
            Logging.Debug("Prohibited Commodities are " + JsonConvert.SerializeObject(edProhibitedCommodities.Select(c => c.Value).ToList()));
            return edProhibitedCommodities;
        }

        // Obtain the list of commodities from the profile
        public static List<MarketInfoItem> CommodityQuotesFromProfile(JObject json)
        {
            var eddnCommodityMarketQuotes = new List<MarketInfoItem>();
            if (json?["commodities"] != null)
            {
                eddnCommodityMarketQuotes = json["commodities"]
                    .Select(c => JsonConvert.DeserializeObject<MarketInfoItem>(c.ToString())).ToList();
            }
            return eddnCommodityMarketQuotes;
        }

        // Obtain the list of ships available at the station from the profile
        public static List<ShipyardInfoItem> ShipyardFromProfile(JObject json)
        {
            List<ShipyardInfoItem> edShipyardShips = new List<ShipyardInfoItem>();
            if (json?["ships"] != null)
            {
                edShipyardShips = json?["ships"]["shipyard_list"].Children().Values()
                    .Select(s => JsonConvert.DeserializeObject<ShipyardInfoItem>(s.ToString())).ToList();

                if (json["ships"]["unavailable_list"] != null)
                {
                    edShipyardShips.AddRange(json["ships"]["unavailable_list"]
                        .Select(s => JsonConvert.DeserializeObject<ShipyardInfoItem>(s.ToString())).ToList());
                }
            }
            return edShipyardShips;
        }
    }
}