using System;
using System.Collections.Generic;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class ShutdownEvent ( DateTime timestamp ) : Event( timestamp, NAME )
    {
        public const string NAME = "Shutdown";
        public const string DESCRIPTION = "Triggered on a clean shutdown of the game";
        public const string SAMPLE = "{ \"timestamp\":\"2018-02-05T05:41:51Z\", \"event\":\"Shutdown\" }";

        public static bool Handle ( DateTime timestamp, string line, ref List<Event> events, bool fromLogLoad )
        {
            if ( fromLogLoad ) { return true; } // Skip handling this during log loading
            events.Add( new ShutdownEvent( timestamp ) { raw = line, fromLoad = false } );
            return true;
        }
    }
}
