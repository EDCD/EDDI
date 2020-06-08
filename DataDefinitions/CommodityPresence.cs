namespace EddiDataDefinitions
{
    public class CommodityPresence
    {
        public CommodityDefinition commodityDefinition { get; }
        public string commodity => commodityDefinition.localizedName;
        public string edname => commodityDefinition.edname;
        public decimal percentage { get; } // Out of 100

        public CommodityPresence(CommodityDefinition commodity, decimal percentage)
        {
            this.commodityDefinition = commodity;
            this.percentage = percentage;
        }
    }
}
