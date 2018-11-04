using EddiDataDefinitions;

namespace EDDNResponder
{
    class EDDNEconomy
    {
        public string Name;
        public string Name_Localized;
        public decimal Proportion = 0M;

        public EDDNEconomy(EconomyShare economyShareEconomy)
        {
            Name = economyShareEconomy.economy.edname;
            Name_Localized = economyShareEconomy.economy.fallbackLocalizedName;
            Proportion = economyShareEconomy.proportion;
        }
    }
}
