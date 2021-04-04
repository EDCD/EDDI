using Newtonsoft.Json;
using Utilities;

namespace EddiDataDefinitions
{
    /// <summary>
    /// A target
    /// </summary>
    public class Target
    {
        // The name of the target
        [PublicAPI]
        public string name { get; set; } = string.Empty;

        // The model of the ship
        [PublicAPI]
        public string ship { get; set; }

        // The rank of the target
        public CombatRating CombatRank { get; set; }

        [PublicAPI]
        public string rank => CombatRank?.localizedName ?? "unknown combat rank";

        // The faction in which the target is aligned
        [PublicAPI]
        public string faction { get; set; }

        // The superpower in which the target is aligned
        public Superpower Allegiance { get; set; }

        [PublicAPI]
        public string allegiance => (Allegiance ?? Superpower.None).localizedName;

        // The power in which the target is pledged
        public Power Power { get; set; }

        [PublicAPI]
        public string power => (Power ?? Power.None).localizedName;

        // The legal status of the target
        public LegalStatus LegalStatus { get; set; }

        [PublicAPI]
        public string legalstatus => (LegalStatus ?? LegalStatus.None).localizedName;

        // Any bounties assigned to the target
        [PublicAPI]
        public int? bounty { get; set; }

        // Default Constructor
        public Target() { }

        [JsonConstructor]
        public Target(string name, CombatRating combatrank, string ship)
        {
            this.name = name;
            this.CombatRank = combatrank;
            this.ship = ship;
        }
    }
}

