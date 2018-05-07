using EddiEvents;
using System;
using System.Collections.Generic;

namespace EddiCargoMonitor
{
    public class LimpetLaunchedEvent : Event
    {
        public const string NAME = "Limpet launched";
        public const string DESCRIPTION = "Triggered launch a limpet from your ship";
        public const string SAMPLE = "{ \"timestamp\":\"2018-04-07T20:05:07Z\", \"event\":\"LaunchDrone\", \"Type\":\"Collection\" }";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static LimpetLaunchedEvent()
        {
            VARIABLES.Add("kind", "The kind of limpet launched");
        }

        public string kind { get; }

        public LimpetLaunchedEvent(DateTime timestamp, string kind) : base(timestamp, NAME)
        {
            this.kind = kind;
        }
    }
}
