using Newtonsoft.Json.Linq;
using RestSharp;
using System.Threading;
using System.Threading.Tasks;
using Utilities;

namespace EddiSpanshService
{
    public partial class SpanshService : ISpanshService
    {
        private const string baseUrl = "https://spansh.co.uk/api/";
        private readonly IRestClient spanshRestClient;
        public SpanshService(RestClient restClient = null)
        {
            spanshRestClient = restClient ?? new RestClient(baseUrl);
        }

        private Task<JToken> GetRouteResponseTask(string route)
        {
            return new Task<JToken>(() =>
            {
                var routeResponse = JObject.Parse(route);
                if (routeResponse["error"] != null)
                {
                    Logging.Debug(routeResponse["error"].ToString());
                    return null;
                }

                var job = routeResponse["job"].ToString();
                JObject routeResult = GetJobResponse(job);
                while (routeResult["status"] != null && routeResult["status"].ToString() == "queued" &&
                       routeResult["error"] == null)
                {
                    Thread.Sleep(1000);
                    routeResult = GetJobResponse(job);
                }

                if (routeResult["error"] != null)
                {
                    Logging.Debug(routeResult["error"].ToString());
                    return null;
                }

                return routeResult["result"];
            });
        }

        /// <summary>
        /// Fetch results from the the Spansh API
        /// </summary>
        /// <param name="job">The ID of the job which must be retrieved</param>
        /// <returns></returns>
        private JObject GetJobResponse(string job)
        {
            var Jobrequest = new RestRequest("results/" + job);
            var response = spanshRestClient.Get(Jobrequest);
            return JObject.Parse(response.Content);
        }
    }
}
