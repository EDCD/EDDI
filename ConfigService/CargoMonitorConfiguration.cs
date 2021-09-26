using EddiDataDefinitions;
using System;
using System.Collections.ObjectModel;
using Newtonsoft.Json;

namespace EddiConfigService
{
    /// <summary>Storage for configuration of cargo details</summary>
    [JsonObject(MemberSerialization.OptOut), RelativePath(@"\cargomonitor.json")]
    public class CargoMonitorConfiguration : Config
    {
        public ObservableCollection<Cargo> cargo { get; set; } = new ObservableCollection<Cargo>();

        public int cargocarried { get; set; }

        public DateTime updatedat { get; set; }
    }
}