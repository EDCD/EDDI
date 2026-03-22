using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class SquadronStatusEvent ( DateTime timestamp, string name, int? squadronId, string status )
        : Event( timestamp, NAME )
    {
        public const string NAME = "Squadron status";
        public const string DESCRIPTION = "Triggered when your status with a squadron has changed";
        public const string SAMPLE = null;

        [PublicAPI("The squadron name")]
        public string name { get; private set; } = name;

        [PublicAPI( "The squadron's numeric ID" )]
        public int? squadronID { get; private set; } = squadronId;

        [PublicAPI("The squadron status (`applied`, `created`, `disbanded`, `invited`, `joined`, `kicked`, `left`)")]
        public string status { get; private set; } = status;
    }
}
