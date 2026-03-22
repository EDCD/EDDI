using EddiDataDefinitions;
using System;
using System.Collections.Generic;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class MaterialDiscardedEvent ( DateTime timestamp, Material material, int amount ) : Event( timestamp, NAME )
    {
        public const string NAME = "Material discarded";
        public const string DESCRIPTION = "Triggered when you discard a material";
        public const string SAMPLE = "{\"timestamp\":\"2016-06-10T14:32:03Z\",\"event\":\"MaterialDiscarded\",\"Category\":\"Encoded\",\"Name\":\"shieldcyclerecordings\", \"Count\":3}";

        [PublicAPI("The name of the discarded material")]
        public string name { get; private set; } = material?.localizedName;

        [PublicAPI("The amount of the discarded material")]
        public int amount { get; private set; } = amount;

        [PublicAPI( "The total amount of the discarded material remaining in your inventory" )]
        public int total { get; set; }

        // Not intended to be user facing

        public string edname { get; private set; } = material?.edname;

        public static bool Handle ( DateTime timestamp, string line, IDictionary<string, object> data, ref List<Event> events, bool fromLogLoad )
        {
            if ( fromLogLoad ) { return true; } // Skip handling this during log loading

            var material = Material.FromEDName(JsonParsing.getString(data, "Name"));
            var amount = JsonParsing.getInt( data, "Count" );
            events.Add( new MaterialDiscardedEvent( timestamp, material, amount ) { raw = line, fromLoad = false } );
            return true;
        }
    }
}
