using EliteDangerousDataDefinitions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EliteDangerousEvents
{
    public class HeatDamageEvent : Event
    {
        public const string NAME = "Heat damage";
        public const string DESCRIPTION = "Triggered when your ship is taking damage from excessive heat";
        public static HeatDamageEvent SAMPLE = new HeatDamageEvent(DateTime.Now);
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static HeatDamageEvent()
        {
            SAMPLE.raw = "{\"timestamp\":\"2016-09-25T12:00:23Z\",\"event\":\"HeatDamage\"}";
        }

        public HeatDamageEvent(DateTime timestamp) : base(timestamp, NAME)
        {
        }
    }
}
