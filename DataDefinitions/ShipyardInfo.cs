using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace EddiDataDefinitions
{
    public class ShipyardInfo
    {
        [JsonProperty]
        public long id { get; set; }
        [JsonProperty]
        public string shiptype { get; set; }

        // Station prices
        [JsonProperty]
        public long shipprice { get; set; }

        public ShipyardInfo()
        { }

        public ShipyardInfo(ShipyardInfo ShipyardInfo)
        {
            this.id = ShipyardInfo.id;
            this.shiptype = ShipyardInfo.shiptype;
            this.shipprice = ShipyardInfo.shipprice;
        }

        public ShipyardInfo(long id, string ShipType, long ShipPrice)
        {
            this.id = id;
            this.shiptype = ShipType;
            this.shipprice = ShipPrice;
        }
    }
}