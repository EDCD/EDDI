using Newtonsoft.Json;

namespace EddiConfigService.Configurations
{
    /// <summary>Configuration for the Galnet monitor</summary>
    [JsonObject(MemberSerialization.OptOut), RelativePath(@"\galnetmonitor.json")]
    public class GalnetConfiguration : Config
    {
        public string lastuuid { get; set; }

        public string language { get; set; } = "English";

        public bool galnetAlwaysOn { get; set; } = false;
    }
}
