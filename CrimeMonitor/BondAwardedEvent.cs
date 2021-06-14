using EddiEvents;
using System;
using Utilities;

namespace EddiCrimeMonitor
{
    [PublicAPI]
    public class BondAwardedEvent : Event
    {
        public const string NAME = "Bond awarded";
        public const string DESCRIPTION = "Triggered when you are awarded a combat bond";
        public const string SAMPLE = "{\"timestamp\":\"2016-06-10T14:32:03Z\",\"event\":\"FactionKillBond\",\"Reward\":500,\"AwardingFaction\":\"Jarildekald Public Industry\",\"VictimFaction\":\"Lencali Freedom Party\"}";

        [PublicAPI("The name of the faction awarding the bond")]
        public string awardingfaction { get; private set; }

        [PublicAPI("The name of the faction whose ship you destroyed")]
        public string victimfaction { get; private set; }

        [PublicAPI("The number of credits received")]
        public long reward { get; private set; }

        public BondAwardedEvent(DateTime timestamp, string awardingfaction, string victimfaction, long reward) : base(timestamp, NAME)
        {
            this.awardingfaction = awardingfaction;
            this.victimfaction = victimfaction;
            this.reward = reward;
        }
    }
}
