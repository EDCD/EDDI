using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EddiEvents
{
    public class HullDamagedEvent : Event
    {
        public const string NAME = "Hull damaged";
        public const string DESCRIPTION = "Triggered when your hull is damaged to a certain extent";
        public const string SAMPLE = "{\"timestamp\":\"2016-06-10T14:32:03Z\",\"event\":\"HullDamage\",\"Health\":0.805}";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static HullDamagedEvent()
        {
            VARIABLES.Add("health", "The percentage health of the hull");
        }

        [JsonProperty("health")]
        public decimal health { get; private set; }

        public HullDamagedEvent(DateTime timestamp, decimal health) : base(timestamp, NAME)
        {
            this.health = health;
        }
    }
}
