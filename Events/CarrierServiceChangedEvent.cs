using System;
using EddiDataDefinitions;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class CarrierServiceChangedEvent : Event
    {
        public const string NAME = "Carrier service changed";
        public const string DESCRIPTION = "Triggered when you change the services available at your fleet carrier";
        public const string SAMPLE = "{ \"timestamp\":\"2020-03-17T12:38:54Z\", \"event\":\"CarrierCrewServices\", \"CarrierID\":3700005632, \"CrewRole\":\"Outfitting\", \"Operation\":\"Activate\", \"CrewName\":\"Eugene Johnson\" }";

        [PublicAPI("The type of service change (One of 'Activate', 'Deactivate', 'Pause', 'Resume', or 'Replace')")]
        public string operation { get; private set; }

        [PublicAPI("The service being changed")]
        public string service => Service.localizedName;

        [PublicAPI("The invariant name of the service being changed")]
        public string invariantService => Service.invariantName;

        [PublicAPI("The crew member responsible for the service")]
        public string crew { get; private set; }

        // Not intended to be user facing
        public long? carrierID { get; private set; }

        public StationService Service { get; private set; }

        public CarrierServiceChangedEvent(DateTime timestamp, long? carrierId, string operation, StationService crewRole, string crewName) : base(timestamp, NAME)
        {
            carrierID = carrierId;
            this.operation = operation;
            Service = crewRole;
            crew = crewName;
        }
    }
}