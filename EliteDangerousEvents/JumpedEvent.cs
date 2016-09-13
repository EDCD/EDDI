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
        public static JumpedEvent SAMPLE = new JumpedEvent(DateTime.Now, "Shinrarta Dezhra", 55.71875M, 17.59375M, 27.15625M);

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
