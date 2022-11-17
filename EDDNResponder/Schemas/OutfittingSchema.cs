using EddiDataDefinitions;
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
    public class OutfittingSchema : ISchema, ICapiSchema
    {
        public List<string> edTypes => new List<string> { "Outfitting" };

        // Track this so that we do not send duplicate data from the journal and from CAPI.
        private long? lastSentMarketID;

        public bool Handle(string edType, ref IDictionary<string, object> data, EDDNState eddnState)
        {
            try
            {
                if (!edTypes.Contains(edType)) { return false; }
                if (eddnState?.GameVersion is null) { return false; }

                var marketID = JsonParsing.getLong(data, "MarketID");
                if (lastSentMarketID != marketID && data.TryGetValue("Items", out var modulesList))
                {
                    // Only send the message if we have modules
                    if (modulesList is List<object> modules && modules.Any())
                    {
                        lastSentMarketID = marketID;

                        var handledData = new Dictionary<string, object>() as IDictionary<string, object>;
                        handledData["timestamp"] = data["timestamp"];
                        handledData["systemName"] = data["StarSystem"];
                        handledData["systemName"] = data["StationName"];
                        handledData["marketId"] = data["MarketID"];
                        handledData["modules"] = modules
                            .Select(m => JObject.FromObject(m)["Name"]?.ToString())
                            .Where(m => ApplyModuleNameFilter(m))
                            .Where(m => !Module.IsPowerPlay(m))
                            .ToList();

                        // Apply data augments
                        handledData = eddnState.GameVersion.AugmentVersion(handledData);

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

        public void Send(IDictionary<string, object> data)
        {
            EDDNSender.SendToEDDN("https://eddn.edcd.io/schemas/outfitting/2", data);
        }

        public IDictionary<string, object> Handle(JObject profileJson, JObject marketJson, JObject shipyardJson, JObject fleetCarrierJson, EDDNState eddnState)
        {
            try
            {
                // Modules are included in shipyardJson
                if (shipyardJson?["modules"] is null || eddnState?.GameVersion is null) { return null; }

                var systemName = profileJson?["lastSystem"]?["name"]?.ToString();
                var stationName = shipyardJson["name"].ToString();
                var marketID = shipyardJson["id"].ToObject<long>();
                var timestamp = shipyardJson["timestamp"].ToObject<DateTime?>();

                // Sanity check - we must have a valid timestamp
                if (timestamp == null) { return null; }

                // Build our modules list
                var modules = shipyardJson["modules"].Children().Values()
                    .Where(m => ApplyModuleSkuFilter(m))
                    .Select(m => m["name"]?.ToString())
                    .Where(m => ApplyModuleNameFilter(m))
                    .ToList();

                // Continue if our modules list is not empty
                if (modules.Any())
                {
                    lastSentMarketID = marketID;


                    var data = new Dictionary<string, object>() as IDictionary<string, object>;
                    data.Add("timestamp", Dates.FromDateTimeToString(timestamp));
                    data.Add("systemName", systemName);
                    data.Add("stationName", stationName);
                    data.Add("marketId", marketID);
                    data.Add("modules", modules);

                    // Apply data augments
                    data = eddnState.GameVersion.AugmentVersion(data, "CAPI-shipyard");

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

        public void SendCapi(IDictionary<string, object> data)
        {
            EDDNSender.SendToEDDN("https://eddn.edcd.io/schemas/outfitting/2", data);
        }

        private bool ApplyModuleNameFilter(string m)
        {
            // Filter items that aren't weapons/utilities (Hpt_*), standard/internal modules (Int_*) or armour (*_Armour_*)
            // and the "Int_PlanetApproachSuite" module (for historical reasons)
            return (
                       m.StartsWith("Int_", StringComparison.InvariantCultureIgnoreCase) ||
                       m.StartsWith("Hpt_", StringComparison.InvariantCultureIgnoreCase) ||
                       m.Contains("_Armour_") || m.Contains("_armour_")
                   ) &&
                   m != "Int_PlanetApproachSuite";
        }

        private bool ApplyModuleSkuFilter(JToken m)
        {
            // Filter items that have a non-null "sku" property, unless it's "ELITE_HORIZONS_V_PLANETARY_LANDINGS" (i.e. PowerPlay and tech broker items).
            return m != null && (
                string.IsNullOrEmpty(m["sku"]?.ToString()) || 
                (m["sku"]?.ToString().Equals("ELITE_HORIZONS_V_PLANETARY_LANDINGS", StringComparison.InvariantCultureIgnoreCase) ?? false)
                );
        }
    }
}