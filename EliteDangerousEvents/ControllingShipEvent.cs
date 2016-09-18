using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EliteDangerousEvents
{
    public class ControllingShipEvent : Event
    {
        public const string NAME = "Controlling ship";
        public const string DESCRIPTION = "Triggered when you switch control from your fighter to your ship";
        public static ControllingShipEvent SAMPLE = new ControllingShipEvent(DateTime.Now);
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static ControllingShipEvent()
        {
            SAMPLE.raw = "{\"timestamp\":\"2016-07-22T10:53:19Z\",\"event\":\"VehicleSwitch\",\"To\":\"Mothership\"}";
        }

        public ControllingShipEvent(DateTime timestamp) : base(timestamp, NAME)
        {
        }
    }
}
