using Newtonsoft.Json;
using System.Collections.Generic;

namespace EddiDataDefinitions
{
    /// <summary>
    /// A target
    /// </summary>
    public class Target
    {
        /// <summary>The status of the friend</summary>
        public string status { get; set; } = string.Empty;

        // The name of the target
        public string name { get; set; } = string.Empty;

        // The rank of the target
        public string rank => CombatRankDef?.localizedName ?? "unknown combat rank";

        // The power in which the target is aligned
        public string power => (PowerDef ?? Power.None).localizedName;

        public string allegiance => (PowerDef ?? Power.None).Allegiance.localizedName;

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

