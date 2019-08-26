using EddiDataDefinitions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Utilities;

namespace Eddi
{
    public class MarketInfoReader
    {
        public string timestamp { get; set; }
        public DateTime timeStamp => DateTime.Parse("yyyy-MM-ddTHH:mm:ssZ");
        public long MarketID { get; set; }
        public string StationName { get; set; }
        public string StarSystem { get; set; }
        public List<MarketInfo> Items { get; set; }

        public MarketInfoReader()
        {
            Items = new List<MarketInfo>();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")] // this usage is perfectly correct    
        public static MarketInfoReader FromFile(string filename = null)
        {
            MarketInfoReader info = new MarketInfoReader();

            string data = Files.FromSavedGames("Market.json");
            if (data != null)
            {
                info = JsonConvert.DeserializeObject<MarketInfoReader>(data);
            }
            return info;
        }
    }
}
