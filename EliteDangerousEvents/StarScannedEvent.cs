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
        public const string DESCRIPTION = "Triggered when you complete a scan of a stellar body";
        public static StarScannedEvent SAMPLE = new StarScannedEvent(DateTime.Now, "Alnitak", "O", 26.621094M, 2305180672M, -5.027969M);
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static StarScannedEvent()
        {
            SAMPLE.raw = "{\"timestamp\":\"2016-07-25T10:02:38Z\",\"event\":\"Scan\",\"BodyName\":\"Alnitak\",\"DistanceFromArrivalLS\":0.000000,\"StarType\":\"O\",\"StellarMass\":26.621094,\"Radius\":2305180672.000000,\"AbsoluteMagnitude\":-5.027969,\"OrbitalPeriod\":5755731.500000,\"RotationPeriod\":90114.937500}";

            VARIABLES.Add("name", "The name of the star that has been scanned");
            VARIABLES.Add("stellarclass", "The stellar class of the star that has been scanned (O, G, etc)");
            VARIABLES.Add("mass", "The mass of the star that has been scanned, relative to Sol's mass");
            VARIABLES.Add("radius", "The radius of the star that has been scanned, in metres");
            VARIABLES.Add("absolutemagnitude", "The absolute magnitude of the star that has been scanned");
        }

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
