using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class ShieldsUpEvent ( DateTime timestamp ) : Event( timestamp, NAME )
    {
        public const string NAME = "Shields up";
        public const string DESCRIPTION = "Triggered when your ship's shields come online";
        public const string SAMPLE = "{\"timestamp\":\"2016-07-22T10:53:19Z\",\"event\":\"ShieldState\",\"ShieldsUp\":true}";
    }
}
