using EddiDataDefinitions;
using Newtonsoft.Json.Linq;
using RestSharp;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Utilities;

namespace EddiSpanshService
{
    public partial class SpanshService
    {
        // Request a route from the Spansh Neutron Plotter.
        public List<NavWaypoint> GetNeutronRoute(string currentSystem, string targetSystem, decimal jumpRangeLy, int efficiency = 60)
        {
            var request = NeutronRouteRequest(currentSystem, targetSystem, jumpRangeLy, efficiency);
            var initialResponse = spanshRestClient.Get(request);

            if (string.IsNullOrEmpty(initialResponse.Content))
            {
                Logging.Debug("Spansh API is not responding");
                return null;
            }

            var route = Task.FromResult(GetRouteResponse(initialResponse.Content)).Result;

            return ParseNeutronRoute(route);
        }

        private IRestRequest NeutronRouteRequest(string currentSystem, string targetSystem, decimal jumpRangeLy, int efficiency)
        {
            var request = new RestRequest("route");
            return request.AddParameter("efficiency", efficiency)
                .AddParameter("range", jumpRangeLy.ToString(CultureInfo.InvariantCulture).Replace(",", "."))
                .AddParameter("from", currentSystem)
                .AddParameter("to", targetSystem);
        }

        public List<NavWaypoint> ParseNeutronRoute(JToken routeResult)
        {
            var results = new List<NavWaypoint>();

            foreach (var jump in routeResult["system_jumps"].ToObject<JArray>())
            {
                var waypoint = new NavWaypoint()
                {
                    systemName = jump["system"].ToObject<string>(),
                    systemAddress = jump["id64"].ToObject<ulong?>(),
                    x = jump["x"].ToObject<decimal>(),
                    y = jump["y"].ToObject<decimal>(),
                    z = jump["z"].ToObject<decimal>(),
                    distanceTravelled = jump["distance_jumped"].ToObject<decimal>(),
                    distanceRemaining = jump["distance_left"].ToObject<decimal>(),
                    hasNeutronStar = jump["neutron_star"].ToObject<bool>(),
                };
                results.Add(waypoint);
            }

            return results;
        }
    }
}
