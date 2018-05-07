namespace EddiDataDefinitions
{
    public class CommodityAmount
    {
        public CommodityDefinition commodityDefinition { get; }
        public string commodity => commodityDefinition.localizedName;
        public string edname => commodityDefinition.edname;
        public int amount { get; }

        public CommodityAmount(CommodityDefinition commodity, int amount)
        {
            this.commodityDefinition = commodity;
            this.amount = amount;
        }
    }
}
