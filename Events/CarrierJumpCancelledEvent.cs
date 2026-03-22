using EddiDataDefinitions;
using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class CarrierJumpCancelledEvent ( DateTime timestamp, long carrierId, StationModel carrierType )
        : Event( timestamp, NAME )
    {
        public const string NAME = "Carrier jump cancelled";
        public const string DESCRIPTION = "Triggered when you cancel a scheduled fleet carrier jump";
        public const string SAMPLE = "{ \"timestamp\":\"2020-05-11T18:53:47Z\", \"event\":\"CarrierJumpCancelled\", \"CarrierID\":3700357376 }";

        [PublicAPI( "The carrier's numeric ID" )]
        public long carrierID { get; private set; } = carrierId;

        [PublicAPI( "The carrier type (e.g. Fleet Carrier or Squadron Carrier), as an object with 'localizedName' and 'invariantName' properties" )]
        public StationModel carrierType { get; private set; } = carrierType;
    }
}