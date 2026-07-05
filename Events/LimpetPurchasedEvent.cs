using System;
using System.Collections.Generic;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class LimpetPurchasedEvent ( DateTime timestamp, int amount, int price ) : Event( timestamp, NAME )
    {
        public const string NAME = "Limpet purchased";
        public const string DESCRIPTION = "Triggered when you buy limpets from a station";
        public const string SAMPLE = "{ \"timestamp\":\"2016-09-21T06:53:53Z\", \"event\":\"BuyDrones\", \"Type\":\"Drones\", \"Count\":19, \"BuyPrice\":101, \"TotalCost\":1919 }";

        [PublicAPI("The amount of limpets purchased")]
        public int amount { get; } = amount;

        [PublicAPI("The price paid per limpet")]
        public int price { get; } = price;

        public static bool Handle ( DateTime timestamp, string line, IDictionary<string, object> data, ref List<Event> events, bool fromLogLoad )
        {
            data.TryGetValue( "Count", out var val );
            var amount = (int)(long)val;
            data.TryGetValue( "BuyPrice", out val );
            var price = (int)(long)val;
            events.Add( new LimpetPurchasedEvent( timestamp, amount, price ) { raw = line, fromLoad = fromLogLoad } );
            return true;
        }
    }
}
