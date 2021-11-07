using EddiDataDefinitions;
using Newtonsoft.Json;
using System.Collections.ObjectModel;

namespace EddiConfigService
{
    /// <summary>Storage for configuration of material amounts</summary>
    [JsonObject(MemberSerialization.OptOut), RelativePath(@"\materialmonitor.json")]
    public class MaterialMonitorConfiguration : Config
    {
        public ObservableCollection<MaterialAmount> materials { get; set; } = new ObservableCollection<MaterialAmount>();
    }
}
