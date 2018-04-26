using EddiDataDefinitions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace EddiEvents
{
    public class CombatPromotionEvent : Event
    {
        public const string NAME = "Combat promotion";
        public const string DESCRIPTION = "Triggered when your combat rank increases";
        public const string SAMPLE = "{\"timestamp\":\"2016-06-10T14:32:03Z\",\"event\":\"Promotion\",\"Combat\":5}";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static CombatPromotionEvent()
        {
            VARIABLES.Add("rating", "The commander's new combat rating");
        }

        [JsonProperty("rating")]
        public string rating{ get; private set; }

        public CombatPromotionEvent(DateTime timestamp, CombatRating rating) : base(timestamp, NAME)
        {
            this.rating = rating.localizedName;
        }
    }
}
