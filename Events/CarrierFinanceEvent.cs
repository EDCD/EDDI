using EddiDataDefinitions;
using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class CarrierFinanceEvent (
        DateTime timestamp,
        long carrierId,
        StationModel carrierType,
        int taxRate,
        int reservePercent,
        long carrierBalance,
        long reserveBalance,
        long availableBalance )
        : Event( timestamp, NAME )
    {
        public const string NAME = "Carrier finance";
        public const string DESCRIPTION = "Triggered when changing tax rates or reserve funds at your fleet carrier";
        public const string SAMPLE = "{ \"timestamp\":\"2020-03-26T10:36:32Z\", \"event\":\"CarrierFinance\", \"CarrierID\":3700005632, \"TaxRate\":5, \"CarrierBalance\":3278186, \"ReserveBalance\":0, \"AvailableBalance\":475108, \"ReservePercent\":0 }\t";

        [PublicAPI("The overall tax rate of your fleet carrier")]
        public int taxRate { get; private set; } = taxRate;

        [PublicAPI("The percentage of your carrier's credit balance reserved fleet carrier expenses")]
        public int reservePercent { get; private set; } = reservePercent;

        [PublicAPI("Your fleet carrier's current total credit balance")]
        public long bankBalance { get; private set; } = carrierBalance;

        [PublicAPI("The current credit balance reserved for fleet carrier expenses")]
        public long bankReservedBalance { get; private set; } = reserveBalance;

        [PublicAPI("Your fleet carrier's current available credits")]
        public long bankAvailableBalance { get; private set; } = availableBalance;

        [PublicAPI( "The carrier's numeric ID" )]
        public long carrierID { get; private set; } = carrierId;

        [PublicAPI( "The carrier type (e.g. Fleet Carrier or Squadron Carrier), as an object with 'localizedName' and 'invariantName' properties" )]
        public StationModel carrierType { get; private set; } = carrierType;
    }
}