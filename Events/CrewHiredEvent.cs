﻿using EddiDataDefinitions;
using System;
using System.Collections.Generic;
using Utilities;

namespace EddiEvents
{
    public class CrewHiredEvent : Event
    {
        public const string NAME = "Crew hired";
        public const string DESCRIPTION = "Triggered when you hire crew";
        public const string SAMPLE = "{\"timestamp\":\"2016-08-09T08: 46:29Z\",\"event\":\"CrewHire\",\"Name\":\"Margaret Parrish\",\"CrewID\":236064708,\"Faction\":\"The Dark Wheel\",\"Cost\":15000,\"CombatRank\":1}";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static CrewHiredEvent()
        {
            VARIABLES.Add("name", "The name of the crewmember being hired");
            VARIABLES.Add("crewid", "The ID of the crewmember being hired");
            VARIABLES.Add("faction", "The faction of the crewmember being hired");
            VARIABLES.Add("price", "The price of the crewmember being hired");
            VARIABLES.Add("combatrating", "The combat rating of the crewmember being hired");
        }

        [PublicAPI]
        public string name { get; private set; }

        [PublicAPI]
        public long crewid { get; private set; }

        [PublicAPI]
        public string faction { get; private set; }

        [PublicAPI]
        public long price { get; private set; }

        [PublicAPI]
        public string combatrating { get; private set; }

        public CrewHiredEvent(DateTime timestamp, string name, long crewid, string faction, long price, CombatRating combatrating) : base(timestamp, NAME)
        {
            this.name = name;
            this.crewid = crewid;
            this.faction = faction;
            this.price = price;
            this.combatrating = combatrating.localizedName;
        }
    }
}
