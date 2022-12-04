using EddiEddnResponder.Sender;
using JetBrains.Annotations;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using Utilities;

namespace EddiEddnResponder.Schemas
{
    [UsedImplicitly]
    public class CommoditySchema : ISchema, ICapiSchema
    {
        public List<string> edTypes => new List<string> { "Market" };

        // Track this so that we do not send duplicate data from the journal and from CAPI.
        private long? lastSentMarketID;

        public bool Handle(string edType, ref IDictionary<string, object> data, EDDNState eddnState)
        {
            try
            {
                if (!edTypes.Contains(edType)) { return false; }
                if (eddnState?.GameVersion is null) { return false; }

                var marketID = JsonParsing.getLong(data, "MarketID");
                if (lastSentMarketID == marketID) 
                {
                    lastSentMarketID = null;
                    return false; 
                }

                // Only send the message if we have commodities
                if (data.TryGetValue("Items", out var commoditiesList) &&
                    commoditiesList is List<object> commodities && 
                    commodities.Any())
                {
                    var handledData = new Dictionary<string, object>() as IDictionary<string, object>;
                    handledData["timestamp"] = data["timestamp"];
                    handledData["systemName"] = data["StarSystem"];
                    handledData["systemName"] = data["StationName"];
                    handledData["marketId"] = data["MarketID"];
                    handledData["commodities"] = JArray.FromObject(data["Items"])
                        .Where(c => ApplyJournalMarketFilter(c))
                        .Select(c => FormatCommodity(c, true))
                        .ToList();

                    // Remove localized names
                    handledData = eddnState.PersonalData.Strip(handledData);

                    // Apply data augments
                    handledData = eddnState.GameVersion.AugmentVersion(handledData);

                    data = handledData;
                    EDDNSender.SendToEDDN("https://eddn.edcd.io/schemas/commodity/3", handledData, eddnState);
                    return true;
                }
            }
            catch (Exception e)
            {
                e.Data.Add("edType", edType);
                e.Data.Add("Data", data);
                e.Data.Add("EDDN State", eddnState);
                Logging.Error($"{GetType().Name} failed to handle journal data.");
            }
            return false;
        }

        private bool ApplyJournalMarketFilter(JToken c)
        {
            // Don't serialize non-marketable commodities such as drones / limpets
            if (c?["Category"]?.ToString()
                    .Replace("$MARKET_category_", "")
                    .Replace(";", "")
                    .Replace("-", "")
                    .ToLowerInvariant() == "nonmarketable")
            {
                return false;
            }
            if (c?["Name"]?.ToString().ToLowerInvariant() == "drones")
            {
                return false;
            }
            return true;
        }

        public IDictionary<string, object> Handle(JObject profileJson, JObject marketJson, JObject shipyardJson, JObject fleetCarrierJson, bool fromLegacyServer, EDDNState eddnState)
        {
            try
            {
                if (marketJson?["commodities"] is null || eddnState?.GameVersion is null) { return null; }

                var systemName = profileJson?["lastSystem"]?["name"]?.ToString();
                var stationName = marketJson["name"].ToString();
                var marketID = marketJson["id"].ToObject<long>();
                var timestamp = marketJson["timestamp"].ToObject<DateTime?>();

                // Sanity check - we must have a valid timestamp
                if (timestamp == null) { return null; }

                // Build our commodities lists
                var commodities = JArray.FromObject(marketJson["commodities"]?.ToObject<JArray>()?
                    .Where(c => ApplyFrontierApiMarketFilter(c))
                    .Select(c => FormatCommodity(c.ToObject<JObject>(), false)) ?? new List<JObject>());
                var prohibitedCommodities = marketJson["prohibited"]?.Children().Values();
                var economies = marketJson["economies"].Children().Values()
                    .Select(e => JObject.FromObject(e)).ToList();

                // Continue if our commodities list is not empty
                if (commodities.Any())
                {
                    var data = new Dictionary<string, object>() as IDictionary<string, object>;
                    data.Add("timestamp", timestamp);
                    data.Add("systemName", systemName);
                    data.Add("stationName", stationName);
                    data.Add("marketId", marketID);
                    data.Add("commodities", commodities);
                    data.Add("economies", economies);
                    data.Add("prohibited", prohibitedCommodities);

                    // Remove localized names
                    data = eddnState.PersonalData.Strip(data);

                    // Apply data augments
                    data = eddnState.GameVersion.AugmentVersion(data);

                    var gameVersionOverride = fromLegacyServer ? "CAPI-Legacy-market" : "CAPI-Live-market";
                    EDDNSender.SendToEDDN("https://eddn.edcd.io/schemas/commodity/3", data, eddnState, gameVersionOverride);
                    lastSentMarketID = marketID;
                    return data;
                }
            }
            catch (Exception e)
            {
                e.Data.Add(@"\profile", profileJson);
                e.Data.Add(@"\market", marketJson);
                e.Data.Add("EDDN State", eddnState);
                Logging.Error($"{GetType().Name} failed to handle Frontier API data.");
            }
            return null;
        }

        private bool ApplyFrontierApiMarketFilter(JToken c)
        {
            try
            {
                // Don't serialize non-marketable commodities such as drones / limpets
                // Skip commodities with "categoryName": "NonMarketable"(i.e.Limpets - not purchasable in station market) or a non - empty"legality": string(not normally traded at this station market).

                if (!string.IsNullOrEmpty(c?["legality"]?.ToString()))
                {
                    return false;
                }
                if (c?["categoryname"]?.ToString().ToLowerInvariant() == "nonmarketable")
                {
                    return false;
                }
                return true;
            }
            catch (Exception e)
            {
                e.Data.Add("commodity", c);
                Logging.Error("Failed to filter Frontier API commodity", e);
                return false;
            }
        }

        private JObject FormatCommodity(JToken c, bool fromJournal)
        {
            var handledC = new JObject();
            if (fromJournal)
            {
                // Reformat commodity name to match the Frontier API
                handledC["name"] = c["Name"]?.ToString()
                    .Replace("$", "")
                    .Replace("_name", "")
                    .Replace(";", "");
                handledC["meanPrice"] = c["MeanPrice"];
                handledC["buyPrice"] = c["BuyPrice"];
                handledC["demand"] = c["Demand"];
                handledC["demandBracket"] = c["DemandBracket"];
                handledC["sellPrice"] = c["SellPrice"];
                handledC["stock"] = c["Stock"];
                handledC["stockBracket"] = c["StockBracket"];

                var statusFlags = new HashSet<string>();
                if (c["Producer"].ToObject<bool?>() == true)
                {
                    statusFlags.Add("Producer");
                }
                if (c["Consumer"].ToObject<bool?>() == true)
                {
                    statusFlags.Add("Consumer");
                }
                if (c["Rare"].ToObject<bool?>() == true)
                {
                    statusFlags.Add("Rare");
                }
                if (statusFlags.Any())
                {
                    handledC["statusFlags"] = JToken.FromObject(statusFlags);
                }
            }
            else
            {
                handledC["name"] = c["name"];
                handledC["meanPrice"] = c["meanPrice"];
                handledC["buyPrice"] = c["buyPrice"];
                handledC["demand"] = c["demand"];
                handledC["demandBracket"] = c["demandBracket"];
                handledC["sellPrice"] = c["sellPrice"];
                handledC["stock"] = c["stock"];
                handledC["stockBracket"] = c["stockBracket"];
                if (c["statusFlags"] is IEnumerable<object> statusFlags && statusFlags.Any())
                {
                    handledC["statusFlags"] = c["statusFlags"];
                }
            }
            return handledC;
        }
    }
}