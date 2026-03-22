using EddiDataDefinitions;
using System;
using System.Collections.Generic;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class SquadronStartupEvent ( DateTime timestamp, string name, int? squadronId, SquadronRank rank )
        : Event( timestamp, NAME )
    {
        public const string NAME = "Squadron startup";
        public const string DESCRIPTION = "Triggered at startup to provide basic squadron information";
        public const string SAMPLE = null;

        [PublicAPI("The squadron name")]
        public string name { get; private set; } = name;

        [PublicAPI( "The squadron's numeric ID" )]
        public int? squadronID { get; private set; } = squadronId;

        [PublicAPI("Your current squadron rank, as an object with properties 'rankID', 'invariantName', and 'localizedName'")]
        public SquadronRank rank { get; private set; } = rank;

        public static bool Handle ( DateTime timestamp, string line, IDictionary<string, object> data, ref List<Event> events, bool fromLogLoad )
        {
            var name = JsonParsing.getString(data, "SquadronName");
            var squadronID = JsonParsing.getOptionalInt( data, "SquadronID" );
            var rankID = JsonParsing.getInt(data, "CurrentRank");
            var rankName = JsonParsing.getString(data, "CurrentRankName");
            var rankNameLocalized = JsonParsing.getString(data, "CurrentRankName_Localised");
            var rank = new SquadronRank(rankID, rankName, rankNameLocalized);

            events.Add( new SquadronStartupEvent( timestamp, name, squadronID, rank ) { raw = line, fromLoad = fromLogLoad } );
            return true;
        }
    }
}