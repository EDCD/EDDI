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
        // Request a route from the Spansh Carrier Plotter.
        public async Task<NavWaypointCollection> GetCarrierRouteAsync(string currentSystem, string[] targetSystems, long usedCarrierCapacity, bool calculateTotalFuelRequired = true, string[] refuel_destinations = null, bool fromUIquery = false )
        {
            if (!fromUIquery)
            {
                // The Spansh Carrier Plotter uses case sensitive system names. Use the TypeAhead API to normalize casing.
                var wp = await GetWaypointsBySystemNameAsync(currentSystem, CancellationToken.None).ConfigureAwait(false);
                currentSystem = wp.FirstOrDefault()?.systemName;

                for (var i = 0; i < targetSystems.Length; i++)
                {
                    wp = await GetWaypointsBySystemNameAsync( targetSystems[ i ], CancellationToken.None ).ConfigureAwait( false );
                    targetSystems[i] = wp.FirstOrDefault()?.systemName;
                }                
            }

            if (string.IsNullOrEmpty(currentSystem) || targetSystems.Any(s => string.IsNullOrEmpty(s)))
            {
                Logging.Warn("Carrier route plotting is not available, origin or target is not defined.");
                return null;
            }

            try
            {
                var requestUri = CarrierRouteRequest(currentSystem, targetSystems, usedCarrierCapacity, calculateTotalFuelRequired, refuel_destinations);
                var initialResponse = await spanshHttpClient.GetAsync( requestUri, CancellationToken.None ).ConfigureAwait( false );
                initialResponse.EnsureSuccessStatusCode();
                var responseJson = await initialResponse.Content.ReadAsStringAsync().ConfigureAwait( false );

                if ( string.IsNullOrEmpty( responseJson ) )
                {
                    Logging.Warn( "Spansh API is not responding" );
                    return null;
                }

                if ( JObject.Parse( responseJson ).TryGetValue( "job", StringComparison.OrdinalIgnoreCase, out var value ) && value.ToString() is string jobId )
                {
                    var result = await GetRouteResponseAsync( jobId, CancellationToken.None ).ConfigureAwait( false );
                    if ( result is null )
                    {
                        Logging.Warn( $"Spansh API returned no route to system {targetSystems.LastOrDefault()}." );
                        return null;
                    }

                    return ParseCarrierRoute( result );
                }
            }
            catch ( HttpRequestException he )
            {
                Logging.Error( he.Message, he );
            }

            return null;
        }

        private string CarrierRouteRequest ( string currentSystem, string[] targetSystems, long usedCarrierCapacity,
            bool calculateTotalFuelRequired, string[] refuel_destinations )
        {
            var relativePath = "fleetcarrier/route";
            var queryParams = new List<string>
            {
                $"source={Uri.EscapeDataString( currentSystem )}",
                $"capacity_used={usedCarrierCapacity}",
                $"calculate_starting_fuel={( calculateTotalFuelRequired ? 1 : 0 )}"
            };

            foreach ( var destination in targetSystems )
            {
                queryParams.Add( $"destinations={Uri.EscapeDataString( destination )}" );
            }

            if ( refuel_destinations != null )
            {
                foreach ( var destination in refuel_destinations )
                {
                    queryParams.Add( $"refuel_destinations={Uri.EscapeDataString( destination )}" );
                }
            }

            return $"{relativePath}?{string.Join( "&", queryParams )}";
        }

        private NavWaypointCollection ParseCarrierRoute(JToken routeResult)
        {
            if (routeResult is null) { return null; }

            var results = new List<NavWaypoint>();

            foreach (var jump in routeResult["jumps"]?.ToObject<JArray>() ?? new JArray())
            {
                if ( jump[ "id64" ] != null && jump["x"] != null && jump["y"] != null && jump["z"] != null)
                {
                    var waypoint = new NavWaypoint(jump["name"]?.ToObject<string>(), jump["id64"].ToObject<ulong>(), jump["x"].ToObject<decimal>(),
                        jump["y"].ToObject<decimal>(), jump["z"].ToObject<decimal>())
                    {
                        fuelUsed = jump["fuel_used"]?.ToObject<int>(),
                        hasIcyRing = jump["has_icy_ring"]?.ToObject<bool>(),
                        hasPristineMining = jump["is_system_pristine"]?.ToObject<bool>(),
                        isDesiredDestination = jump["is_desired_destination"]?.ToObject<bool>(),
                    };
                    results.Add(waypoint);
                }
            }

            return new NavWaypointCollection(results) { FillVisitedGaps = true };
        }
    }
}
