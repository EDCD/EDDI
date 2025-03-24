using EddiDataDefinitions;
using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class PowerRankEvent : Event
    {
        public const string NAME = "Power rank";
        public const string DESCRIPTION = "Triggered when you are awarded a new rank by your pledged Powerplay power";
        public const string SAMPLE = @"{""timestamp"":""2025-03-19T14:04:20Z"",""event"":""PowerplayRank"",""Power"":""Pranav Antal"",""Rank"":22}";

        [PublicAPI("The name of the powerplay power to whom the commander is pledged")]
        public string power => (Power ?? Power.None).localizedName;

        [PublicAPI("The new rank you have been awarded")]
        public int rank { get; set; }
        
        // Not intended to be user facing

        public Power Power { get; private set; }

        public PowerRankEvent ( DateTime timestamp, Power Power, int rank ) : base(timestamp, NAME)
        {
            this.Power = Power;
            this.rank = rank;
        }
    }
}
