using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using Utilities;

namespace EddiDataDefinitions
{
    public class FrontierApiStation
    {
        /// <summary>Unique 64 bit id value for station</summary>
        public long? marketId { get; set; }

        /// <summary>The name</summary>
        public string name { get; set; }

        /// <summary>A list of the services offered by this station</summary>
        public List<KeyValuePair<string, string>> stationServices { get; set; } = new List<KeyValuePair<string, string>>();

        /// <summary>What are the economies at the station, with proportions for each</summary>
        public List<FrontierApiEconomyShare> economyShares { get; set; } = new List<FrontierApiEconomyShare>();

        /// <summary>Commodity market quotes as-received from the profile</summary>
        public List<MarketInfoItem> eddnCommodityMarketQuotes { get; set; } = new List<MarketInfoItem>();

        /// <summary>Prohibited commodities as-received from the profile</summary>
        public List<KeyValuePair<long, string>> prohibitedCommodities { get; set; } = new List<KeyValuePair<long, string>>();

        /// <summary>Outfitting modules as-received from the profile</summary>
        public List<OutfittingInfoItem> outfitting { get; set; } = new List<OutfittingInfoItem>();

        /// <summary>Ship models as-received from the profile</summary>
        public List<ShipyardInfoItem> ships { get; set; } = new List<ShipyardInfoItem>();

        /// <summary>The market JSON object</summary>
        public JObject marketJson { get; set; }

        /// <summary>The shipyard JSON object</summary>
        public JObject shipyardJson { get; set; }

        // Admin - the last time the market information present changed
        public DateTime commoditiesupdatedat;

        // Admin - the last time the outfitting information present changed
        public DateTime outfittingupdatedat;

        // Admin - the last time the shipyard information present changed
        public DateTime shipyardupdatedat;

        public static FrontierApiStation FromJson(JObject marketJson, JObject shipyardJson)
        {
            if (marketJson != null && shipyardJson != null && marketJson["id"]?.ToObject<ulong>() != shipyardJson["id"]?.ToObject<ulong>())
            {
                throw new ArgumentException("Frontier API market and shipyard endpoint data are not synchronized.");
            }

            // Obtain the list of station economies
            List<FrontierApiEconomyShare> EconomiesFromProfile(JObject json)
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
                Logging.Debug("Economies are: ", economyShares);
                return economyShares;
            }

            // Obtain the list of commodities
            List<MarketInfoItem> CommodityQuotesFromProfile(JObject json)
            {
                var eddnCommodityMarketQuotes = new List<MarketInfoItem>();
                if (json?["commodities"] != null)
                {
                    eddnCommodityMarketQuotes = json["commodities"]
                        .Select(c => JsonConvert.DeserializeObject<MarketInfoItem>(c.ToString())).ToList();
                }
                return eddnCommodityMarketQuotes;
            }

            // Obtain the list of prohibited commodities
            List<KeyValuePair<long, string>> ProhibitedCommoditiesFromProfile(JObject json)
            {
                var edProhibitedCommodities = new List<KeyValuePair<long, string>>();
                if (json != null && json["prohibited"] != null)
                {
                    foreach (var jToken in json["prohibited"])
                    {
                        var prohibitedJSON = (JProperty)jToken;
                        var prohibitedCommodity = new KeyValuePair<long, string>(long.Parse(prohibitedJSON.Name), prohibitedJSON.Value.ToString());
                        edProhibitedCommodities.Add(prohibitedCommodity);
                    }
                }
                Logging.Debug("Prohibited Commodities are: ", edProhibitedCommodities.Select(c => c.Value).ToList());
                return edProhibitedCommodities;
            }

            // Obtain the list of outfitting modules
            List<OutfittingInfoItem> OutfittingFromProfile(JObject json)
            {
                var edModules = new List<OutfittingInfoItem>();

                if (json != null && json["modules"] != null)
                {
                    foreach (var jToken in json["modules"])
                    {
                        var moduleJsonProperty = (JProperty)jToken;
                        JObject moduleJson = (JObject)moduleJsonProperty.Value;
                        // Not interested in paintjobs, decals, ...
                        string moduleCategory =
                            (string)moduleJson[
                                "category"]; // need to convert from LINQ to string
                        switch (moduleCategory)
                        {
                            case "weapon":
                            case "module":
                            case "utility":
                            {
                                edModules.Add(
                                    JsonConvert.DeserializeObject<OutfittingInfoItem>(
                                        moduleJson.ToString()));
                            }
                                break;
                        }
                    }
                }
                return edModules;
            }

            // Obtain the list of ships available at the station from the profile
            List<ShipyardInfoItem> ShipyardFromProfile(JObject json)
            {
                List<ShipyardInfoItem> edShipyardShips = new List<ShipyardInfoItem>();
                if (json?["ships"] != null)
                {
                    edShipyardShips = json["ships"]?["shipyard_list"]?.Children().Values()
                        .Select(s => JsonConvert.DeserializeObject<ShipyardInfoItem>(s.ToString()))
                        .ToList() ?? new List<ShipyardInfoItem>();

                    if (json["ships"]["unavailable_list"] != null)
                    {
                        edShipyardShips.AddRange(json["ships"]["unavailable_list"]
                            .Select(s => JsonConvert.DeserializeObject<ShipyardInfoItem>(s.ToString())).ToList());
                    }
                }
                return edShipyardShips;
            }

