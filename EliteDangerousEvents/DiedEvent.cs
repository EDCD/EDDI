using EliteDangerousDataDefinitions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EliteDangerousEvents
{
    public class DiedEvent : Event
    {
        public const string NAME = "Died";
        public const string DESCRIPTION = "Triggered when you have died";
        public static DiedEvent SAMPLE = new DiedEvent(DateTime.Now, new List<string>{}, new List<Ship> { }, new List<CombatRating> { });
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static DiedEvent()
        {
            SAMPLE.raw = "{\"timestamp\":\"2016-06-10T14:32:03Z\",\"event\":\"Died\",\"KillerName\":\"Cmdr HRC1\",\"Ship\":\"Vulture\",\"Rank\":\"Competent\"}";

            VARIABLES.Add("commanders", "The names of the commanders who killed you");
            VARIABLES.Add("ships", "The ships the commanders who killed you were flying");
            VARIABLES.Add("ranks", "The ranks of the commanders who killed you");
            VARIABLES.Add("ratings", "The ratings of the commanders who killed you");
        }

        [JsonProperty("commanders")]
        public List<string> commanders { get; private set; }

        [JsonProperty("ships")]
        public List<Ship> ships { get; private set; }

        [JsonProperty("ranks")]
        public List<CombatRating> ranks { get; private set; }

        public DiedEvent(DateTime timestamp, List<string> commanders, List<Ship> ships, List<CombatRating> ranks) : base(timestamp, NAME)
        {
            this.commanders = commanders;
            this.ships = ships;
            this.ranks = ranks;
        }
    }
}
