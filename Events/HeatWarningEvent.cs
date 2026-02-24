using System;
using System.Collections.Generic;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class HeatWarningEvent : Event
    {
        public const string NAME = "Heat warning";
        public const string DESCRIPTION = "Triggered when your ship's heat exceeds 100% and you begin to take module damage.";
        public const string SAMPLE = "{\"timestamp\":\"2016-09-25T12:00:23Z\",\"event\":\"HeatWarning\"}";

        public HeatWarningEvent(DateTime timestamp) : base(timestamp, NAME)
        { }

        public static bool Handle ( DateTime timestamp, string line, ref List<Event> events, bool fromLogLoad )
        {
            if ( fromLogLoad ) { return true; } // Skip handling this during log loading
            events.Add( new HeatWarningEvent( timestamp ) { raw = line, fromLoad = false } );
            return true;
        }
    }
}
