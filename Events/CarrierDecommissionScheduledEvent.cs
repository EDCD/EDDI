using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class CarrierDecommissionScheduledEvent : Event
    {
        public const string NAME = "Carrier decommission scheduled";
        public const string DESCRIPTION = "Triggered when you request the decommissioning of your fleet carrier";
        public const string SAMPLE = "{ \"timestamp\":\"2020-03-11T15:12:26Z\", \"event\":\"CarrierDecommission\", \"CarrierID\":3700005632, \"ScrapRefund\":4850000000, \"ScrapTime\":1584601200 }";

        [PublicAPI("The amount which will be refunded to you when the fleet carrier is decommissioned")]
        public ulong refund { get; private set; }

        [PublicAPI("The total days until the fleet carrier is decommissioned, expressed as a decimal")]
        public decimal? totalDays => Convert.ToDecimal( decommissionTimespan?.TotalDays ?? 0 );

        [PublicAPI("The days component of the time remaining before the fleet carrier is decommissioned")]
        public int? days => decommissionTimespan?.Days ?? 0;

        [PublicAPI("The hours component of the time remaining before the fleet carrier is decommissioned")]
        public int? hours => decommissionTimespan?.Hours ?? 0;

        [PublicAPI("The minutes component of the time remaining before the fleet carrier is decommissioned")]
        public int? minutes => decommissionTimespan?.Minutes ?? 0;

        // Not intended to be user facing
        public long? carrierID { get; private set; }

        public TimeSpan? decommissionTimespan { get; private set; }

        public CarrierDecommissionScheduledEvent(DateTime timestamp, long? carrierId, ulong refund, TimeSpan? decommissionTimespan) : base(timestamp, NAME)
        {
            carrierID = carrierId;
            this.refund = refund;
            this.decommissionTimespan = decommissionTimespan;
        }
    }
}