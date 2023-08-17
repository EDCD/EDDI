using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class CarrierFinanceEvent : Event
    {
        public const string NAME = "Carrier finance";
        public const string DESCRIPTION = "Triggered when changing tax rates or reserve funds at your fleet carrier";
        public const string SAMPLE = "{ \"timestamp\":\"2020-03-26T10:36:32Z\", \"event\":\"CarrierFinance\", \"CarrierID\":3700005632, \"TaxRate\":5, \"CarrierBalance\":3278186, \"ReserveBalance\":0, \"AvailableBalance\":475108, \"ReservePercent\":0 }\t";

        [PublicAPI("The overall tax rate of your fleet carrier")]
        public int taxRate { get; private set; }

        [PublicAPI("The percentage of your carrier's credit balance reserved fleet carrier expenses")]
        public int reservePercent { get; private set; }

        [PublicAPI("Your fleet carrier's current total credit balance")]
        public long bankBalance { get; private set; }

        [PublicAPI("The current credit balance reserved for fleet carrier expenses")]
        public long bankReservedBalance { get; private set; }

        [PublicAPI("Your fleet carrier's current available credits")]
        public long bankAvailableBalance { get; private set; }

        // Not intended to be user facing
        public long? carrierID { get; private set; }

        public CarrierFinanceEvent(DateTime timestamp, long? carrierId, int taxRate, int reservePercent, long carrierBalance, long reserveBalance, long availableBalance) : base(timestamp, NAME)
        {
            carrierID = carrierId;
            this.taxRate = taxRate;
            this.reservePercent = reservePercent;
            this.bankBalance = carrierBalance;
            this.bankReservedBalance = reserveBalance;
            this.bankAvailableBalance = availableBalance;
        }
    }
}