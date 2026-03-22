using EddiDataDefinitions;
using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class CarrierFuelDepositEvent (
        DateTime timestamp,
        long carrierId,
        StationModel carrierType,
        int amount,
        int total )
        : Event( timestamp, NAME )
    {
        public const string NAME = "Carrier fuel deposit";
        public const string DESCRIPTION = "Triggered when depositing fuel at your fleet carrier";
        public const string SAMPLE = "{ \"timestamp\":\"2020-03-19T09:17:29Z\", \"event\":\"CarrierDepositFuel\", \"CarrierID\":3700005632, \"Amount\":45, \"Total\":112 }";

        [PublicAPI("The amount of tritium fuel added to your fleet carrier")]
        public int amount { get; private set; } = amount;

        [PublicAPI("The total tritium fuel in your fleet carrier's fuel tank")]
        public int total { get; private set; } = total;

        [PublicAPI( "The carrier's numeric ID" )]
        public long carrierID { get; private set; } = carrierId;

        [PublicAPI( "The carrier type (e.g. Fleet Carrier or Squadron Carrier), as an object with 'localizedName' and 'invariantName' properties" )]
        public StationModel carrierType { get; private set; } = carrierType;
    }
}