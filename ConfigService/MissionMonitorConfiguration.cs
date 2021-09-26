using EddiDataDefinitions;
using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;

namespace EddiConfigService
{
    /// <summary>Storage for configuration of mission details</summary>
    [JsonObject(MemberSerialization.OptOut), RelativePath(@"\missionmonitor.json")]
    public class MissionMonitorConfiguration : Config
    {
        public ObservableCollection<Mission> missions { get; set; } = new ObservableCollection<Mission>();

        public DateTime updatedat { get; set; }

        public int goalsCount { get; set; }

        public int missionsCount { get; set; }

        public int? missionWarning { get; set; } = 60;
    }
}
