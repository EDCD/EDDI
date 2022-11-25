using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class CrewMemberLaunchedEvent : Event
    {
        public const string NAME = "Crew member launched";
        public const string DESCRIPTION = "Triggered when a crew member launches the fighter";
        public const string SAMPLE = "{\"timestamp\":\"2017-03-09T12:28:10Z\", \"event\":\"CrewLaunchFighter\", \"Crew\":\"M. Volgrand\", \"ID\":13}";

        [PublicAPI("The name of the crew member who launched")]
        public string crew { get; private set; }

        [PublicAPI("True if the crew member joined via telepresence")]
        public bool? telepresence { get; private set; }

        public int id { get; private set; }

        public CrewMemberLaunchedEvent(DateTime timestamp, string crew, int id, bool? telepresence) : base(timestamp, NAME)
        {
            this.crew = crew;
            this.telepresence = telepresence;
            this.id = id;
        }
    }
}
