using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class LimpetSoldEvent ( DateTime timestamp, int amount, long price ) : Event( timestamp, NAME )
    {
        public const string NAME = "Limpet sold";
        public const string DESCRIPTION = "Triggered when you sell limpets to a station";
        public const string SAMPLE = "{ \"timestamp\":\"2016-09-24T00:03:25Z\", \"event\":\"SellDrones\", \"Type\":\"Drones\", \"Count\":8, \"SellPrice\":101, \"TotalSale\":808 }";

        [PublicAPI("The amount of limpets sold")]
        public int amount { get; } = amount;

        [PublicAPI("The price obtained per limpet")]
        public long price { get; } = price;
    }
}
