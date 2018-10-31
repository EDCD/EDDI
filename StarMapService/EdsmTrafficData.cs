using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EddiStarMapService
{
    public partial class StarMapService
    {
        public Dictionary<string, object> GetStarMapHostility(string system, long? edsmId = null)
        {
            Dictionary<string, object> traffic = GetStarMapTraffic(system, edsmId);
            Dictionary<string, object> deaths = GetStarMapDeaths(system, edsmId);
            Dictionary<string, object> hostility = new Dictionary<string, object>
            {
                { "total", decimal.Divide((long)deaths["total"], (long)traffic["total"])*100 },
                { "week", decimal.Divide((long)deaths["week"], (long)traffic["week"])*100 },
                { "day", decimal.Divide((long)deaths["day"], (long)traffic["day"])*100 }
            };

            return hostility;
        }

        public Dictionary<string, object> GetStarMapTraffic(string system, long? edsmId = null)
        {
            if (system == null) { return null; }

            var client = new RestClient(baseUrl);
            var request = new RestRequest("api-system-v1/traffic", Method.POST);
            request.AddParameter("systemName", system);
            request.AddParameter("systemId", edsmId);
            var clientResponse = client.Execute<Dictionary<string, object>>(request);
            if (clientResponse.IsSuccessful)
            {
                JObject response = JObject.Parse(clientResponse.Content);
                return ParseStarMapTraffic(response);
            }
            return null;
        }

        private static Dictionary<string, object> ParseStarMapTraffic(JObject response)
        {
            Dictionary<string, object> traffic = ((JObject)response["traffic"]).ToObject<Dictionary<string, object>>();
            return traffic;
        }

        public Dictionary<string, object> GetStarMapDeaths(string system, long? edsmId = null)
        {
            if (system == null) { return null; }

            var client = new RestClient(baseUrl);
            var request = new RestRequest("api-system-v1/deaths", Method.POST);
            request.AddParameter("systemName", system);
            request.AddParameter("systemId", edsmId);
            var clientResponse = client.Execute<Dictionary<string, object>>(request);
            if (clientResponse.IsSuccessful)
            {
                JObject response = JObject.Parse(clientResponse.Content);
                return ParseStarMapDeaths(response);
            }
            return null;
        }

        private static Dictionary<string, object> ParseStarMapDeaths(JObject response)
        {
            Dictionary<string, object> deaths = ((JObject)response["deaths"]).ToObject<Dictionary<string, object>>();
            return deaths;
        }
    }
}
