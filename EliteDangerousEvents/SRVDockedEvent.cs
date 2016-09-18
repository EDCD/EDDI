using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EliteDangerousEvents
{
    public class SRVDockedEvent : Event
    {
        public const string NAME = "SRV docked";
        public const string DESCRIPTION = "Triggered when you dock an SRV with your ship";
        public static SRVDockedEvent SAMPLE = new SRVDockedEvent(DateTime.Now);
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static SRVDockedEvent()
        {
            SAMPLE.raw = "{\"timestamp\":\"2016-06-10T14:32:03Z\",\"event\":\"DockSRV\"}";
        }

        public SRVDockedEvent(DateTime timestamp) : base(timestamp, NAME)
        {
        }
    }
}
