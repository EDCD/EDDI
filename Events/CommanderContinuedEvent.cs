﻿using EddiDataDefinitions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace EddiEvents
{
    public class CommanderContinuedEvent : Event
    {
        public const string NAME = "Commander continued";
        public const string DESCRIPTION = "Triggered when you continue an existing game";
        public const string SAMPLE = "{\"timestamp\":\"2016-06-10T14:32:03Z\",\"event\":\"LoadGame\",\"Commander\":\"HRC1\",\"FID\":\"F44396\",\"Horizons\":true,\"Ship\":\"CobraMkIII\",\"ShipID\":1,\"GameMode\":\"Group\",\"Group\":\"Mobius\",\"Credits\":600120,\"Loan\":0,\"ShipName\":\"jewel of parhoon\",\"ShipIdent\":\"hr-17f\",\"FuelLevel\":3.964024,\"FuelCapacity\":8}";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static CommanderContinuedEvent()
        {
            VARIABLES.Add("commander", "The commander's name");
            VARIABLES.Add("horizons", "The game version is 'Horizons'");
            VARIABLES.Add("ship", "The commander's ship");
            VARIABLES.Add("shipid", "The ID of the commander's ship");
            VARIABLES.Add("mode", "The game mode (Open, Group or Solo)");
            VARIABLES.Add("group", "The name of the group (only if mode == Group)");
            VARIABLES.Add("credits", "The number of credits the commander has");
            VARIABLES.Add("loan", "The current loan the commander has");
            VARIABLES.Add("fuel", "The current fuel level of the commander's vehicle");
            VARIABLES.Add("fuelcapacity", "The total fuel capacity of the commander's vehicle");
            VARIABLES.Add("startlanded", "True if the commander is starting landed");
            VARIABLES.Add("startdead", "True if the commander is starting dead / at the rebuy screen");
        }

        [JsonProperty("commander")]
        public string commander { get; private set; }

        [JsonProperty("horizons")]
        public bool horizons { get; private set; }

        [JsonProperty("ship")]
        public string ship { get; private set; }

        [JsonProperty("shipid")]
        public int? shipid { get; private set; }

        [JsonProperty("shipname")]
        public string shipname { get; private set; }

        [JsonProperty("shipident")]
        public string shipident { get; private set; }

        [JsonProperty("startlanded")]
        public bool? startlanded { get; private set; }

        [JsonProperty("startdead")]
        public bool? startdead { get; private set; }

        [JsonProperty("mode")]
        public string mode { get; private set; }

        [JsonProperty("group")]
        public string group { get; private set; }

        [JsonProperty("credits")]
        public long credits { get; private set; }

        [JsonProperty("loan")]
        public long loan { get; private set; }

        [JsonProperty("fuel")]
        public decimal? fuel { get; private set; }

        [JsonProperty("fuelcapacity")]
        public decimal? fuelcapacity { get; private set; }

        // Not intended to be user facing
        public string frontierID { get; private set; }

        public CommanderContinuedEvent(DateTime timestamp, string commander, string frontierID, bool horizons, int shipId, string ship, string shipName, string shipIdent, bool? startedLanded, bool? startDead, GameMode mode, string group, long credits, long loan, decimal? fuel, decimal? fuelcapacity) : base(timestamp, NAME)
        {
            this.commander = commander;
            this.frontierID = frontierID;
            this.horizons = horizons;
            this.shipid = shipId;
            this.ship = ShipDefinitions.FromEDModel(ship).model;
            this.shipname = shipName;
            this.shipident = shipIdent;
            this.startlanded = startedLanded;
            this.startdead = startDead;
            this.mode = (mode == null ? null : mode.localizedName);
            this.group = group;
            this.credits = credits;
            this.loan = loan;
            this.fuel = fuel;
            this.fuelcapacity = fuelcapacity;
        }
    }
}
