using EddiDataDefinitions;
using Newtonsoft.Json;
using System.Collections.Generic;
using Utilities;

namespace Eddi
{
    public class ShipyardInfoReader
    {
        public long MarketID { get; set; }
        public string StationName { get; set; }
        public string StarSystem { get; set; }
        public bool Horizons { get; set; }
        public bool AllowCobraMkIV { get; set; }
        public List<ShipyardInfo> PriceList { get; set; }

        public ShipyardInfoReader()
        {
            PriceList = new List<ShipyardInfo>();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")] // this usage is perfectly correct    
        public static ShipyardInfoReader FromFile(string filename = null)
        {
            ShipyardInfoReader info = new ShipyardInfoReader();

            string data = Files.FromSavedGames("Shipyard.json");
            if (data != null)
            {
                info = JsonConvert.DeserializeObject<ShipyardInfoReader>(data);
            }
            return info;
        }
    }
}

