using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class SquadronRankEvent : Event
    {
        public const string NAME = "Squadron rank";
        public const string DESCRIPTION = "Triggered when your rank with a squadron has changed";
        public const string SAMPLE = null;

        [PublicAPI("The squadron name")]
        public string name { get; private set; }

        [PublicAPI("The old rank")]
        public int oldrank { get; private set; }

        [PublicAPI("The new rank")]
        public int newrank { get; private set; }

        public SquadronRankEvent(DateTime timestamp, string name, int oldrank, int newrank) : base(timestamp, NAME)
        {
            this.name = name;
            this.oldrank = oldrank;
            this.newrank = newrank;
        }
    }
}
