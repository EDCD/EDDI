using EddiDataDefinitions;
using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class SquadronRankEvent (
        DateTime timestamp,
        string name,
        int? squadronId,
        SquadronRank oldrank,
        SquadronRank newrank )
        : Event( timestamp, NAME )
    {
        public const string NAME = "Squadron rank";
        public const string DESCRIPTION = "Triggered when your rank with a squadron has changed";
        public const string SAMPLE = null;

        [PublicAPI("The squadron name")]
        public string name { get; private set; } = name;

        [PublicAPI( "The squadron's numeric ID" )]
        public int? squadronID { get; private set; } = squadronId;

        [PublicAPI( "Your old squadron rank, as an object with properties 'rankID', 'invariantName', and 'localizedName'" )]
        public SquadronRank oldrank { get; private set; } = oldrank;

        [PublicAPI( "Your new squadron rank, as an object with properties 'rankID', 'invariantName', and 'localizedName'" )]
        public SquadronRank newrank { get; private set; } = newrank;
    }
}
