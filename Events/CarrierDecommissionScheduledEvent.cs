using EddiDataDefinitions;
using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class CarrierDecommissionScheduledEvent (
        DateTime timestamp,
        long carrierId,
        StationModel carrierType,
        ulong refund,
        TimeSpan? decommissionTimespan )
        : Event( timestamp, NAME )
    {
        public const string NAME = "Carrier decommission scheduled";
        public const string DESCRIPTION = "Triggered when you request the decommissioning of your fleet carrier";
        public const string SAMPLE = "{ \"timestamp\":\"2020-03-11T15:12:26Z\", \"event\":\"CarrierDecommission\", \"CarrierID\":3700005632, \"ScrapRefund\":4850000000, \"ScrapTime\":1584601200 }";

        [PublicAPI("The amount which will be refunded to you when the fleet carrier is decommissioned")]
        public ulong refund { get; private set; } = refund;

        [PublicAPI("The total days until the fleet carrier is decommissioned, expressed as a decimal")]
        public decimal? totalDays => Convert.ToDecimal( decommissionTimespan?.TotalDays ?? 0 );

        [PublicAPI("The days component of the time remaining before the fleet carrier is decommissioned")]
        public int? days => decommissionTimespan?.Days ?? 0;

        [PublicAPI("The hours component of the time remaining before the fleet carrier is decommissioned")]
        public int? hours => decommissionTimespan?.Hours ?? 0;

        [PublicAPI("The minutes component of the time remaining before the fleet carrier is decommissioned")]
        public int? minutes => decommissionTimespan?.Minutes ?? 0;

        [PublicAPI( "The carrier's numeric ID" )]
        public long carrierID { get; private set; } = carrierId;

        [PublicAPI( "The carrier type (e.g. Fleet Carrier or Squadron Carrier), as an object with 'localizedName' and 'invariantName' properties" )]
        public StationModel carrierType { get; private set; } = carrierType;

        // Not intended to be user facing

        public TimeSpan? decommissionTimespan { get; private set; } = decommissionTimespan;
    }
}