using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace EddiEvents
{
    public class FighterRebuiltEvent : Event
    {
        public const string NAME = "Fighter rebuilt";
        public const string DESCRIPTION = "Triggered when a ship's fighter is rebuilt in the hangar";
        public const string SAMPLE = "{\"timestamp\":\"2016-07-22T10:53:19Z\",\"event\":\"FighterRebuilt\",\"Loadout\":\"four\", \"ID\":134}";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static FighterRebuiltEvent()
        {
            VARIABLES.Add("loadout", "The loadout of the fighter");
            VARIABLES.Add("id", "The fighter's id");
        }

        [JsonProperty("loadout")]
        public string loadout { get; private set; }

        [JsonProperty("id")]
        public int id { get; private set; }

        public FighterRebuiltEvent(DateTime timestamp, string loadout, int id) : base(timestamp, NAME)
        {
            this.loadout = loadout;
        }
    }
}
