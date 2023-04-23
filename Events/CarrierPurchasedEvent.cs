using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class CarrierPurchasedEvent : Event
    {
        public const string NAME = "Carrier purchased";
        public const string DESCRIPTION = "Triggered when you purchase a fleet carrier";
        public const string SAMPLE = "{ \"timestamp\":\"2020-05-20T02:18:58Z\", \"event\":\"CarrierBuy\", \"CarrierID\":3700996608, \"BoughtAtMarket\":3223259392, \"Location\":\"Mitnahas\", \"SystemAddress\":7267218695553, \"Price\":5000000000, \"Variant\":\"CarrierDockB\", \"Callsign\":\"P17-H9H\" }";

        // System variables

        [PublicAPI("The name of the system in which the carrier is located after purchase")]
        public string systemname { get; private set; }

        // Carrier variables

        [PublicAPI("The callsign (alphanumeric designation) of the carrier")]
        public string callsign { get; private set; }

        [PublicAPI("The purchase price of the carrier")]
        public long? price { get; private set; }

        // These properties are not intended to be user facing

        public ulong systemAddress { get; private set; }

        public long? carrierId { get; private set; }

        public CarrierPurchasedEvent(DateTime timestamp, long? carrierId, string carrierCallsign, string systemName, ulong systemAddress, long? price) : base(timestamp, NAME)
        {
            // System
            this.systemname = systemName;
            this.systemAddress = systemAddress;

            // Carrier
            this.carrierId = carrierId;
            this.callsign = carrierCallsign;
            this.price = price;
        }
    }
}