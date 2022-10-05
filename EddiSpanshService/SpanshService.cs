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
        Uri BuildUri(IRestRequest request);
        IRestResponse<T> Execute<T>(IRestRequest request);
        IRestResponse Get(IRestRequest request);
    }

    public partial class SpanshService : ISpanshService
    {
        private const string baseUrl = "https://spansh.co.uk/api/";
        private readonly ISpanshRestClient spanshRestClient;

        // The default timeout for requests to Spansh. Requests can override this by setting `RestRequest.Timeout`. Both are in milliseconds.
        private const int DefaultTimeoutMilliseconds = 10000;

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

            public Uri BuildUri(IRestRequest request) => restClient.BuildUri(request);

            public IRestResponse<T> Execute<T>(IRestRequest request)
            {
                var response = restClient.Execute<T>(request);
                return response;
            }

            public IRestResponse Get(IRestRequest request)
            {
                var response = Execute<object>(request);
                return response;
            }
        }

        public SpanshService(ISpanshRestClient restClient = null)
        {
            spanshRestClient = restClient ?? new SpanshRestClient(baseUrl);
        }

        private async Task<JToken> GetRouteResponseTask(string data)
        {
            return await Task.Run(() =>
            {
                var jobID = GetJobID(data);
                if (string.IsNullOrEmpty(jobID)) return null;
                
                var jobRequest = new RestRequest("results/" + jobID);
                JObject routeResult = null;
                while (routeResult is null || (routeResult["status"]?.ToString() == "queued"))
                {
                    Thread.Sleep(500);
                    var response = spanshRestClient.Get(jobRequest);

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
            });
        }

        private string GetJobID(string route)
        {
            var routeResponse = JObject.Parse(route);
            if (routeResponse["error"] != null)
            {
                Logging.Debug(routeResponse["error"].ToString());
                return null;
            }
            return routeResponse["job"].ToString();
        }
    }
}
