using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using EddiDataDefinitions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Utilities;

namespace EddiCompanionAppService
{
    public partial class CompanionAppService
    {
        /// <summary>Create a  profile given the results from a /profile call</summary>
        public static Profile ProfileFromJson(string data, DateTime timestamp)
        {
            if (!string.IsNullOrEmpty(data))
            {
                return ProfileFromJson(JObject.Parse(data), timestamp);
            }
            return null;
        }

        /// <summary>Create a profile given the results from a /profile call</summary>
        public static Profile ProfileFromJson(JObject json, DateTime timestamp)
        {
            Profile Profile = new Profile
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
                    combatrating = CombatRating.FromRank((int)json["commander"]["rank"]["combat"]),
                    traderating = TradeRating.FromRank((int)json["commander"]["rank"]["trade"]),
                    explorationrating = ExplorationRating.FromRank((int)json["commander"]["rank"]["explore"]),
                    cqcrating = CQCRating.FromRank((int)json["commander"]["rank"]["cqc"]),
                    empirerating = EmpireRating.FromRank((int)json["commander"]["rank"]["empire"]),
                    federationrating = FederationRating.FromRank((int)json["commander"]["rank"]["federation"]),
                    crimerating = (int)json["commander"]["rank"]["crime"],
                    servicerating = (int)json["commander"]["rank"]["service"],
                    powerrating = (int)json["commander"]["rank"]["power"],

                    credits = (long)json["commander"]["credits"],
                    debt = (long)json["commander"]["debt"]
                };
                Profile.Cmdr = Commander;
                Profile.docked = (bool)json["commander"]["docked"];
                Profile.alive = (bool)json["commander"]["alive"];

                if (json["commander"]["capabilities"] != null)
                {
                    var contexts = new ProfileContexts
                    {
                        allowCobraMkIV = (bool)json["commander"]["capabilities"]["AllowCobraMkIV"],
                        inHorizons = (bool)json["commander"]["capabilities"]["Horizons"]
                    };
                    Profile.contexts = contexts;
                }

                string systemName = json["lastSystem"] == null ? null : (string)json["lastSystem"]["name"];
                if (systemName != null)
                {
                    Profile.CurrentStarSystem = new ProfileStarSystem
                    {
                        // Caution: The "id" property here may not match the systemAddress
                        systemName = systemName,
                        // Caution: The "faction" property here may return the edName for the faction rather than the invariant name
                    };
                }

                if (json["lastStarport"] != null)
                {
                    Profile.LastStation = new ProfileStation
                    {
                        name = (string)json["lastStarport"]["name"],
                        marketId = (long?)json["lastStarport"]["id"]
                    };
                    if ((bool)json["commander"]["docked"])
                    {
                        Profile.LastStation.systemname = Profile.CurrentStarSystem.systemName;
                    }
                }
            }

