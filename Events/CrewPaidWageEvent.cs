using System;
using System.Collections.Generic;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class CrewPaidWageEvent ( DateTime timestamp, string name, long crewid, long amount )
        : Event( timestamp, NAME )
    {
        public const string NAME = "Crew paid wage";
        public const string DESCRIPTION = "Triggered when npc crew receives a profit share";
        public const string SAMPLE = "{\"timestamp\":\"2019-03-09T18:46:52Z\", \"event\":\"NpcCrewPaidWage\", \"NpcCrewName\":\"Xenia Hoover\", \"NpcCrewId\":236064708, \"Amount\":8649}";

        [PublicAPI( "The name of the crewmember")]
        public string name { get; private set; } = name;

        [PublicAPI( "The ID of the crewmember")]
        public long crewid { get; private set; } = crewid;

        [PublicAPI( "The amount paid to the crewmember")]
        public long amount { get; private set; } = amount;

        public static bool Handle ( DateTime timestamp, string line, IDictionary<string, object> data, ref List<Event> events, bool fromLogLoad )
        {
            if ( fromLogLoad ) { return true; } // Skip handling this during log loading

            var name = JsonParsing.getString(data, "NpcCrewName");
            var crewid = JsonParsing.getLong(data, "NpcCrewId");
            var amount = JsonParsing.getLong(data, "Amount");

            if ( amount > 0 )
            {
                events.Add( new CrewPaidWageEvent( timestamp, name, crewid, amount ) { raw = line, fromLoad = fromLogLoad } );
            }
            return true;
        }
    }
}
