using EddiDataDefinitions;

namespace EDDNResponder
{
    class EDDNEconomy
    {
        // Schema ref.: https://github.com/EDSM-NET/EDDN/blob/b21bdf76e549ea9e69fedf3ad6434b2fcdc5a5e7/schemas/commodity-v3.0.json#L105

        public string name;
        public decimal proportion = 0M;

        public EDDNEconomy(EconomyShare economyShareEconomy)
        {
            name = economyShareEconomy.economy.edname;
            proportion = economyShareEconomy.proportion;
        }
    }
}
