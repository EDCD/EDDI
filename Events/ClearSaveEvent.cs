using System;
using System.Collections.Generic;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class ClearedSaveEvent ( DateTime timestamp, string name, string frontierId ) : Event( timestamp, NAME )
    {
        public const string NAME = "Cleared save";
        public const string DESCRIPTION = "Triggered when you clear your save";
        public const string SAMPLE = "{\"timestamp\":\"2016-06-10T14:32:03Z\",\"event\":\"ClearSavedGame\",\"FID\":\"F0000000\",\"Name\":\"HRC1\"}";

        [PublicAPI("The name of the player whose save has been cleared")]
        public string name { get; private set; } = name;

        // Not intended to be user facing
        
        public string frontierID { get; private set; } = frontierId;

        public static bool Handle ( DateTime timestamp, string line, IDictionary<string, object> data, ref List<Event> events, bool fromLogLoad )
        {
            var name = JsonParsing.getString(data, "Name");
            var frontierID = JsonParsing.getString(data, "FID");
            events.Add( new ClearedSaveEvent( timestamp, name, frontierID ) { raw = line, fromLoad = fromLogLoad } );
            return true;
        }
    }
}
