using EddiStarMapService;
using JetBrains.Annotations;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Utilities;

namespace Tests
{
    internal class FakeEdsmHttpClient : IEdsmHttpClient
    {
        private readonly Dictionary<string, string> CannedContent = new Dictionary<string, string>();

        public Task<string> GetAsync ( string url )
        {
            return ExecuteAsync( url );
        }

        public Task<string> PostAsync ( string url, HttpContent sendContent )
        {
            return ExecuteAsync( url, sendContent );
        }

        private async Task<string> ExecuteAsync( string url, HttpContent sendContent = null )
        {
            var resourceString = $"{url}";

            // this will throw if given a resource not in the canned dictionaries: that's OK
            if ( sendContent != null )
            {
                try
                {
                    var json = await sendContent.ReadAsStringAsync().ConfigureAwait(false);
                    var parameters = JsonConvert.DeserializeObject<Dictionary<string, object>>( json );
                    foreach ( var parameter in parameters )
                    {
                        resourceString += $"{(resourceString.Contains("?") ? "&" : "?")}{parameter.Key}={parameter.Value}";
                    }
                }
                catch ( JsonException jex )
                {
                    Logging.Error( jex.Message, jex );
                    throw;
                }
            }

            var match = CannedContent.FirstOrDefault(kvp =>
                string.Equals(kvp.Key, resourceString, StringComparison.OrdinalIgnoreCase));
            if ( match.Value == null )
            {
                Logging.Debug( $"No canned response found for: {resourceString}" );
                throw new KeyNotFoundException( resourceString );
            }

            return match.Value;

        }

        [UsedImplicitly]
        public void Expect(string resource, string content)
        {
            CannedContent[resource] = content;
        }
    }
}