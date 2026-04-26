using JetBrains.Annotations;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Utilities;

namespace EddiDataDefinitions
{
    public class ModuleInfo ( DateTime timestamp, List<ModuleInfoItem> modules )
    {
        [JsonProperty]
        public DateTime timestamp { get; } = timestamp;

        [JsonProperty]
        public List<ModuleInfoItem> Modules { get; } = modules ?? [ ];

        [UsedImplicitly]
        public static bool TryFromFile (
            DateTime journalTimeStamp,
            [CanBeNull] out ModuleInfo info, [CanBeNull] out string rawModules, 
            string filename = "ModulesInfo.json" )
        {
            info = null;
            rawModules = null;

            var (raw, parsed, isRecent) = Files.FromSavedGamesAsync(
                filename,
                extract: json =>
                {
                    var o = JsonConvert.DeserializeObject<ModuleInfo>( json );
                    return (o?.timestamp, o);
                },
                compareTo: journalTimeStamp
            ).GetResultOrTimeout( TimeSpan.FromSeconds( 5 ) );

            if ( isRecent && parsed?.Modules != null )
            {
                info = parsed;
                rawModules = raw;
                return true;
            }

            return false;
        }
    }
}
