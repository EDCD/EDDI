using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace EddiEvents
{
    public class BondAwardedEvent : Event
    {
        public const string NAME = "Bond awarded";
        public const string DESCRIPTION = "Triggered when you are awarded a combat bond";
        public const string SAMPLE = "{\"timestamp\":\"2016-06-10T14:32:03Z\",\"event\":\"FactionKillBond\",\"Reward\":500,\"AwardingFaction\":\"Jarildekald Public Industry\",\"VictimFaction\":\"Lencali Freedom Party\"}";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static BondAwardedEvent()
        {

            VARIABLES.Add("awardingfaction", "The name of the faction awarding the bond");
            VARIABLES.Add("victimfaction", "The name of the faction whose ship you destroyed");
            VARIABLES.Add("reward", "The number of credits received");
        }

        [JsonProperty("awardingfaction")]
        public string awardingfaction { get; private set; }

        [JsonProperty("victimfaction")]
        public string victimfaction { get; private set; }

        [JsonProperty("reward")]
        public long reward { get; private set; }

        public BondAwardedEvent(DateTime timestamp, string awardingfaction, string victimfaction, long reward) : base(timestamp, NAME)
        {
            this.awardingfaction = awardingfaction;
            this.victimfaction = victimfaction;
            this.reward = reward;
        }
    }
}
