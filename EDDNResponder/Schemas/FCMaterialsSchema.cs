using EddiEddnResponder.Sender;
using JetBrains.Annotations;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Utilities;

namespace EddiEddnResponder.Schemas
{
    [UsedImplicitly]
    public class FCMaterialsSchema : ISchema, ICapiSchema
    {
        public List<string> edTypes => new List<string> { "FCMaterials" };

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

                // Strip localized names
                data = eddnState.PersonalData.Strip(data);

                // Apply data augments
                data = eddnState.GameVersion.AugmentVersion(data);

                return true;
            }
            catch (Exception e)
            {
                Logging.Error($"{GetType().Name} failed to handle journal data.", e);
                return false;
            }
        }

        public IDictionary<string, object> Handle(JObject profileJson, JObject marketJson, JObject shipyardJson, JObject fleetCarrierJson, bool fromLegacyServer, EDDNState eddnState)
        {
            try
            {
                var carrierID = marketJson?["name"]?.ToString();
                var marketID = marketJson?["id"]?.ToObject<long?>();

                var carrierRegex = new Regex(@"^\w{3}-\w{3}$");
                if (string.IsNullOrEmpty(carrierID) ||
                    !carrierRegex.IsMatch(carrierID) ||
                    eddnState?.GameVersion is null)
                { return null; }

                if (marketID != null && lastSentMarketID != marketID)
                {
                    var items = marketJson["orders"]?["onfootmicroresources"]?.ToObject<JObject>();
                    var saleItems = items?["sales"]?.ToObject<JToken>();
                    var purchaseItems = items?["purchases"]?.ToObject<JToken>();
                    var timestamp = marketJson["timestamp"]?.ToObject<DateTime?>();

                    // Sanity check - we must have a valid timestamp
                    if (timestamp == null) { return null; }

                    if ((saleItems != null && saleItems.Children().Any()) || (purchaseItems != null && purchaseItems.Children().Any()))
                    {
                        var data = new Dictionary<string, object>() as IDictionary<string, object>;
                        data.Add("timestamp", timestamp);
                        data.Add("event", "FCMaterials");
                        data.Add("MarketID", marketID);
                        data.Add("CarrierID", carrierID);
                        data.Add("Items", items);

                        // Strip localized names
                        data = eddnState.PersonalData.Strip(data);

                        // Apply data augments
                        data = eddnState.GameVersion.AugmentVersion(data);

                        var gameVersionOverride = fromLegacyServer ? "CAPI-Legacy-market" : "CAPI-Live-market";
                        EDDNSender.SendToEDDN("https://eddn.edcd.io/schemas/fcmaterials_capi/1", data, eddnState, gameVersionOverride);
                        lastSentMarketID = marketID;
                        return data;
                    }
                }
            }
            catch (Exception e)
            {
                Logging.Error($"{GetType().Name} failed to handle Frontier API data.", e);
            }

            return null;
        }
    }
}