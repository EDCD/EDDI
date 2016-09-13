using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EliteDangerousEvents
{
    public class LiftoffEvent : Event
    {
        public const string NAME = "Liftoff";
        public static LiftoffEvent SAMPLE = new LiftoffEvent(DateTime.Now, -15.232M, 50.210M);

        [JsonProperty("longitude")]
        public decimal longitude { get; private set; }

        [JsonProperty("latitude")]
        public decimal latitude { get; private set; }

        public LiftoffEvent(DateTime timestamp, decimal longitude, decimal latitude) : base(timestamp, NAME)
        {
            this.longitude = longitude;
            this.latitude = latitude;
        }
    }
}
