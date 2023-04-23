using EddiConfigService;
using EddiCore;
using EddiDataDefinitions;
using EddiDataProviderService;
using EddiEvents;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using Utilities;

namespace EddiNavigationService.QueryResolvers
{
    [UsedImplicitly]
    internal class CargoSourceMissionResolver : IQueryResolver
    {
        public QueryType Type => QueryType.source;
        public RouteDetailsEvent Resolve ( Query query ) => GetMissionCargoSourceRoute ( query.StringArg0 );

        /// <summary> Route to the nearest star system that can be used to source active mission cargo </summary>
        /// <param name="system"> (Optional) If set, calculate relative to the named starting system rather than the current system </param>
        /// <returns> The query result </returns>
        private RouteDetailsEvent GetMissionCargoSourceRoute ( string system = null )
        {
            var cargoConfig = ConfigService.Instance.cargoMonitorConfiguration;
            var inventory = cargoConfig.cargo.ToList();
            var missionsCount = inventory.Sum(c => c.haulageData.Count);
            StarSystem searchSystem = null;
            int systemsCount = 0;
            var missionids = new List<long>();       // List of mission IDs for the next system
            var sourceList = new SortedList<long, StarSystem>();
            var navRouteList = new NavWaypointCollection();

            if ( missionsCount > 0 )
            {
                var curr = EDDI.Instance?.CurrentStarSystem;
                var currentSystem = curr?.systemname;
                var fromHere = system == currentSystem;

                if ( curr != null )
                {
                    foreach ( Cargo cargo in inventory.Where ( c => c.haulageData.Any () ).ToList () )
                    {
                        foreach ( Haulage haulage in cargo.haulageData
                                     .Where ( h =>
                                         h.status == "Active" &&
                                         h.sourcesystem != null
                                         ).ToList () )
                        {
                            if ( fromHere && ( haulage.originsystem != currentSystem ) )
                            {
                                break;
                            }

                            var dest = StarSystemSqLiteRepository.Instance.GetOrFetchStarSystem(haulage.sourcesystem);
                            long distance = (long)(Functions.StellarDistanceLy(curr.x, curr.y, curr.z, dest.x, dest.y, dest.z) ?? (0 * 100));
                            if ( !sourceList.TryGetValue ( distance, out var _ ) )
                            {
                                sourceList.Add ( distance, dest );
                            }

                            missionids.Add ( haulage.missionid );
                        }
                    }

                    searchSystem = sourceList.Values.FirstOrDefault ();
                    systemsCount = sourceList.Count;

                    // Update the navRouteList
                    navRouteList.Waypoints.Add ( new NavWaypoint ( curr ) { visited = true } );
                    if ( ( searchSystem != null ) && ( curr.systemname != searchSystem.systemname ) )
                    {
                        navRouteList.Waypoints.Add ( new NavWaypoint ( searchSystem ) { visited = searchSystem.systemname == curr.systemname } );
                    }
                }
            }
            return new RouteDetailsEvent ( DateTime.UtcNow, QueryType.source.ToString (), searchSystem?.systemname, null, navRouteList, systemsCount, missionids );
        }
    }

    [UsedImplicitly]
    internal class ExpiringMissionResolver : IQueryResolver
    {
        public QueryType Type => QueryType.expiring;
        public RouteDetailsEvent Resolve ( Query query ) => GetExpiringMissionRoute ();

