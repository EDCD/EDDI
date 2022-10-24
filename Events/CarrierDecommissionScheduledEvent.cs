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
        public decimal? totalDays { get; private set; }

        [PublicAPI("The days component of the time remaining before the fleet carrier is decommissioned")]
        public int? days { get; private set; }

        [PublicAPI("The hours component of the time remaining before the fleet carrier is decommissioned")]
        public int? hours { get; private set; }

        [PublicAPI("The minutes component of the time remaining before the fleet carrier is decommissioned")]
        public int? minutes { get; private set; }
        
        // Not intended to be user facing
        public long? carrierID { get; private set; }

        public CarrierDecommissionScheduledEvent(DateTime timestamp, long? carrierId, ulong refund, TimeSpan? decommissionTimespan) : base(timestamp, NAME)
        {
            carrierID = carrierId;
            this.refund = refund;
            this.totalDays = Convert.ToDecimal(decommissionTimespan?.TotalDays ?? 0);
            this.days = decommissionTimespan?.Days ?? 0;
            this.hours = decommissionTimespan?.Hours ?? 0;
            this.minutes = decommissionTimespan?.Minutes ?? 0;
        }
    }
}