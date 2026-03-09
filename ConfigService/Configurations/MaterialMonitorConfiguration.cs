using EddiDataDefinitions;
using Newtonsoft.Json;
using System.Collections.ObjectModel;

namespace EddiConfigService.Configurations
{
    /// <summary>Storage for configuration of material amounts</summary>
    [JsonObject(MemberSerialization.OptOut), RelativePath(@"\materialmonitor.json")]
    public class MaterialMonitorConfiguration : Config
    {
        private ObservableCollection<MaterialAmount> _materials = new();

        public ObservableCollection<MaterialAmount> materials
        {
            get => _materials;
            set
            {
                if ( Equals( value, _materials ) )
                {
                    return;
                }

                _materials = value;
                OnPropertyChanged();
            }
        }
    }
}
