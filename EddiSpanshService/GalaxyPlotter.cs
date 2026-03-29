using EddiDataDefinitions;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Utilities;

namespace EddiSpanshService
{
    public partial class SpanshService
    {
        // Request a route from the Spansh Galaxy Plotter.
        public async Task<NavWaypointCollection> GetGalaxyRouteAsync (string currentSystem, string targetSystem, Ship ship, int? cargoCarriedTons = null, bool is_supercharged = false, bool use_supercharge = true, bool use_injections = false, bool exclude_secondary = false, bool fromUIquery = false)
        {
            if (!fromUIquery)
            {
                // The Spansh Galaxy Plotter uses case-sensitive system names. Use the TypeAhead API to normalize casing.
                var wp = await GetWaypointsBySystemNameAsync( targetSystem, CancellationToken.None ).ConfigureAwait(false);
                targetSystem = wp.FirstOrDefault()?.systemName;
                if (string.IsNullOrEmpty(targetSystem))
                {
                    Logging.Warn("Neutron route plotting is not available, requested star system is unknown.");
                    return null;
                }

                if (ship is null)
                {
                    Logging.Warn("Neutron route plotting is not available, ship details are unknown.");
                    return null;
                }                
            }
            
            GetShipJumpDetails(ship, out var fuel_power, out var fuel_multiplier, out var optimal_mass,
                out var base_mass, out var tank_size, out var internal_tank_size,
                out var max_fuel_per_jump, out var range_boost, out var supercharge_multiplier );

            try
            {
                var requestUri = GalaxyRouteRequest(currentSystem, targetSystem, cargoCarriedTons, fuel_power, fuel_multiplier, optimal_mass, base_mass, tank_size, internal_tank_size, max_fuel_per_jump, range_boost, supercharge_multiplier, is_supercharged, use_supercharge, use_injections, exclude_secondary);
                var initialResponse = await spanshHttpClient.GetAsync( requestUri, CancellationToken.None ).ConfigureAwait(false);
                initialResponse.EnsureSuccessStatusCode();
                var responseJson = await initialResponse.Content.ReadAsStringAsync().ConfigureAwait(false);

                if ( string.IsNullOrEmpty( responseJson ) )
                {
                    Logging.Warn( "Spansh API is not responding" );
                    return null;
                }

                if ( JObject.Parse( responseJson ).TryGetValue( "job", StringComparison.OrdinalIgnoreCase, out var value ) && value.ToString() is string jobId )
                {                
                    var result = await GetRouteResponseAsync( jobId, CancellationToken.None ).ConfigureAwait(false);
                    if ( result is null )
                    {
                        Logging.Warn( $"Spansh API returned no route to system {targetSystem}." );
                        return null;
                    }

                    return ParseGalaxyRoute( result );
                }
            }
            catch ( TaskCanceledException )
            {
                // Task cancelled, nothing to do except return.
            }
            catch ( HttpRequestException he )
            {
                Logging.Warn( he.Message, he );
            }

            return null;
        }

        private static void GetShipJumpDetails(Ship ship, out double fuel_power, out double fuel_multiplier, out double optimal_mass, out decimal base_mass, out decimal tank_size, out decimal internal_tank_size, out decimal max_fuel_per_jump, out double range_boost, out int supercharge_multiplier)
        {
            // Optimal mass
            optimal_mass = ship.frameshiftdrive.GetFsdOptimalMass();

            // Unladen mass
            base_mass = ship.unladenmass;

            // Max fuel per jump
            max_fuel_per_jump = ship.maxfuelperjump;

            // FSD Rating constant / 1000
            fuel_multiplier = ship.frameshiftdrive.GetFsdRatingConstant() / 1000;

            // FSD Power Constant
            fuel_power = ship.frameshiftdrive.GetFsdPowerConstant();

            // Guardian module range boost
            range_boost = ship.compartments.FirstOrDefault( c =>
                c.module.edname.Contains( "Int_GuardianFSDBooster", StringComparison.InvariantCultureIgnoreCase ) )?.module?.GetGuardianFSDBoost() ?? 0;

            // Fuel tank capacity
            tank_size = ship.fueltanktotalcapacity ?? 16;

            // Fuel reservoir capacity
            internal_tank_size = ship.activeFuelReservoirCapacity;

            // Neutron boost multiplier (4x for standard supercharge, 6x for Caspian Explorer Overcharge Booster Frame Shift Drive Mk II
            supercharge_multiplier =
                ship.frameshiftdrive.edname.Contains( "OverchargeBooster_Mkii", StringComparison.OrdinalIgnoreCase )
                    ? 6
                    : 4;
        }

        private static string GalaxyRouteRequest ( string currentSystem, string targetSystem, int? cargoCarriedTons,
            double fuel_power, double fuel_multiplier, double optimal_mass, decimal base_mass, decimal tank_size,
            decimal internal_tank_size, decimal max_fuel_per_jump, double range_boost, int supercharge_multiplier,
            bool is_supercharged, bool use_supercharge, bool use_injections, bool exclude_secondary )
        {
            var relativePath = "generic/route";
            var queryParams = new List<string>
            {
                $"source={Uri.EscapeDataString( currentSystem )}",
                $"destination={Uri.EscapeDataString(targetSystem)}",
                $"is_supercharged={(is_supercharged ? 1 : 0)}",
                $"use_supercharge={(use_supercharge ? 1 : 0)}",
                $"use_injections={(use_injections ? 1 : 0)}",
                $"exclude_secondary={(exclude_secondary ? 1 : 0)}",
                $"fuel_power={fuel_power}",
                $"fuel_multiplier={fuel_multiplier}",
                $"optimal_mass={optimal_mass}",
                $"base_mass={base_mass.ToInvariantString()}",
                $"tank_size={tank_size.ToInvariantString()}",
                $"internal_tank_size={internal_tank_size.ToInvariantString()}",
                $"max_fuel_per_jump={max_fuel_per_jump.ToInvariantString()}",
                $"range_boost={range_boost}",
                $"supercharge_multiplier={supercharge_multiplier}"
            };

            if ( cargoCarriedTons.HasValue )
            {
                queryParams.Add( $"cargo={cargoCarriedTons.Value}" );
            }

            return $"{relativePath}?{string.Join( "&", queryParams )}";
        }

        private static NavWaypointCollection ParseGalaxyRoute (JToken routeResult)
        {
            if (routeResult is null) { return null; }

            var results = new List<NavWaypoint>();

            foreach (var jump in routeResult["jumps"]?.ToObject<JArray>() ?? [ ] )
            {
                if ( jump[ "id64" ] != null && jump["x"] != null && jump["y"] != null && jump["z"] != null)
                {
                    var waypoint = new NavWaypoint(jump["name"]?.Value<string>(), jump["id64"].Value<ulong>(), jump["x"].Value<decimal>(),
                        jump["y"].Value<decimal>(), jump["z"].Value<decimal>())
                    {
                        hasNeutronStar = jump["has_neutron"]?.Value<bool>() ?? false,
                        isScoopable = jump["is_scoopable"]?.Value<bool?>() ?? false,
                        refuelRecommended = jump["must_refuel"]?.Value<bool?>() ?? false
                    };
                    results.Add(waypoint);
                }
            }

            return new NavWaypointCollection(results) {FillVisitedGaps = true};
        }
    }
}
