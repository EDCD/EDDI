using EddiDataDefinitions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace EddiConfigService
{
    /// <summary>Storage for configuration of criminal record details</summary>
    [JsonObject(MemberSerialization.OptOut), RelativePath(@"\crimemonitor.json")]
    public class CrimeMonitorConfiguration : Config
    {
        public ObservableCollection<FactionRecord> criminalrecord { get; set; } = new ObservableCollection<FactionRecord>();
        
        public Dictionary<string, string> homeSystems { get; set; } = new Dictionary<string, string>();

        public long claims { get; set; }
        
        public long fines { get; set; }
        
        public long bounties { get; set; }
        
        public string targetSystem { get; set; }
        
        public DateTime updatedat { get; set; }
    }
}