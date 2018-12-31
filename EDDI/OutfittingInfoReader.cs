using EddiDataDefinitions;
using Newtonsoft.Json;
using System.Collections.Generic;
using Utilities;

namespace Eddi
{
    public class OutfittingInfoReader
    {
        public long MarketID { get; set; }
        public string StationName { get; set; }
        public string StarSystem { get; set; }
        public bool Horizons { get; set; }
        public List<OutfittingInfo> Items { get; set; }

        public OutfittingInfoReader()
        {
            Items = new List<OutfittingInfo>();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")] // this usage is perfectly correct    
        public static OutfittingInfoReader FromFile(string filename = null)
        {
            OutfittingInfoReader info = new OutfittingInfoReader();

            string data = Files.FromSavedGames("Outfitting.json");
            if (data != null)
            {
                info = JsonConvert.DeserializeObject<OutfittingInfoReader>(data);
            }
            return info;
        }
    }
}

