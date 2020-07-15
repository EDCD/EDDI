using EddiDataDefinitions;
using EddiEvents;
using System;
using System.Collections.Generic;
using Utilities;

namespace EddiCargoMonitor
{
    public class CommodityRefinedEvent : Event
    {
        public const string NAME = "Commodity refined";
        public const string DESCRIPTION = "Triggered when you refine a commodity from the refinery";
        public static string SAMPLE = "{ \"timestamp\":\"2016-09-30T18:00:22Z\", \"event\":\"MiningRefined\", \"Type\":\"$hydrogenperoxide_name;\", \"Type_Localised\":\"Hydrogen Peroxide\" }";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static CommodityRefinedEvent()
        {
            VARIABLES.Add("commodity", "The name of the commodity refined");
        }

        public string commodity => commodityDefinition?.localizedName ?? "unknown commodity";

        // Not intended to be user facing

        [VoiceAttackIgnore]
        public CommodityDefinition commodityDefinition { get; }

        public CommodityRefinedEvent(DateTime timestamp, CommodityDefinition commodity) : base(timestamp, NAME)
        {
            this.commodityDefinition = commodity;
        }
    }
}
