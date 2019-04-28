using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace EddiEvents
{
    public class CrewPaidWageEvent : Event
    {
        public const string NAME = "Crew paid wage";
        public const string DESCRIPTION = "Triggered when npc crew receives a profit share";
        public const string SAMPLE = "{\"timestamp\":\"2019-03-09T18:46:52Z\", \"event\":\"NpcCrewPaidWage\", \"NpcCrewName\":\"Xenia Hoover\", \"NpcCrewId\":236064708, \"Amount\":0}";

        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static CrewPaidWageEvent()
        {
            VARIABLES.Add("name", "The name of the crewmember");
            VARIABLES.Add("crewid", "The ID of the crewmember");
            VARIABLES.Add("amount", "The amount paid to the crewmember");
        }

        [JsonProperty("name")]
        public string name { get; private set; }

        [JsonProperty("crewid")]
        public long crewid { get; private set; }

        [JsonProperty("amount")]
        public long amount { get; private set; }

        public CrewPaidWageEvent(DateTime timestamp, string name, long crewid, long amount) : base(timestamp, NAME)
        {
            this.name = name;
            this.crewid = crewid;
            this.amount = amount;
        }
    }
}
