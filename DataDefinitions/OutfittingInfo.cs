using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Utilities;

namespace EddiDataDefinitions
{
    public class OutfittingInfo
    {
        public DateTime timestamp { get; set; }
        public long MarketID { get; set; }
        public string StationName { get; set; }
        public string StarSystem { get; set; }
        public bool Horizons { get; set; }
        public List<OutfittingInfoItem> Items { get; set; }

        public OutfittingInfo()
        {
            Items = new List<OutfittingInfoItem>();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")] // this usage is perfectly correct    
        public static OutfittingInfo FromFile(string filename = null)
        {
            OutfittingInfo info = new OutfittingInfo();

            string data = Files.FromSavedGames("Outfitting.json");
            if (data != null)
            {
                info = JsonConvert.DeserializeObject<OutfittingInfo>(data);
            }
            return info;
        }
    }
}

