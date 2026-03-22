using EddiDataDefinitions;
using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class CarrierServiceChangedEvent (
        DateTime timestamp,
        long carrierId,
        StationModel carrierType,
        string operation,
        StationService crewRole,
        string crewName )
        : Event( timestamp, NAME )
    {
        public const string NAME = "Carrier service changed";
        public const string DESCRIPTION = "Triggered when you change the services available at your fleet carrier";
        public const string SAMPLE = "{ \"timestamp\":\"2020-03-17T12:38:54Z\", \"event\":\"CarrierCrewServices\", \"CarrierID\":3700005632, \"CrewRole\":\"Outfitting\", \"Operation\":\"Activate\", \"CrewName\":\"Eugene Johnson\" }";

        [PublicAPI("The type of service change (One of 'Activate', 'Deactivate', 'Pause', 'Resume', or 'Replace')")]
        public string operation { get; private set; } = operation;

        [PublicAPI("The service being changed")]
        public string service => Service.localizedName;

        [PublicAPI("The invariant name of the service being changed")]
        public string invariantService => Service.invariantName;

        [PublicAPI("The crew member responsible for the service")]
        public string crew { get; private set; } = crewName;

        // Carrier variables

        [PublicAPI( "The carrier's numeric ID" )]
        public long carrierID { get; private set; } = carrierId;

        [PublicAPI( "The carrier type (e.g. Fleet Carrier or Squadron Carrier), as an object with 'localizedName' and 'invariantName' properties" )]
        public StationModel carrierType { get; private set; } = carrierType;

        // Not intended to be user facing

        public StationService Service { get; private set; } = crewRole;
    }
}