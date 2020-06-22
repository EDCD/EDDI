using EddiDataDefinitions;
using Newtonsoft.Json.Linq;
using RestSharp;
using System.Collections.Generic;
using Utilities;

namespace EddiStarMapService
{
    public partial class StarMapService
    {
        public Traffic GetStarMapHostility(string systemName, long? edsmId = null)
        {
            Traffic traffic = GetStarMapTraffic(systemName, edsmId);
            Traffic deaths = GetStarMapDeaths(systemName, edsmId);

            if (traffic != null && deaths != null)
            {
                Traffic hostility = new Traffic(
                    decimal.Divide((long)deaths.total, (long)traffic.total) * 100,
                    decimal.Divide((long)deaths.week, (long)traffic.week) * 100,
                    decimal.Divide((long)deaths.day, (long)traffic.day) * 100
                );
                return hostility;
            }
            return new Traffic();
        }

        public Traffic GetStarMapTraffic(string systemName, long? edsmId = null)
        {
            if (systemName == null) { return null; }


            var request = new RestRequest("api-system-v1/traffic", Method.POST);
            request.AddParameter("systemName", systemName);
            request.AddParameter("systemId", edsmId);
            var clientResponse = restClient.Execute<Dictionary<string, object>>(request);
            if (clientResponse.IsSuccessful)
            {
                var token = JToken.Parse(clientResponse.Content);
                if (token is JObject response)
                {
                    return ParseStarMapTraffic(response);
                }
            }
            return new Traffic();
        }

        public Traffic ParseStarMapTraffic(JObject response)
        {
            if (response.IsNullOrEmpty()) { return new Traffic(); }
            Traffic traffic = ((JObject)response["traffic"]).ToObject<Traffic>();
            return traffic;
        }

        public Traffic GetStarMapDeaths(string systemName, long? edsmId = null)
        {
            if (systemName == null) { return null; }


            var request = new RestRequest("api-system-v1/deaths", Method.POST);
            request.AddParameter("systemName", systemName);
            request.AddParameter("systemId", edsmId);
            var clientResponse = restClient.Execute<Dictionary<string, object>>(request);
            if (clientResponse.IsSuccessful)
            {
                var token = JToken.Parse(clientResponse.Content);
                if (token is JObject response)
                {
                    return ParseStarMapDeaths(response);
                }
            }
            return new Traffic();
        }

        public Traffic ParseStarMapDeaths(JObject response)
        {
            if (response.IsNullOrEmpty()) { return new Traffic(); }
            Traffic deaths = ((JObject)response["deaths"]).ToObject<Traffic>();
            return deaths;
        }
    }
}
