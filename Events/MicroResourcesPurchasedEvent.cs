using EddiDataDefinitions;
using System;
using System.Collections.Generic;
using System.Linq;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class MicroResourcesPurchasedEvent (
        DateTime timestamp,
        List<MicroResourceAmount> resourceAmounts,
        int price,
        long? marketid )
        : Event( timestamp, NAME )
    {
        public const string NAME = "Micro resources purchased";
        public const string DESCRIPTION = "Triggered when you buy micro resources";
        public static readonly string[] SAMPLES =
        [
            "{ \"timestamp\":\"2021-04-30T21:41:34Z\", \"event\":\"BuyMicroResources\", \"Name\":\"healthpack\", \"Name_Localised\":\"Medkit\", \"Category\":\"Consumable\", \"Count\":2, \"Price\":2000, \"MarketID\":3221524992 }",
            "{ \"timestamp\":\"2024-05-25T05:05:16Z\", \"event\":\"BuyMicroResources\", \"TotalCount\":10, \"MicroResources\":[ { \"Name\":\"opticalfibre\", \"Name_Localised\":\"Optical Fibre\", \"Category\":\"Component\", \"Count\":10 } ], \"Price\":6000, \"MarketID\":3707594240 }"
        ];

        [PublicAPI( "A list of purchased micro resources with name, category, and amount for each" )]
        public List<MicroResourceAmount> resourceamounts { get; } = resourceAmounts ?? [ ];

        [ PublicAPI( "The total count of micro resources purchased" ) ]
        public int totalamount => resourceamounts.Sum( r => r.amount );

        [ PublicAPI( "The total price paid for all micro resources" ) ]
        public int price { get; } = price;

        // Not intended to be user facing

        public long? marketid { get; } = marketid;
    }
}
