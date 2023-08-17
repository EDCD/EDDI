using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class CarrierBankTransferEvent : Event
    {
        public const string NAME = "Carrier bank transfer";
        public const string DESCRIPTION = "Triggered when you transfer money to or from your fleet carrier";
        public const string SAMPLE = "{ \"timestamp\":\"2020-03-24T15:34:46Z\", \"event\":\"CarrierBankTransfer\", \"CarrierID\":3700005632, \"Deposit\":80000, \"PlayerBalance\":717339604128, \"CarrierBalance\":3020010 }";

        [PublicAPI("The amount deposited with the fleet carrier")]
        public ulong? deposit { get; private set; }

        [PublicAPI("The amount withdrawn from the fleet carrier")]
        public ulong? withdrawal { get; private set; }

        [PublicAPI("Your updated credit balance")]
        public ulong cmdrBalance { get; private set; }

        [PublicAPI("The fleet carrier's updated credit balance")]
        public long bankBalance { get; private set; }
        
        // Not intended to be user facing
        public long carrierID { get; private set; }

        public CarrierBankTransferEvent(DateTime timestamp, long carrierId, ulong? deposit, ulong? withdrawal, ulong cmdrBalance, long carrierBalance) : base(timestamp, NAME)
        {
            carrierID = carrierId;
            this.deposit = deposit;
            this.withdrawal = withdrawal;
            this.cmdrBalance = cmdrBalance;
            this.bankBalance = carrierBalance;
        }
    }
}