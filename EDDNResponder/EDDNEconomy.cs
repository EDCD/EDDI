using EddiDataDefinitions;

namespace EDDNResponder
{
    class EDDNEconomy
    {
        public string name;
        public decimal proportion = 0M;

        public EDDNEconomy(CompanionAppEconomy companionAppEconomy)
        {
            name = companionAppEconomy.name;
            proportion = companionAppEconomy.proportion;
        }
    }
}
