using EddiDataDefinitions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EddiEvents
{
    public class FederationPromotionEvent : Event
    {
        public const string NAME = "Federation promotion";
        public const string DESCRIPTION = "Triggered when your rank increases with the Federation";
        public const string SAMPLE = "{ \"timestamp\":\"2017-10-06T23:47:32Z\", \"event\":\"Promotion\", \"Federation\":13 }";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static FederationPromotionEvent()
        {
            VARIABLES.Add("rank", "The commander's new Federation rank");
        }

        [JsonProperty("rating")]
        public string rank { get; private set; }

        public FederationPromotionEvent(DateTime timestamp, FederationRating rating) : base(timestamp, NAME)
        {
            this.rank = rating.name;
        }
    }
}
