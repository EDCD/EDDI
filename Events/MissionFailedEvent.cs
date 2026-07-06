using System;
using System.Collections.Generic;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class MissionFailedEvent ( DateTime timestamp, ulong missionid, string name, long fine )
        : Event( timestamp, NAME )
    {
        public const string NAME = "Mission failed";
        public const string DESCRIPTION = "Triggered when you fail a mission";
        public const string SAMPLE = "{ \"timestamp\":\"2016-09-25T12:53:01Z\", \"event\":\"MissionFailed\", \"Name\":\"Mission_PassengerVIP_name\", \"MissionID\":26493517 }";

        [PublicAPI("The ID of the mission")]
        public ulong missionid { get; private set; } = missionid;

        [PublicAPI("The name of the mission")]
        public string name { get; private set; } = name;

        [PublicAPI("The fine levied")]
        public long fine { get; private set; } = fine;

        public static bool Handle ( DateTime timestamp, string line, IDictionary<string, object> data, ref List<Event> events, bool fromLogLoad )
        {
            var missionid = JsonParsing.getULong(data, "MissionID");
            var name = JsonParsing.getString(data, "Name");
            var fine = JsonParsing.getOptionalLong(data, "Fine") ?? 0;
            events.Add( new MissionFailedEvent( timestamp, missionid, name, fine ) { raw = line, fromLoad = fromLogLoad } );
            return true;
        }
    }
}
