namespace EddiDataDefinitions
{
    // Optional enum: A null value indicates that the commodity is not normally sold/purchased at this station, but is currently temporarily for sale/purchase
    public enum CommodityBracket
    {
        None = 0,
        Low = 1,
        Medium = 2,
        High = 3
    }
}