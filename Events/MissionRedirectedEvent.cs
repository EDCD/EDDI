using System;
using System.Collections.Generic;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class MissionRedirectedEvent (
        DateTime timestamp,
        ulong missionid,
        string name,
        string newdestinationstation,
        string olddestinationstation,
        string newdestinationsystem,
        string olddestinationsystem )
        : Event( timestamp, NAME )
    {
        public const string NAME = "Mission redirected";
        public const string DESCRIPTION = "Triggered when a mission is redirected";
        public const string SAMPLE = "{ \"timestamp\": \"2017-08-01T09:04:07Z\", \"event\": \"MissionRedirected\", \"MissionID\": 65367315, \"Name\":\"Mission_Courier\", \"NewDestinationStation\": \"Metcalf Orbital\", \"OldDestinationStation\": \"Cuffey Orbital\", \"NewDestinationSystem\": \"Cemiess\", \"OldDestinationSystem\": \"Vequess\" }";

        [PublicAPI("The ID of the mission")]
        public ulong missionid { get; private set; } = missionid;

        [PublicAPI("The name of the mission")]
        public string name { get; private set; } = name;

        [PublicAPI("The new destination station for the mission")]
        public string newdestinationstation { get; private set; } = newdestinationstation;

        [PublicAPI("The old destination station for the mission")]
        public string olddestinationstation { get; private set; } = olddestinationstation;

        [PublicAPI("The new destination system for the mission")]
        public string newdestinationsystem { get; private set; } = newdestinationsystem;

        [PublicAPI("The old destination system for the mission")]
        public string olddestinationsystem { get; private set; } = olddestinationsystem;

        public static bool Handle ( DateTime timestamp, string line, IDictionary<string, object> data, ref List<Event> events, bool fromLogLoad )
        {
            var missionid = JsonParsing.getULong(data, "MissionID");
            var name = JsonParsing.getString(data, "Name");
            var newdestinationstation = JsonParsing.getString(data, "NewDestinationStation");
            var olddestinationstation = JsonParsing.getString(data, "OldDestinationStation");
            var newdestinationsystem = JsonParsing.getString(data, "NewDestinationSystem");
            var olddestinationsystem = JsonParsing.getString(data, "OldDestinationSystem");
            events.Add( new MissionRedirectedEvent( timestamp, missionid, name, newdestinationstation, olddestinationstation, newdestinationsystem, olddestinationsystem ) { raw = line, fromLoad = fromLogLoad } );
            return true;
        }
    }
}
