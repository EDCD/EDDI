using Newtonsoft.Json;

namespace EddiDataDefinitions
{
    public class ShipyardInfoItem
    {
        [JsonProperty]
        public long id { get; set; }
        [JsonProperty]
        public string shiptype { get; set; }

        // Station prices
        [JsonProperty]
        public long shipprice { get; set; }

        public ShipyardInfoItem()
        { }

        public ShipyardInfoItem(ShipyardInfoItem ShipyardInfo)
        {
            this.id = ShipyardInfo.id;
            this.shiptype = ShipyardInfo.shiptype;
            this.shipprice = ShipyardInfo.shipprice;
        }

        public ShipyardInfoItem(long id, string ShipType, long ShipPrice)
        {
            this.id = id;
            this.shiptype = ShipType;
            this.shipprice = ShipPrice;
        }
    }
}