using EddiDataDefinitions;
using System;
using System.Collections.Generic;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class MissionsEvent ( DateTime timestamp, List<Mission> missions,
        IEnumerable<ulong> zeroExpiryMissionIds = null ) : Event( timestamp, NAME )
    {
        public const string NAME = "Missions";
        public const string DESCRIPTION = "Triggered at startup, with basic information of the Mission Log";
        public const string SAMPLE = null;

        // Not intended to be user facing

        public List<Mission> missions { get; private set; } = missions;
        public IReadOnlySet<ulong> zeroExpiryMissionIds { get; private set; } = new HashSet<ulong>( zeroExpiryMissionIds ?? [] );

        public static bool Handle ( DateTime timestamp, string line, IDictionary<string, object> data, ref List<Event> events, bool fromLogLoad )
        {
            var missions = new List<Mission>();
            var zeroExpiryMissionIds = new HashSet<ulong>();
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
                        var hasExpiry = missionProperties.ContainsKey( "Expires" );
                        var expires = hasExpiry ? JsonParsing.getDecimal( missionProperties, "Expires" ) : 0;
                        var expiryIsZero = hasExpiry && expires == 0;
                        if ( status == MissionStatus.Active && expiryIsZero )
                        {
                            zeroExpiryMissionIds.Add( missionId );
                        }

                        // Colonization missions use the actual unit timestamp rather than a timestamp offset so we need to handle those as a special case.
                        DateTime? expiry = !hasExpiry || expiryIsZero
                            ? null
                            : name == "$Mission_Colonisation_Initial_Name;"
                                ? Dates.fromTimestamp( Convert.ToInt64( expires ) ) ?? DateTime.MinValue
                                : timestamp.AddSeconds( (double)expires );

                        var newMission = new Mission( missionId, name, expiry, status )
                        {
                            localisedname = localizedName
                        };
                        missions.Add( newMission );
                    }
                }
            }

            events.Add( new MissionsEvent( timestamp, missions, zeroExpiryMissionIds )
                { raw = line, fromLoad = fromLogLoad } );
            return true;
        }
    }
}