        /// <summary> Route to the star system where missions shall expire first </summary>
        /// <returns> The query result </returns>
        private RouteDetailsEvent GetExpiringMissionRoute ()
        {
            var missionsConfig = ConfigService.Instance.missionMonitorConfiguration;
            var missions = missionsConfig.missions.ToList();
            var missionids = new List<long>();       // List of mission IDs for the next system  
            string searchSystem = null;
            long expiringSeconds = 0;
            var navRouteList = new NavWaypointCollection();

            if ( missions.Count > 0 )
            {
                foreach ( Mission mission in missions
                             .Where ( m => (m.statusEDName == "Active")
                                         && (m.expiry >= DateTime.UtcNow)
                                         && !string.IsNullOrEmpty ( m.destinationsystem ) )
                             .ToList () )
                {
                    if ( (expiringSeconds == 0) || (mission.expiryseconds < expiringSeconds) )
                    {
                        expiringSeconds = mission.expiryseconds ?? 0;
                        searchSystem = mission.destinationsystem;
                    }
                }

                if ( !string.IsNullOrEmpty ( searchSystem ) )
                {
                    var curr = EDDI.Instance?.CurrentStarSystem;
                    var dest = StarSystemSqLiteRepository.Instance.GetOrFetchStarSystem(searchSystem); // Destination star system

                    navRouteList.Waypoints.Add ( new NavWaypoint ( curr ) { visited = true } );
                    if ( curr?.systemname != dest?.systemname )
                    {
                        navRouteList.Waypoints.Add ( new NavWaypoint ( dest ) { visited = dest?.systemname == curr?.systemname } );
                    }
                }

                // Get mission IDs for 'expiring' system
                missionids = NavigationService.GetSystemMissionIds ( searchSystem );
            }
            return new RouteDetailsEvent ( DateTime.UtcNow, QueryType.expiring.ToString (), searchSystem, null, navRouteList, expiringSeconds, missionids );
        }
    }

    [UsedImplicitly]
    internal class FarthestMissionResolver : IQueryResolver
    {
        public QueryType Type => QueryType.farthest;
        public RouteDetailsEvent Resolve ( Query query ) => GetFarthestMissionRoute ();

        /// <summary> Route to the star system furthest from the current star system with active missions </summary>
        /// <returns> The query result </returns>
        private RouteDetailsEvent GetFarthestMissionRoute ()
        {
            var missionsConfig = ConfigService.Instance.missionMonitorConfiguration;
            var missions = missionsConfig.missions.ToList();
            var missionids = new List<long>();       // List of mission IDs for the next system
            string searchSystem = null;
            var navRouteList = new NavWaypointCollection();

            if ( missions.Count > 0 )
            {
                var curr = EDDI.Instance?.CurrentStarSystem;
                var farthestList = new SortedList<decimal, NavWaypoint>();
                foreach ( Mission mission in missions.Where ( m => m.statusDef == MissionStatus.Active ).ToList () )
                {
                    if ( (mission.destinationsystems != null) && mission.destinationsystems.Any () )
                    {
                        foreach ( var system in mission.destinationsystems )
                        {
                            var dest = StarSystemSqLiteRepository.Instance.GetOrFetchStarSystem(system.systemName); // Destination star system
                            decimal distance = NavigationService.CalculateDistance(curr, dest);
                            if ( !farthestList.ContainsKey ( distance ) )
                            {
                                farthestList.Add ( distance, system );

                            }
                        }
                    }
                    else if ( !string.IsNullOrEmpty ( mission.destinationsystem ) )
                    {
                        var dest = StarSystemSqLiteRepository.Instance.GetOrFetchStarSystem(mission.destinationsystem); // Destination star system
                        decimal distance = NavigationService.CalculateDistance(curr, dest);
                        if ( !farthestList.ContainsKey ( distance ) )
                        {
                            farthestList.Add ( distance, new NavWaypoint ( dest ) { visited = dest.systemname == curr?.systemname } );
                        }
                    }
                }
                // Farthest system is last in the list
                searchSystem = farthestList.Values.LastOrDefault ()?.systemName;

                navRouteList.Waypoints.Add ( new NavWaypoint ( curr ) { visited = true } );
                if ( curr?.systemname != farthestList.Values.LastOrDefault ()?.systemName )
                {
                    navRouteList.Waypoints.Add ( farthestList.Values.LastOrDefault () );
                }

                // Get mission IDs for 'farthest' system
                missionids = NavigationService.GetSystemMissionIds ( searchSystem );
            }
            return new RouteDetailsEvent ( DateTime.UtcNow, QueryType.farthest.ToString (), searchSystem, null, navRouteList, missionids.Count, missionids );
        }
    }

