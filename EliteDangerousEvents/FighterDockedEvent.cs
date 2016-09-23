using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EliteDangerousEvents
{
    public class FighterDockedEvent : Event
    {
        public const string NAME = "Fighter docked";
        public const string DESCRIPTION = "Triggered when you dock a fighter with your ship";
        public const string SAMPLE = "{\"timestamp\":\"2016-06-10T14:32:03Z\",\"event\":\"DockFighter\"}";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static FighterDockedEvent()
        {
        }

        public FighterDockedEvent(DateTime timestamp) : base(timestamp, NAME)
        {
        }
    }
}
