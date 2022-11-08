using System;
using System.Collections.Generic;

namespace EddiDataDefinitions
{
    public class MarketInfo
    {
        public DateTime timestamp { get; set; }
        public long MarketID { get; set; }
        public string StationName { get; set; }
        public string StarSystem { get; set; }
        public List<MarketInfoItem> Items { get; }

        public MarketInfo()
        {
            Items = new List<MarketInfoItem>();
        }
    }
}