    [UsedImplicitly]
    internal class MostMissionsResolver : IQueryResolver
    {
        public QueryType Type => QueryType.most;
        public RouteDetailsEvent Resolve ( Query query ) => GetMostMissionRoute(query.StringArg0);

        /// <summary> Route to the star system that provides the most active missions </summary>
        /// <returns> The query result </returns>
        private RouteDetailsEvent GetMostMissionRoute ( string targetSystemName )
        {
            var missionsConfig = ConfigService.Instance.missionMonitorConfiguration;
            var missions = missionsConfig.missions.ToList();
            var missionids = new List<long>();       // List of mission IDs for the next system
            string searchSystem = null;
            long mostCount = 0;
            var navRouteList = new NavWaypointCollection();

            if ( missions.Count > 0 )
            {
                // Determine the number of missions per individual system
                List<string> systems = new List<string>();  // Mission systems
                List<int> systemsCount = new List<int>();   // Count of missions per system
                foreach ( Mission mission in missions.Where ( m => m.statusDef == MissionStatus.Active ).ToList () )
                {
                    if ( mission.destinationsystems?.Any () ?? false )
                    {
                        foreach ( var system in mission.destinationsystems )
                        {
                            int index = systems.IndexOf(system.systemName);
                            if ( index == -1 )
                            {
                                systems.Add ( system.systemName );
                                systemsCount.Add ( 1 );
                            }
                            else
                            {
                                systemsCount[ index ] += 1;
                            }
                        }
                    }
                    else if ( !string.IsNullOrEmpty ( mission.destinationsystem ) )
                    {
                        int index = systems.IndexOf(mission.destinationsystem);
                        if ( index == -1 )
                        {
                            systems.Add ( mission.destinationsystem );
                            systemsCount.Add ( 1 );
                        }
                        else
                        {
                            systemsCount[ index ] += 1;
                        }
                    }
                }

                // Sort the 'most' systems by distance
                var mostList = new SortedList<decimal, StarSystem>();   // List of 'most' systems, sorted by distance
                mostCount = systemsCount.Max ();
                var curr = !string.IsNullOrEmpty(targetSystemName)
                    ? StarSystemSqLiteRepository.Instance.GetOrFetchStarSystem(targetSystemName, true, false)
                    : EDDI.Instance?.CurrentStarSystem;
                for ( int i = 0; i < systems.Count; i++ )
                {
                    if ( systemsCount[ i ] == mostCount )
                    {
                        var dest = StarSystemSqLiteRepository.Instance.GetOrFetchStarSystem(systems[i]); // Destination star system
                        if ( dest?.x != null )
                        {
                            mostList.Add ( NavigationService.CalculateDistance( curr, dest ), dest );
                        }
                    }
                }

                // Nearest 'most' system is first in the list
                searchSystem = mostList.Values.FirstOrDefault ()?.systemname;

                navRouteList.Waypoints.Add ( new NavWaypoint ( curr ) { visited = true } );
                if ( curr?.systemname != mostList.Values.FirstOrDefault ()?.systemname )
                {
                    navRouteList.Waypoints.Add ( new NavWaypoint ( mostList.Values.FirstOrDefault () ) { visited = mostList.Values.FirstOrDefault ()?.systemname == curr?.systemname } );
                }

                // Get mission IDs for 'most' system
                missionids = NavigationService.GetSystemMissionIds ( searchSystem );
            }
            return new RouteDetailsEvent ( DateTime.UtcNow, QueryType.most.ToString (), searchSystem, null, navRouteList, mostCount, missionids );
        }
    }

    [UsedImplicitly]
    internal class NearestMissionResolver : IQueryResolver
    {
        public QueryType Type => QueryType.nearest;
        public RouteDetailsEvent Resolve ( Query query ) => GetNearestMissionRoute ();

