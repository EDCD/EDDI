using EddiDataDefinitions;
using System;
using System.Collections.Generic;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class CommodityRefinedEvent ( DateTime timestamp, CommodityDefinition commodity ) : Event( timestamp, NAME )
    {
        public const string NAME = "Commodity refined";
        public const string DESCRIPTION = "Triggered when you refine a commodity from the refinery";
        public static readonly string SAMPLE = "{ \"timestamp\":\"2016-09-30T18:00:22Z\", \"event\":\"MiningRefined\", \"Type\":\"$hydrogenperoxide_name;\", \"Type_Localised\":\"Hydrogen Peroxide\" }";

        [PublicAPI("The name of the refined commodity")]
        public string commodity => commodityDefinition?.localizedName ?? "unknown commodity";

        // Not intended to be user facing

        public CommodityDefinition commodityDefinition { get; } = commodity;

        public static bool Handle ( DateTime timestamp, string line, IDictionary<string, object> data, ref List<Event> events, bool fromLogLoad )
        {
            var commodityName = JsonParsing.getString(data, "Type");
            var commodity = CommodityDefinition.FromEDName(commodityName);
            if ( commodity == null )
            {
                Logging.Error( "Failed to map cargo type " + commodityName + " to commodity definition", line );
            }
            events.Add( new CommodityRefinedEvent( timestamp, commodity ) { raw = line, fromLoad = fromLogLoad } );
            return true;
        }
    }
}
