using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Utilities;

namespace EddiStarMapService
{
    public partial class StarMapService
    {
        public async Task sendStarMapCommentAsync ( ulong systemAddress, string comment )
        {
            if ( !TrySetEdsmCredentials() ) { return; }

            var url = $"{baseUrl}api-logs-v1/set-comment";
            var parameters = new Dictionary<string, string>
            {
                { "apiKey", apiKey },
                { "commanderName", commanderName },
                { "systemId64", systemAddress.ToString() },
                { "comment", comment }
            };
            var httpContent = new FormUrlEncodedContent( parameters );
            var responseJson = await edsmHttpClient.PostAsync(url, httpContent).ConfigureAwait(false);
            var response = responseJson is null ? null : JsonConvert.DeserializeObject<StarMapLogResponse>( responseJson );
            if ( response?.msgnum != 100 )
            {
                Logging.Warn( "EDSM responded with " + response?.msg );
            }
        }

        public async Task<Dictionary<string, string>> getStarMapCommentsAsync ()
        {
            if ( !TrySetEdsmCredentials() ) { return new Dictionary<string, string>(); }

            var url = $"{baseUrl}api-logs-v1/get-comments";
            var parameters = new Dictionary<string, string>
            {
                { "apiKey", apiKey },
                { "commanderName", commanderName }
            };
            var httpContent = new FormUrlEncodedContent( parameters );
            var responseJson = await edsmHttpClient.PostAsync(url, httpContent).ConfigureAwait(false);
            var response = responseJson is null ? null : JsonConvert.DeserializeObject<StarMapCommentResponse>( responseJson );
            if ( response?.msgnum != 100 )
            {
                Logging.Warn( "EDSM responded with " + response?.msg );
            }

            var vals = new Dictionary<string, string>();
            if ( response?.comments != null )
            {
                foreach ( var entry in response.comments )
                {
                    if ( !string.IsNullOrEmpty( entry.comment ) )
                    {
                        Logging.Debug( "Comment found for " + entry.system );
                        vals[ entry.system ] = entry.comment;
                    }
                }
            }
            return vals;
        }

        // response from the Star Map comment API
        [JsonObject]
        class StarMapCommentResponse
        {
            public int msgnum { get; set; }
            public string msg { get; set; }
            public List<StarMapResponseCommentEntry> comments { get; set; }
        }

        [JsonObject]
        class StarMapResponseCommentEntry
        {
            public string system { get; set; }
            public string comment { get; set; }
            public DateTime? lastUpdate { get; set; }
        }
    }
}