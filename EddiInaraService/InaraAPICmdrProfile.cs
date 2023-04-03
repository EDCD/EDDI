using EddiConfigService;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EddiInaraService
{
    public partial class InaraService
    {
        // Documentation: https://inara.cz/inara-api-docs/

        // Returns basic information about commander from Inara like ranks, 
        // squadron, a link to the commander's Inara profile, etc. 
        // If no cmdrName is given then the profile of the commander current user is returned.

        public InaraCmdr GetCommanderProfile(string cmdrName = null)
        {
            if (currentGameVersion != null && currentGameVersion < minGameVersion) { return null; }

            return string.IsNullOrEmpty(cmdrName) 
                ? GetCommanderProfiles( null )?.FirstOrDefault() 
                : GetCommanderProfiles( new[] { cmdrName } )?.FirstOrDefault();
        }

        public List<InaraCmdr> GetCommanderProfiles(IEnumerable<string> cmdrNames)
        {
            var cmdrs = new List<InaraCmdr>();

            var events = new List<InaraAPIEvent>();
            if ( cmdrNames is null )
            {
                events.Add( new InaraAPIEvent( DateTime.UtcNow, "getCommanderProfile", new Dictionary<string, object>() ) );
            }
            else
            {
                foreach ( string cmdrName in cmdrNames )
                {
                    events.Add( new InaraAPIEvent( DateTime.UtcNow, "getCommanderProfile", new Dictionary<string, object>()
                    {
                        { "searchName", cmdrName }
                    } ) );
                }
            }
            var responses = SendEventBatch(events, ConfigService.Instance.inaraConfiguration);
            foreach (var inaraResponse in responses)
            {
                var jsonCmdr = JsonConvert.SerializeObject(inaraResponse.eventData);
                cmdrs.Add(JsonConvert.DeserializeObject<InaraCmdr>(jsonCmdr));
            }
            return cmdrs;
        }
    }
}
