using EddiDataDefinitions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Utilities;

namespace EddiCompanionAppService
{
    public partial class CompanionAppService
    {
        /// <summary>Create a  profile given the results from a /profile call</summary>
        public static FrontierApiProfile ProfileFromJson(string data, DateTime timestamp)
        {
            if (!string.IsNullOrEmpty(data))
            {
                return ProfileFromJson(JObject.Parse(data), timestamp);
            }
            return null;
        }

        /// <summary>Create a profile given the results from a /profile call</summary>
        public static FrontierApiProfile ProfileFromJson(JObject json, DateTime timestamp)
        {
            FrontierApiProfile Profile = new FrontierApiProfile
            {
                json = json,
                timestamp = timestamp
            };

            if (json["commander"] != null)
            {
                FrontierApiCommander Commander = new FrontierApiCommander
                {
                    // Caution: The "id" property here may not match the FID returned from the player journal
                    name = (string)json["commander"]["name"],
                    combatrating = CombatRating.FromRank((int?)json["commander"]["rank"]["combat"] ?? 0),
                    traderating = TradeRating.FromRank((int?)json["commander"]["rank"]["trade"] ?? 0),
                    explorationrating = ExplorationRating.FromRank((int?)json["commander"]["rank"]["explore"] ?? 0),
                    cqcrating = CQCRating.FromRank((int?)json["commander"]["rank"]["cqc"] ?? 0),
                    empirerating = EmpireRating.FromRank((int?)json["commander"]["rank"]["empire"] ?? 0),
                    federationrating = FederationRating.FromRank((int?)json["commander"]["rank"]["federation"] ?? 0),
                    mercenaryrating = MercenaryRating.FromRank((int?)json["commander"]["rank"]["soldier"] ?? 0),
                    exobiologistrating = ExobiologistRating.FromRank((int?)json["commander"]["rank"]["exobiologist"] ?? 0),
                    crimerating = (int?)json["commander"]["rank"]["crime"] ?? 0,
                    servicerating = (int?)json["commander"]["rank"]["service"] ?? 0,
                    powerrating = (int?)json["commander"]["rank"]["power"] ?? 0,

                    credits = (long?)json["commander"]["credits"] ?? 0,
                    debt = (long?)json["commander"]["debt"] ?? 0
                };
                Profile.Cmdr = Commander;
                Profile.docked = (bool)json["commander"]["docked"];
                Profile.onFoot = (bool)json["commander"]["onfoot"];
                Profile.alive = (bool)json["commander"]["alive"];

                if (json["commander"]["capabilities"] != null)
                {
                    var contexts = new FrontierApiProfileContexts
                    {
                        allowCobraMkIV = (bool?)json["commander"]["capabilities"]["AllowCobraMkIV"] ?? false,
                        hasHorizons = (bool?)json["commander"]["capabilities"]["Horizons"] ?? false,
                        hasOdyssey = (bool?)json["commander"]["capabilities"]["Odyssey"] ?? false
                    };
                    Profile.contexts = contexts;
                }

                string systemName = json["lastSystem"] == null ? null : (string)json["lastSystem"]["name"];
                if (systemName != null)
                {
                    Profile.CurrentStarSystem = new FrontierApiProfileStarSystem
                    {
                        // Caution: The "id" property here may not match the systemAddress
                        systemName = systemName,
                        // Caution: The "faction" property here may return the edName for the faction rather than the invariant name
                    };
                }

                if (json != null)
                {
                    Profile.LastStation = new FrontierApiProfileStation
                    {
                        name = ((string)json["lastStarport"]?["name"])?.ReplaceEnd('+'),
                        marketId = (long?)json["lastStarport"]?["id"]
                    };
                    if ((bool)json["commander"]["docked"])
                    {
                        Profile.LastStation.systemname = Profile.CurrentStarSystem.systemName;
                    }
                }
            }

            return Profile;
        }

        public FrontierApiProfile Station(ulong? systemAddress, string systemName)
        {
            try
            {
                Logging.Debug("Getting station market data");
                string market = obtainData(ServerURL() + MARKET_URL, out DateTime marketTimestamp);
                market = "{\"lastStarport\":" + market + "}";
                JObject marketJson = JObject.Parse(market);
                var lastStation = ProfileStation(marketTimestamp, JObject.FromObject(marketJson["lastStarport"]));
                lastStation.systemAddress = systemAddress;
                lastStation.systemname = systemName;
                lastStation = ProfileStationOutfittingAndShipyard(lastStation);
                cachedProfile.LastStation = lastStation;
            }
            catch (EliteDangerousCompanionAppException ex)
            {
                // not Logging.Error as Rollbar is getting spammed when the server is down
                Logging.Info(ex.Message);
            }
            return cachedProfile;
        }

        public static FrontierApiProfileStation ProfileStation(DateTime marketTimestamp, JObject marketJson)
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
                    commoditiesupdatedat = Dates.fromDateTimeToSeconds(marketTimestamp),
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

        private FrontierApiProfileStation ProfileStationOutfittingAndShipyard(FrontierApiProfileStation lastStation)
        {
            if (lastStation.stationServices.Exists(s => s.Key.ToLowerInvariant() == "outfitting"))
            {
                Logging.Debug("Getting station outfitting data");
                string outfitting = obtainData(ServerURL() + SHIPYARD_URL, out DateTime outfittingTimestamp);
                outfitting = "{\"lastStarport\":" + outfitting + "}";
                JObject outfittingJson = JObject.Parse(outfitting);
                lastStation.outfitting = OutfittingFromProfile(outfittingJson);
                lastStation.outfittingupdatedat = Dates.fromDateTimeToSeconds(outfittingTimestamp);
            }
            if (lastStation.stationServices.Exists(s => s.Key.ToLowerInvariant() == "shipyard"))
            {
                Logging.Debug("Getting station shipyard data");
                Thread.Sleep(5000);
                string shipyard = obtainData(ServerURL() + SHIPYARD_URL, out DateTime shipyardTimestamp);
                shipyard = "{\"lastStarport\":" + shipyard + "}";
                JObject shipyardJson = JObject.Parse(shipyard);
                lastStation.ships = ShipyardFromProfile(shipyardJson);
                lastStation.shipyardupdatedat = Dates.fromDateTimeToSeconds(shipyardTimestamp);
            }
            return lastStation;
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
