using EliteDangerousDataDefinitions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EliteDangerousEvents
{
    public class HeatWarningEvent : Event
    {
        public const string NAME = "Heat warning";
        public const string DESCRIPTION = "Triggered when your ship's heat exceeds 100%";
        public static HeatWarningEvent SAMPLE = new HeatWarningEvent(DateTime.Now);
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static HeatWarningEvent()
        {
            SAMPLE.raw = "{\"timestamp\":\"2016-09-25T12:00:23Z\",\"event\":\"HeatWarning\"}";
        }

        public HeatWarningEvent(DateTime timestamp) : base(timestamp, NAME)
        {
        }
    }
}
