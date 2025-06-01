using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Threading;
using System.Threading.Tasks;
using Utilities;

namespace EddiSpanshService
{
    public interface ISpanshRestClient
    {
        Uri BuildUri ( IRestRequest request );
        IRestResponse<T> Execute<T> ( IRestRequest request );
        IRestResponse Get ( IRestRequest request );
        IRestResponse Post ( IRestRequest request );
        Task<IRestResponse<T>> ExecuteAsync<T> ( IRestRequest request );
        Task<IRestResponse> GetAsync ( IRestRequest request );
        Task<IRestResponse> PostAsync ( IRestRequest request );
    }

    public partial class SpanshService
    {
        private const string baseUrl = "https://spansh.co.uk/api/";
        private readonly ISpanshRestClient spanshRestClient;

        // The default timeout for requests to Spansh. Requests can override this by setting `RestRequest.Timeout`. Both are in milliseconds.
        private const int DefaultTimeoutMilliseconds = 10000;

        // The number of times to retry a failed request
        private const int maxRetries = 3;

        private class SpanshRestClient : ISpanshRestClient
        {
            private readonly RestClient restClient;

            public SpanshRestClient(string baseUrl)
            {
                restClient = new RestClient(baseUrl)
                {
                    Timeout = DefaultTimeoutMilliseconds
                };
            }

            public Uri BuildUri ( IRestRequest request ) => restClient.BuildUri( request );

            public IRestResponse<T> Execute<T> ( IRestRequest request )
            {
                IRestResponse<T> response = null;
                var retryCount = 0;
                while ( retryCount < maxRetries )
                {
                    response = restClient.ExecuteAsync<T>( request ).GetAwaiter().GetResult();
                    if ( response.IsSuccessful )
                    {
                        return response;
                    }
                    retryCount++;
                    Thread.Sleep( 20 ^ retryCount ); // Wait for 500 milliseconds before retrying
                }
                return response;
            }

            public async Task<IRestResponse<T>> ExecuteAsync<T> ( IRestRequest request )
            {
                IRestResponse<T> response = null;
                var retryCount = 0;
                while ( retryCount < maxRetries )
                {
                    response = await restClient.ExecuteAsync<T>( request );
                    if ( response.IsSuccessful )
                    {
                        return response;
                    }
                    retryCount++;
                    Thread.Sleep( 20^retryCount ); // Wait for 500 milliseconds before retrying
                }
                return response;
            }

            public IRestResponse Get ( IRestRequest request )
            {
                var response = ExecuteAsync<object>( request ).GetAwaiter().GetResult();
                return response;
            }

            public async Task<IRestResponse> GetAsync ( IRestRequest request )
            {
                var response = await ExecuteAsync<object>( request );
                return response;
            }

            /// <summary>
            /// Post a search request with a json payload
            /// </summary>
            /// <param name="request"></param>
            /// <returns></returns>
            public IRestResponse Post ( IRestRequest request )
            {
                var response = ExecuteAsync<object>( request ).GetAwaiter().GetResult();
                if ( !IsResponseOk( response ) ) { return null; }
                return response;
            }

            /// <summary>
            /// Post a search request with a json payload
            /// </summary>
            /// <param name="request"></param>
            /// <returns></returns>
            public async Task<IRestResponse> PostAsync ( IRestRequest request )
            {
                var response = await ExecuteAsync<object>( request );
                if ( !IsResponseOk( response ) )
                { return null; }
                return response;
            }

            private static bool IsResponseOk ( IRestResponse response )
            {
                if ( response is null )
                {
                    Logging.Warn( "Spansh API is not responding" );
                    return false;
                }
                if ( !response.IsSuccessful )
                {
                    if ( response.ErrorException != null )
                    {
                        Logging.Warn( $"Spansh API responded with: {response.ResponseStatus} - {response.ErrorException.Message}",
                            response );
                    }
                    else
                    {
                        Logging.Warn( $"Spansh API responded with: {response.StatusCode} - {response.StatusDescription}", response );
                    }
                    return false;
                }
                if ( string.IsNullOrEmpty( response.Content ) )
                {
                    Logging.Warn( "Spansh API responded without providing any data", response );
                    return false;
                }
                return true;
            }
        }

        public SpanshService(ISpanshRestClient restClient = null)
        {
            spanshRestClient = restClient ?? new SpanshRestClient(baseUrl);
        }

        private async Task<JToken> GetRouteResponseAsync(string data)
        {
            return await Task.Run(async () =>
            {
                var jobID = GetJobID(data);
                if (string.IsNullOrEmpty(jobID)) return null;
                
                var jobRequest = new RestRequest("results/" + jobID);
                JObject routeResult = null;
                while (routeResult is null || (routeResult["status"]?.ToString() == "queued"))
                {
                    Thread.Sleep(500);
                    var response = await spanshRestClient.GetAsync(jobRequest);

                    if (response.ResponseStatus == ResponseStatus.TimedOut)
                    {
                        Logging.Warn(response.ErrorMessage, jobRequest);
                        return null;
                    }

                    routeResult = JObject.Parse(response.Content);
                    if (routeResult["error"] != null)
                    {
                        Logging.Debug(routeResult["error"].ToString());
                        return null;
                    }
                }

                return routeResult["result"];
            }).ConfigureAwait(false);
        }

        private string GetJobID(string route)
        {
            var routeResponse = JObject.Parse(route);
            if (routeResponse["error"] != null)
            {
                Logging.Debug(routeResponse["error"].ToString());
                return null;
            }
            return routeResponse["job"]?.ToString();
        }
    }
}
