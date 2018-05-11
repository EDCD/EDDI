using EddiDataDefinitions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace EddiEvents
{
    public class ShipTargetedEvent : Event
    {
        public const string NAME = "Ship targeted";
        public const string DESCRIPTION = "Triggered when the current player selects a new target";
        public const string SAMPLE = "{\"timestamp\":\"2018-05-09T23:19:49Z\",\"event\":\"ShipTargeted\",\"TargetLocked\":true,\"Ship\":\"adder\",\"ScanStage\":3,\"PilotName\":\"$npc_name_decorate:#name=Phoenix;\",\"PilotName_Localised\":\"Phoenix\",\"PilotRank\":\"Competent\",\"ShieldHealth\":100.000000,\"HullHealth\":100.000000,\"Faction\":\"Union Cosmos\",\"LegalStatus\":\"Lawless\",\"Subsystem\":\"$int_powerplant_size3_class3_name;\",\"Subsystem_Localised\":\"Power Plant\",\"SubsystemHealth\":100.000000}";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static ShipTargetedEvent()
        {
            VARIABLES.Add("targetlocked", "True if a ship has been targeted");
            VARIABLES.Add("ship", "the model of the ship");
            VARIABLES.Add("scanstage", "the stage of the ship scan");
            VARIABLES.Add("name", "The name of the pilot");
            VARIABLES.Add("rank", "The combat rank of the pilot");
            VARIABLES.Add("faction", "The faction of the pilot");
            VARIABLES.Add("legalstatus", "The legal status of the pilot");
            VARIABLES.Add("shieldhealth", "The health of the shields");
            VARIABLES.Add("hullhealth", "The health of the hull");
            VARIABLES.Add("subsystem", "The subsystem targetted");
            VARIABLES.Add("subsystemhealth", "The health of the subsystem targetted");
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
        public string rank { get; private set; }

        [JsonProperty("faction")]
        public string faction { get; private set; }

        [JsonProperty("legalstatus")]
        public string legalstatus { get; private set; }

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

        public ShipTargetedEvent(DateTime timestamp, bool targetlocked, string ship, int? scanstage, string name, string rank, string faction, string legalstatus, int? bounty, decimal? shieldhealth, decimal? hullhealth, string subsystem, decimal? subsystemhealth) : base(timestamp, NAME)
        {
            this.targetlocked = targetlocked;
            this.ship = ship;
            this.scanstage = scanstage;
            this.name = name;
            this.rank = rank;
            this.faction = faction;
            this.legalstatus = legalstatus;
            this.bounty = bounty;
            this.shieldhealth = shieldhealth;
            this.hullhealth = hullhealth;
            this.subsystem = subsystem;
            this.subsystemhealth = subsystemhealth;
        }
    }
}
