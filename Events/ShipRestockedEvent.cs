using System;
using System.Collections.Generic;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class ShipRestockedEvent ( DateTime timestamp, long price ) : Event( timestamp, NAME )
    {
        public const string NAME = "Ship restocked";
        public const string DESCRIPTION = "Triggered when you restock your ship's ammunition";
        public const string SAMPLE = "{ \"timestamp\":\"2016-09-20T11:13:00Z\", \"event\":\"BuyAmmo\", \"Cost\":36001 }";

        [PublicAPI("The price of restocking")]
        public long price { get; private set; } = price;

        public static bool Handle ( DateTime timestamp, string line, IDictionary<string, object> data, ref List<Event> events, bool fromLogLoad )
        {
            if ( fromLogLoad ) { return true; } // Skip handling this during log loading

            data.TryGetValue( "Cost", out var val );
            var price = (long)val;
            events.Add( new ShipRestockedEvent( timestamp, price ) { raw = line, fromLoad = fromLogLoad } );
            return true;
        }
    }
}
