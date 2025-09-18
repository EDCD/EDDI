using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Utilities;

namespace EddiSpanshService
{
    public partial class SpanshService
    {
        public enum QueryGroup
        {
            bodies, 
            stations, 
            systems
        }

        public async Task<JToken> QueryAsync ( QueryGroup queryGroup, [ NotNull ] Dictionary<string, object> searchFilters, CancellationToken cancellationToken, int? maxResults = 500, int? pageId = 0 )
        {
            var retryCount = 0;
            while ( retryCount < MaxRetries )
            {
                try
                {
                    string responseJson;
                    using ( var requestContent = GetRequestContent( searchFilters, maxResults, pageId ) )
                    {
                        var response = await spanshHttpClient.PostAsync( $"{queryGroup}/search", requestContent, cancellationToken );
                        response.EnsureSuccessStatusCode();
                        responseJson = await response.Content.ReadAsStringAsync();

                        if ( string.IsNullOrEmpty( responseJson ) )
                        {
                            Logging.Warn( "Spansh API returned no result" );
                            return null;
                        }
                    }

                    try
                    {
                        var jResponse = JToken.Parse( responseJson );
                        if ( jResponse.Contains( "error" ) )
                        {
                            Logging.Debug( $"Spansh responded with: {jResponse[ "error" ]}" );
                        }
                        else
                        {
                            return jResponse;
                        }
                    }
                    catch ( Exception e )
                    {
                        Logging.Error( "Failed to parse Spansh response", e );
                    }
                }
                catch ( HttpRequestException he )
                {
                    // Increment retry count and apply back-off
                    retryCount++;
                    if ( retryCount < MaxRetries )
                    {
                        var backoff = InitialBackoffMilliseconds * (int)Math.Pow(2, retryCount - 1); // Exponential back-off
                        await Task.Delay( backoff, cancellationToken );
                    }
                    else
                    {
                        Logging.Error( he.Message, he );
                    }
                }
            }

            return null;
        }

        public async Task<JToken> DistanceOrderedQueryAsync ( QueryGroup queryGroup, decimal fromX, decimal fromY, decimal fromZ, [ NotNull ] Dictionary<string, object> searchFilters, CancellationToken cancellationToken )
        {
            var retryCount = 0;
            while ( retryCount < MaxRetries )
            {
                try
                {
                    string responseJson;
                    using ( var requestContent = GetDistanceOrderedRequestContent( fromX, fromY, fromZ, searchFilters ) )
                    {
                        var response = await spanshHttpClient.PostAsync( $"{queryGroup}/search", requestContent, cancellationToken );
                        response.EnsureSuccessStatusCode();
                        responseJson = await response.Content.ReadAsStringAsync();
                    }

                    try
                    {
                        var jResponse = JToken.Parse( responseJson );
                        if ( jResponse.Contains( "error" ) )
                        {
                            Logging.Debug( $"Spansh responded with: {jResponse[ "error" ]}" );
                        }
                        else
                        {
                            return jResponse;
                        }
                    }
                    catch ( Exception e )
                    {
                        Logging.Error( "Failed to parse Spansh response", e );
                    }
                }
                catch ( HttpRequestException he )
                {
                    // Increment retry count and apply back-off
                    retryCount++;
                    if ( retryCount < MaxRetries )
                    {
                        var backoff = InitialBackoffMilliseconds * (int)Math.Pow(2, retryCount - 1); // Exponential back-off
                        await Task.Delay( backoff, cancellationToken );
                    }
                    else
                    {
                        Logging.Error( he.Message, he );
                    }
                }
            }

            return null;
        }

        // TODO: Handle multi-page responses?
        private static StringContent GetRequestContent ( [NotNull] Dictionary<string, object> filter, int? maxResults, int? pageId )
        {
            var jsonObject = new
            {
                filters = filter,
                size = maxResults,
                page = pageId
            };

            return new StringContent( JsonConvert.SerializeObject( jsonObject ), Encoding.UTF8, "application/json" );
        }

        private static StringContent GetDistanceOrderedRequestContent ( decimal fromX, decimal fromY, decimal fromZ, [NotNull] Dictionary<string, object> filter )
        {
            var jsonObject = new
            {
                filters = filter,
                sort = new List<object>
                {
                    new { distance = new { direction = "asc" } },
                    new { distance_to_arrival = new { direction = "asc" } }
                },
                size = 10,
                page = 0,
                reference_coords = new Dictionary<string, object>
                {
                    { "x", fromX }, { "y", fromY }, { "z", fromZ }
                }
            };

            return new StringContent( JsonConvert.SerializeObject( jsonObject ), Encoding.UTF8, "application/json" );
        }
    }
}