using EddiDataDefinitions;
using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class SquadronRankEvent : Event
    {
        public const string NAME = "Squadron rank";
        public const string DESCRIPTION = "Triggered when your rank with a squadron has changed";
        public const string SAMPLE = null;

        [PublicAPI("The squadron name")]
        public string name { get; private set; }

        [PublicAPI( "The squadron's numeric ID" )]
        public int? squadronID { get; private set; }

        [PublicAPI( "Your old squadron rank, as an object with properties 'rankID', 'invariantName', and 'localizedName'" )]
        public SquadronRank oldrank { get; private set; }

        [PublicAPI( "Your new squadron rank, as an object with properties 'rankID', 'invariantName', and 'localizedName'" )]
        public SquadronRank newrank { get; private set; }

        public SquadronRankEvent ( DateTime timestamp, string name, int? squadronID, SquadronRank oldrank, SquadronRank newrank ) : base(timestamp, NAME)
        {
            this.name = name;
            this.squadronID = squadronID;
            this.oldrank = oldrank;
            this.newrank = newrank;
        }
    }
}
