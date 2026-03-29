using EddiDataDefinitions;
using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class CarrierDecommissionCancelledEvent ( DateTime timestamp, long carrierId, StationModel carrierType )
        : Event( timestamp, NAME )
    {
        public const string NAME = "Carrier decommission cancelled";
        public const string DESCRIPTION = "Triggered when you cancel the decommissioning of your fleet carrier";
        public const string SAMPLE = "{ \"timestamp\":\"2020-03-11T15:12:38Z\", \"event\":\"CarrierCancelDecommission\", \"CarrierID\":3700005632 }";

        [PublicAPI( "The carrier's numeric ID" )]
        public long carrierID { get; private set; } = carrierId;

        [PublicAPI( "The carrier type (e.g. Fleet Carrier or Squadron Carrier), as an object with 'localizedName' and 'invariantName' properties" )]
        public StationModel carrierType { get; private set; } = carrierType;
    }
}