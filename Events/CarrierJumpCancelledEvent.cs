using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class CarrierJumpCancelledEvent : Event
    {
        public const string NAME = "Carrier jump cancelled";
        public const string DESCRIPTION = "Triggered when you cancel a scheduled fleet carrier jump";
        public const string SAMPLE = "{ \"timestamp\":\"2020-05-11T18:53:47Z\", \"event\":\"CarrierJumpCancelled\", \"CarrierID\":3700357376 }";

        // These properties are not intended to be user facing

        public long carrierId { get; private set; }

        public CarrierJumpCancelledEvent(DateTime timestamp, long carrierId) : base(timestamp, NAME)
        {
            this.carrierId = carrierId;
        }
    }
}