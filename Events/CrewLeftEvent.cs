using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class CrewLeftEvent ( DateTime timestamp, string captain, bool? telepresence ) : Event( timestamp, NAME )
    {
        public const string NAME = "Crew left";
        public const string DESCRIPTION = "Triggered when you leave a crew";
        public const string SAMPLE = "{\"timestamp\":\"2016-08-09T08: 46:29Z\",\"event\":\"QuitACrew\",\"Captain\":\"$cmdr_decorate:#name=Jameson;\"}";

        [PublicAPI("The name of the captain of the crew you have left")]
        public string captain { get; private set; } = captain;

        [PublicAPI("True if you joined via telepresence")]
        public bool? telepresence { get; private set; } = telepresence;
    }
}
