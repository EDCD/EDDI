using EliteDangerousDataDefinitions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EliteDangerousEvents
{
    public class TradePromotionEvent : Event
    {
        public const string NAME = "Trade promotion";
        public const string DESCRIPTION = "Triggered when you trade rank increases";
        public static TradePromotionEvent SAMPLE = new TradePromotionEvent(DateTime.Now, 5);
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static TradePromotionEvent()
        {
            SAMPLE.raw = "{\"timestamp\":\"2016-06-10T14:32:03Z\",\"event\":\"Promotion\",\"Trade\":5}";

            VARIABLES.Add("rank", "The commander's new trade rank (Peniless, etc)");
            VARIABLES.Add("rating", "The commander's new numerical trade rating (0-8)");
        }

        [JsonProperty("rank")]
        public string rank { get; private set; }
        [JsonProperty("rating")]
        public int rating{ get; private set; }

        public TradePromotionEvent(DateTime timestamp, int rating) : base(timestamp, NAME)
        {
            this.rating = rating;
            this.rank = Commander.tradeRanks[rating];
        }
    }
}
