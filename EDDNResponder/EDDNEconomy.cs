using EddiDataDefinitions;

namespace EDDNResponder
{
    class EDDNEconomy
    {
        public string name;
        public decimal proportion = 0M;

        public EDDNEconomy(EconomyShare economyShareEconomy)
        {
            name = economyShareEconomy.name;
            proportion = economyShareEconomy.proportion;
        }
    }
}
