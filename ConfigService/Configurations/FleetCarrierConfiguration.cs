using EddiDataDefinitions;
using Newtonsoft.Json;

namespace EddiConfigService.Configurations
{
    /// <summary>Configuration for the Fleet Carrier monitor</summary>
    [JsonObject(MemberSerialization.OptOut), RelativePath(@"\fleetcarrier.json")]
    public class FleetCarrierConfiguration : Config
    {
        private FleetCarrier _fleetCarrier;
        private FleetCarrier _squadronCarrier;

        public FleetCarrier fleetCarrier
        {
            get => _fleetCarrier;
            set
            {
                if ( Equals( value, _fleetCarrier ) )
                {
                    return;
                }

                _fleetCarrier = value;
                OnPropertyChanged();
            }
        }

        public FleetCarrier squadronCarrier
        {
            get => _squadronCarrier;
            set
            {
                if ( Equals( value, _squadronCarrier ) )
                {
                    return;
                }

                _squadronCarrier = value;
                OnPropertyChanged();
            }
        }
    }
}
