namespace EddiDataDefinitions
{
    public class FrontierApiEconomyShare ( string edName, decimal proportion )
    {
        public string edName { get; } = edName;
        public decimal proportion { get; } = proportion;

        public EconomyShare ToEconomyShare()
        {
            return new EconomyShare(edName, proportion);
        }
    }
}