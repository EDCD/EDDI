using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EliteDangerousEvents
{
    public class SelfDestructEvent : Event
    {
        public const string NAME = "Self destruct";
        public const string DESCRIPTION = "Triggered when you start the self destruct sequence";
        public static SelfDestructEvent SAMPLE = new SelfDestructEvent(DateTime.Now);
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static SelfDestructEvent()
        {
            SAMPLE.raw = "{\"timestamp\":\"2016-07-22T10:53:19Z\",\"event\":\"SelfDestruct\"}";
        }

        public SelfDestructEvent(DateTime timestamp) : base(timestamp, NAME)
        {
        }
    }
}
