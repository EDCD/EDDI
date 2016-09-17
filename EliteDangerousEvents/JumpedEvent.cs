using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EliteDangerousEvents
{
    public class JumpedEvent : Event
    {
        public const string NAME = "Jumped";
        public const string DESCRIPTION = "Triggered when you jump from one system to another";
        public static JumpedEvent SAMPLE = new JumpedEvent(DateTime.Now, "Shinrarta Dezhra", 55.71875M, 17.59375M, 27.15625M, "Federation", "The Pilot's Federation", "Boom", "High Technology", "Corporate", "High");
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static JumpedEvent()
        {
            VARIABLES.Add("system", "The name of the system to which the commander has jumped");
            VARIABLES.Add("x", "The X co-ordinate of the system to which the commander has jumped");
            VARIABLES.Add("y", "The Y co-ordinate of the system to which the commander has jumped");
            VARIABLES.Add("z", "The Z co-ordinate of the system to which the commander has jumped");
            VARIABLES.Add("allegiance", "The allegiance of the system to which the commander has jumped");
            VARIABLES.Add("faction", "The faction controlling the system to which the commander has jumped");
            VARIABLES.Add("factionstate", "The state of the faction controlling the system to which the commander has jumped");
            VARIABLES.Add("economy", "The economy of the system to which the commander has jumped");
            VARIABLES.Add("government", "The government of the system to which the commander has jumped");
            VARIABLES.Add("security", "The security of the system to which the commander has jumped");
        }

        [JsonProperty("system")]
        public string system { get; private set; }

        [JsonProperty("x")]
        public decimal x { get; private set; }

        [JsonProperty("y")]
        public decimal y { get; private set; }

        [JsonProperty("z")]
        public decimal z { get; private set; }

        [JsonProperty("allegiance")]
        public string allegiance { get; private set; }

        [JsonProperty("faction")]
        public string faction { get; private set; }

        [JsonProperty("economy")]
        public string economy { get; private set; }

        [JsonProperty("government")]
        public string government { get; private set; }

        [JsonProperty("security")]
        public string security { get; private set; }

        public JumpedEvent(DateTime timestamp, string system, decimal x, decimal y, decimal z, string allegiance, string faction, string factionstate, string economy, string government, string security) : base(timestamp, NAME)
        {
            this.system = system;
            this.x = x;
            this.y = y;
            this.z = z;
            this.allegiance = allegiance;
            this.faction = faction;
            this.economy = economy;
            this.government = government;
            this.security = security;
        }
    }
}
