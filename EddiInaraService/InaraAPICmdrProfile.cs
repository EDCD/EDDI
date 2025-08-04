using EddiConfigService;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Utilities;

namespace EddiInaraService
{
    public partial class InaraService
    {
        // Documentation: https://inara.cz/inara-api-docs/

        // Returns basic information about commander from Inara like ranks, 
        // squadron, a link to the commander's Inara profile, etc. 
        // If no cmdrName is given then the profile of the commander current user is returned.

        public async Task<InaraCmdr> GetCommanderProfileAsync ( string cmdrName = null )
        {
            if ( string.IsNullOrEmpty( cmdrName ) )
            {
                var result = await GetCommanderProfilesAsync( null ).ConfigureAwait( false );
                return result?.FirstOrDefault();
            }
            else
            {
                var result = await GetCommanderProfilesAsync( new List<string> { cmdrName } ).ConfigureAwait( false );
                return result?.FirstOrDefault();
            }
        }

        public async Task<List<InaraCmdr>> GetCommanderProfilesAsync(IList<string> cmdrNames)
        {
            var cmdrs = new List<InaraCmdr>();
            var events = new List<InaraAPIEvent>();
            if ( cmdrNames is null || !cmdrNames.Any() )
            {
                if ( !string.IsNullOrEmpty(ConfigService.Instance.inaraConfiguration.apiKey) )
                {
                    events.Add( new InaraAPIEvent( DateTime.UtcNow, "getCommanderProfile", new Dictionary<string, object>() ) );
                }
                else
                {
                    // We cannot default to personal Inara Commander details when no name is given unless
                    // we've configured a personal API key.
                    Logging.Warn("Inara API Error: Please enter your API key if you wish to enable looking up your own profile on Inara.");
                }
            }
            else
            {
                foreach ( string cmdrName in cmdrNames )
                {
                    if ( string.IsNullOrEmpty( cmdrName ) ) { continue; }
                    events.Add( new InaraAPIEvent( DateTime.UtcNow, "getCommanderProfile",
                        new Dictionary<string, object> { { "searchName", cmdrName } } ) );
                }
            }

            var responses = await SendEventBatchAsync( events, ConfigService.Instance.inaraConfiguration ).ConfigureAwait( false );
            foreach (var inaraResponse in responses)
            {
                var jsonCmdr = JsonConvert.SerializeObject(inaraResponse.eventData);
                cmdrs.Add(JsonConvert.DeserializeObject<InaraCmdr>(jsonCmdr));
            }
            return cmdrs;
        }
    }
}
