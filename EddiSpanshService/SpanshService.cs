using RestSharp;
using System.Threading;
using Newtonsoft.Json.Linq;
using Utilities;

namespace EddiSpanshService
{
    public partial class SpanshService
    {
        private static JToken GetRouteResponse(string route)
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
        }

        /// <summary>
        /// Fetch results from the the Spansh API
        /// </summary>
        /// <param name="job">The ID of the job which must be retrieved</param>
        /// <returns></returns>
        private static JObject GetJobResponse(string job)
        {
            var client = new RestClient("https://spansh.co.uk/api/");
            var Jobrequest = new RestRequest("results/" + job);

            var response = client.Get(Jobrequest);
            return JObject.Parse(response.Content);
        }
    }
}
