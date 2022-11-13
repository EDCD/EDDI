namespace EddiDataDefinitions
{
    public class FrontierApiEconomyShare
    {
        public string edName { get; }
        public decimal proportion { get; }

        public FrontierApiEconomyShare(string edName, decimal proportion)
        {
            this.edName = edName;
            this.proportion = proportion;
        }

        public EconomyShare ToEconomyShare()
        {
            return new EconomyShare(edName, proportion);
        }
    }
}