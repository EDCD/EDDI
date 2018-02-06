using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace EddiEvents
{
    public class ShutdownEvent : Event
    {
        public const string NAME = "Shutdown";
        public const string DESCRIPTION = "Triggered on a clean shutdown of the game";
        public const string SAMPLE = "{ \"timestamp\":\"2018-02-05T05:41:51Z\", \"event\":\"Shutdown\" }";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static ShutdownEvent()
        {
        }

        public ShutdownEvent(DateTime timestamp) : base(timestamp, NAME)
        {
        }
    }
}
