using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class SafeEvent ( DateTime timestamp ) : Event( timestamp, NAME )
    {
        public const string NAME = "Safe";
        public const string DESCRIPTION = "Triggered when you are no longer in danger";
        public const string SAMPLE = null;
    }
}
