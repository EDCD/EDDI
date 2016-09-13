using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EliteDangerousEvents
{
    public class DockedEvent : Event
    {
        public const string NAME = "Docked";
        public static DockedEvent SAMPLE = new DockedEvent(DateTime.Now, "Jameson Memorial");

        [JsonProperty("station")]
        public string station { get; private set; }

        public DockedEvent(DateTime timestamp, string station) : base(timestamp, NAME)
        {
            this.station = station;
        }
    }
}