        /// <summary> Route to the nearest star system with active missions </summary>
        /// <returns> The query result </returns>
        private RouteDetailsEvent GetNearestMissionRoute ()
        {
            var missionsConfig = ConfigService.Instance.missionMonitorConfiguration;
            var missions = missionsConfig.missions.ToList();
            var missionids = new List<long>();       // List of mission IDs for the next system
            string searchSystem = null;
            var navRouteList = new NavWaypointCollection();

            var curr = EDDI.Instance?.CurrentStarSystem;     // Current star system
            if ( missions.Count > 0 )
            {
                var nearestList = new SortedList<decimal, StarSystem>();
                foreach ( Mission mission in missions.Where ( m => m.statusDef == MissionStatus.Active ).ToList () )
                {
                    if ( (mission.destinationsystems != null) && mission.destinationsystems.Any () )
                    {
                        foreach ( var system in mission.destinationsystems )
                        {
                            var dest = StarSystemSqLiteRepository.Instance.GetOrFetchStarSystem(system.systemName); // Destination star system
                            decimal distance = NavigationService.CalculateDistance(curr, dest);
                            if ( !nearestList.ContainsKey ( distance ) )
                            {
                                nearestList.Add ( distance, dest );

                            }
                        }
                    }
                    else if ( !string.IsNullOrEmpty ( mission.destinationsystem ) )
                    {
                        var dest = StarSystemSqLiteRepository.Instance.GetOrFetchStarSystem(mission.destinationsystem); // Destination star system
                        decimal distance = NavigationService.CalculateDistance(curr, dest);
                        if ( !nearestList.ContainsKey ( distance ) )
                        {
                            nearestList.Add ( distance, dest );
                        }
                    }
                }
                // Nearest system is first in the list
                searchSystem = nearestList.Values.FirstOrDefault ()?.systemname;

                navRouteList.Waypoints.Add ( new NavWaypoint ( curr ) { visited = true } );
                if ( curr?.systemname != nearestList.Values.FirstOrDefault ()?.systemname )
                {
                    navRouteList.Waypoints.Add ( new NavWaypoint ( nearestList.Values.FirstOrDefault () ) { visited = nearestList.Values.FirstOrDefault ()?.systemname == curr?.systemname } );
                }

                // Get mission IDs for 'farthest' system
                missionids = NavigationService.GetSystemMissionIds ( searchSystem );
            }
            return new RouteDetailsEvent ( DateTime.UtcNow, QueryType.nearest.ToString (), searchSystem, null, navRouteList, missionids.Count, missionids );
        }
    }

    [UsedImplicitly]
    internal class RepetiveNearestNeighborMissionResolver : IQueryResolver
    {
        public QueryType Type => QueryType.route;
        public RouteDetailsEvent Resolve ( Query query ) => GetRepetiveNearestNeighborMissionRoute ( query.StringArg0 );

