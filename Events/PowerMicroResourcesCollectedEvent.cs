using EddiDataDefinitions;
using System;
using System.Collections.Generic;
using System.Linq;
using Utilities;

namespace EddiEvents
{
    [ PublicAPI ]
    public class PowerMicroResourcesCollectedEvent : Event
    {
        public const string NAME = "Power micro resources collected";
        public const string DESCRIPTION = "Triggered when collecting micro resources from a Powerplay contact";
        public static readonly string[] SAMPLES =
        [
            @"{""timestamp"":""2024-10-23T19:59:28Z"",""event"":""RequestPowerMicroResources"",""TotalCount"":3,""MicroResources"":[{""Name"":""powerspyware"",""Name_Localised"":""Power Tracker Malware"",""Category"":""Data"",""Count"":3}],""MarketID"":3930400257}"
        ];

        [PublicAPI( "A list of collected micro resources with name, category, and amount for each" )]
        public List<MicroResourceAmount> resourceamounts { get; }

        [PublicAPI( "The total count of micro resources collected" )]
        public int totalamount => resourceamounts.Sum( r => r.amount );

        // Not intended to be user facing

        public long marketId { get; private set; } // Tourist beacons and guardian structures are reported as settlements without MarketID 

        public PowerMicroResourcesCollectedEvent ( DateTime timestamp, long marketId, List<MicroResourceAmount> resourceAmounts ) : base(timestamp, NAME)
        {
            this.marketId = marketId;
            this.resourceamounts = resourceAmounts ?? [ ];
        }
    }
}
