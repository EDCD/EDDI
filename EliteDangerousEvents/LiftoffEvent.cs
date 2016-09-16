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
        public const string DESCRIPTION = "Triggered when your ship lifts off from a planet's surface";
        public static LiftoffEvent SAMPLE = new LiftoffEvent(DateTime.Now, -15.232M, 50.210M);
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static LiftoffEvent()
        {
            VARIABLES.Add("longitude", "The longitude from where the commander has lifted off");
            VARIABLES.Add("latitude", "The latitude from where the commander has lifted off");
        }

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
