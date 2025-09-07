using EddiDataDefinitions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace EddiConfigService.Configurations
{
    /// <summary>Storage for configuration of mission details</summary>
    [JsonObject(MemberSerialization.OptOut), RelativePath(@"\missionmonitor.json")]
    public class MissionMonitorConfiguration : Config
    {
        public List<Mission> missions { get; set; } = new List<Mission>();

        public DateTime updatedat { get; set; }

        public int goalsCount { get; set; }

        public int missionsCount { get; set; }

        public int? missionWarning { get; set; } = 60;
    }
}
