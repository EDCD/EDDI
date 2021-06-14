using EddiDataDefinitions;
using EddiEvents;
using System;
using Utilities;

namespace EddiCargoMonitor
{
    [PublicAPI]
    public class PowerCommodityFastTrackedEvent : Event
    {
        public const string NAME = "Power commodity fast tracked";
        public const string DESCRIPTION = "Triggered when a commander fast tracks a commodity of a power";
        public const string SAMPLE = @"{ ""timestamp"":""2016-12-02T16:10:53Z"", ""event"":""PowerplayFastTrack"", ""Power"":""Aisling Duval"", ""Cost"":100000 }";

        [PublicAPI("The name of the power for which the commander is fast tracking the commodity")]
        public string power => (Power ?? Power.None).localizedName;

        [PublicAPI("The number of credits spent fast tracking")]
        public int amount { get; private set; }

        // Not intended to be user facing

        public Power Power { get; private set; }

        public PowerCommodityFastTrackedEvent(DateTime timestamp, Power Power, int amount) : base(timestamp, NAME)
        {
            this.Power = Power;
            this.amount = amount;
        }
    }
}
