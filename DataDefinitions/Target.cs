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
        public string rank => CombatRankDef?.localizedName ?? "unknown combat rank";

        // The faction in which the target is aligned
        public string faction { get; set; }

        // The superpower in which the target is aligned
        public string allegiance { get; set; }

        // The power in which the target is pledged
        public string power => (PowerDef ?? Power.None).localizedName;

        // The legal status of the target
        public string legalstatus => (LegalStatusDef ?? LegalStatus.None).localizedName;

        // Any bounties assigned to the target
        public int? bounty { get; set; }

        public CombatRating CombatRankDef { get; set; }
        public LegalStatus LegalStatusDef { get; set; }
        public Power PowerDef { get; set; }

        // Default Constructor
        public Target() { }

        [JsonConstructor]
        public Target(string name, CombatRating combatrank)
        {
            this.name = name;
            this.CombatRankDef = combatrank;
        }
    }
}