        /// <summary> Route that provides the shortest total travel path to complete all missions using the 'Repetitive Nearest Neighbor' Algorithm (RNNA) </summary>
        /// <param name="homeSystem"> (Optional) If set, calculate relative to the named starting system rather than the current system </param>
        /// <returns> The query result </returns>
        private RouteDetailsEvent GetRepetiveNearestNeighborMissionRoute ( string homeSystem = null )
        {
            var missionsConfig = ConfigService.Instance.missionMonitorConfiguration;
            var missions = missionsConfig.missions.ToList();
            var navRouteList = new NavWaypointCollection();
            string searchSystem = null;
            int routeCount = 0;

            var systems = new List<string>();      // List of eligible mission destination systems
            var missionids = new List<long>();       // List of mission IDs for the next system

            var homeStarSystem = NavigationService.Instance.DataProviderService.GetSystemData(homeSystem, showCoordinates: false, showBodies: false, showStations: false, showFactions: false);
            NavWaypoint homeSystemWaypoint = null;
            if ( homeStarSystem != null )
            {
                homeSystemWaypoint = new NavWaypoint ( homeStarSystem );
            }

            if ( missions.Count > 0 )
            {
                // Add current star system first
                var curr = EDDI.Instance?.CurrentStarSystem;
                systems.Add ( curr?.systemname );

                // Add origin systems for 'return to origin' missions to the 'systems' list
                foreach ( Mission mission in missions.Where ( m => m.statusDef != MissionStatus.Failed ).ToList () )
                {
                    if ( mission.originreturn && !systems.Contains ( mission.originsystem ) )
                    {
                        systems.Add ( mission.originsystem );
                    }
                }

                // Add destination systems for applicable mission types to the 'systems' list
                foreach ( Mission mission in missions.Where ( m => m.statusDef == MissionStatus.Active ).ToList () )
                {
                    if ( mission.tagsList.Any ( t => t.IncludeInMissionRouting ) )
                    {
                        if ( !( mission.destinationsystems?.Any () ?? false ) )
                        {
                            if ( !string.IsNullOrEmpty ( mission.destinationsystem ) && !systems.Contains ( mission.destinationsystem ) )
                            {
                                systems.Add ( mission.destinationsystem );
                            }
                        }
                        else
                        {
                            foreach ( var system in mission.destinationsystems )
                            {
                                if ( !systems.Contains ( system.systemName ) )
                                {
                                    systems.Add ( system.systemName );
                                }
                            }
                        }
                    }
                }

                // Calculate the missions route using the 'Repetitive Nearest Neighbor' Algorithm (RNNA)
                var navWaypoints = NavigationService.Instance.DataProviderService.GetSystemsData(systems.ToArray(), true, false, false, false).Select(s => new NavWaypoint(s)).ToList();
                if ( CalculateRepetiveNearestNeighbor ( navWaypoints, missions, out var sortedRoute, homeSystemWaypoint ) )
                {
                    // Prepend our current system to the route if it is not already present
                    if ( EDDI.Instance?.CurrentStarSystem != null && 
                         sortedRoute.FirstOrDefault ()?.systemAddress != EDDI.Instance.CurrentStarSystem.systemAddress )
                    {
                        sortedRoute = sortedRoute.Prepend ( new NavWaypoint ( EDDI.Instance.CurrentStarSystem ) ).ToList ();
                        sortedRoute[ 0 ].visited = true;
                    }

                    navRouteList = new NavWaypointCollection ( sortedRoute );
                    searchSystem = navRouteList.Waypoints.FirstOrDefault ( w => !w.visited )?.systemName;
                    routeCount = navRouteList.Waypoints.Count;

                    Logging.Debug ( "Calculated Route Selected = " + string.Join ( ", ", sortedRoute.Select ( w => w.systemName ) ) + ", Total Distance = " + navRouteList.RouteDistance );

                    // Get mission IDs for 'search' system
                    missionids = NavigationService.GetSystemMissionIds ( searchSystem );
                }
                else
                {
                    Logging.Debug ( "Unable to meet missions route calculation criteria" );
                }
            }
            return new RouteDetailsEvent ( DateTime.UtcNow, QueryType.route.ToString (), searchSystem, null, navRouteList, routeCount, missionids );
        }

