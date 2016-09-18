using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EliteDangerousEvents
{
    public class FighterLaunchedEvent : Event
    {
        public const string NAME = "Fighter launched";
        public const string DESCRIPTION = "Triggered when you launch a fighter from your ship";
        public static FighterLaunchedEvent SAMPLE = new FighterLaunchedEvent(DateTime.Now, "starter", true);
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static FighterLaunchedEvent()
        {
            SAMPLE.raw = "{\"timestamp\":\"2016-06-10T14:32:03Z\",\"event\":\"LaunchFighter\",\"Loadout\":\"starter\",\"PlayerControlled\":true}";

            VARIABLES.Add("loadout", "The fighter's loadout");
            VARIABLES.Add("playercontrolled", "True if the fighter is controlled by the player");
        }

        [JsonProperty("loadout")]
        public string loadout { get; private set; }

        [JsonProperty("playercontrolled")]
        public bool playercontrolled { get; private set; }

        public FighterLaunchedEvent(DateTime timestamp, string loadout, bool playercontrolled) : base(timestamp, NAME)
        {
            this.loadout = loadout;
            this.playercontrolled = playercontrolled;
        }
    }
}
