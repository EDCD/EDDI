using EddiDataDefinitions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace EddiEvents
{
    public class ShipTargetedEvent : Event
    {
        public const string NAME = "Ship targeted";
        public const string DESCRIPTION = "Triggered when the player selects a target";
        public const string SAMPLE = "{\"timestamp\":\"2018-05-09T23:19:49Z\",\"event\":\"ShipTargeted\",\"TargetLocked\":true,\"Ship\":\"adder\",\"ScanStage\":3,\"PilotName\":\"$npc_name_decorate:#name=Phoenix;\",\"PilotName_Localised\":\"Phoenix\",\"PilotRank\":\"Competent\",\"ShieldHealth\":100.000000,\"HullHealth\":100.000000,\"Faction\":\"Union Cosmos\",\"LegalStatus\":\"Lawless\",\"Subsystem\":\"$int_powerplant_size3_class3_name;\",\"Subsystem_Localised\":\"Power Plant\",\"SubsystemHealth\":100.000000}";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static ShipTargetedEvent()
        {
            VARIABLES.Add("targetlocked", "True when a ship has been targeted. False when a target has been lost/deselected");
            VARIABLES.Add("ship", "the model of the ship");
            VARIABLES.Add("scanstage", "the stage of the ship scan (e.g. 0, 1, 2, or 3)");
            VARIABLES.Add("name", "The name of the pilot");
            VARIABLES.Add("rank", "The rank of the pilot");
            VARIABLES.Add("faction", "The faction of the pilot");
            VARIABLES.Add("power", "The aligned power of the pilot (if player is pledged)");
            VARIABLES.Add("legalstatus", "The legal status of the pilot");
            VARIABLES.Add("shieldhealth", "The health of the shields");
            VARIABLES.Add("hullhealth", "The health of the hull");
            VARIABLES.Add("subsystem", "The subsystem targeted");
            VARIABLES.Add("subsystemhealth", "The health of the subsystem targeted");
        }

        [JsonProperty("targetlocked")]
        public bool targetlocked { get; private set; }

        [JsonProperty("ship")]
        public string ship { get; private set; }

        [JsonProperty("scanstage")]
        public int? scanstage { get; private set; }

        [JsonProperty("name")]
        public string name { get; private set; }

        [JsonProperty("rank")]
        public string rank => CombatRank?.localizedName ?? "unknown combat rank";

        [JsonProperty("faction")]
        public string faction { get; private set; }

        [JsonProperty("power")]
        public string power => (Power ?? Power.None).localizedName;

        [JsonProperty("legalstatus")]
        public string legalstatus => (LegalStatus ?? LegalStatus.None).localizedName;

        [JsonProperty("bounty")]
        public int? bounty { get; private set; }

        [JsonProperty("shieldhealth")]
        public decimal? shieldhealth { get; private set; }

        [JsonProperty("hullhealth")]
        public decimal? hullhealth { get; private set; }

        [JsonProperty("subsystem")]
        public string subsystem { get; private set; }

        [JsonProperty("subsystemhealth")]
        public decimal? subsystemhealth { get; private set; }

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
