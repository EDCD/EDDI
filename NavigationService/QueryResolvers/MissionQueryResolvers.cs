using EddiConfigService;
using EddiCore;
using EddiDataDefinitions;
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
        public Dictionary<string, object> SpanshQueryFilter => null;
        public RouteDetailsEvent Resolve ( Query query, StarSystem startSystem ) => GetMissionCargoSourceRoute ( startSystem, query.StringArg0 );

        /// <summary> Route to the nearest star system that can be used to source active mission cargo </summary>
        /// <param name="currentSystem"> The current star system </param>
        /// <param name="fromSystemName"> (Optional) If set, calculate relative to the named starting system rather than the current system </param>
        /// <returns> The query result </returns>
        private RouteDetailsEvent GetMissionCargoSourceRoute ( [NotNull] StarSystem currentSystem, string fromSystemName = null )
        {
            var missions = ConfigService.Instance.missionMonitorConfiguration.missions.ToList();
            if ( missions.All( m => m.sourcesystem == null ) ) { return null; }

            var haulageMissionIds = new HashSet<long>(); // List of mission IDs for the next system
            var sortedSourceSystems = new SortedList<decimal, NavWaypoint>();

            // The route will start in the current system
            var navRouteList = new NavWaypointCollection(Convert.ToDecimal(currentSystem.x), Convert.ToDecimal(currentSystem.y), Convert.ToDecimal(currentSystem.z));
            var currentSystemWaypoint = new NavWaypoint( currentSystem ) { visited = true };
            navRouteList.Waypoints.Add( currentSystemWaypoint );

            foreach ( var mission in missions.Where( m => m.statusDef == MissionStatus.Active && m.sourcesystem != null ) )
            {
                if ( fromSystemName == currentSystemWaypoint.systemName && mission.originsystem != currentSystemWaypoint.systemName )
                {
                    // We are already at the named system and this is not the system where this haulage originates
                    break;
                }

                var dest = EDDI.Instance.DataProvider.GetOrFetchSystemWaypoint( mission.sourcesystem );
                var distance = dest.DistanceFromStarSystem(currentSystemWaypoint) ?? 0;
                if ( !sortedSourceSystems.TryGetValue( distance, out _ ) )
                {
                    sortedSourceSystems.Add( distance, dest );
                }
                haulageMissionIds.Add( mission.missionid );
            }

            var searchSystem = sortedSourceSystems.Values.FirstOrDefault ();

            // Update the navRouteList
            if ( ( searchSystem != null ) && ( currentSystem.systemAddress != searchSystem.systemAddress ) )
            {
                navRouteList.Waypoints.Add ( searchSystem );
            }
            return new RouteDetailsEvent( DateTime.UtcNow, QueryType.source.ToString(), searchSystem?.systemName, searchSystem?.systemAddress, null, null, navRouteList, sortedSourceSystems.Count, haulageMissionIds.ToList() );
        }
    }

    [UsedImplicitly]
    internal class ExpiringMissionResolver : IQueryResolver
    {
        public QueryType Type => QueryType.expiring;
        public Dictionary<string, object> SpanshQueryFilter => null;
        public RouteDetailsEvent Resolve ( Query query, StarSystem startSystem ) => GetExpiringMissionRoute ( startSystem );

        /// <summary> Route to the star system where missions shall expire first </summary>
        /// <returns> The query result </returns>
        private RouteDetailsEvent GetExpiringMissionRoute ( [NotNull] StarSystem startSystem )
        {
            var missions = ConfigService.Instance.missionMonitorConfiguration.missions.ToList();
            if ( missions.Count == 0 ) { return null; }
            var navRouteList = new NavWaypointCollection(Convert.ToDecimal(startSystem.x), Convert.ToDecimal(startSystem.y), Convert.ToDecimal(startSystem.z));

            var expiringMission = missions
                .Where( m => m.statusEDName == "Active"
                    && m.expiry >= DateTime.UtcNow
                    && !string.IsNullOrEmpty( m.destinationsystem ) ).OrderBy( m => m.expiryseconds ?? 0 )
                .FirstOrDefault();

            var dest = EDDI.Instance.DataProvider.GetOrFetchSystemWaypoint( expiringMission?.destinationsystem ); // Destination star system
            if ( dest != null )
            {
                navRouteList.Waypoints.Add ( new NavWaypoint ( startSystem ) { visited = true } );
                if ( startSystem.systemAddress != dest.systemAddress )
                {
                    navRouteList.Waypoints.Add ( dest );
                }
            }

            // Get mission IDs for 'expiring' system
            var missionIDs = NavigationService.GetSystemMissionIds ( dest?.systemName ); // List of mission IDs for the next system  
            return new RouteDetailsEvent ( DateTime.UtcNow, QueryType.expiring.ToString (), dest?.systemName, dest?.systemAddress, null, null, navRouteList, expiringMission?.expiryseconds ?? 0, missionIDs );
        }
    }

    [UsedImplicitly]
    internal class FarthestMissionResolver : IQueryResolver
    {
        public QueryType Type => QueryType.farthest;
        public Dictionary<string, object> SpanshQueryFilter => null;
        public RouteDetailsEvent Resolve ( Query query, StarSystem startSystem ) => GetFarthestMissionRoute ( startSystem );

        /// <summary> Route to the star system furthest from the current star system with active missions </summary>
        /// <returns> The query result </returns>
        private RouteDetailsEvent GetFarthestMissionRoute ( [ NotNull ] StarSystem startSystem )
        {
            var missions = ConfigService.Instance.missionMonitorConfiguration.missions.ToList();
            if ( missions.Count == 0 ) { return null; }
            var navRouteList = new NavWaypointCollection(Convert.ToDecimal(startSystem.x), Convert.ToDecimal(startSystem.y), Convert.ToDecimal(startSystem.z));
            var startSystemWaypoint = new NavWaypoint( startSystem ) { visited = true };
            navRouteList.Waypoints.Add( startSystemWaypoint );

            var farthestList = new SortedList<decimal, NavWaypoint>();
            foreach ( var mission in missions.Where( m => m.statusDef == MissionStatus.Active ).ToList() )
            {
                if ( mission.destinationsystems != null && mission.destinationsystems.Any() )
                {
                    foreach ( var dest in mission.destinationsystems )
                    {
                        AddAndSortByDistance( dest );
                    }
                }
                else if ( !string.IsNullOrEmpty( mission.destinationsystem ) )
                {
                    var dest = EDDI.Instance.DataProvider.GetOrFetchSystemWaypoint(mission.destinationsystem); // Destination star system
                    if ( dest?.systemAddress != startSystemWaypoint.systemAddress )
                    {
                        AddAndSortByDistance( dest );
                    }
                }
                continue;

                void AddAndSortByDistance ( NavWaypoint dest )
                {
                    var distance = dest.DistanceFromStarSystem(startSystemWaypoint) ?? 0;
                    if ( !farthestList.ContainsKey( distance ) )
                    {
                        farthestList.Add( distance, dest );
                    }
                }
            }

            // Farthest system is last in the list
            var searchSystem = farthestList.Values.LastOrDefault ();

            if ( startSystemWaypoint.systemAddress != searchSystem?.systemAddress )
            {
                navRouteList.Waypoints.Add ( searchSystem );
            }

            // Get mission IDs for 'farthest' system
            var missionIDs = NavigationService.GetSystemMissionIds ( searchSystem?.systemName ); // List of mission IDs for the next system
            return new RouteDetailsEvent ( DateTime.UtcNow, QueryType.farthest.ToString (), searchSystem?.systemName, searchSystem?.systemAddress, null, null, navRouteList, missionIDs.Count, missionIDs );
        }
    }

    [UsedImplicitly]
    internal class MostMissionsResolver : IQueryResolver
    {
        public QueryType Type => QueryType.most;
        public Dictionary<string, object> SpanshQueryFilter => null;
        public RouteDetailsEvent Resolve ( Query query, StarSystem startSystem ) => GetMostMissionRoute( query.StringArg0, startSystem );

        /// <summary> Route to the star system that provides the most active missions </summary>
        /// <returns> The query result </returns>
        private RouteDetailsEvent GetMostMissionRoute ( [ NotNull ] string targetSystemName, [ NotNull ] StarSystem startSystem )
        {
            var missions = ConfigService.Instance.missionMonitorConfiguration.missions.ToList();
            if ( missions.Count == 0 ) { return null; }
            var navRouteList = new NavWaypointCollection(Convert.ToDecimal(startSystem.x), Convert.ToDecimal(startSystem.y), Convert.ToDecimal(startSystem.z));
            
            // Determine the number of missions per individual system
            var systemsByMissionCount = new Dictionary<string, int>();

            foreach ( var mission in missions.Where ( m => m.statusDef == MissionStatus.Active ) )
            {
                if ( mission.destinationsystems?.Any () ?? false )
                {
                    foreach ( var system in mission.destinationsystems )
                    {
                        systemsByMissionCount[ system.systemName ] += 1;
                    }
                }
                else if ( !string.IsNullOrEmpty ( mission.destinationsystem ) )
                {
                    systemsByMissionCount[ mission.destinationsystem ] += 1;
                }
            }

            // Sort the 'most' systems by distance (in case of a tie in the mission count)
            var mostList = new SortedList<decimal, NavWaypoint>();   // List of 'most' systems, sorted by distance
            var curr = !string.IsNullOrEmpty(targetSystemName)
                ? EDDI.Instance.DataProvider.GetOrFetchSystemWaypoint( targetSystemName )                
                : new NavWaypoint( startSystem );
            if ( curr is null ) { return null; }
            curr.visited = true;
            navRouteList.Waypoints.Add( curr );

            foreach ( var kv in systemsByMissionCount.Where(s => s.Value == systemsByMissionCount.Values.Max()) )
            {
                var dest = EDDI.Instance.DataProvider.GetOrFetchSystemWaypoint( kv.Key ); // Destination star system
                if ( dest?.x != null )
                {
                    var distance = dest.DistanceFromStarSystem(curr) ?? 0;
                    mostList.Add( distance, dest );
                }
            }

            // Nearest 'most' system is first in the list
            var searchSystem = mostList.Values.FirstOrDefault ();
            if ( curr.systemAddress != searchSystem?.systemAddress )
            {
                navRouteList.Waypoints.Add ( mostList.Values.FirstOrDefault() );
            }

            // Get mission IDs for 'most' system
            var missionIDs = NavigationService.GetSystemMissionIds ( searchSystem?.systemName ); // List of mission IDs for the next system
            return new RouteDetailsEvent ( DateTime.UtcNow, QueryType.most.ToString (), searchSystem?.systemName, searchSystem?.systemAddress, null, null, navRouteList, systemsByMissionCount.Values.Max(), missionIDs );
        }
    }

    [UsedImplicitly]
    internal class NearestMissionResolver : IQueryResolver
    {
        public QueryType Type => QueryType.nearest;
        public Dictionary<string, object> SpanshQueryFilter => null;
        public RouteDetailsEvent Resolve ( Query query, StarSystem startSystem ) => GetNearestMissionRoute ( startSystem );

        /// <summary> Route to the nearest star system with active missions </summary>
        /// <returns> The query result </returns>
        private RouteDetailsEvent GetNearestMissionRoute ( [ NotNull ] StarSystem startSystem )
        {
            var missions = ConfigService.Instance.missionMonitorConfiguration.missions.ToList();
            if ( missions.Count == 0 ) { return null; }
            var navRouteList = new NavWaypointCollection(Convert.ToDecimal(startSystem.x), Convert.ToDecimal(startSystem.y), Convert.ToDecimal(startSystem.z));
            var startSystemWaypoint = new NavWaypoint( startSystem ) { visited = true };
            navRouteList.Waypoints.Add( startSystemWaypoint );

            var nearestList = new SortedList<decimal, NavWaypoint>();
            foreach ( var mission in missions.Where ( m => m.statusDef == MissionStatus.Active ) )
            {
                if ( mission.destinationsystems != null && mission.destinationsystems.Any () )
                {
                    foreach ( var dest in mission.destinationsystems )
                    {
                        if ( dest is null ) { continue; }

                        var distance = dest.DistanceFromStarSystem(startSystemWaypoint) ?? 0;
                        if ( !nearestList.ContainsKey ( distance ) )
                        {
                            nearestList.Add ( distance, dest );
                        }
                    }
                }
                else if ( !string.IsNullOrEmpty ( mission.destinationsystem ) )
                {
                    var dest = EDDI.Instance.DataProvider.GetOrFetchSystemWaypoint( mission.destinationsystem ); // Destination star system
                    var distance = dest.DistanceFromStarSystem(startSystemWaypoint) ?? 0;
                    if ( !nearestList.ContainsKey ( distance ) )
                    {
                        nearestList.Add ( distance, dest );
                    }
                }
            }

            // Nearest system is ordered first in the nearestList
            var searchSystem = nearestList.Values.FirstOrDefault();
            if ( searchSystem != null && startSystem.systemAddress != searchSystem.systemAddress )
            {
                navRouteList.Waypoints.Add ( searchSystem );
            }

            // Get mission IDs for 'farthest' system
            var missionIDs = NavigationService.GetSystemMissionIds ( searchSystem?.systemName ); // List of mission IDs for the next system
            return new RouteDetailsEvent ( DateTime.UtcNow, QueryType.nearest.ToString (), searchSystem?.systemName, searchSystem?.systemAddress, null, null, navRouteList, missionIDs.Count, missionIDs );
        }
    }

    [UsedImplicitly]
    internal class RepetiveNearestNeighborMissionResolver : IQueryResolver
    {
        public QueryType Type => QueryType.route;
        public Dictionary<string, object> SpanshQueryFilter => null;
        public RouteDetailsEvent Resolve ( Query query, StarSystem startSystem ) => GetRepetiveNearestNeighborMissionRoute ( startSystem, query.StringArg0 );

        /// <summary> Route that provides the shortest total travel path to complete all missions using the 'Repetitive Nearest Neighbor' Algorithm (RNNA) </summary>
        /// <param name="currentSystem"> The current star system </param>
        /// <param name="homeSystem"> (Optional) If set, calculate relative to the named starting system rather than the current system </param>
        /// <returns> The query result </returns>
        private RouteDetailsEvent GetRepetiveNearestNeighborMissionRoute ( [ NotNull ] StarSystem currentSystem, string homeSystem = null )
        {
            var missions = ConfigService.Instance.missionMonitorConfiguration.missions.ToList();
            if ( missions.Count == 0 ) { return null; }

            // List of eligible mission destination systems
            var systems = new List<string> 
            {
                // Add current star system first
                currentSystem.systemname
            };      

            // Add origin systems for 'return to origin' missions to the 'systems' list
            foreach ( var mission in missions.Where ( m => m.statusDef != MissionStatus.Failed ) )
            {
                if ( mission.originreturn && !string.IsNullOrEmpty( mission.originsystem ) && !systems.Contains ( mission.originsystem ) )
                {
                    systems.Add ( mission.originsystem );
                }
            }

            // Add destination systems for applicable mission types to the 'systems' list
            foreach ( var mission in missions.Where ( m => m.statusDef == MissionStatus.Active ) )
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
            var navWaypoints = EDDI.Instance.DataProvider.GetOrFetchSystemWaypoints( systems.ToArray() );
            var homeSystemWaypoint = EDDI.Instance.DataProvider.GetOrFetchSystemWaypoint( homeSystem );
            if ( CalculateRepetiveNearestNeighbor ( navWaypoints, missions, out var sortedRoute, homeSystemWaypoint ) )
            {
                var searchSystem = sortedRoute.FirstOrDefault ( w => !w.visited );

                // Prepend our current system to the route if it is not already present
                if ( sortedRoute.FirstOrDefault ()?.systemAddress != currentSystem.systemAddress )
                {
                    sortedRoute = sortedRoute.Prepend ( new NavWaypoint ( currentSystem ) { visited = true } ).ToList ();
                }

                var navRouteList = new NavWaypointCollection ( sortedRoute );
                navRouteList.UpdateLocationData( currentSystem.systemAddress, currentSystem.x, currentSystem.y, currentSystem.z );
                var routeCount = navRouteList.Waypoints.Count;

                Logging.Debug ( "Calculated Route Selected = " + string.Join ( ", ", sortedRoute.Select ( w => w.systemName ) ) + ", Total Distance = " + navRouteList.RouteDistance );

                // Get mission IDs for 'search' system
                var missionIds = NavigationService.GetSystemMissionIds ( searchSystem?.systemName );       // List of mission IDs for the next system
                return new RouteDetailsEvent( DateTime.UtcNow, QueryType.route.ToString(), searchSystem?.systemName, searchSystem?.systemAddress, null, null, navRouteList, routeCount, missionIds );
            }

            Logging.Debug ( "Unable to meet missions route calculation criteria" );
            return null;
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
                        var distance = dest.DistanceFromStarSystem( curr ) ?? 0;
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