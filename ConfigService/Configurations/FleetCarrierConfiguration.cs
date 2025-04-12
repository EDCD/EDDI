using EddiDataDefinitions;
using Newtonsoft.Json;

namespace EddiConfigService.Configurations
{
    /// <summary>Configuration for the Fleet Carrier monitor</summary>
    [JsonObject(MemberSerialization.OptOut), RelativePath(@"\fleetcarrier.json")]
    public class FleetCarrierConfiguration : Config
    {
        public FleetCarrier fleetCarrier { get; set; }
    }
}
