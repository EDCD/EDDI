using Newtonsoft.Json;

namespace EddiDataDefinitions
{
    public class OutfittingInfo
    {
        [JsonProperty]
        public long id { get; set; }
        [JsonProperty]
        public string name { get; set; }

        // Station prices
        [JsonProperty]
        public int buyprice { get; set; }

        public OutfittingInfo()
        { }

        public OutfittingInfo(OutfittingInfo OutfittingInfo)
        {
            this.id = OutfittingInfo.id;
            this.name = OutfittingInfo.name;
            this.buyprice = OutfittingInfo.buyprice;
        }

        public OutfittingInfo(long id, string Name, int BuyPrice)
        {
            this.id = id;
            this.name = Name;
            this.buyprice = BuyPrice;
        }
    }
}