using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class CarrierDecommissionCancelledEvent : Event
    {
        public const string NAME = "Carrier decommission cancelled";
        public const string DESCRIPTION = "Triggered when you cancel the decommissioning of your fleet carrier";
        public const string SAMPLE = "{ \"timestamp\":\"2020-03-11T15:12:38Z\", \"event\":\"CarrierCancelDecommission\", \"CarrierID\":3700005632 }";

        // Not intended to be user facing
        public long? carrierID { get; private set; }

        public CarrierDecommissionCancelledEvent(DateTime timestamp, long? carrierId) : base(timestamp, NAME)
        {
            carrierID = carrierId;
        }
    }
}