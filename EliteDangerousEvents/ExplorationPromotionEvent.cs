using EliteDangerousDataDefinitions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EliteDangerousEvents
{
    public class ExplorationPromotionEvent : Event
    {
        public const string NAME = "Exploration promotion";
        public const string DESCRIPTION = "Triggered when your exploration rank increases";
        public static ExplorationPromotionEvent SAMPLE = new ExplorationPromotionEvent(DateTime.Now, 5);
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static ExplorationPromotionEvent()
        {
            VARIABLES.Add("rank", "The commander's new exploration rank (Aimless, etc)");
            VARIABLES.Add("rating", "The commander's new exploration combat rating (0-8)");
        }

        [JsonProperty("rank")]
        public string rank { get; private set; }
        [JsonProperty("rating")]
        public int rating{ get; private set; }

        public ExplorationPromotionEvent(DateTime timestamp, int rating) : base(timestamp, NAME)
        {
            this.rating = rating;
            this.rank = Commander.exploreRanks[rating];
        }
    }
}
