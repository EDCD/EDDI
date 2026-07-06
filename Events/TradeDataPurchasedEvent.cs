using System;
using System.Collections.Generic;
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

        public static bool Handle ( DateTime timestamp, string line, IDictionary<string, object> data, ref List<Event> events, bool fromLogLoad )
        {
            if ( fromLogLoad ) { return true; } // Skip handling this during log loading

            var system = JsonParsing.getString(data, "System");
            data.TryGetValue( "Cost", out var val );
            var price = (long)val;

            events.Add( new TradeDataPurchasedEvent( timestamp, system, price ) { raw = line, fromLoad = fromLogLoad } );
            return true;
        }
    }
}
