using EddiDataDefinitions;
using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class SRVDockedEvent : Event
    {
        public const string NAME = "SRV docked";
        public const string DESCRIPTION = "Triggered when you dock an SRV with your ship";
        public const string SAMPLE = @"{ ""timestamp"":""2022-11-24T23:45:10Z"", ""event"":""DockSRV"", ""SRVType"":""combat_multicrew_srv_01"", ""SRVType_Localised"":""SRV Scorpion"", ""ID"":53 }";

        [PublicAPI("The srv's id")]
        public int? id { get; private set; }

        [PublicAPI("The localized SRV type")]
        public string srvType => vehicleDefinition?.localizedName;

        [PublicAPI("The invariant SRV type")]
        public string srvTypeInvariant => vehicleDefinition?.invariantName;

        // Not intended to be public facing at this time
        public VehicleDefinition vehicleDefinition { get; private set; }

        public SRVDockedEvent(DateTime timestamp, VehicleDefinition vehicleDefinition, int? id) : base(timestamp, NAME)
        {
            this.vehicleDefinition = vehicleDefinition;
            this.id = id;
        }
    }
}
