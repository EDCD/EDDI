using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class CrewSessionEndedEvent : Event
    {
        public const string NAME = "Crew session ended";
        public const string DESCRIPTION = "Triggered when you disband your crew and end the multicrew session";
        public const string SAMPLE = @"{ ""timestamp"":""2017-12-08T04:29:17Z"", ""event"":""EndCrewSession"", ""OnCrime"":false }";

        [PublicAPI("True if the multicrew session was disbanded after a crime was committed")]
        public bool? onCrime { get; private set; }

        [PublicAPI("True if this was a telepresence multicrew session")]
        public bool? telepresence { get; private set; }

        public CrewSessionEndedEvent(DateTime timestamp, bool? onCrime, bool? telepresence) : base(timestamp, NAME)
        {
            this.onCrime = onCrime;
            this.telepresence = telepresence;
        }
    }
}
