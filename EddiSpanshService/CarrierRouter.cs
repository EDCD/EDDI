using System.Collections.Generic;
using EddiDataDefinitions;
using Newtonsoft.Json.Linq;
using RestSharp;
using System.Threading.Tasks;
using Utilities;

namespace EddiSpanshService
{
    public partial class SpanshService
    {
        // Request a route from the Spansh Carrier Plotter.
        public List<NavWaypoint> GetCarrierRoute(string currentSystem, string[] targetSystems, long usedCarrierCapacity, bool calculateTotalFuelRequired = true, string[] refuel_destinations = null)
        {
            var request = CarrierRouteRequest(currentSystem, targetSystems, usedCarrierCapacity, calculateTotalFuelRequired, refuel_destinations);
            var initialResponse = spanshRestClient.Get(request);

            if (string.IsNullOrEmpty(initialResponse.Content))
            {
                Logging.Debug("Spansh API is not responding");
                return null;
            }

            var route = Task.FromResult(GetRouteResponse(initialResponse.Content)).Result;

            return ParseCarrierRoute(route);
        }

        private IRestRequest CarrierRouteRequest(string currentSystem, string[] targetSystems, long usedCarrierCapacity, bool calculateTotalFuelRequired, string[] refuel_destinations)
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
        
        private List<NavWaypoint> ParseCarrierRoute(JToken routeResult)
        {
            var results = new List<NavWaypoint>();

            foreach (var jump in routeResult["jumps"].ToObject<JArray>())
            {
                var waypoint = new NavWaypoint(jump["name"].ToObject<string>(), jump["x"].ToObject<decimal>(), jump["y"].ToObject<decimal>(), jump["z"].ToObject<decimal>())
                {
                    systemAddress = jump["id64"].ToObject<ulong?>(),
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
