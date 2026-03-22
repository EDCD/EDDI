using System;
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
    }
}
