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
        public const string SAMPLE = "{\"timestamp\":\"2016-06-10T14:32:03Z\",\"event\":\"Died\",\"KillerName\":\"Cmdr HRC1\",\"KillerShip\":\"Vulture\",\"KillerRank\":\"Competent\"}";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static DiedEvent()
        {
            VARIABLES.Add("commanders", "The names of the commanders who killed you");
            VARIABLES.Add("ships", "The ships the commanders who killed you were flying");
            VARIABLES.Add("ratings", "The ratings of the commanders who killed you");
        }

        [JsonProperty("commanders")]
        public List<string> commanders { get; private set; }

        [JsonProperty("ships")]
        public List<string> ships { get; private set; }

        [JsonProperty("ratings")]
        public List<CombatRating> ratings { get; private set; }

        public DiedEvent(DateTime timestamp, List<string> commanders, List<string> ships, List<CombatRating> ratings) : base(timestamp, NAME)
        {
            this.commanders = commanders;
            this.ships = ships;
            this.ratings = ratings;
        }
    }
}
