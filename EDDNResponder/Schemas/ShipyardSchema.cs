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
    public class ShipyardSchema : ISchema, ICapiSchema
    {
        public List<string> edTypes => new List<string> { "Shipyard" };

        // Track this so that we do not send duplicate data from the journal and from CAPI.
        private long? lastSentMarketID;

        public IDictionary<string, object> Handle(string edType, IDictionary<string, object> data, EDDNState eddnState, out bool handled)
        {
            handled = false;
            if (edType is null || !edTypes.Contains(edType)) { return null; }
            if (data is null || eddnState?.GameVersion is null) { return null; }

            var marketID = JsonParsing.getLong(data, "MarketID");
            if (lastSentMarketID != marketID && data.TryGetValue("PriceList", out var shipsList))
            {
                // Only send the message if we have ships
                if (shipsList is List<object> ships && ships.Any())
                {
                    lastSentMarketID = marketID;

                    void UpdateKeyName(string oldKey, string newKey)
                    {
                        data[newKey] = data[oldKey];
                        data.Remove(oldKey);
                    }

                    UpdateKeyName("StarSystem", "systemName");
                    UpdateKeyName("StationName", "stationName");
                    UpdateKeyName("MarketID", "marketId");
                    UpdateKeyName("AllowCobraMkIV", "allowCobraMkIV");
                    data.Remove("PriceList");
                    data.Add("ships", ships.Select(o => JObject.FromObject(o)["ShipType"]).ToList());

                    // Apply data augments
                    data = eddnState.GameVersion.AugmentVersion(data);

                    handled = true;
                    return data;
                }
            }

            return null;
        }

        public void Send(IDictionary<string, object> data)
        {
            EDDNSender.SendToEDDN("https://eddn.edcd.io/schemas/shipyard/2", data);
        }

        public IDictionary<string, object> Handle(JObject profileJson, JObject marketJson, JObject shipyardJson, JObject fleetCarrierJson, EDDNState eddnState, out bool handled)
        {
            handled = false;
            if (shipyardJson?["ships"] is null || eddnState?.GameVersion is null) { return null; }

            var systemName = profileJson?["lastSystem"]?["name"]?.ToString();
            var stationName = shipyardJson["name"].ToString();
            var marketID = shipyardJson["id"].ToObject<long>();
            var timestamp = shipyardJson["timestamp"].ToObject<DateTime?>();
            var allowCobraMkIV = profileJson?["commander"]?["capabilities"]?["AllowCobraMkIV"]?.ToObject<bool?>() ?? false;

            // Sanity check - we must have a valid timestamp
            if (timestamp == null) { return null; }

            // Build our ships list
            var ships = shipyardJson["ships"]?["shipyard_list"]?.Children().Values()
                .Select(s => s["ShipType"]?.ToString()).ToList() ?? new List<string>();
            if (shipyardJson["ships"]["unavailable_list"] != null)
            {
                ships.AddRange(shipyardJson["ships"]?["unavailable_list"]?
                    .Select(s => s?["ShipType"]?.ToString()).ToList() ?? new List<string>());
            }

            // Continue if our ships list is not empty
            if (ships.Any())
            {
                lastSentMarketID = marketID;

                var data = new Dictionary<string, object>() as IDictionary<string, object>;
                data.Add("timestamp", Dates.FromDateTimeToString(timestamp));
                data.Add("systemName", systemName);
                data.Add("stationName", stationName);
                data.Add("marketId", marketID);
                data.Add("ships", ships);
                data.Add("allowCobraMkIV", allowCobraMkIV);

                // Apply data augments
                data = eddnState.GameVersion.AugmentVersion(data, "CAPI-shipyard");

                handled = true;
                return data;
            }

            return null;
        }

        public void SendCapi(IDictionary<string, object> data)
        {
            EDDNSender.SendToEDDN("https://eddn.edcd.io/schemas/shipyard/2", data);
        }
    }
}