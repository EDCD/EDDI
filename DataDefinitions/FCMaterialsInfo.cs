using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace EddiDataDefinitions
{
    public class FCMaterialsInfo
    {
        public DateTime timestamp { get; set; }

        [JsonProperty("MarketID")]
        public long CarrierID { get; set; }

        public string CarrierName { get; set; }

        [JsonProperty("CarrierID")]
        public string callsign { get; set; }

        public List<FCMaterialInfoItem> Items { get; }

        public FCMaterialsInfo()
        {
            Items = new List<FCMaterialInfoItem>();
        }
    }
}
