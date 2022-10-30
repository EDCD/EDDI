using System;
using EddiDataDefinitions;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class CarrierFuelDepositEvent : Event
    {
        public const string NAME = "Carrier fuel deposit";
        public const string DESCRIPTION = "Triggered when depositing fuel at your fleet carrier";
        public const string SAMPLE = "{ \"timestamp\":\"2020-03-19T09:17:29Z\", \"event\":\"CarrierDepositFuel\", \"CarrierID\":3700005632, \"Amount\":45, \"Total\":112 }";

        [PublicAPI("The amount of tritium fuel added to your fleet carrier")]
        public int amount { get; private set; }

        [PublicAPI("The total tritium fuel in your fleet carrier's fuel tank")]
        public int total { get; private set; }

        // Not intended to be user facing
        public long? carrierID { get; private set; }

        public CarrierFuelDepositEvent(DateTime timestamp, long? carrierId, int amount, int total) : base(timestamp, NAME)
        {
            carrierID = carrierId;
            this.amount = amount;
            this.total = total;
        }
    }
}