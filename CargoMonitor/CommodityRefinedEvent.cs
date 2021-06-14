using EddiDataDefinitions;
using EddiEvents;
using System;
using Utilities;

namespace EddiCargoMonitor
{
    [PublicAPI]
    public class CommodityRefinedEvent : Event
    {
        public const string NAME = "Commodity refined";
        public const string DESCRIPTION = "Triggered when you refine a commodity from the refinery";
        public static string SAMPLE = "{ \"timestamp\":\"2016-09-30T18:00:22Z\", \"event\":\"MiningRefined\", \"Type\":\"$hydrogenperoxide_name;\", \"Type_Localised\":\"Hydrogen Peroxide\" }";

        [PublicAPI("The name of the refined commodity")]
        public string commodity => commodityDefinition?.localizedName ?? "unknown commodity";

        // Not intended to be user facing

        public CommodityDefinition commodityDefinition { get; }

        public CommodityRefinedEvent(DateTime timestamp, CommodityDefinition commodity) : base(timestamp, NAME)
        {
            this.commodityDefinition = commodity;
        }
    }
}
