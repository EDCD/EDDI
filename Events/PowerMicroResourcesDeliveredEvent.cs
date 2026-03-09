using EddiDataDefinitions;
using System;
using System.Collections.Generic;
using System.Linq;
using Utilities;

namespace EddiEvents
{
    [ PublicAPI ]
    public class PowerMicroResourcesDeliveredEvent : Event
    {
        public const string NAME = "Power micro resources delivered";
        public const string DESCRIPTION = "Triggered when delivering micro resources to a Powerplay contact";
        public static readonly string[] SAMPLES =
        [
            @"{ ""timestamp"":""2024-10-19T15:01:28Z"", ""event"":""DeliverPowerMicroResources"", ""TotalCount"":2, ""MicroResources"":[ { ""Name"":""powerelectronics"", ""Name_Localised"":""Electronics Package"", ""Category"":""Item"", ""Count"":2 } ], ""MarketID"":3223182848 }"
        ];

        [PublicAPI( "A list of delivered micro resources with name, category, and amount for each" )]
        public List<MicroResourceAmount> resourceamounts { get; }

        [PublicAPI( "The total count of micro resources delivered" )]
        public int totalamount => resourceamounts.Sum( r => r.amount );

        // Not intended to be user facing

        public long marketId { get; private set; }

        public PowerMicroResourcesDeliveredEvent ( DateTime timestamp, long marketId, List<MicroResourceAmount> resourceAmounts ) : base(timestamp, NAME)
        {
            this.marketId = marketId;
            this.resourceamounts = resourceAmounts ?? [ ];
        }
    }
}
