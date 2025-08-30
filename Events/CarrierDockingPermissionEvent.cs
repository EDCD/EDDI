using EddiDataDefinitions;
using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class CarrierDockingPermissionEvent : Event
    {
        public const string NAME = "Carrier docking permission";
        public const string DESCRIPTION = "Triggered when changing the docking permission criteria at your fleet carrier";
        public const string SAMPLE = "{ \"timestamp\":\"2020-03-11T15:07:25Z\", \"event\":\"CarrierDockingPermission\", \"CarrierID\":3700005632, \"DockingAccess\":\"squadronfriends\", \"AllowNotorious\":true }";

        [PublicAPI("The carrier's current docking access (one of one of 'all', 'squadronfriends', 'friends', or 'none')")]
        public string dockingAccess { get; private set; }

        [PublicAPI("True if docking access may be granted to notorious commanders")]
        public bool allowNotorious { get; private set; }

        [PublicAPI( "The carrier's numeric ID" )]
        public long carrierID { get; private set; }

        [PublicAPI( "The carrier type (e.g. Fleet Carrier or Squadron Carrier), as an object with 'localizedName' and 'invariantName' properties" )]
        public StationModel carrierType { get; private set; }

        public CarrierDockingPermissionEvent ( DateTime timestamp, long carrierId, StationModel carrierType,
            string dockingAccess, bool allowNotorious ) : base(timestamp, NAME)
        {
            this.carrierID = carrierId;
            this.carrierType = carrierType;
            this.dockingAccess = dockingAccess;
            this.allowNotorious = allowNotorious;
        }
    }
}