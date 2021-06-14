using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class DataVoucherAwardedEvent : Event
    {
        public const string NAME = "Data voucher awarded";
        public const string DESCRIPTION = "Triggered when you are awarded a data voucher";
        public const string SAMPLE = "{\"timestamp\":\"2016-06-10T14:32:03Z\",\"event\":\"DatalinkVoucher\",\"Reward\":500,\"VictimFaction\":\"Jarildekald Public Industry\",\"PayeeFaction\":\"Lencali Freedom Party\"}";

        [PublicAPI("The number of credits received")]
        public long reward { get; private set; }

        [PublicAPI("The name of the faction whose data you scanned")]
        public string victimfaction { get; private set; }

        [PublicAPI("The name of the faction awarding the voucher")]
        public string payeefaction { get; private set; }

        public DataVoucherAwardedEvent(DateTime timestamp, string payeefaction, string victimfaction, long reward) : base(timestamp, NAME)
        {
            this.reward = reward;
            this.victimfaction = victimfaction;
            this.payeefaction = payeefaction;
        }
    }

    public class Reward
    {
        [PublicAPI]
        public string faction { get; private set; }
        
        [PublicAPI]
        public long amount { get; private set; }

        public Reward(string faction, long amount)
        {
            this.faction = faction;
            this.amount = amount;
        }
    }
}
