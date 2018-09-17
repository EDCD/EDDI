namespace EddiDataDefinitions
{
    public class EconomyShare
    {
        public string name => economy.localizedName;
        public Economy economy;
        public decimal proportion;

        public EconomyShare(string name, decimal proportion)
        {
            this.economy = Economy.FromName(name) ?? Economy.FromEDName(name);
            this.proportion = proportion;
        }

        public EconomyShare(Economy economy, decimal proportion)
        {
            this.economy = economy;
            this.proportion = proportion;
        }
    }
}
