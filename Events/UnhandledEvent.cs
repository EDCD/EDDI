using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class UnhandledEvent ( DateTime timestamp, string type ) : Event( timestamp, NAME )
    {
        public const string NAME = "Unhandled event";
        public const string DESCRIPTION = "Triggered when EDDI encounters an event that we don't otherwise handle";
        public const string SAMPLE = null;

        public string edType { get; } = type;
    }
}
