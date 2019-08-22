using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace EddiEvents
{
    public class FighterLaunchedEvent : Event
    {
        public const string NAME = "Fighter launched";
        public const string DESCRIPTION = "Triggered when you launch a fighter from your ship";
        public const string SAMPLE = "{\"timestamp\":\"2019-04-29T00:07:46Z\", \"event\":\"LaunchFighter\", \"Loadout\":\"four\", \"ID\":13, \"PlayerControlled\":false}";

        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static FighterLaunchedEvent()
        {
            VARIABLES.Add("loadout", "The fighter's loadout");
            VARIABLES.Add("id", "The fighter's id");
            VARIABLES.Add("playercontrolled", "True if the fighter is controlled by the player");
        }

        [JsonProperty("loadout")]
        public string loadout { get; private set; }

        [JsonProperty("id")]
        public int id { get; private set; }

        [JsonProperty("playercontrolled")]
        public bool playercontrolled { get; private set; }

        public FighterLaunchedEvent(DateTime timestamp, string loadout, int id, bool playercontrolled) : base(timestamp, NAME)
        {
            this.loadout = loadout;
            this.id = id;
            this.playercontrolled = playercontrolled;
        }
    }
}
