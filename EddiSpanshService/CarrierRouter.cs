using EddiDataDefinitions;
using Newtonsoft.Json.Linq;
using RestSharp;
using System.Collections.Generic;
using System.Threading.Tasks;
using Utilities;

namespace EddiSpanshService
{
    public partial class SpanshService
    {
        // Request a route from the Spansh Carrier Plotter.
        public static async Task<List<NavWaypoint>> GetCarrierRoute(string currentSystem, string[] targetSystems, long usedCarrierCapacity, bool calculateTotalFuelRequired = true, string[] refuel_destinations = null)
        {
            var client = new RestClient("https://spansh.co.uk/api/");
            var request = CarrierRouteRequest(currentSystem, targetSystems, usedCarrierCapacity, calculateTotalFuelRequired, refuel_destinations);
            var initialResponse = client.Get(request);

            if (string.IsNullOrEmpty(initialResponse.Content))
            {
                Logging.Debug("Spansh API is not responding");
                return null;
            }

            var route = await Task.FromResult(GetRouteResponse(initialResponse.Content));

            return ParseCarrierRoute(route);
        }

        private static IRestRequest CarrierRouteRequest(string currentSystem, string[] targetSystems, long usedCarrierCapacity, bool calculateTotalFuelRequired, string[] refuel_destinations)
        {
            var request = new RestRequest("fleetcarrier/route");
            request
                .AddParameter("source", currentSystem)
                .AddParameter("capacity_used", usedCarrierCapacity)
                .AddParameter("calculate_starting_fuel", calculateTotalFuelRequired ? 1 : 0)
                ;
            foreach (var destination in targetSystems)
            {
                request.AddParameter("destinations", destination);
            }
            if (refuel_destinations != null)
            {
                foreach (var destination in refuel_destinations)
                {
                    request.AddParameter("refuel_destinations", destination);
                }
            }

            return request;
        }
        
        private static List<NavWaypoint> ParseCarrierRoute(JToken routeResult)
        {
            var results = new List<NavWaypoint>();

            foreach (var jump in routeResult["jumps"].ToObject<JArray>())
            {
                var waypoint = new NavWaypoint()
                {
                    systemName = jump["name"].ToObject<string>(),
                    systemAddress = jump["id64"].ToObject<ulong?>(),
                    x = jump["x"].ToObject<decimal>(),
                    y = jump["y"].ToObject<decimal>(),
                    z = jump["z"].ToObject<decimal>(),
                    distanceTravelled = jump["distance"].ToObject<decimal>(),
                    distanceRemaining = jump["distance_to_destination"].ToObject<decimal>(),
                    fuelUsed = jump["fuel_used"].ToObject<int>(),
                    hasIcyRing = jump["has_icy_ring"].ToObject<bool>(),
                    hasPristineMining = jump["is_system_pristine"].ToObject<bool>(),
                    isDesiredDestination = jump["is_desired_destination"].ToObject<bool>(),
                };
                results.Add(waypoint);
            }

            return results;
        }
    }
}
