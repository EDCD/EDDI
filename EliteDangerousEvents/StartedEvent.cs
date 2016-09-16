using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EliteDangerousEvents
{
    public class StartedEvent : Event
    {
        public const string NAME = "Started";
        public const string DESCRIPTION = "Triggered when Elite: Dangerous starts";
        public static StartedEvent SAMPLE = new StartedEvent(DateTime.Now);
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        public StartedEvent(DateTime timestamp) : base(timestamp, NAME)
        {
        }
    }
}
