using System;
using System.Collections.Generic;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class CrewFiredEvent ( DateTime timestamp, string name, long crewid ) : Event( timestamp, NAME )
    {
        public const string NAME = "Crew fired";
        public const string DESCRIPTION = "Triggered when you fire crew";
        public const string SAMPLE = "{\"timestamp\":\"2016-08-09T08: 46:29Z\",\"event\":\"CrewFire\",\"Name\":\"Margaret Parrish\",\"CrewID\":236064708}";

        [PublicAPI("The name of the crewmember being fired")]
        public string name { get; private set; } = name;

        [PublicAPI("The ID of the crewmember being assigned")]
        public long crewid { get; private set; } = crewid;

        public static bool Handle ( DateTime timestamp, string line, IDictionary<string, object> data, ref List<Event> events, bool fromLogLoad )
        {
            var name = JsonParsing.getString(data, "Name");
            var crewid = JsonParsing.getLong(data, "CrewID");
            events.Add( new CrewFiredEvent( timestamp, name, crewid ) { raw = line, fromLoad = fromLogLoad } );
            return true;
        }
    }
}
