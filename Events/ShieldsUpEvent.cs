using System;
using System.Collections.Generic;

namespace EddiEvents
{
    public class ShieldsUpEvent : Event
    {
        public const string NAME = "Shields up";
        public const string DESCRIPTION = "Triggered when your ship's shields come online";
        public const string SAMPLE = "{\"timestamp\":\"2016-07-22T10:53:19Z\",\"event\":\"ShieldState\",\"ShieldsUp\":true}";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static ShieldsUpEvent()
        {
        }

        public ShieldsUpEvent(DateTime timestamp) : base(timestamp, NAME)
        {
        }
    }
}
