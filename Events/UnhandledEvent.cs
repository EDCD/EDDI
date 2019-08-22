using System;
using System.Collections.Generic;

namespace EddiEvents
{
    public class UnhandledEvent : Event
    {
        public const string NAME = "Unhandled event";
        public const string DESCRIPTION = "Triggered when EDDI encounters an event that we don't otherwise handle";
        public const string SAMPLE = null;
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static UnhandledEvent()
        {
        }

        public string edType { get; private set; }

        public UnhandledEvent(DateTime timestamp, string type) : base(timestamp, NAME)
        {
            this.edType = type;
        }
    }
}
