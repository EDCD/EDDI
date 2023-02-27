using Newtonsoft.Json;
using System;

namespace EddiConfigService.Configurations
{
    /// <summary>Storage of credentials for a single Elite: Dangerous user to access Inara</summary>
    [JsonObject(MemberSerialization.OptOut), RelativePath(@"\inara.json")]
    public class InaraConfiguration : Config
    {
        [JsonProperty("apiKey")]
        public string apiKey { get; set; }

        [JsonProperty("commanderName")]
        public string commanderName { get; set; }

        [JsonProperty("commanderFrontierID")]
        public string commanderFrontierID { get; set; }

        [JsonProperty("lastSync")]
        public DateTime lastSync { get; set; }

        [JsonProperty("isAPIkeyValid")]
        public bool isAPIkeyValid { get; set; } = true;
    }
}
