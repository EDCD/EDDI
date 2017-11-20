using EddiDataDefinitions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace EddiEvents
{
    public class KilledEvent : Event
    {
        public const string NAME = "Killed";
        public const string DESCRIPTION = "Triggered when you kill another player";
        public const string SAMPLE = "{\"timestamp\":\"2016-06-10T14:32:03Z\",\"event\":\"PVPKill\",\"Victim\":\"Fred\",\"CombatRank\":2}";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static KilledEvent()
        {
            VARIABLES.Add("victim", "The name of the player killed");
            VARIABLES.Add("rating", "The combat rating of the player killed");
        }

        [JsonProperty("victim")]
        public string victim { get; private set; }

        [JsonProperty("rating")]
        public string rating { get; private set; }

        public KilledEvent(DateTime timestamp, string victim, CombatRating rating) : base(timestamp, NAME)
        {
            this.victim = victim;
            this.rating = (rating == null ? null : rating.name);
        }
    }
}
