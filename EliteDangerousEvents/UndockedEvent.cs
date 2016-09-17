using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EliteDangerousEvents
{
    public class UndockedEvent : Event
    {
        public const string NAME = "Undocked";
        public const string DESCRIPTION = "Triggered when your ship undocks from a station or outpost";
        public static UndockedEvent SAMPLE = new UndockedEvent(DateTime.Now, "Long Sight Base");
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static UndockedEvent()
        {
            SAMPLE.raw = "{\"timestamp\":\"2016-06-10T14:32:03Z\",\"event\":\"Undocked\",\"StationName\":\"Long Sight Base\"}";

            VARIABLES.Add("station", "The station from which the commander has undocked");
        }

        [JsonProperty("station")]
        public string station { get; private set; }

        public UndockedEvent(DateTime timestamp, string station) : base(timestamp, NAME)
        {
            this.station = station;
        }
    }
}
