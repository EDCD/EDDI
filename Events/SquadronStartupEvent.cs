using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace EddiEvents
{
    public class SquadronStartupEvent : Event
    {
        public const string NAME = "Squadron startup";
        public const string DESCRIPTION = "Triggered at startup to provide basic squadron information";
        public const string SAMPLE = null;
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static SquadronStartupEvent()
        {
            VARIABLES.Add("name", "The squadron name");
            VARIABLES.Add("rank", "The current rank");

        }

        [JsonProperty("name")]
        public string name { get; private set; }

        [JsonProperty("rank")]
        public int rank { get; private set; }

        public SquadronStartupEvent(DateTime timestamp, string name, int rank) : base(timestamp, NAME)
        {
            this.name = name;
            this.rank = rank;
        }
    }
}