using System;
using System.Collections.Generic;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class ShipShutdownEvent ( DateTime timestamp ) : Event( timestamp, NAME )
    {
        public const string NAME = "Ship shutdown";
        public const string DESCRIPTION = "Triggered when your ship's system is forced to shutdown";
        public const string SAMPLE = @"{ ""timestamp"":""2017-01-05T23:15:06Z"", ""event"":""SystemsShutdown"" }";

        [PublicAPI( "True if shutdown is momentary, with flickering power which does not fully disable the ship" )]
        public bool partialshutdown { get; set; }

        public static bool Handle ( DateTime timestamp, string line, ref List<Event> events, bool fromLogLoad )
        {
            if ( fromLogLoad ) { return true; } // Skip handling this during log loading

            events.Add( new ShipShutdownEvent( timestamp ) { raw = line, fromLoad = fromLogLoad } );

            return true;
        }
    }
}
