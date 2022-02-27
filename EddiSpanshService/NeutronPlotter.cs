using System.Collections.Generic;
using EddiDataDefinitions;
using Newtonsoft.Json.Linq;
using RestSharp;
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
                var waypoint = new NavWaypoint(jump["system"].ToObject<string>(), jump["x"].ToObject<decimal>(), jump["y"].ToObject<decimal>(), jump["z"].ToObject<decimal>())
                {
                    systemAddress = jump["id64"].ToObject<ulong?>(),
                    hasNeutronStar = jump["neutron_star"].ToObject<bool>(),
                };
                results.Add(waypoint);
            }

            return results;
        }
    }
}
