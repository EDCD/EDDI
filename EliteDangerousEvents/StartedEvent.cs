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
        public static StartedEvent SAMPLE = new StartedEvent(DateTime.Now);

        public StartedEvent(DateTime timestamp) : base(timestamp, NAME)
        {
        }
    }
}
