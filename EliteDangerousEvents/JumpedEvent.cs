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
        public static JumpedEvent SAMPLE = new JumpedEvent(DateTime.Now, "Shinrarta Dezhra", 55.71875M, 17.59375M, 27.15625M);
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static JumpedEvent()
        {
            VARIABLES.Add("system", "The name of the system to which the commander has jumped");
            VARIABLES.Add("x", "The X co-ordinate of the system to which the commander has jumped");
            VARIABLES.Add("y", "The Y co-ordinate of the system to which the commander has jumped");
            VARIABLES.Add("z", "The Z co-ordinate of the system to which the commander has jumped");
        }

        [JsonProperty("system")]
        public string system { get; private set; }

        [JsonProperty("x")]
        public decimal x { get; private set; }

        [JsonProperty("y")]
        public decimal y { get; private set; }

        [JsonProperty("z")]
        public decimal z { get; private set; }

        public JumpedEvent(DateTime timestamp, string system, decimal x, decimal y, decimal z) : base(timestamp, NAME)
        {
            this.system = system;
            this.x = x;
            this.y = y;
            this.z = z;
        }
    }
}
