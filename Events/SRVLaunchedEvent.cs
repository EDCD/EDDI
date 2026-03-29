using EddiDataDefinitions;
using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class SRVLaunchedEvent (
        DateTime timestamp,
        string loadout,
        bool playercontrolled,
        VehicleDefinition vehicleDefinition,
        int? id )
        : Event( timestamp, NAME )
    {
        public const string NAME = "SRV launched";
        public const string DESCRIPTION = "Triggered when you launch an SRV from your ship";
        public const string SAMPLE = @"{ ""timestamp"":""2022-11-24T23:44:25Z"", ""event"":""LaunchSRV"", ""SRVType"":""combat_multicrew_srv_01"", ""SRVType_Localised"":""SRV Scorpion"", ""Loadout"":""default"", ""ID"":53, ""PlayerControlled"":true }";

        [PublicAPI("The SRV's loadout")]
        public string loadout { get; private set; } = loadout;

        [PublicAPI("True if the SRV is controlled by the player")]
        public bool playercontrolled { get; private set; } = playercontrolled;

        [PublicAPI("The vehicle ID assigned to the SRV")]
        public int? id { get; private set; } = id;

        [PublicAPI("The localized SRV type")]
        public string srvType => vehicleDefinition?.localizedName;

        [PublicAPI("The invariant SRV type")]
        public string srvTypeInvariant => vehicleDefinition?.invariantName;

        // Not intended to be public facing at this time
        public VehicleDefinition vehicleDefinition { get; private set; } = vehicleDefinition;
    }
}
