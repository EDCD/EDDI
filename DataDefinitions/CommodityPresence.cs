using Utilities;

namespace EddiDataDefinitions
{
    public class CommodityPresence ( CommodityDefinition commodity, decimal percentage )
    {
        [PublicAPI( "localized commodity name" )]
        public string commodity => commodityDefinition.localizedName;

        [PublicAPI( "percentage present" )]
        public decimal percentage { get; } = percentage; // Out of 100

        // Not intended to be user facing

        public CommodityDefinition commodityDefinition { get; } = commodity;

        public string edname => commodityDefinition.edname;
    }
}
