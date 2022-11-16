using System;
using EddiEddnResponder.Sender;
using JetBrains.Annotations;
using Newtonsoft.Json.Linq;
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
                if (lastSentMarketID == marketID) return false;
                
                lastSentMarketID = marketID;

                // Strip localized names
                data = eddnState.PersonalData.Strip(data);

                // Apply data augments
                data = eddnState.GameVersion.AugmentVersion(data);

                return true;
            }
            catch (Exception e)
            {
                e.Data.Add("edType", edType);
                e.Data.Add("Data", data);
                e.Data.Add("EDDN State", eddnState);
                Logging.Error($"{GetType().Name} failed to handle journal data.");
                return false;
            }
        }

        public void Send(IDictionary<string, object> data)
        {
            EDDNSender.SendToEDDN("https://eddn.edcd.io/schemas/fcmaterials_journal/1", data);
        }

        public IDictionary<string, object> Handle(JObject profileJson, JObject marketJson, JObject shipyardJson, JObject fleetCarrierJson, EDDNState eddnState, out bool handled)
        {
            handled = false;
            var carrierID = marketJson?["name"]?.ToString();
            var marketID = marketJson?["id"]?.ToObject<long?>();

            var carrierRegex = new Regex(@"^\w{3}-\w{3}$");
            if (string.IsNullOrEmpty(carrierID) ||
                !carrierRegex.IsMatch(carrierID) ||
                eddnState?.GameVersion is null)
            { return null; }

            if (marketID != null && lastSentMarketID != marketID)
            {
                lastSentMarketID = marketID;

                var items = marketJson["orders"]?["onfootmicroresources"]?.ToObject<JObject>();
                var saleItems = items?["sales"]?.ToObject<JObject>() ?? new JObject();
                var purchaseItems = items?["purchases"]?.ToObject<JObject>() ?? new JObject();
                if (saleItems.Children().Any() || purchaseItems.Children().Any())
                {
                    var data = new Dictionary<string, object>() as IDictionary<string, object>;
                    data.Add("event", "FCMaterials");
                    data.Add("MarketID", marketID);
                    data.Add("CarrierID", carrierID);
                    data.Add("Items", items);

                    // Strip localized names
                    data = eddnState.PersonalData.Strip(data);

                    // Apply data augments
                    data = eddnState.GameVersion.AugmentVersion(data, "CAPI-market");

                    handled = true;
                    return data;
                }
            }

            return null;
        }

        public void SendCapi(IDictionary<string, object> data)
        {
            EDDNSender.SendToEDDN("https://eddn.edcd.io/schemas/fcmaterials_capi/1", data);
        }
    }
}