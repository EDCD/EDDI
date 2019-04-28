using EddiEvents;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace EddiCrimeMonitor
{
    public class BountyPaidEvent : Event
    {
        public const string NAME = "Bounty paid";
        public const string DESCRIPTION = "Triggered when you pay a bounty";
        public const string SAMPLE = "{ \"timestamp\":\"2018-03-19T10:25:10Z\", \"event\":\"PayBounties\", \"Amount\":400, \"AllFines\":false, \"Faction\":\"$faction_Federation;\", \"Faction_Localised\":\"Federation\", \"ShipID\":9, \"BrokerPercentage\":25.000000 }";

        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static BountyPaidEvent()
        {
            VARIABLES.Add("amount", "The amount of the bounty paid");
            VARIABLES.Add("brokerpercentage", "Broker percentage (if paid via a Broker)");
            VARIABLES.Add("allbounties", "Whether this payment covers all current bounties (true or false)");
            VARIABLES.Add("faction", "The faction to which the bounty was paid");
            VARIABLES.Add("shipid", "The ship id of the ship associated with the fine");
        }

        [JsonProperty("amount")]
        public long amount { get; private set; }

        [JsonProperty("brokerpercentage")]
        public decimal? brokerpercentage { get; private set; }

        [JsonProperty("allbounties")]
        public bool allbounties { get; private set; }

        [JsonProperty("faction")]
        public string faction { get; private set; }

        [JsonProperty("shipid")]
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
