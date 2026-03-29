using EddiDataDefinitions;
using System;
using System.Collections.Generic;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class MaterialDiscoveredEvent ( DateTime timestamp, Material material ) : Event( timestamp, NAME )
    {
        public const string NAME = "Material discovered";
        public const string DESCRIPTION = "Triggered when you discover a material";
        public const string SAMPLE = "{ \"timestamp\":\"2016-09-21T14:07:19Z\", \"event\":\"MaterialDiscovered\", \"Category\":\"Raw\", \"Name\":\"iron\", \"DiscoveryNumber\":3 }";

        [PublicAPI("The name of the discovered material")]
        public string name { get; private set; } = material?.localizedName;

        public static bool Handle ( DateTime timestamp, string line, IDictionary<string, object> data, ref List<Event> events, bool fromLogLoad )
        {
            if ( fromLogLoad ) { return true; } // Skip handling this during log loading
            var material = Material.FromEDName(JsonParsing.getString(data, "Name"));
            events.Add( new MaterialDiscoveredEvent( timestamp, material ) { raw = line, fromLoad = false } );
            return true;
        }
    }
}
