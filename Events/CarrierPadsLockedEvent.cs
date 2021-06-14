using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class CarrierPadsLockedEvent : Event
    {
        public const string NAME = "Carrier pads locked";
        public const string DESCRIPTION = "Triggered when your fleet carrier locks landing pads prior to jumping";
        public static CarrierPadsLockedEvent SAMPLE = new CarrierPadsLockedEvent(DateTime.UtcNow, 3700357376);

        // These properties are not intended to be user facing

        public long carrierId { get; private set; }

        public CarrierPadsLockedEvent(DateTime timestamp, long carrierId) : base(timestamp, NAME)
        {
            this.carrierId = carrierId;
        }
    }
}