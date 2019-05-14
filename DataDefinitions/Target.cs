using Newtonsoft.Json;
using System.Collections.Generic;

namespace EddiDataDefinitions
{
    /// <summary>
    /// A target
    /// </summary>
    public class Target
    {
        // The name of the target
        public string name { get; set; } = string.Empty;

        // The model of the ship
        public string ship { get; set; }

        // The rank of the target
        public CombatRating CombatRank { get; set; }
        public string rank => CombatRank?.localizedName ?? "unknown combat rank";

        // The faction in which the target is aligned
        public string faction { get; set; }

        // The superpower in which the target is aligned
        public Superpower Allegiance { get; set; }
        public string allegiance => (Allegiance ?? Superpower.None).localizedName;

        // The power in which the target is pledged
        public Power Power { get; set; }
        public string power => (Power ?? Power.None).localizedName;

        // The legal status of the target
        public LegalStatus LegalStatus { get; set; }
        public string legalstatus => (LegalStatus ?? LegalStatus.None).localizedName;

        // Any bounties assigned to the target
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

