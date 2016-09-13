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
        public static ExplorationPromotionEvent SAMPLE = new ExplorationPromotionEvent(DateTime.Now, 5);

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
