using EddiDataDefinitions;
using System;
using System.Collections.Generic;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class CommodityPurchasedEvent (
        DateTime timestamp,
        long marketid,
        CommodityDefinition commodity,
        int amount,
        int price )
        : Event( timestamp, NAME )
    {
        public const string NAME = "Commodity purchased";
        public const string DESCRIPTION = "Triggered when you buy a commodity from the markets";
        public const string SAMPLE = "{ \"timestamp\":\"2018-04-07T16:29:39Z\", \"event\":\"MarketBuy\", \"MarketID\":3224801280, \"Type\":\"coffee\", \"Count\":1, \"BuyPrice\":1198, \"TotalCost\":1198 }";

        [PublicAPI("The market ID of the purchased commodity")]
        public long marketid { get; } = marketid;

        [PublicAPI("The name of the purchased commodity")]
        public string commodity => commodityDefinition?.localizedName ?? "unknown commodity";

        [PublicAPI("The amount of the purchased commodity")]
        public int amount { get; } = amount;

        [PublicAPI("The price paid per unit of the purchased commodity")]
        public int price { get; } = price;

        // Not intended to be user facing

        public CommodityDefinition commodityDefinition { get; } = commodity;

        public static bool Handle ( DateTime timestamp, string line, IDictionary<string, object> data, ref List<Event> events, bool fromLogLoad )
        {
            var marketId = JsonParsing.getLong(data, "MarketID");
            var commodityName = JsonParsing.getString(data, "Type");
            var commodity = CommodityDefinition.FromEDName(commodityName);
            if ( commodity == null )
            {
                Logging.Error( "Failed to map cargo type " + commodityName + " to commodity definition", line );
            }
            var amount = JsonParsing.getInt(data, "Count");
            var price = JsonParsing.getInt(data, "BuyPrice");
            events.Add( new CommodityPurchasedEvent( timestamp, marketId, commodity, amount, price ) { raw = line, fromLoad = fromLogLoad } );
            return true;
        }
    }
}
