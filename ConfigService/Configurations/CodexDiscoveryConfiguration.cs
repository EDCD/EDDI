using Newtonsoft.Json;
using System.Collections.Generic;

namespace EddiConfigService.Configurations
{
    /// <summary>Configuration for codex entry discoveries</summary>
    [JsonObject(MemberSerialization.OptOut), RelativePath(@"\codexdiscoveries.json")]
    public class CodexDiscoveryConfiguration : Config
    {
        private HashSet<long> _discoveredEntryIds = [];

        [JsonProperty("discoveredEntryIds")]
        public HashSet<long> discoveredEntryIds
        {
            get => _discoveredEntryIds ?? [];
            set
            {
                if (value == _discoveredEntryIds)
                {
                    return;
                }

                _discoveredEntryIds = value;
                OnPropertyChanged();
            }
        }
    }
}
