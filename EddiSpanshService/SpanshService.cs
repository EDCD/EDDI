using Newtonsoft.Json.Linq;
using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Utilities;

namespace EddiSpanshService
{
    public interface ISpanshHttpClient
    {
        Task<HttpResponseMessage> GetAsync ( string requestUri, CancellationToken cancellationToken );
        Task<HttpResponseMessage> PostAsync ( string requestUri, HttpContent content, CancellationToken cancellationToken );
    }

    public partial class SpanshService
    {
        private const string baseUrl = "https://spansh.co.uk/api/";
        private readonly ISpanshHttpClient spanshHttpClient;

        // Allow injection of a fake client for testing
        public SpanshService ( ISpanshHttpClient httpClient = null )
        {
            spanshHttpClient = httpClient ?? new SpanshHttpClient( baseUrl );
        }

        // Default HttpClient‐based implementation
        private class SpanshHttpClient : ISpanshHttpClient
        {
            private readonly HttpClient client;

            // The default timeout for requests to Spansh. Requests can override this by setting `RestRequest.Timeout`. Both are in milliseconds.
            private const int DefaultTimeoutMilliseconds = 10000;

            // The number of times to retry a failed request
            private const int MaxRetries = 3;

            public SpanshHttpClient (string baseUrl)
            {
                client = new HttpClient
                {
                    BaseAddress = new Uri( baseUrl ),
                    Timeout = TimeSpan.FromMilliseconds( DefaultTimeoutMilliseconds )
                };
            }

            public async Task<HttpResponseMessage> GetAsync ( string requestUri, CancellationToken cancellationToken )
            {
                HttpResponseMessage response = null;

                for ( var retry = 0; retry < MaxRetries; retry++ )
                {
                    response = await client.GetAsync( requestUri, cancellationToken );
                    if ( response.IsSuccessStatusCode && EnsureSuccess(response) )
                    {
                        return response;
                    }

                    await Task.Delay( (int)Math.Pow( 2, retry ) * 100 );
                }

                return response;
            }

            public async Task<HttpResponseMessage> PostAsync ( string requestUri, HttpContent content, CancellationToken cancellationToken )
            {
                HttpResponseMessage response = null;

                for ( var retry = 0; retry < MaxRetries; retry++ )
                {
                    response = await client.PostAsync( requestUri, content, cancellationToken );
                    if ( response.IsSuccessStatusCode && EnsureSuccess( response ) )
                    {
                        return response;
                    }

                    await Task.Delay( (int)Math.Pow( 2, retry ) * 100 );
                }

                return response;
            }
        }

        private async Task<JToken> GetRouteResponseAsync ( string jobId, CancellationToken cancellationToken )
        {
            if ( string.IsNullOrEmpty( jobId ) ) { return null; }

            // Poll until the job finishes
            JObject routeResult = null;
            while ( routeResult is null || routeResult[ "state" ]?.ToString() == "started" )
            {
                await Task.Delay( 500 );
                var getResponse = await spanshHttpClient.GetAsync( $"results/{jobId}", cancellationToken );
                if ( getResponse.StatusCode == HttpStatusCode.RequestTimeout )
                {
                    Logging.Warn( $"Spansh API timeout on GET results/{jobId}" );
                    return null;
                }

                var getJson = await getResponse.Content.ReadAsStringAsync();
                routeResult = JObject.Parse( getJson );

                if ( routeResult[ "error" ] != null )
                {
                    Logging.Debug( routeResult[ "error" ].ToString() );
                    return null;
                }
            }

            return routeResult[ "result" ];
        }

        private static bool EnsureSuccess ( HttpResponseMessage response )
        {
            if ( response == null )
            {
                Logging.Warn( "Spansh API did not respond" );
                return false;
            }

            if ( !response.IsSuccessStatusCode )
            {
                Logging.Warn( $"Spansh API responded with: {(int)response.StatusCode} - {response.ReasonPhrase}" );
                return false;
            }

            return true;
        }

        private string GetJobID ( string json )
        {
            var parser = JObject.Parse(json);
            if ( parser[ "error" ] != null )
            {
                Logging.Debug( parser[ "error" ].ToString() );
                return null;
            }

            return parser[ "job" ]?.ToString();
        }
    }
}
