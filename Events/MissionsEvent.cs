using EddiDataDefinitions;
using System;
using System.Collections.Generic;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class MissionsEvent : Event
    {
        public const string NAME = "Missions";
        public const string DESCRIPTION = "Triggered at startup, with basic information of the Mission Log";
        public const string SAMPLE = null;

        // Not intended to be user facing

        public List<Mission> missions { get; private set; }

        public MissionsEvent(DateTime timestamp, List<Mission> missions) : base(timestamp, NAME)
        {
            this.missions = missions;
        }

        public static bool Handle ( DateTime timestamp, string line, IDictionary<string, object> data, ref List<Event> events, bool fromLogLoad )
        {
            var missions = new List<Mission>();
            foreach ( var status in new[] { MissionStatus.Active, MissionStatus.Failed, MissionStatus.Complete } )
            {
                if ( data.TryGetValue( status.invariantName, out var val ) )
                {
                    var missionLog = (List<object>)val;
                    foreach ( var mission in missionLog )
                    {
                        var missionProperties = (Dictionary<string, object>)mission;
                        var missionId = JsonParsing.getULong( missionProperties, "MissionID" );
                        var name = JsonParsing.getString( missionProperties, "Name" );
                        var localizedName = JsonParsing.getString( missionProperties, "Name_Localised" );
                        var expires = JsonParsing.getDecimal( missionProperties, "Expires" );

                        // Colonization missions use the actual unit timestamp rather than a timestamp offset so we need to handle those as a special case.
                        var expiry = name == "$Mission_Colonisation_Initial_Name;"
                            ? Dates.fromTimestamp( Convert.ToInt64( expires ) ) ?? DateTime.MinValue
                            : timestamp.AddSeconds( (double)expires );

                        // If mission is 'Active' and expires = 0, then set status to 'Claim'
                        var missionStatus = status == MissionStatus.Active && expires == 0
                            ? MissionStatus.Claim
                            : status;
                        var newMission = new Mission( missionId, name, expiry, missionStatus )
                        {
                            localisedname = localizedName
                        };
                        missions.Add( newMission );
                    }
                }
            }

            events.Add( new MissionsEvent( timestamp, missions ) { raw = line, fromLoad = fromLogLoad } );
            return true;
        }
    }
}
