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

        public IDictionary<string, object> Handle(string edType, IDictionary<string, object> data, EDDNState eddnState, out bool handled)
        {
            handled = false;
            if (edType is null || !edTypes.Contains(edType)) { return data; }
            if (data is null || eddnState?.GameVersion is null) { return data; }

            var marketID = JsonParsing.getLong(data, "MarketID");
            if (lastSentMarketID == marketID) { return data; }

            // Only send the message if we have commodities
            if (!data.TryGetValue("Items", out var commoditiesList) || 
                !(commoditiesList is JArray commodities) || 
                !commodities.Any())
            {
                return data;
            }

            lastSentMarketID = marketID;

            void UpdateKeyName(string oldKey, string newKey)
            {
                data[newKey] = data[oldKey];
                data.Remove(oldKey);
            }

            UpdateKeyName("StarSystem", "systemName");
            UpdateKeyName("StationName", "stationName");
            UpdateKeyName("MarketID", "marketId");
            data.Remove("Items");
            data.Add("commodities", commodities
                .Where(c => ApplyJournalMarketFilter(c))
                .Select(c => FormatCommodity(c.ToObject<JObject>(), true))
                .ToList());

            // Apply data augments
            data = eddnState.GameVersion.AugmentVersion(data);

            handled = true;
            return data;
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

        public void Send(IDictionary<string, object> data)
        {
            EDDNSender.SendToEDDN("https://eddn.edcd.io/schemas/commodity/3", data);
        }

        public IDictionary<string, object> Handle(JObject profileJson, JObject marketJson, JObject shipyardJson, JObject fleetCarrierJson, EDDNState eddnState, out bool handled)
        {
            handled = false;
            if (marketJson?["commodities"] is null || eddnState?.GameVersion is null) { return null; }

            var systemName = profileJson?["lastSystem"]?["name"]?.ToString();
            var stationName = marketJson["StationName"].ToString();
            var marketID = marketJson["MarketID"].ToObject<long>();
            var timestamp = marketJson["timestamp"].ToObject<DateTime?>();

            // Sanity check - we must have a valid timestamp
            if (timestamp == null) { return null; }

            // Sanity check - the location information must match our tracking data
            if (systemName != eddnState.Location.systemName ||
                stationName != eddnState.Location.stationName ||
                marketID != eddnState.Location.marketId) { return null; }

            // Build our commodities lists
            var commodities = marketJson["commodities"].Children().Values()
                .Where(c => ApplyFrontierApiMarketFilter(c))
                .Select(c => FormatCommodity(c.ToObject<JObject>(), false))
                .ToList();
            var prohibitedCommodities = marketJson["prohibited"]?.Children().Values()
                .Select(pc => pc.Value<string>());
            var economies = marketJson["economies"];

            // Continue if our commodities list is not empty
            if (commodities.Any())
            {
                lastSentMarketID = marketID;

                var data = new Dictionary<string, object>() as IDictionary<string, object>;
                data.Add("timestamp", timestamp);
                data.Add("systemName", systemName);
                data.Add("stationName", stationName);
                data.Add("marketId", marketID);
                data.Add("commodities", commodities);
                data.Add("economies", economies);
                data.Add("prohibited", prohibitedCommodities);

                // Apply data augments
                data = eddnState.GameVersion.AugmentVersion(data, "CAPI-market");

                handled = true;
                return data;
            }

            return null;
        }


        public void SendCapi(IDictionary<string, object> data)
        {
            EDDNSender.SendToEDDN("https://eddn.edcd.io/schemas/commodity/3", data);
        }

        private bool ApplyFrontierApiMarketFilter(JToken c)
        {
            // Don't serialize non-marketable commodities such as drones / limpets
            // Skip commodities with "categoryName": "NonMarketable"(i.e.Limpets - not purchasable in station market) or a non - empty"legality": string(not normally traded at this station market).

            if (!string.IsNullOrEmpty(c?["legality"]?.ToString()))
            {
                return false;
            }
            if (c?["categoryName"]?.ToString().ToLowerInvariant() == "nonmarketable")
            {
                return false;
            }
            return true;
        }

        private JObject FormatCommodity(JObject c, bool fromJournal)
        {
            if (fromJournal)
            {
                void UpdateKeyName(string oldKey, string newKey)
                {
                    c[newKey] = c[oldKey];
                    c.Remove(oldKey);
                }

                // Reformat name to match the Frontier API
                c["Name"] = c["Name"]?.ToString()
                    .Replace("$", "")
                    .Replace("_name","")
                    .Replace(";", "");

                UpdateKeyName("Name", "name");
                UpdateKeyName("MeanPrice", "meanPrice");

                UpdateKeyName("BuyPrice", "buyPrice");
                UpdateKeyName("Demand", "demand");
                UpdateKeyName("DemandBracket", "demandBracket");

                UpdateKeyName("SellPrice", "sellPrice");
                UpdateKeyName("Stock", "stock");
                UpdateKeyName("StockBracket", "stockBracket");

                var statusFlags = new HashSet<string>();
                if (c["Producer"].ToObject<bool>())
                {
                    statusFlags.Add("Producer");
                }
                if (c["Consumer"].ToObject<bool>())
                {
                    statusFlags.Add("Consumer");
                }
                if (c["Rare"].ToObject<bool>())
                {
                    statusFlags.Add("Rare");
                }
                if (statusFlags.Any())
                {
                    c.Add("statusFlags", JToken.FromObject(statusFlags));
                }

                c.Remove("Name_Localised");
                c.Remove("Category");
                c.Remove("Category_Localised");
                c.Remove("Consumer");
                c.Remove("Rare");
                c.Remove("Producer");
            }

            else
            {
                c.Remove("legality");
                c.Remove("categoryName");
                c.Remove("locName");
            }

            c.Remove("id");

            return c;
        }
    }
}