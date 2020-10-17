using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Utilities;

namespace EddiDataDefinitions
{
    public class MarketInfo
    {
        public DateTime timestamp { get; set; }
        public long MarketID { get; set; }
        public string StationName { get; set; }
        public string StarSystem { get; set; }
        public List<MarketInfoItem> Items { get; set; }

        public MarketInfo()
        {
            Items = new List<MarketInfoItem>();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")] // this usage is perfectly correct    
        public static MarketInfo FromFile(string filename = null)
        {
            MarketInfo info = new MarketInfo();

            string data = Files.FromSavedGames("Market.json");
            if (data != null)
            {
                info = JsonConvert.DeserializeObject<MarketInfo>(data);
            }
            return info;
        }
    }
}
