namespace EddiDataDefinitions
{
    public class CommodityAmount
    {
        public CommodityDefinition commodityDefinition { get; private set; }
        public string commodity => commodityDefinition.localizedName;
        public string edname => commodityDefinition.edname;
        public int amount { get; private set; }

        public CommodityAmount(CommodityDefinition commodity, int amount)
        {
            this.commodityDefinition = commodity;
            this.amount = amount;
        }
    }
}