            FrontierApiStation lastStation = null;
            try
            {
                string lastStarport = ((string)marketJson?["name"])?.ReplaceEnd('+') ?? ((string)shipyardJson?["name"])?.ReplaceEnd('+');
                long? marketId = (long?)marketJson?["id"] ?? (long?)shipyardJson?["id"];
                lastStation = new FrontierApiStation
                {
                    name = lastStarport,
                    marketId = marketId,
                };
                if (marketJson != null)
                {
                    lastStation.economyShares = EconomiesFromProfile(marketJson);
                    lastStation.eddnCommodityMarketQuotes = CommodityQuotesFromProfile(marketJson);
                    lastStation.prohibitedCommodities = ProhibitedCommoditiesFromProfile(marketJson);
                    lastStation.commoditiesupdatedat = marketJson["timestamp"]?.ToObject<DateTime?>() ?? DateTime.MinValue;
                    lastStation.marketJson = marketJson;

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
                if (shipyardJson != null)
                {
                    lastStation.outfitting = OutfittingFromProfile(shipyardJson);
                    lastStation.ships = ShipyardFromProfile(shipyardJson);
                    lastStation.shipyardJson = shipyardJson;
                    lastStation.outfittingupdatedat = shipyardJson["timestamp"]?.ToObject<DateTime?>() ?? DateTime.MinValue;
                    lastStation.shipyardupdatedat = shipyardJson["timestamp"]?.ToObject<DateTime?>() ?? DateTime.MinValue;
                }
            }
            catch (JsonException ex)
            {
                Logging.Error("Failed to parse companion station data", ex);
            }
            Logging.Debug("Station is: ", lastStation);
            return lastStation;
        }

        public Station UpdateStation(DateTime profileTimeStamp, Station stationToUpdate)
        {
            Logging.Debug($"Updating station from Frontier API.", new Dictionary<string, object>()
            {
                { "Station To Update", stationToUpdate },
                { "Frontier API Data", this }
            });

            if (stationToUpdate is null)
            {
                return null;
            }
            if (stationToUpdate.updatedat > Dates.fromDateTimeToSeconds(profileTimeStamp))
            {
                // The current station is already more up to date
                return stationToUpdate;
            }
            if (stationToUpdate.marketId != marketId)
            {
                // The market IDs do not match, the stations are not the same
                return stationToUpdate;
            }

            try
            {
                stationToUpdate.economyShares = economyShares.Select(e => e.ToEconomyShare()).ToList();
                stationToUpdate.stationServices = stationServices.Select(s => StationService.FromEDName(s.Key)).ToList();
            }
            catch (Exception e)
            {
                Logging.Error("Failed to update station economy and services from profile.", e);
            }

            if (commoditiesupdatedat != DateTime.MinValue &&
                (stationToUpdate.commoditiesupdatedat ?? 0) < Dates.fromDateTimeToSeconds(commoditiesupdatedat))
            {
                try
                {
                    stationToUpdate.commodities = eddnCommodityMarketQuotes.Select(c => c.ToCommodityMarketQuote()).Where(c => c != null).ToList();
                    stationToUpdate.prohibited = prohibitedCommodities.Select(p => CommodityDefinition.CommodityDefinitionFromEliteID(p.Key) ?? CommodityDefinition.FromEDName(p.Value)).ToList();
                    stationToUpdate.commoditiesupdatedat = Dates.fromDateTimeToSeconds(commoditiesupdatedat);
                }
                catch (Exception e)
                {
                    Logging.Error("Failed to update station market from profile.", e);
                }
            }
            if (outfittingupdatedat != DateTime.MinValue && (stationToUpdate.outfittingupdatedat ?? 0) < Dates.fromDateTimeToSeconds(outfittingupdatedat))
            {
                try
                {
                    stationToUpdate.outfitting = outfitting.Select(m => m.ToModule()).Where(m => m != null).ToList();
                    stationToUpdate.outfittingupdatedat = Dates.fromDateTimeToSeconds(outfittingupdatedat);
                }
                catch (Exception e)
                {
                    Logging.Error("Failed to update station outfitting from profile.", e);
                }

            }
            if (shipyardupdatedat != DateTime.MinValue && (stationToUpdate.shipyardupdatedat ?? 0) < Dates.fromDateTimeToSeconds(shipyardupdatedat))
            {
                try
                {
                    stationToUpdate.shipyard = ships.Select(s => s.ToShip()).Where(s => s != null).ToList();
                    stationToUpdate.shipyardupdatedat = Dates.fromDateTimeToSeconds(shipyardupdatedat);
                }
                catch (Exception e)
                {
                    Logging.Error("Failed to update station shipyard from profile.", e);
                }
            }
            stationToUpdate.updatedat = Dates.fromDateTimeToSeconds(profileTimeStamp);
            return stationToUpdate;
        }
    }
}