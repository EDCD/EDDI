using EddiDataDefinitions;
using Newtonsoft.Json.Linq;
using RestSharp;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Utilities;

namespace EddiSpanshService
{
    public partial class SpanshService
    {
        // Request a route from the Spansh Galaxy Plotter.
        public NavWaypointCollection GetGalaxyRoute(string currentSystem, string targetSystem, Ship ship, int? cargoCarriedTons = null, bool is_supercharged = false, bool use_supercharge = true, bool use_injections = false, bool exclude_secondary = false)
        {
            if (ship is null)
            {
                Logging.Warn("Neutron route plotting is not available, ship details are unknown.");
                return null;
            }

            GetShipJumpDetails(ship, out decimal fuel_power, out decimal fuel_multiplier, out decimal optimal_mass,
                out decimal base_mass, out decimal tank_size, out decimal internal_tank_size,
                out decimal max_fuel_per_jump, out decimal range_boost);

            var request = GalaxyRouteRequest(currentSystem, targetSystem, cargoCarriedTons, fuel_power, fuel_multiplier, optimal_mass, base_mass, tank_size, internal_tank_size, max_fuel_per_jump, range_boost, is_supercharged, use_supercharge, use_injections, exclude_secondary);
            var initialResponse = spanshRestClient.Get(request);

            if (string.IsNullOrEmpty(initialResponse.Content))
            {
                Logging.Debug("Spansh API is not responding");
                return null;
            }

            var routeTask = GetRouteResponseTask(initialResponse.Content);
            Task.WaitAll(routeTask);
            return ParseGalaxyRoute(routeTask.Result);
        }

        private void GetShipJumpDetails(Ship ship, out decimal fuel_power, out decimal fuel_multiplier, out decimal optimal_mass, out decimal base_mass, out decimal tank_size, out decimal internal_tank_size, out decimal max_fuel_per_jump, out decimal range_boost)
        {
            // Optimal mass
            optimal_mass = ship.optimalmass;

            // Unladen mass
            base_mass = ship.unladenmass;

            // Max fuel per jump
            max_fuel_per_jump = ship.maxfuelperjump;

            // FSD Rating constant
            Constants.ratingConstantFSD.TryGetValue(ship.frameshiftdrive.grade, out decimal ratingConstant);
            fuel_multiplier = ratingConstant / 1000;

            // FSD Power Constant
            Constants.powerConstantFSD.TryGetValue(ship.frameshiftdrive.@class, out fuel_power);

            // Guardian module range boost
            range_boost = 0;
            Module module = ship.compartments.FirstOrDefault(c => c.module.edname.Contains("Int_GuardianFSDBooster"))?.module;
            if (module != null)
            {
                Constants.guardianBoostFSD.TryGetValue(module.@class, out range_boost);
            }

            // Fuel tank capacity
            tank_size = ship.fueltanktotalcapacity ?? 16;

            // Fuel reservoir capacity
            internal_tank_size = ship.activeFuelReservoirCapacity;
        }

        private IRestRequest GalaxyRouteRequest(string currentSystem, string targetSystem, int? cargoCarriedTons, decimal fuel_power, decimal fuel_multiplier, decimal optimal_mass, decimal base_mass, decimal tank_size, decimal internal_tank_size, decimal max_fuel_per_jump, decimal range_boost, bool is_supercharged, bool use_supercharge, bool use_injections, bool exclude_secondary)
        {
            var request = new RestRequest("generic/route");
            request
                .AddParameter("source", currentSystem)
                .AddParameter("destination", targetSystem)
                .AddParameter("is_supercharged", is_supercharged ? 1 : 0)
                .AddParameter("use_supercharge", use_supercharge ? 1 : 0)
                .AddParameter("use_injections", use_injections ? 1 : 0)
                .AddParameter("exclude_secondary", exclude_secondary ? 1 : 0)
                .AddParameter("fuel_power", fuel_power)
                .AddParameter("fuel_multiplier", fuel_multiplier)
                .AddParameter("optimal_mass", optimal_mass)
                .AddParameter("base_mass", base_mass)
                .AddParameter("tank_size", tank_size)
                .AddParameter("internal_tank_size", internal_tank_size)
                .AddParameter("max_fuel_per_jump", max_fuel_per_jump)
                .AddParameter("range_boost", range_boost)
                ;
            if (cargoCarriedTons != null)
            {
                request.AddParameter("cargo", cargoCarriedTons);
            }
            return request;
        }
        
        private NavWaypointCollection ParseGalaxyRoute(JToken routeResult)
        {
            if (routeResult is null) { return null; }

            var results = new List<NavWaypoint>();

            foreach (var jump in routeResult["jumps"].ToObject<JArray>())
            {
                var waypoint = new NavWaypoint(jump["name"].ToObject<string>(), jump["x"].ToObject<decimal>(), jump["y"].ToObject<decimal>(), jump["z"].ToObject<decimal>())
                {
                    systemAddress = jump["id64"].ToObject<ulong?>(),
                    hasNeutronStar = jump["has_neutron"].ToObject<bool>(),
                    isScoopable = jump["is_scoopable"].ToObject<bool?>() ?? false,
                    refuelRecommended = jump["must_refuel"].ToObject<bool?>() ?? false
                };
                results.Add(waypoint);
            }

            return new NavWaypointCollection(results) {FillVisitedGaps = true};
        }
    }
}
