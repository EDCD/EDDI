using EddiDataDefinitions;
using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class SearchAndRescueEvent (
        DateTime timestamp,
        CommodityDefinition commodity,
        int? amount,
        long reward,
        long marketId )
        : Event( timestamp, NAME )
    {
        public const string NAME = "Search and rescue";
        public const string DESCRIPTION = "Triggered when delivering items to a Search and Rescue contact";
        public const string SAMPLE = "{ \"timestamp\":\"2017-08-26T01:58:24Z\", \"event\":\"SearchAndRescue\", \"MarketID\": 128666762, \"Name\":\"occupiedcryopod\", \"Count\":2, \"Reward\":5310 }";

        [PublicAPI("The amount of the item recovered")]
        public int? amount { get; } = amount;

        [PublicAPI("The monetary reward for completing the search and rescue")]
        public long reward { get; } = reward;

        [PublicAPI("The localized name of the commodity recovered")]
        public string localizedcommodityname => commodity.localizedName;

        [PublicAPI("The commodity (object) recovered")]
        public CommodityDefinition commodity { get; } = commodity;

        // Not intended to be user facing

        public long marketId { get; private set; } = marketId;
    }
}
