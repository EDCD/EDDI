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
    }
}

