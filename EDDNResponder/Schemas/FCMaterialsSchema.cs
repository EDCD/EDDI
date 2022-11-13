using EddiEddnResponder.Sender;
using JetBrains.Annotations;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Diagnostics;
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

        public IDictionary<string, object> Handle(string edType, IDictionary<string, object> data, EDDNState eddnState, out bool handled)
        {
            handled = false;
            if (edType is null || !edTypes.Contains(edType)) { return null; }
            if (data is null || eddnState?.GameVersion is null) { return null; }

            var marketID = JsonParsing.getLong(data, "MarketID");
            if (lastSentMarketID != marketID)
            {
                lastSentMarketID = marketID;

                // Strip localized names
                data = eddnState.PersonalData.Strip(data);

                // Apply data augments
                data = eddnState.GameVersion.AugmentVersion(data);

                handled = true;
                return data;
            }

            return null;
        }

        public void Send(IDictionary<string, object> data)
        {
            EDDNSender.SendToEDDN("https://eddn.edcd.io/schemas/fcmaterials_journal/1", data);
        }

        public IDictionary<string, object> Handle(JObject profileJson, JObject marketJson, JObject shipyardJson, JObject fleetCarrierJson, EDDNState eddnState, out bool handled)
        {
            handled = false;
            var carrierID = fleetCarrierJson?["name"]?["callsign"]?.ToString();
            var marketID = fleetCarrierJson?["market"]?["id"]?.ToObject<long?>();

            var carrierRegex = new Regex(@"^\w{3}-\w{3}$");
            if (string.IsNullOrEmpty(carrierID) ||
                !carrierRegex.IsMatch(carrierID) ||
                eddnState?.GameVersion is null)
            { return null; }

            if (marketID != null && lastSentMarketID != marketID)
            {
                lastSentMarketID = marketID;

                var items = fleetCarrierJson["orders"]?["onfootmicroresources"]?.Children().Values()
                    .Select(o => FormatOnFootMaterial(o.ToObject<JObject>())).ToList() ?? new List<JObject>();
                if (items.Any())
                {
                    var data = new Dictionary<string, object>() as IDictionary<string, object>;
                    data.Add("event", "FCMaterials");
                    data.Add("MarketID", marketID);
                    data.Add("CarrierID", carrierID);
                    data.Add("Items", items);

                    // Strip localized names
                    data = eddnState.PersonalData.Strip(data);

                    // Apply data augments (only if the game is running)
                    if (Process.GetProcessesByName("Elite - Dangerous (Client)").Any())
                    {
                        data = eddnState.GameVersion.AugmentVersion(data);
                    }

                    handled = true;
                    return data;
                }
            }

            return null;
        }

        private JObject FormatOnFootMaterial(JObject o)
        {
            o.Remove("locName");
            return o;
        }

        public void SendCapi(IDictionary<string, object> data)
        {
            EDDNSender.SendToEDDN("https://eddn.edcd.io/schemas/fcmaterials_capi/1", data);
        }
    }
}