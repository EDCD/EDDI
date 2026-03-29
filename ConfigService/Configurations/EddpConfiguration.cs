using EddiDataDefinitions;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace EddiConfigService.Configurations
{
    /// <summary>Configuration for the EDDP monitor</summary>
    [JsonObject(MemberSerialization.OptOut), RelativePath(@"\eddpmonitor.json")]
    public class EddpConfiguration : Config
    {
        private List<BgsWatch> _watches = [];

        public List<BgsWatch> watches
        {
            get => _watches;
            private set
            {
                if ( Equals( value, _watches ) )
                {
                    return;
                }

                _watches = value;
                OnPropertyChanged();
            }
        }
    }
}
