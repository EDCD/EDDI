using EddiDataDefinitions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EddiEvents
{
    public class JumpingEvent : Event
    {
        public const string NAME = "Jumping";
        public const string DESCRIPTION = "Triggered when you start a jump to another system";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static JumpingEvent()
        {
            VARIABLES.Add("system", "The name of the system to which the commander is jumping");
            VARIABLES.Add("x", "The X co-ordinate of the system to which the commander is jumping");
            VARIABLES.Add("y", "The Y co-ordinate of the system to which the commander is jumping");
            VARIABLES.Add("z", "The Z co-ordinate of the system to which the commander is jumping");
        }

        [JsonProperty("system")]
        public string system { get; private set; }

        [JsonProperty("x")]
        public decimal x { get; private set; }

        [JsonProperty("y")]
        public decimal y { get; private set; }

        [JsonProperty("z")]
        public decimal z { get; private set; }

        public JumpingEvent(DateTime timestamp, string system, decimal x, decimal y, decimal z) : base(timestamp, NAME)
        {
            this.system = system;
            this.x = x;
            this.y = y;
            this.z = z;
        }
    }
}
