using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace EddiEvents
{
    public class SquadronRankEvent : Event
    {
        public const string NAME = "Squadron rank";
        public const string DESCRIPTION = "Triggered when your rank with a squadron has changed";
        public const string SAMPLE = null;
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static SquadronRankEvent()
        {
            VARIABLES.Add("name", "The squadron name");
            VARIABLES.Add("oldrank", "The old rank");
            VARIABLES.Add("newrank", "The new rank");
        }

        [JsonProperty("name")]
        public string name { get; private set; }

        [JsonProperty("oldrank")]
        public int oldrank { get; private set; }

        [JsonProperty("newrank")]
        public int newrank { get; private set; }

        public SquadronRankEvent(DateTime timestamp, string name, int oldrank, int newrank) : base(timestamp, NAME)
        {
            this.name = name;
            this.oldrank = oldrank;
            this.newrank = newrank;
        }
    }
}
