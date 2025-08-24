using EddiDataDefinitions;
using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class SquadronStartupEvent : Event
    {
        public const string NAME = "Squadron startup";
        public const string DESCRIPTION = "Triggered at startup to provide basic squadron information";
        public const string SAMPLE = null;

        [PublicAPI("The squadron name")]
        public string name { get; private set; }

        [PublicAPI( "The squadron's numeric ID" )]
        public int? squadronID { get; private set; }

        [PublicAPI("Your current squadron rank, as an object with properties 'rankID', 'invariantName', and 'localizedName'")]
        public SquadronRank rank { get; private set; }
        
        public SquadronStartupEvent ( DateTime timestamp, string name, int? squadronID, SquadronRank rank ) : base(timestamp, NAME)
        {
            this.name = name;
            this.squadronID = squadronID;
            this.rank = rank;
        }
    }
}