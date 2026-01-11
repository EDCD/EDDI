using Newtonsoft.Json.Linq;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
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

        const int MaxRetries = 3; // Maximum number of retries
        const int InitialBackoffMilliseconds = 100; // Initial back-off time in milliseconds

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

            public SpanshHttpClient (string baseUrl)
            {
                client = new HttpClient
                {
                    BaseAddress = new Uri( baseUrl ),
                    Timeout = TimeSpan.FromMilliseconds( DefaultTimeoutMilliseconds )
                };
                client.DefaultRequestHeaders.UserAgent
                    .ParseAdd( $"{Constants.EDDI_NAME}/{Constants.EDDI_VERSION}" );
                client.DefaultRequestHeaders.Accept
                    .Add( new MediaTypeWithQualityHeaderValue( "application/json" ) );
            }

            public async Task<HttpResponseMessage> GetAsync ( string requestUri, CancellationToken cancellationToken )
            {
                HttpResponseMessage response = null;

                for ( var retry = 0; retry < MaxRetries; retry++ )
                {
                    response = await client.GetAsync( requestUri, cancellationToken ).ConfigureAwait(false);
                    if ( EnsureSuccess(response) )
                    {
                        return response;
                    }

                    await Task.Delay( (int)Math.Pow( 2, retry ) * 100, cancellationToken ).ConfigureAwait(false);
                }

                return response;
            }

            public Task<HttpResponseMessage> PostAsync ( string requestUri, HttpContent content, CancellationToken cancellationToken )
            {
                return client.PostAsync( requestUri, content, cancellationToken );
            }
        }

        private async Task<JToken> GetRouteResponseAsync ( string jobId, CancellationToken cancellationToken )
        {
            if ( string.IsNullOrEmpty( jobId ) ) { return null; }

            // Poll until the job finishes
            try
            {
                JObject routeResult = null;
                while ( routeResult is null || routeResult[ "state" ]?.ToString() == "started" )
                {
                    await Task.Delay( 500, cancellationToken ).ConfigureAwait( false );
                    var getResponse = await spanshHttpClient.GetAsync( $"results/{jobId}", cancellationToken )
                        .ConfigureAwait( false );
                    if ( getResponse.StatusCode == HttpStatusCode.RequestTimeout )
                    {
                        Logging.Warn( $"Spansh API timeout on GET results/{jobId}" );
                        return null;
                    }

                    var getJson = await getResponse.Content.ReadAsStringAsync().ConfigureAwait( false );
                    routeResult = JObject.Parse( getJson );

                    if ( routeResult[ "error" ] != null )
                    {
                        Logging.Debug( routeResult[ "error" ].ToString() );
                        return null;
                    }
                }

                return routeResult[ "result" ];
            }
            catch ( TaskCanceledException )
            {
                // Task cancelled, nothing to do except return.
            }

            return null;
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
    }
}
