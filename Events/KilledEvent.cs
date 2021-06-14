using EddiDataDefinitions;
using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class KilledEvent : Event
    {
        public const string NAME = "Killed";
        public const string DESCRIPTION = "Triggered when you kill another player";
        public const string SAMPLE = "{\"timestamp\":\"2016-06-10T14:32:03Z\",\"event\":\"PVPKill\",\"Victim\":\"Fred\",\"CombatRank\":2}";

        [PublicAPI("The name of the player killed")]
        public string victim { get; private set; }

        [PublicAPI("The combat rating of the player killed")]
        public string rating { get; private set; }

        public KilledEvent(DateTime timestamp, string victim, CombatRating rating) : base(timestamp, NAME)
        {
            this.victim = victim;
            this.rating = rating?.localizedName;
        }
    }
}
