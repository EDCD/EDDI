using EddiDataDefinitions;
using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class CarrierPadsLockedEvent : Event
    {
        public const string NAME = "Carrier pads locked";
        public const string DESCRIPTION = "Triggered when your fleet carrier locks landing pads prior to jumping";
        public static CarrierPadsLockedEvent SAMPLE = new(DateTime.UtcNow, 3700357376, StationModel.FleetCarrier );

        [PublicAPI( "The carrier's numeric ID" )]
        public long carrierID { get; private set; }

        [PublicAPI( "The carrier type (e.g. Fleet Carrier or Squadron Carrier), as an object with 'localizedName' and 'invariantName' properties" )]
        public StationModel carrierType { get; private set; }

        public CarrierPadsLockedEvent ( DateTime timestamp, long carrierId, StationModel carrierType ) : base(timestamp, NAME)
        {
            this.carrierID = carrierId;
            this.carrierType = carrierType;
        }
    }
}