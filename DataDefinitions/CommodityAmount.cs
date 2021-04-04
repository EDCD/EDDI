using Utilities;

namespace EddiDataDefinitions
{
    public class CommodityAmount
    {
        [PublicAPI]
        public string commodity => commodityDefinition.localizedName;

        [PublicAPI]
        public int amount { get; }

        // Not intended to be user facing

        public CommodityDefinition commodityDefinition { get; }

        public string edname => commodityDefinition.edname;

        public CommodityAmount(CommodityDefinition commodity, int amount)
        {
            this.commodityDefinition = commodity;
            this.amount = amount;
        }
    }
}