        private bool CalculateRepetiveNearestNeighbor ( List<NavWaypoint> inputSystems, List<Mission> missions, out List<NavWaypoint> outputRoute, NavWaypoint homeSystem = null )
        {
            var found = false;
            outputRoute = new List<NavWaypoint> ();

            var numSystems = inputSystems.Count;
            if ( numSystems > 1 )
            {
                var bestRoute = new List<NavWaypoint>();
                var bestDistance = 0M;

                // Pre-load all system distances
                if ( homeSystem != null )
                {
                    inputSystems.Add ( homeSystem );
                }
                var distMatrix = new decimal[inputSystems.Count][];
                for ( int i = 0; i < inputSystems.Count; i++ )
                {
                    distMatrix[ i ] = new decimal[ inputSystems.Count ];
                }
                for ( int i = 0; i < ( inputSystems.Count - 1 ); i++ )
                {
                    var curr = inputSystems.Find(s => s.systemName == inputSystems[i].systemName);
                    for ( int j = i + 1; j < inputSystems.Count; j++ )
                    {
                        var dest = inputSystems.Find(s => s.systemName == inputSystems[j].systemName);
                        var distance = Functions.StellarDistanceLy(curr.x, curr.y, curr.z, dest.x, dest.y, dest.z) ?? 0;
                        distMatrix[ i ][ j ] = distance;
                        distMatrix[ j ][ i ] = distance;
                    }
                }

                // Repetitive Nearest Neighbor Algorithm (RNNA)
                // Iterate through all possible routes by changing the starting system
                for ( int i = 0; i < numSystems; i++ )
                {
                    // If starting system is a destination for a 'return to origin' mission, then not a viable route
                    if ( DestinationOriginReturn ( inputSystems[ i ].systemName, missions ) )
                    { continue; }

                    var route = new List<NavWaypoint>();
                    var totalDistance = 0M;
                    int currIndex = i;

                    // Repeat until all systems (except starting system) are in the route
                    while ( route.Count < ( numSystems - 1 ) )
                    {
                        var nearestList = new SortedList<decimal, int>();

                        // Iterate through the remaining systems to find nearest neighbor
                        for ( int j = 1; j < numSystems; j++ )
                        {
                            // Wrap around the list
                            int destIndex = (i + j) < numSystems ? i + j : i + j - numSystems;
                            if ( ( homeSystem != null ) && ( destIndex == 0 ) )
                            { destIndex = numSystems; }

                            // Check if destination system previously added to the route
                            if ( route.IndexOf ( inputSystems[ destIndex ] ) == -1 )
                            {
                                decimal distance = distMatrix[currIndex][destIndex];
                                if ( !nearestList.ContainsKey ( distance ) )
                                {
                                    nearestList.Add ( distance, destIndex );
                                }
                            }
                        }
                        // Set the 'Nearest' system as the new 'current' system
                        currIndex = nearestList.Values.FirstOrDefault ();

                        // Add 'nearest' system to the route list and add its distance to total distance traveled
                        route.Add ( inputSystems[ currIndex ] );
                        totalDistance += nearestList.Keys.FirstOrDefault ();
                    }

                    // Add 'starting system' to complete the route & add its distance to total distance traveled
                    int startIndex = (homeSystem != null) && (i == 0) ? numSystems : i;
                    route.Add ( inputSystems[ startIndex ] );
                    if ( currIndex == numSystems )
                    { currIndex = 0; }
                    totalDistance += distMatrix[ currIndex ][ startIndex ];
                    Logging.Debug ( "Build Route Iteration #" + i + " - Route = " + string.Join ( "_", route ) + ", Total Distance = " + totalDistance );

                    // Use this route if total distance traveled is less than previous iterations
                    if ( ( bestDistance == 0 ) || ( totalDistance < bestDistance ) )
                    {
                        bestRoute.Clear ();
                        int homeIndex = route.IndexOf(inputSystems[homeSystem != null ? numSystems : 0]);
                        if ( homeIndex < ( route.Count - 1 ) )
                        {
                            // Rotate list to place homesystem at the end
                            bestRoute = route.Skip ( homeIndex + 1 )
                                .Concat ( route.Take ( homeIndex + 1 ) )
                                .ToList ();
                        }
                        else
                        {
                            bestRoute = route.ToList ();
                        }
                    }
                }

                if ( bestRoute.Count == numSystems )
                {
                    // Filter any repetitive systems in the route
                    outputRoute = bestRoute
                        .GroupBy ( r => r.systemAddress )
                        .Select ( r => r.First () )
                        .ToList ();
                    found = true;
                }
            }
            return found;
        }

        private bool DestinationOriginReturn ( string destination, List<Mission> missions )
        {
            foreach ( Mission mission in missions.Where ( m => m.originreturn ).ToList () )
            {
                if ( mission.destinationsystems == null )
                {
                    if ( mission.destinationsystem == destination )
                    {
                        return true;
                    }
                }
                else
                {
                    var system = mission.destinationsystems.FirstOrDefault(ds => ds.systemName == destination);
                    if ( system != null )
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}