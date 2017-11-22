using EddiDataDefinitions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

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
            VARIABLES.Add("LocalRank", "The commander's new Federation rank translated into the chosen labguage");
        }

        [JsonProperty("rating")]
        public string rank { get; private set; }

        [JsonProperty("LocalRank")]
        public string LocalRank
        {
            get
            {
                if (rank != null && rank != "")
                {
                    return FederationRating.FromName(rank).LocalName;
                }
                else return null;
            }
        }

        public FederationPromotionEvent(DateTime timestamp, FederationRating rating) : base(timestamp, NAME)
        {
            this.rank = rating.name;
        }
    }
}
