using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EliteDangerousEvents
{
    public class ShieldsDownEvent : Event
    {
        public const string NAME = "Shields down";
        public const string DESCRIPTION = "Triggered when your ship's shields go offline";
        public static ShieldsDownEvent SAMPLE = new ShieldsDownEvent(DateTime.Now);
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static ShieldsDownEvent()
        {
            SAMPLE.raw = "{\"timestamp\":\"2016-07-22T10:53:19Z\",\"event\":\"ShieldState\",\"ShieldsUp\":false}";
        }

        public ShieldsDownEvent(DateTime timestamp) : base(timestamp, NAME)
        {
        }
    }
}
