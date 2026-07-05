using System;
using System.Collections.Generic;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class MissionAbandonedEvent ( DateTime timestamp, ulong missionid, string name, long fine )
        : Event( timestamp, NAME )
    {
        public const string NAME = "Mission abandoned";
        public const string DESCRIPTION = "Triggered when you abandon a mission";
        public const string SAMPLE = "{ \"timestamp\":\"2016-09-25T12:53:01Z\", \"event\":\"MissionAbandoned\", \"Name\":\"Mission_PassengerVIP_name\", \"MissionID\":26493517, \"Fine\":20000 }";

        [PublicAPI("The ID of the mission")]
        public ulong missionid { get; private set; } = missionid;

        [PublicAPI("The name of the mission")]
        public string name { get; private set; } = name;

        [PublicAPI("The fine levied")]
        public long fine { get; private set; } = fine;

        public static bool Handle ( DateTime timestamp, string edType, string line, IDictionary<string, object> data, ref List<Event> events, bool fromLogLoad )
        {
            switch ( edType )
            {
                case "CommunityGoalDiscard":
                    var cgid = JsonParsing.getULong(data, "CGID");
                    events.Add( new MissionAbandonedEvent( timestamp, cgid, "MISSION_CommunityGoal", 0 ) { raw = line, fromLoad = fromLogLoad } );
                    return true;
                case "MissionAbandoned":
                    var missionid = JsonParsing.getULong(data, "MissionID");
                    var name = JsonParsing.getString(data, "Name");
                    var fine = JsonParsing.getOptionalLong(data, "Fine") ?? 0;
                    events.Add( new MissionAbandonedEvent( timestamp, missionid, name, fine ) { raw = line, fromLoad = fromLogLoad } );
                    return true;
                default:
                    return false;
            }
        }
    }
}
