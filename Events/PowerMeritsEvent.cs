using EddiDataDefinitions;
using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class PowerMeritsEvent : Event
    {
        public const string NAME = "Power merits";
        public const string DESCRIPTION = "Triggered when you are awarded merits by your pledged Powerplay power";
        public const string SAMPLE = @"{""timestamp"":""2025-03-19T19:02:41Z"",""event"":""PowerplayMerits"",""Power"":""Li Yong-Rui"",""MeritsGained"":45552,""TotalMerits"":271788}";

        [PublicAPI("The name of the powerplay power to whom the commander is pledged")]
        public string power => (Power ?? Power.None).localizedName;

        [PublicAPI("The merits awarded for completing a powerplay objective")]
        public int gained { get; set; }

        [PublicAPI( "Your total accumulated merits" )]
        public int total { get; set; }

        // Not intended to be user facing

        public Power Power { get; private set; }

        public PowerMeritsEvent ( DateTime timestamp, Power Power, int meritsGained, int meritsTotal) : base(timestamp, NAME)
        {
            this.Power = Power;
            this.gained = meritsGained;
            this.total = meritsTotal;
        }
    }
}
