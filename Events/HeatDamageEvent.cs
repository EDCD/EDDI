using System;
using System.Collections.Generic;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class HeatDamageEvent ( DateTime timestamp ) : Event( timestamp, NAME )
    {
        public const string NAME = "Heat damage";
        public const string DESCRIPTION = "Triggered when you begin to take hull damage due to excessive heat.";
        public const string SAMPLE = "{\"timestamp\":\"2016-09-25T12:00:23Z\",\"event\":\"HeatDamage\"}";

        public static bool Handle ( DateTime timestamp, string line, ref List<Event> events, bool fromLogLoad )
        {
            if ( fromLogLoad ) { return true; } // Skip handling this during log loading
            events.Add( new HeatDamageEvent( timestamp ) { raw = line, fromLoad = false } );
            return true;
        }
    }
}
