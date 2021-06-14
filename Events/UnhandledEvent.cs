using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class UnhandledEvent : Event
    {
        public const string NAME = "Unhandled event";
        public const string DESCRIPTION = "Triggered when EDDI encounters an event that we don't otherwise handle";
        public const string SAMPLE = null;

        public string edType { get; }

        public UnhandledEvent(DateTime timestamp, string type) : base(timestamp, NAME)
        {
            this.edType = type;
        }
    }
}
