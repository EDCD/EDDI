using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EliteDangerousEvents
{
    public class CockpitBreachedEvent : Event
    {
        public const string NAME = "Cockpit breached";
        public const string DESCRIPTION = "Triggered when your ship's cockpit is broken";
        public static CockpitBreachedEvent SAMPLE = new CockpitBreachedEvent(DateTime.Now);
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static CockpitBreachedEvent()
        {
            SAMPLE.raw = "{\"timestamp\":\"2016-06-10T14:32:03Z\",\"event\":\"CockpitBreached\"}";
        }

        public CockpitBreachedEvent(DateTime timestamp) : base(timestamp, NAME)
        {
        }
    }
}
