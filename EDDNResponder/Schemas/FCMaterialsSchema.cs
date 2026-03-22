using EddiEddnResponder.Sender;
using EddiEddnResponder.Toolkit;
using JetBrains.Annotations;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using Utilities;

namespace EddiEddnResponder.Schemas
{
    [UsedImplicitly]
    public class FCMaterialsSchema : ISchema, ICapiSchema
    {
        public List<string> edTypes => [ "FCMaterials" ];

        // Track this so that we do not send duplicate data from the journal and from CAPI.
        private long? lastSentMarketID;

        public bool Handle(string edType, ref IDictionary<string, object> data, EDDNState eddnState, EDDNSender eddnSender )
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
                data = PersonalDataStripper.Strip(data, edType);

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

        public IDictionary<string, object> Handle ( JObject profileJson, JObject marketJson, JObject shipyardJson,
            JObject fleetCarrierJson, EDDNState eddnState, EDDNSender eddnSender )
        {
            try
            {
                var carrierID = marketJson?["name"]?.ToString();
                var marketID = marketJson?["id"]?.ToObject<long?>();

                if (string.IsNullOrEmpty(carrierID) ||
                    !GeneratedRegex.FleetCarrierIdRegex().IsMatch(carrierID) ||
                    eddnState?.GameVersion is null)
                { return null; }

                if (marketID != null && lastSentMarketID != marketID)
                {
                    var items = marketJson["orders"]?["onfootmicroresources"]?.ToObject<JObject>();
                    var timestamp = marketJson["timestamp"]?.ToObject<DateTime?>();

                    // Sanity check - we must have a valid timestamp
                    if (timestamp == null) { return null; }

                    if (items?.HasValues ?? false)
                    {
                        var data = new Dictionary<string, object>() as IDictionary<string, object>;
                        data.Add("timestamp", timestamp);
                        data.Add("event", "FCMaterials");
                        data.Add("MarketID", marketID);
                        data.Add("CarrierID", carrierID);
                        data.Add("Items", items);

                        // Strip localized names
                        data = PersonalDataStripper.Strip(data);

                        // Apply data augments
                        data = eddnState.GameVersion.AugmentVersion(data);

                        eddnSender.SendToEDDN("https://eddn.edcd.io/schemas/fcmaterials_capi/1", data, eddnState, "CAPI-Live-market" );
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