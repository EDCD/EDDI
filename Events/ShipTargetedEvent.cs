using EddiDataDefinitions;
using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class ShipTargetedEvent : Event
    {
        public const string NAME = "Ship targeted";
        public const string DESCRIPTION = "Triggered when the player selects a target";
        public const string SAMPLE = "{\"timestamp\":\"2018-05-09T23:19:49Z\",\"event\":\"ShipTargeted\",\"TargetLocked\":true,\"Ship\":\"adder\",\"ScanStage\":3,\"PilotName\":\"$npc_name_decorate:#name=Phoenix;\",\"PilotName_Localised\":\"Phoenix\",\"PilotRank\":\"Competent\",\"ShieldHealth\":100.000000,\"HullHealth\":100.000000,\"Faction\":\"Union Cosmos\",\"LegalStatus\":\"Lawless\",\"Subsystem\":\"$int_powerplant_size3_class3_name;\",\"Subsystem_Localised\":\"Power Plant\",\"SubsystemHealth\":100.000000}";

        [PublicAPI("True when a ship has been targeted. False when a target has been lost/deselected")]
        public bool targetlocked { get; private set; }

        [PublicAPI("the model of the ship")]
        public string ship { get; private set; }

        [PublicAPI("the stage of the ship scan (e.g. 0, 1, 2, or 3)")]
        public int? scanstage { get; private set; }

        [PublicAPI("The name of the pilot")]
        public string name { get; private set; }

        [PublicAPI("The rank of the pilot")]
        public string rank => CombatRank?.localizedName ?? "unknown combat rank";

        [PublicAPI("The faction of the pilot")]
        public string faction { get; private set; }

        [PublicAPI("The aligned power of the pilot (if player is pledged)")]
        public string power => (Power ?? Power.None).localizedName;

        [PublicAPI("The legal status of the pilot")]
        public string legalstatus => (LegalStatus ?? LegalStatus.None).localizedName;

        [PublicAPI("The bounty being offered by system authorities for destruction of the ship")]
        public int? bounty { get; private set; }

        [PublicAPI("The health of the shields")]
        public decimal? shieldhealth { get; private set; }

        [PublicAPI("The health of the hull")]
        public decimal? hullhealth { get; private set; }

        [PublicAPI("The subsystem targeted")]
        public string subsystem { get; private set; }

        [PublicAPI("The health of the subsystem targeted")]
        public decimal? subsystemhealth { get; private set; }

        // Not intended to be user facing

        public CombatRating CombatRank { get; }

        public LegalStatus LegalStatus { get; }

        public Power Power { get; }

        public ShipTargetedEvent(DateTime timestamp, bool targetlocked, string ship, int? scanstage, string name, CombatRating rank, string faction, Power power, LegalStatus legalstatus, int? bounty, decimal? shieldhealth, decimal? hullhealth, string subsystem, decimal? subsystemhealth) : base(timestamp, NAME)
        {
            this.targetlocked = targetlocked;
            this.ship = ship;
            this.scanstage = scanstage;
            this.name = name;
            this.CombatRank = rank;
            this.faction = faction;
            this.Power = power;
            this.LegalStatus = legalstatus;
            this.bounty = bounty;
            this.shieldhealth = shieldhealth;
            this.hullhealth = hullhealth;
            this.subsystem = subsystem;
            this.subsystemhealth = subsystemhealth;
        }
    }
}
