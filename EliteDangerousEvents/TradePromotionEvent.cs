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
        public static TradePromotionEvent SAMPLE = new TradePromotionEvent(DateTime.Now, 5);

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
