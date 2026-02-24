using System;
using System.Collections.Generic;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class SelfDestructEvent : Event
    {
        public const string NAME = "Self destruct";
        public const string DESCRIPTION = "Triggered when you start the self destruct sequence";
        public const string SAMPLE = "{\"timestamp\":\"2016-07-22T10:53:19Z\",\"event\":\"SelfDestruct\"}";

        public SelfDestructEvent(DateTime timestamp) : base(timestamp, NAME)
        { }

        public static bool Handle ( DateTime timestamp, string line, ref List<Event> events, bool fromLogLoad )
        {
            events.Add( new SelfDestructEvent( timestamp ) { raw = line, fromLoad = fromLogLoad } );
            return true;
        }
    }
}
