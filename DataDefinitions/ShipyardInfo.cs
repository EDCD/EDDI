using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Utilities;

namespace EddiDataDefinitions
{
    public class ShipyardInfo
    {
        public DateTime timestamp { get; set; }
        public long MarketID { get; set; }
        public string StationName { get; set; }
        public string StarSystem { get; set; }
        public bool Horizons { get; set; }
        public bool AllowCobraMkIV { get; set; }
        public List<ShipyardInfoItem> PriceList { get; set; }

        public ShipyardInfo()
        {
            PriceList = new List<ShipyardInfoItem>();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")] // this usage is perfectly correct    
        public static ShipyardInfo FromFile(string filename = null)
        {
            ShipyardInfo info = new ShipyardInfo();

            string data = Files.FromSavedGames("Shipyard.json");
            if (data != null)
            {
                info = JsonConvert.DeserializeObject<ShipyardInfo>(data);
            }
            return info;
        }
    }
}

