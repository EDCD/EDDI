using EddiDataDefinitions;
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
        public static Traffic GetStarMapHostility(string systemName, long? edsmId = null)
        {
            Traffic traffic = GetStarMapTraffic(systemName, edsmId);
            Traffic deaths = GetStarMapDeaths(systemName, edsmId);
            Traffic hostility = new Traffic(
                decimal.Divide((long)deaths.total, (long)traffic.total)*100,
                decimal.Divide((long)deaths.week, (long)traffic.week)*100,
                decimal.Divide((long)deaths.day, (long)traffic.day)*100 
            );

            return hostility;
        }

        public static Traffic GetStarMapTraffic(string systemName, long? edsmId = null)
        {
            if (systemName == null) { return null; }

            var client = new RestClient(baseUrl);
            var request = new RestRequest("api-system-v1/traffic", Method.POST);
            request.AddParameter("systemName", systemName);
            request.AddParameter("systemId", edsmId);
            var clientResponse = client.Execute<Dictionary<string, object>>(request);
            if (clientResponse.IsSuccessful)
            {
                JObject response = JObject.Parse(clientResponse.Content);
                return ParseStarMapTraffic(response);
            }
            return null;
        }

        private static Traffic ParseStarMapTraffic(JObject response)
        {
            Traffic traffic = ((JObject)response["traffic"]).ToObject<Traffic>();
            return traffic;
        }

        public static Traffic GetStarMapDeaths(string systemName, long? edsmId = null)
        {
            if (systemName == null) { return null; }

            var client = new RestClient(baseUrl);
            var request = new RestRequest("api-system-v1/deaths", Method.POST);
            request.AddParameter("systemName", systemName);
            request.AddParameter("systemId", edsmId);
            var clientResponse = client.Execute<Dictionary<string, object>>(request);
            if (clientResponse.IsSuccessful)
            {
                JObject response = JObject.Parse(clientResponse.Content);
                return ParseStarMapDeaths(response);
            }
            return null;
        }

        private static Traffic ParseStarMapDeaths(JObject response)
        {
            Traffic deaths = ((JObject)response["deaths"]).ToObject<Traffic>();
            return deaths;
        }
    }
}
