using EddiDataDefinitions;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace EddiConfigService.Configurations
{
    /// <summary>Configuration for the EDDP monitor</summary>
    [JsonObject(MemberSerialization.OptOut), RelativePath(@"\eddpmonitor.json")]
    public class EddpConfiguration : Config
    {
        public List<BgsWatch> watches { get; private set; } = new List<BgsWatch>();
    }
}
