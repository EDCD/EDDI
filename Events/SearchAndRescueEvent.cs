using EddiDataDefinitions;
using System;
using System.Collections.Generic;

namespace EddiEvents
{
    public class SearchAndRescueEvent: Event
    {
        public const string NAME = "Search and rescue";
        public const string DESCRIPTION = "Triggered when delivering items to a Search and Rescue contact";
        public const string SAMPLE = "{ \"timestamp\":\"2017-08-26T01:58:24Z\", \"event\":\"SearchAndRescue\", \"Name\":\"occupiedcryopod\", \"Count\":2, \"Reward\":5310 }";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static SearchAndRescueEvent()
        {
    	    VARIABLES.Add("commodity", "The name of the item recovered");
            VARIABLES.Add("amount", "The amount of the item recovered");
            VARIABLES.Add("reward", "The monetary reward for completing the search and rescue");
        }

        public string commodity => commodityDefinition?.localizedName ?? "unknown commodity";

        public int? amount { get; }

        public long reward { get; }

        public CommodityDefinition commodityDefinition { get; }

        public SearchAndRescueEvent(DateTime timestamp, CommodityDefinition commodity, int? amount, long reward) : base(timestamp, NAME)
        {
            this.amount = amount;
            this.reward = reward;
            this.commodityDefinition = commodity;
        }
    }
}
