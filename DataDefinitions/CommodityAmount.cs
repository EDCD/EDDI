using Newtonsoft.Json;
using Utilities;

namespace EddiDataDefinitions
{
    public class CommodityAmount
    {
        [PublicAPI, JsonIgnore]
        public string commodity => commodityDefinition.localizedName;

        [PublicAPI]
        public int amount { get; }

        // Not intended to be user facing

        public CommodityDefinition commodityDefinition { get; }

        [JsonIgnore]
        public string edname => commodityDefinition.edname;

        public CommodityAmount(CommodityDefinition commodity, int amount)
        {
            this.commodityDefinition = commodity;
            this.amount = amount;
        }
    }
}
