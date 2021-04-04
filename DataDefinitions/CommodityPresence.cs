using Utilities;

namespace EddiDataDefinitions
{
    public class CommodityPresence
    {
        [PublicAPI]
        public string commodity => commodityDefinition.localizedName;

        [PublicAPI]
        public decimal percentage { get; } // Out of 100

        // Not intended to be user facing

        public CommodityDefinition commodityDefinition { get; }

        public string edname => commodityDefinition.edname;

        public CommodityPresence(CommodityDefinition commodity, decimal percentage)
        {
            this.commodityDefinition = commodity;
            this.percentage = percentage;
        }
    }
}
