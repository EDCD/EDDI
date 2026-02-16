using JetBrains.Annotations;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Utilities;

namespace EddiDataDefinitions
{
    public class ModuleInfo
    {
        [JsonProperty]
        public DateTime timestamp { get; }

        [JsonProperty]
        public List<ModuleInfoItem> Modules { get; } 

        public ModuleInfo(DateTime timestamp, List<ModuleInfoItem> modules)
        {
            this.timestamp = timestamp;
            Modules = modules ?? new List<ModuleInfoItem>();
        }

        [UsedImplicitly]
        public static bool TryFromFile (
            DateTime journalTimeStamp,
            [CanBeNull] out ModuleInfo info, [CanBeNull] out string rawModules, 
            string filename = "ModulesInfo.json" )
        {
            info = null;
            rawModules = null;

            var (raw, parsed) = Files.FromSavedGamesAsync(
                filename,
                extract: json =>
                {
                    var o = JsonConvert.DeserializeObject<ModuleInfo>( json );
                    return (o?.timestamp, o);
                },
                compareTo: journalTimeStamp
            ).GetResultOrTimeout( TimeSpan.FromSeconds( 5 ) );

            if ( parsed?.Modules != null )
            {
                return false;
            }

            info = parsed;
            rawModules = raw;
            return true;
        }
    }
}
