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
        [PublicAPI("the name of the target")]
        public string name { get; set; } = string.Empty;

        // The model of the ship
        [PublicAPI("the model of the ship")]
        public string ship { get; set; }

        // The rank of the target
        public CombatRating CombatRank { get; set; }

        [PublicAPI( "rank of the pilot" )]
        public string rank => CombatRank?.localizedName ?? "unknown combat rank";

        [PublicAPI("the faction to which the pilot is aligned")]
        public string faction { get; set; }

        // The superpower in which the target is aligned
        // Prioritize power allegiance (when present) over faction
        public Superpower Allegiance
        {
            get => Power?.Allegiance ?? _Allegiance; 
            set => _Allegiance = value;
        }
        private Superpower _Allegiance;

        [PublicAPI( "superpower to which the minor faction is aligned" )]
        public string allegiance => ( Allegiance ?? Superpower.None ).localizedName;

        // The power in which the target is pledged
        public Power Power { get; set; }

        [PublicAPI( "power ( Aisling Duval, Yuri Grom, Denton Patreus, etc) to which the pilot is pledged" )]
        public string power => (Power ?? Power.None).localizedName;

        // The legal status of the target
        public LegalStatus LegalStatus { get; set; }

        [PublicAPI( "the legal status (clean, enemy, wanted, warrant, etc) of the pilot" )]
        public string legalstatus => (LegalStatus ?? LegalStatus.None).localizedName;

        // Any bounties assigned to the target
        [PublicAPI("the bounty assigned to the target")]
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