            return Profile;
        }

        public Profile Station(long? systemAddress, string systemName)
        {
            try
            {
                Logging.Debug("Getting station market data");
                string market = obtainProfile(ServerURL() + MARKET_URL, out DateTime marketTimestamp);
                market = "{\"lastStarport\":" + market + "}";
                JObject marketJson = JObject.Parse(market);
                string lastStarport = (string)marketJson["lastStarport"]["name"];
                long? marketId = (long?)marketJson["lastStarport"]["id"];
                cachedProfile.LastStation = new ProfileStation
                {
                    name = lastStarport,
                    marketId = marketId,
                    systemAddress = systemAddress,
                    systemname = systemName,
                    economyShares = EconomiesFromProfile(marketJson),
                    eddnCommodityMarketQuotes = CommodityQuotesFromProfile(marketJson),
                    prohibitedCommodities = ProhibitedCommoditiesFromProfile(marketJson),
                    commoditiesupdatedat = Dates.fromDateTimeToSeconds(marketTimestamp),
                    json = marketJson
                };

                List<KeyValuePair<string, string>> stationServices = new List<KeyValuePair<string, string>>();
                foreach (var jToken in marketJson["lastStarport"]["services"])
                {
                    var serviceJSON = (JProperty)jToken;
                    var service = new KeyValuePair<string, string>(serviceJSON.Name, serviceJSON.Value.ToString());
                    stationServices.Add(service);
                }
                cachedProfile.LastStation.stationServices = stationServices;

                if (stationServices.Exists(s => s.Key.ToLowerInvariant() == "outfitting"))
                {
                    Logging.Debug("Getting station outfitting data");
                    string outfitting = obtainProfile(ServerURL() + SHIPYARD_URL, out DateTime outfittingTimestamp);
                    outfitting = "{\"lastStarport\":" + outfitting + "}";
                    JObject outfittingJson = JObject.Parse(outfitting);
                    cachedProfile.LastStation.outfitting = OutfittingFromProfile(outfittingJson);
                    cachedProfile.LastStation.outfittingupdatedat = Dates.fromDateTimeToSeconds(outfittingTimestamp);
                }

                if (stationServices.Exists(s => s.Key.ToLowerInvariant() == "shipyard"))
                {
                    Logging.Debug("Getting station shipyard data");
                    Thread.Sleep(5000);
                    string shipyard = obtainProfile(ServerURL() + SHIPYARD_URL, out DateTime shipyardTimestamp);
                    shipyard = "{\"lastStarport\":" + shipyard + "}";
                    JObject shipyardJson = JObject.Parse(shipyard);
                    cachedProfile.LastStation.ships = ShipyardFromProfile(shipyardJson);
                    cachedProfile.LastStation.shipyardupdatedat = Dates.fromDateTimeToSeconds(shipyardTimestamp);
                }
            }
            catch (JsonException ex)
            {
                Logging.Error("Failed to parse companion station data", ex);
            }
            catch (EliteDangerousCompanionAppException ex)
            {
                // not Logging.Error as Rollbar is getting spammed when the server is down
                Logging.Info(ex.Message);
            }

            Logging.Debug("Station is " + JsonConvert.SerializeObject(cachedProfile));
            return cachedProfile;
        }

        // Obtain the list of outfitting modules from the profile
        public static List<ProfileModule> OutfittingFromProfile(JObject json)
        {
            var edModules = new List<ProfileModule>();

            if (json["lastStarport"] != null && json["lastStarport"]["modules"] != null)
            {
                foreach (var jToken in json["lastStarport"]["modules"])
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
                                long id = (long)moduleJson["id"];
                                string edName = (string)moduleJson["name"];
                                long cost = (long)moduleJson["cost"];
                                edModules.Add(new ProfileModule(id, edName, moduleCategory, cost, JsonConvert.SerializeObject(jToken)));
                            }
                            break;
                    }
                }
            }
            return edModules;
        }

        // Obtain the list of station economies from the profile
        public static List<ProfileEconomyShare> EconomiesFromProfile(dynamic json)
        {
            var economyShares = new List<ProfileEconomyShare>();

            if (json["lastStarport"] != null && json["lastStarport"]["economies"] != null)
            {
                foreach (dynamic economyJson in json["lastStarport"]["economies"])
                {
                    dynamic economy = economyJson.Value;
                    string name = ((string)economy["name"]).Replace("Agri", "Agriculture");
                    decimal proportion = (decimal)economy["proportion"];
                    economyShares.Add(new ProfileEconomyShare(name, proportion));
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
            if (json["lastStarport"] != null && json["lastStarport"]["prohibited"] != null)
            {
                foreach (JProperty prohibitedJSON in json["lastStarport"]["prohibited"])
                {
                    var prohibitedCommodity = new KeyValuePair<long, string>(long.Parse(prohibitedJSON.Name), prohibitedJSON.Value.ToString());
                    edProhibitedCommodities.Add(prohibitedCommodity);
                }
            }
            Logging.Debug("Prohibited Commodities are " + JsonConvert.SerializeObject(edProhibitedCommodities.Select(c => c.Value).ToList()));
            return edProhibitedCommodities;
        }

        // Obtain the list of commodities from the profile
        private static List<EddnCommodityMarketQuote> CommodityQuotesFromProfile(JObject json)
        {
            var eddnCommodityMarketQuotes = new List<EddnCommodityMarketQuote>();
            if (json["lastStarport"] != null && json["lastStarport"]["commodities"] != null)
            {
                foreach (var jToken in json["lastStarport"]["commodities"])
                {
                    var commodityJSON = (JObject)jToken;
                    try
                    {
                        eddnCommodityMarketQuotes.Add(new EddnCommodityMarketQuote(commodityJSON));
                    }
                    catch (Exception e)
                    {
                        Dictionary<string, object> data = new Dictionary<string, object>()
                        {
                            { "capiJson", JsonConvert.SerializeObject(commodityJSON) },
                            { "Exception", e }
                        };
                        Logging.Error("Failed to handle Frontier API commodity json", data);
                    }
                }
            }

            return eddnCommodityMarketQuotes;
        }

        // Obtain the list of ships available at the station from the profile
        public static List<ProfileShip> ShipyardFromProfile(JObject json)
        {
            List<ProfileShip> edShipyardShips = new List<ProfileShip>();

            if (json["lastStarport"] != null && json["lastStarport"]["ships"] != null)
            {
                // shipyard_list is a JObject containing JObjects but let's code defensively because FDev
                var shipyardList = json["lastStarport"]["ships"]["shipyard_list"].Children();
                foreach (JToken shipToken in shipyardList.Values())
                {
                    JObject shipJson = shipToken as JObject;
                    var Ship = ShipyardShipFromProfile(shipJson);
                    edShipyardShips.Add(Ship);
                }

                // unavailable_list is a JArray containing JObjects
                JArray unavailableList = json["lastStarport"]["ships"]["unavailable_list"] as JArray;
                if (unavailableList != null)
                {
                    foreach (var jToken in unavailableList)
                    {
                        var shipJson = (JObject)jToken;
                        var Ship = ShipyardShipFromProfile(shipJson);
                        edShipyardShips.Add(Ship);
                    }
                }
            }

            return edShipyardShips;
        }

        private static ProfileShip ShipyardShipFromProfile(JObject shipJson)
        {
            long id = (long)shipJson["id"];
            string edName = (string)shipJson["name"];
            long baseValue = (long)shipJson["basevalue"];
            return new ProfileShip(id, edName, baseValue, JsonConvert.SerializeObject(shipJson));
        }
    }
}
