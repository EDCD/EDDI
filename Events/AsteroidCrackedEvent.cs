using System;
using System.Collections.Generic;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class AsteroidCrackedEvent ( DateTime timestamp, string bodyName ) : Event( timestamp, NAME )
    {
        public const string NAME = "Asteroid cracked";
        public const string DESCRIPTION = "Triggered when you break up a 'Motherlode' asteroid for mining";
        public const string SAMPLE = "{ \"timestamp\":\"2020-05-12T17:10:21Z\", \"event\":\"AsteroidCracked\", \"Body\":\"Corona Austr. Dark Region OX-U b2-3 6 A Ring\" }";

        [PublicAPI("The name of the nearest body (normally the ring where the asteroid was found)")]
        public string bodyname { get; private set; } = bodyName;

        public static bool Handle ( DateTime timestamp, string line, IDictionary<string, object> data, ref List<Event> events, bool fromLogLoad )
        {
            if ( fromLogLoad ) { return true; } // Skip handling this during log loading

            var bodyName = JsonParsing.getString(data, "Body");
            events.Add( new AsteroidCrackedEvent( timestamp, bodyName ) { raw = line, fromLoad = fromLogLoad } );
            return true;
        }
    }
}