using EliteDangerousDataDefinitions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EliteDangerousEvents
{
    public class CombatPromotionEvent : Event
    {
        public const string NAME = "Combat promotion";
        public const string DESCRIPTION = "Triggered when your combat rank increases";
        public static CombatPromotionEvent SAMPLE = new CombatPromotionEvent(DateTime.Now, 5);
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static CombatPromotionEvent()
        {
            VARIABLES.Add("rank", "The commander's new combat rank (Harmless, etc)");
            VARIABLES.Add("rating", "The commander's new numerical combat rating (0-8)");
        }

        [JsonProperty("rank")]
        public string rank { get; private set; }
        [JsonProperty("rating")]
        public int rating{ get; private set; }

        public CombatPromotionEvent(DateTime timestamp, int rating) : base(timestamp, NAME)
        {
            this.rating = rating;
            this.rank = Commander.combatRanks[rating];
        }
    }
}
