using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EliteDangerousEvents
{
    public class TouchdownEvent : Event
    {
        public const string NAME = "Touchdown";
        public static TouchdownEvent SAMPLE = new TouchdownEvent(DateTime.Now, -15.232M, 50.210M);

        [JsonProperty("longitude")]
        public decimal longitude { get; private set; }

        [JsonProperty("latitude")]
        public decimal latitude { get; private set; }

        public TouchdownEvent(DateTime timestamp, decimal longitude, decimal latitude) : base(timestamp, NAME)
        {
            this.longitude = longitude;
            this.latitude = latitude;
        }
    }
}
