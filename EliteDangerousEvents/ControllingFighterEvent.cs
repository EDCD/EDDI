using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EliteDangerousEvents
{
    public class ControllingFighterEvent : Event
    {
        public const string NAME = "Controlling fighter";
        public const string DESCRIPTION = "Triggered when you switch control from your ship to your fighter";
        public static ControllingFighterEvent SAMPLE = new ControllingFighterEvent(DateTime.Now);
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static ControllingFighterEvent()
        {
            SAMPLE.raw = "{\"timestamp\":\"2016-07-22T10:53:19Z\",\"event\":\"VehicleSwitch\",\"To\":\"Fighter\"}";
        }

        public ControllingFighterEvent(DateTime timestamp) : base(timestamp, NAME)
        {
        }
    }
}
