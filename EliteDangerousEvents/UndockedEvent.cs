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

        [JsonProperty("station")]
        public string station { get; private set; }

        public UndockedEvent(DateTime timestamp, string station) : base(timestamp, NAME)
        {
            this.station = station;
        }
    }
}
