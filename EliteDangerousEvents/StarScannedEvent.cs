using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EliteDangerousEvents
{
    public class StarScannedEvent : Event
    {
        public const string NAME = "Star scanned";
        public static StarScannedEvent SAMPLE = new StarScannedEvent(DateTime.Now, "Sol", "G", 1M, 1M, 4.83M);

        [JsonProperty("name")]
        public string name { get; private set; }

        [JsonProperty("stellarclass")]
        public string stellarclass { get; private set; }

        [JsonProperty("mass")]
        public decimal mass{ get; private set; }

        [JsonProperty("radius")]
        public decimal radius { get; private set; }

        [JsonProperty("absolutemagnitude")]
        public decimal absolutemagnitude { get; private set; }

        public StarScannedEvent(DateTime timestamp, string name, string stellarclass, decimal mass, decimal radius, decimal absolutemagnitude) : base(timestamp, NAME)
        {
            this.name = name;
            this.stellarclass = stellarclass;
            this.mass = mass;
            this.radius = radius;
            this.absolutemagnitude = absolutemagnitude;
        }
    }
}
