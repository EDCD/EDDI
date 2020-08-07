using Newtonsoft.Json;

namespace EddiDataDefinitions
{
    public class OutfittingInfoItem
    {
        [JsonProperty]
        public long id { get; set; }
        [JsonProperty]
        public string name { get; set; }

        // Station prices
        [JsonProperty]
        public int buyprice { get; set; }

        public OutfittingInfoItem()
        { }

        public OutfittingInfoItem(OutfittingInfoItem OutfittingInfo)
        {
            this.id = OutfittingInfo.id;
            this.name = OutfittingInfo.name;
            this.buyprice = OutfittingInfo.buyprice;
        }

        public OutfittingInfoItem(long id, string Name, int BuyPrice)
        {
            this.id = id;
            this.name = Name;
            this.buyprice = BuyPrice;
        }
    }
}