using EddiEvents;
using System;
using Utilities;

namespace EddiCrimeMonitor
{
    [PublicAPI]
    public class BountyPaidEvent : Event
    {
        public const string NAME = "Bounty paid";
        public const string DESCRIPTION = "Triggered when you pay a bounty";
        public const string SAMPLE = "{ \"timestamp\":\"2018-03-19T10:25:10Z\", \"event\":\"PayBounties\", \"Amount\":400, \"AllFines\":false, \"Faction\":\"$faction_Federation;\", \"Faction_Localised\":\"Federation\", \"ShipID\":9, \"BrokerPercentage\":25.000000 }";

        [PublicAPI("The amount of the bounty paid")]
        public long amount { get; private set; }

        [PublicAPI("Broker percentage (if paid via a Broker)")]
        public decimal? brokerpercentage { get; private set; }

        [PublicAPI("Whether this payment covers all current bounties (true or false)")]
        public bool allbounties { get; private set; }

        [PublicAPI("The faction to which the bounty was paid")]
        public string faction { get; private set; }

        [PublicAPI("The ship id of the ship associated with the fine")]
        public int shipid { get; private set; }

        public BountyPaidEvent(DateTime timestamp, long amount, decimal? brokerpercentage, bool allbounties, string faction, int shipId) : base(timestamp, NAME)
        {
            this.amount = amount;
            this.brokerpercentage = brokerpercentage;
            this.allbounties = allbounties;
            this.faction = faction;
            this.shipid = shipId;
        }
    }
}
