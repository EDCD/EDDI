using EddiDataDefinitions;
using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class ShipTargetedEvent : Event
    {
        public const string NAME = "Ship targeted";
        public const string DESCRIPTION = "Triggered when the player selects a non-Thargoid target";
        public static readonly string[] SAMPLES =
        {
            "{ \"timestamp\":\"2020-05-16T08:14:36Z\", \"event\":\"ShipTargeted\", \"TargetLocked\":true, \"Ship\":\"sidewinder\", \"ScanStage\":0 }",
            "{ \"timestamp\":\"2020-05-16T08:14:37Z\", \"event\":\"ShipTargeted\", \"TargetLocked\":true, \"Ship\":\"sidewinder\", \"ScanStage\":1, \"PilotName\":\"$npc_name_decorate:#name=Noni Ryder;\", \"PilotName_Localised\":\"Noni Ryder\", \"PilotRank\":\"Mostly Harmless\" }",
            "{ \"timestamp\":\"2020-05-16T08:14:39Z\", \"event\":\"ShipTargeted\", \"TargetLocked\":true, \"Ship\":\"sidewinder\", \"ScanStage\":2, \"PilotName\":\"$npc_name_decorate:#name=Noni Ryder;\", \"PilotName_Localised\":\"Noni Ryder\", \"PilotRank\":\"Mostly Harmless\", \"ShieldHealth\":100.000000, \"HullHealth\":100.000000 }",
            "{ \"timestamp\":\"2020-05-16T08:14:45Z\", \"event\":\"ShipTargeted\", \"TargetLocked\":true, \"Ship\":\"sidewinder\", \"ScanStage\":3, \"PilotName\":\"$npc_name_decorate:#name=Noni Ryder;\", \"PilotName_Localised\":\"Noni Ryder\", \"PilotRank\":\"Mostly Harmless\", \"ShieldHealth\":100.000000, \"HullHealth\":100.000000, \"Faction\":\"Balante Jet Posse\", \"LegalStatus\":\"Wanted\", \"Bounty\":1642 }",
            "{ \"timestamp\":\"2018-05-09T23:19:49Z\", \"event\":\"ShipTargeted\", \"TargetLocked\":true, \"Ship\":\"adder\", \"ScanStage\":3, \"PilotName\":\"$npc_name_decorate:#name=Phoenix;\", \"PilotName_Localised\":\"Phoenix\", \"PilotRank\":\"Competent\", \"ShieldHealth\":100.000000, \"HullHealth\":100.000000, \"Faction\":\"Union Cosmos\", \"LegalStatus\":\"Lawless\", \"Subsystem\":\"$int_powerplant_size3_class3_name;\", \"Subsystem_Localised\":\"Power Plant\", \"SubsystemHealth\":100.000000}"
        };

        [PublicAPI("True when a ship has been targeted. False when a target has been lost/deselected")]
        public bool targetlocked { get; private set; }

        [ PublicAPI( "the model of the ship" ) ]
        public string ship => FighterDef != null ? FighterDef.localizedName : ShipDef?.model;

        [PublicAPI("the stage of the ship scan (e.g. 0, 1, 2, or 3)")]
        public int? scanstage { get; private set; }

        [PublicAPI("The name of the pilot (at scan state 1+)")]
        public string name { get; private set; }

        [PublicAPI( "The rank of the pilot (at scan state 1+)" )]
        public string rank => CombatRank?.localizedName;

        [PublicAPI( "The health of the shields (at scan state 2+)" )]
        public decimal? shieldhealth { get; private set; }

        [PublicAPI( "The health of the hull (at scan state 2+)" )]
        public decimal? hullhealth { get; private set; }

        [PublicAPI( "The faction of the pilot (at scan state 3)" )]
        public string faction { get; private set; }

        [PublicAPI( "The aligned power of the pilot (if player is pledged) (at scan state 3)" )]
        public string power => (Power ?? Power.None)?.localizedName;

        [PublicAPI( "The legal status of the pilot (at scan state 3)" )]
        public string legalstatus => LegalStatus?.localizedName;

        [PublicAPI( "The bounty being offered by system authorities for destruction of the ship (at scan state 3)" )]
        public int? bounty { get; private set; }

        [ PublicAPI( "The subsystem targeted (at scan state 3)" ) ]
        public string subsystem { get; private set; }

        [PublicAPI( "The health of the subsystem targeted (at scan state 3)" )]
        public decimal? subsystemhealth { get; private set; }

        // Not intended to be user facing

        public CombatRating CombatRank { get; }

        public LegalStatus LegalStatus { get; }

        public Power Power { get; }

        public Ship ShipDef { get; }

        public VehicleDefinition FighterDef { get; }

        public ShipTargetedEvent(DateTime timestamp, bool targetlocked, Ship shipDef, VehicleDefinition fighterDef, int? scanstage, string name, CombatRating rank, string faction, Power power, LegalStatus legalstatus, int? bounty, decimal? shieldhealth, decimal? hullhealth, string subsystem, decimal? subsystemhealth) : base(timestamp, NAME)
        {
            this.targetlocked = targetlocked;
            this.ShipDef = shipDef;
            this.FighterDef = fighterDef;
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
