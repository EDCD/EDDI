using Newtonsoft.Json;
using System;

namespace EddiConfigService
{
    /// <summary>Storage of credentials for a single Elite: Dangerous user to access EDSM</summary>
    [JsonObject(MemberSerialization.OptOut), RelativePath(@"\edsm.json")]
    public class StarMapConfiguration : Config
    {
        [JsonProperty("apiKey")]
        public string apiKey { get; set; }
        [JsonProperty("commanderName")]
        public string commanderName { get; set; }
        [JsonProperty("lastSync")]
        public DateTime lastFlightLogSync { get; set; } = DateTime.MinValue;
        [JsonProperty("lastJournalSync")]
        public DateTime lastJournalSync { get; set; } = DateTime.MinValue;
    }
}
