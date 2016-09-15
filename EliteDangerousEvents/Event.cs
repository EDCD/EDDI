using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EliteDangerousEvents
{
    public class Event
    {
        [JsonIgnore]
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        [JsonProperty("timestamp")]
        public DateTime timestamp { get; private set; }

        [JsonProperty("type")]
        public string type { get; private set; }

        public Event(DateTime timestamp, string type)
        {
            this.timestamp = timestamp;
            this.type = type;
        }
    }
}
