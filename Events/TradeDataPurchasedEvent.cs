using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class TradeDataPurchasedEvent ( DateTime timestamp, string system, long price ) : Event( timestamp, NAME )
    {
        public const string NAME = "Trade data purchased";
        public const string DESCRIPTION = "Triggered when you purchase trade data";
        public const string SAMPLE = "{ \"timestamp\":\"2016-09-28T13:54:29Z\", \"event\":\"BuyTradeData\", \"System\":\"LFT 926\", \"Cost\":100 }";

        [PublicAPI("The system for which trade data was purchased")]
        public string system { get; private set; } = system;

        [PublicAPI("The price of the purchase")]
        public long price { get; private set; } = price;
    }
}
