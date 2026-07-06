using EddiDataDefinitions;
using System;
using System.Collections.Generic;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class CommoditySoldEvent (
        DateTime timestamp,
        long marketid,
        CommodityDefinition commodity,
        int amount,
        long price,
        long profit,
        bool illegal,
        bool stolen,
        bool blackmarket )
        : Event( timestamp, NAME )
    {
        public const string NAME = "Commodity sold";
        public const string DESCRIPTION = "Triggered when you sell a commodity to the markets";
        public static readonly string[] SAMPLES =
        [
            "{ \"timestamp\":\"2018-04-07T16:29:44Z\", \"event\":\"MarketSell\", \"MarketID\":3224801280, \"Type\":\"coffee\", \"Count\":1, \"SellPrice\":1138, \"TotalSale\":1138, \"AvgPricePaid\":1198 }",
            "{ \"timestamp\":\"2024-08-14T22:46:27Z\", \"event\":\"MarketSell\", \"MarketID\":3221497856, \"Type\":\"autofabricators\", \"Type_Localised\":\"Auto-Fabricators\", \"Count\":139, \"SellPrice\":26782, \"TotalSale\":3722698, \"AvgPricePaid\":3838 }"
        ];

        [PublicAPI("The market ID of the commodity sold")]
        public long marketid { get; } = marketid;

        [PublicAPI ("The name of the commodity sold")]
        public string commodity => commodityDefinition?.localizedName ?? CommodityDefinition.Unknown.localizedName;

        [PublicAPI("The amount of the commodity sold")]
        public int amount { get; } = amount;

        [PublicAPI("The price obtained per unit of the commodity sold")]
        public long price { get; } = price;

        [PublicAPI("The number of credits profit per unit of the commodity sold")]
        public long profit { get; } = profit;

        [PublicAPI("True if the commodity is illegal at the place of sale")]
        public bool illegal { get; } = illegal;

        [PublicAPI("True if the commodity was stolen")]
        public bool stolen { get; } = stolen;

        [PublicAPI("True if the commodity was sold to a black market")]
        public bool blackmarket { get; } = blackmarket;

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
            var sellPrice = JsonParsing.getLong(data, "SellPrice");

            var buyPrice = JsonParsing.getLong(data, "AvgPricePaid");
            // We don't care about buy price, we care about profit per unit
            var profit = sellPrice - buyPrice;

            var illegal = JsonParsing.getOptionalBool(data, "IllegalGoods") ?? false;
            var stolen = JsonParsing.getOptionalBool(data, "StolenGoods") ?? false;
            var blackmarket = JsonParsing.getOptionalBool(data, "BlackMarket") ?? false;

            events.Add( new CommoditySoldEvent( timestamp, marketId, commodity, amount, sellPrice, profit, illegal, stolen, blackmarket ) { raw = line, fromLoad = fromLogLoad } );
            return true;
        }
    }
}
