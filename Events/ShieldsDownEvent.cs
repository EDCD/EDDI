using System;
using System.Collections.Generic;

namespace EddiEvents
{
    public class ShieldsDownEvent : Event
    {
        public const string NAME = "Shields down";
        public const string DESCRIPTION = "Triggered when your ship's shields go offline";
        public const string SAMPLE = "{\"timestamp\":\"2016-07-22T10:53:19Z\",\"event\":\"ShieldState\",\"ShieldsUp\":false}";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static ShieldsDownEvent()
        {
        }

        public ShieldsDownEvent(DateTime timestamp) : base(timestamp, NAME)
        {
        }
    }
}
