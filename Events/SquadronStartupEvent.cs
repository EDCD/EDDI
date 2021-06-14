using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class SquadronStartupEvent : Event
    {
        public const string NAME = "Squadron startup";
        public const string DESCRIPTION = "Triggered at startup to provide basic squadron information";
        public const string SAMPLE = null;

        [PublicAPI("The squadron name")]
        public string name { get; private set; }

        [PublicAPI("The current rank")]
        public int rank { get; private set; }

        public SquadronStartupEvent(DateTime timestamp, string name, int rank) : base(timestamp, NAME)
        {
            this.name = name;
            this.rank = rank;
        }
    }
}