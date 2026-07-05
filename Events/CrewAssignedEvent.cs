using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class CrewAssignedEvent ( DateTime timestamp, string name, long crewid, string role )
        : Event( timestamp, NAME )
    {
        public const string NAME = "Crew assigned";
        public const string DESCRIPTION = "Triggered when you assign crew";
        public const string SAMPLE = "{\"timestamp\":\"2016-08-09T08: 46:29Z\",\"event\":\"CrewAssign\",\"Name\":\"Margaret Parrish\",\"CrewID\":236064708,\"Role\":\"Active\"}";

        [PublicAPI("The name of the crewmember being assigned")]
        public string name { get; private set; } = name;

        [PublicAPI("The ID of the crewmember being assigned")]
        public long crewid { get; private set; } = crewid;

        [PublicAPI("The role to which the crewmember is being assigned")]
        public string role { get; private set; } = role;

        public static bool Handle ( DateTime timestamp, string line, IDictionary<string, object> data, ref List<Event> events, bool fromLogLoad )
        {
            if ( fromLogLoad ) { return true; } // Skip handling this during log loading

            var name = JsonParsing.getString(data, "Name");
            var crewid = JsonParsing.getLong(data, "CrewID");
            var role = EventParsing.CrewRole(data, "Role");
            events.Add( new CrewAssignedEvent( timestamp, name, crewid, role ) { raw = line, fromLoad = fromLogLoad } );
            return true;
        }
    }
}
