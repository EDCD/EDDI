using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace EddiEvents
{
    public class UnderAttackEvent : Event
    {
        public const string NAME = "Under attack";
        public const string DESCRIPTION = "Triggered when under fire (same time as the Under Attack voice message)";
        public const string SAMPLE = "{ \"timestamp\":\"2018-01-31T06:52:53Z\", \"event\":\"UnderAttack\", \"Target\":\"You\" }";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static UnderAttackEvent()
        {
            VARIABLES.Add("target", "The target of the attack (either 'Fighter', 'Mothership', or 'You')");
        }

        [JsonProperty("target")]
        public string target { get; private set; }

        public UnderAttackEvent(DateTime timestamp, string target) : base(timestamp, NAME)
        {
            this.target = target;
        }
    }
}
