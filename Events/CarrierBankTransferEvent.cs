using EddiDataDefinitions;
using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class CarrierBankTransferEvent (
        DateTime timestamp,
        long carrierId,
        StationModel carrierType,
        long? deposit,
        long? withdrawal,
        long cmdrBalance,
        long carrierBalance )
        : Event( timestamp, NAME )
    {
        public const string NAME = "Carrier bank transfer";
        public const string DESCRIPTION = "Triggered when you transfer money to or from your fleet carrier";
        public const string SAMPLE = "{ \"timestamp\":\"2020-03-24T15:34:46Z\", \"event\":\"CarrierBankTransfer\", \"CarrierID\":3700005632, \"Deposit\":80000, \"PlayerBalance\":717339604128, \"CarrierBalance\":3020010 }";

        [PublicAPI("The amount deposited with the fleet carrier")]
        public long? deposit { get; private set; } = deposit;

        [PublicAPI("The amount withdrawn from the fleet carrier")]
        public long? withdrawal { get; private set; } = withdrawal;

        [PublicAPI("Your updated credit balance")]
        public long cmdrBalance { get; private set; } = cmdrBalance;

        [PublicAPI("The fleet carrier's updated credit balance")]
        public long bankBalance { get; private set; } = carrierBalance;

        [PublicAPI( "The carrier's numeric ID" )]
        public long carrierID { get; private set; } = carrierId;

        [PublicAPI( "The carrier type (e.g. Fleet Carrier or Squadron Carrier), as an object with 'localizedName' and 'invariantName' properties" )]
        public StationModel carrierType { get; private set; } = carrierType;
    }
}