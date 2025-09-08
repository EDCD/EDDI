using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Utilities;

namespace EddiStarMapService
{
    public partial class StarMapService
    {
        public async Task<List<StarMapResponseLogEntry>> getStarMapLogAsync ( DateTime? since = null, ulong[] systemAddresses = null )
        {
            if ( !TrySetEdsmCredentials() ) { return new List<StarMapResponseLogEntry>(); }

            var url = $"{baseUrl}api-logs-v1/get-logs";
            var parameters = new Dictionary<string, string>
            {
                { "apiKey", apiKey }, 
                { "commanderName", commanderName }, 
                { "showId", 1.ToString() }
            };
            if ( systemAddresses?.Length == 1 )
            {
                    // When a single system name is provided, the api responds with 
                    // the complete flight logs for that star system
                    parameters.Add( "systemId64", systemAddresses[ 0 ].ToString() );
            }
            else
            {
                if ( since.HasValue && since != DateTime.MinValue )
                {
                    parameters.Add( "startdatetime", Dates.FromDateTimeToString( since.Value ) );
                }
                else
                {
                    // Though not documented in the api, Anthor from EDSM has confirmed that this 
                    // unpublished parameter is valid and overrides "startdatetime" and "enddatetime".
                    parameters.Add( "fullSync", 1.ToString() );
                }
            }

            using ( var httpContent = new FormUrlEncodedContent( parameters ) )
            {
                var responseJson = await edsmHttpClient.PostAsync( url, httpContent );
                var response = responseJson is null
                    ? null
                    : JsonConvert.DeserializeObject<StarMapLogResponse>( responseJson );
                if ( response != null )
                {
                    Logging.Debug( "Response for star map logs is: ", response );

                    if ( response.msgnum != 100 )
                    {
                        // An error occurred
                        throw new EDSMException( response.msg );
                    }

                    if ( response.logs == null )
                    {
                        return null;
                    }

                    if ( systemAddresses?.Length > 0 )
                    {
                        response.logs.RemoveAll( s => !systemAddresses.Contains( s.systemId64 ) );
                    }

                    return response.logs;
                }

                Logging.Debug( "No response received." );
                throw new EDSMException( "No response received." ); // not for localization
            }
        }
    }
}