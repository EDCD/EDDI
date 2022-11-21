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

        public bool Handle(string edType, ref IDictionary<string, object> data, EDDNState eddnState)
        {
            try
            {
                if (!edTypes.Contains(edType)) { return false; }
                if (eddnState?.GameVersion is null) { return false; }

                var marketID = JsonParsing.getLong(data, "MarketID");
                if (lastSentMarketID != marketID && data.TryGetValue("PriceList", out var shipsList))
                {
                    // Only send the message if we have ships
                    if (shipsList is List<object> ships && ships.Any())
                    {
                        lastSentMarketID = marketID;

                        var handledData = new Dictionary<string, object>() as IDictionary<string, object>;
                        handledData["timestamp"] = data["timestamp"];
                        handledData["systemName"] = data["StarSystem"];
                        handledData["systemName"] = data["StationName"];
                        handledData["marketId"] = data["MarketID"];
                        handledData["allowCobraMkIV"] = data["AllowCobraMkIV"];
                        handledData["ships"] = ships.Select(o => JObject.FromObject(o)["ShipType"]).ToList();

                        // Apply data augments
                        handledData = eddnState.GameVersion.AugmentVersion(handledData);

                        EDDNSender.SendToEDDN("https://eddn.edcd.io/schemas/shipyard/2", handledData, eddnState);
                        data = handledData;
                        return true;
                    }
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

        public IDictionary<string, object> Handle(JObject profileJson, JObject marketJson, JObject shipyardJson, JObject fleetCarrierJson, EDDNState eddnState)
        {
            try
            {
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
                    data.Add("timestamp", timestamp);
                    data.Add("systemName", systemName);
                    data.Add("stationName", stationName);
                    data.Add("marketId", marketID);
                    data.Add("ships", ships);
                    data.Add("allowCobraMkIV", allowCobraMkIV);

                    // Apply data augments
                    data = eddnState.GameVersion.AugmentVersion(data);

                    EDDNSender.SendToEDDN("https://eddn.edcd.io/schemas/shipyard/2", data, eddnState, "CAPI-shipyard");
                    return data;
                }
            }
            catch (Exception e)
            {
                e.Data.Add(@"\profile", profileJson);
                e.Data.Add(@"\shipyard", shipyardJson);
                e.Data.Add("EDDN State", eddnState);
                Logging.Error($"{GetType().Name} failed to handle Frontier API data.");
            }

            return null;
        }
    }
}