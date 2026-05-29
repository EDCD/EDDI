using EddiConfigService;
using EddiDataDefinitions;
using EddiSpanshService;
using EddiStarMapService;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Utilities;

[assembly: InternalsVisibleTo( "Tests" )]
namespace EddiDataProviderService
{
    /// <summary>Access data services. Prefer our cache and local database wherever possible.</summary>
    public class DataProviderService
    {
        private readonly StarMapService edsmService;
        internal readonly SpanshService spanshService;
        internal readonly StarSystemSqLiteRepository starSystemRepository;
        private readonly ConcurrentDictionary<ulong, SemaphoreSlim> starSystemResolutionLocks = new();

        private readonly FactionCache factionCache;
        private readonly StarSystemCache starSystemCache;

        public readonly CancellationTokenSource cts = new();

        public readonly bool unitTesting;

        private DataProviderService ( StarMapService edsmService = null,
            SpanshService spanshService = null, StarSystemSqLiteRepository starSystemRepository = null, bool unitTesting = false )
        {
            factionCache = new FactionCache( 3600 ); // Keep a cache of factions for 1 hour
            starSystemCache = new StarSystemCache( 300 ); // Keep a cache of star systems for 5 minutes
            this.edsmService = edsmService;
            this.spanshService = spanshService;
            this.starSystemRepository = starSystemRepository;
            this.unitTesting = unitTesting;
        }

        public static DataProviderService Create ( StarMapService newEdsmService = null,
            SpanshService newSpanshService = null, StarSystemSqLiteRepository newStarSystemRepository = null, bool unitTesting = false )
        {
            return new DataProviderService(
                newEdsmService ?? new StarMapService(),
                newSpanshService ?? new SpanshService(),
                newStarSystemRepository ?? StarSystemSqLiteRepository.Create( unitTesting ),
                unitTesting
                );
        }

        public async Task<List<string>> GetTypeAheadSystemsAsync ( string input, CancellationToken cancellationToken )
        {
            // Prefer type ahead system names for star systems we've visited
            var localSystemNames = await starSystemRepository.GetStarSystemNamesAsync( input, cancellationToken ).ConfigureAwait( false );
            localSystemNames = localSystemNames
                .OrderBy( k => k.LevenshteinDistance( input, StringComparison.CurrentCultureIgnoreCase ) )
                .ToList();
            if ( localSystemNames.Count >= 10 )
            {
                // Return no more than 10 local results
                return localSystemNames.Take( 10 ).ToList();
            }

            // Insufficient local results, use Spansh for a more comprehensive search (this also returns no more than 10 results)
            return await spanshService.GetTypeAheadSystemNamesAsync( input, cancellationToken ).ConfigureAwait( false );
        }

        [NotNull]
        public async Task<StarSystem> GetOrCreateStarSystemAsync (
            ulong systemAddress,
            string systemName,
            bool fetchIfMissing = true,
            bool refreshIfOutdated = true,
            bool showMarketDetails = false )
        {
            if ( systemAddress <= 0 )
            {
                return new StarSystem { systemname = systemName, systemAddress = systemAddress };
            }

            var resolutionLock = starSystemResolutionLocks.GetOrAdd( systemAddress, _ => new SemaphoreSlim( 1, 1 ) );

            await resolutionLock.WaitAsync( cts.Token ).ConfigureAwait( false );
            try
            {
                return await GetOrCreateStarSystemCoreAsync(
                    systemAddress,
                    systemName,
                    fetchIfMissing,
                    refreshIfOutdated,
                    showMarketDetails
                ).ConfigureAwait( false );
            }
            finally
            {
                resolutionLock.Release();
            }
        }
        
        [NotNull]
        private async Task<StarSystem> GetOrCreateStarSystemCoreAsync (
            ulong systemAddress,
            string systemName,
            bool fetchIfMissing,
            bool excludeStaleResults,
            bool showMarketDetails )
        {
            var starSystem = await GetOrFetchStarSystemCoreAsync(
                systemAddress,
                fetchIfMissing,
                excludeStaleResults,
                showMarketDetails,
                fetchEdsmVisitsAndComments: true
                ).ConfigureAwait( false );

            if ( starSystem != null )
            {
                return starSystem;
            }

            starSystem = new StarSystem
            {
                systemname = systemName,
                systemAddress = systemAddress,
                lastupdated = DateTime.UtcNow
            };

            await SaveStarSystemsAsync( new List<StarSystem> { starSystem }, cts.Token ).ConfigureAwait( false );

            return starSystem;
        }

        public async Task<StarSystem> GetOrFetchStarSystemAsync (
            ulong systemAddress,
            bool fetchIfMissing = true,
            bool excludeStaleResults = true,
            bool showMarketDetails = false,
            bool fetchEdsmVisitsAndComments = true )
        {
            if ( systemAddress <= 0 )
            {
                return null;
            }

            var resolutionLock = starSystemResolutionLocks.GetOrAdd( systemAddress, _ => new SemaphoreSlim( 1, 1 ) );

            await resolutionLock.WaitAsync( cts.Token ).ConfigureAwait( false );
            try
            {
                return await GetOrFetchStarSystemCoreAsync(
                    systemAddress,
                    fetchIfMissing,
                    excludeStaleResults,
                    showMarketDetails,
                    fetchEdsmVisitsAndComments
                ).ConfigureAwait( false );
            }
            finally
            {
                resolutionLock.Release();
            }
        }

        private async Task<StarSystem> GetOrFetchStarSystemCoreAsync (
            ulong systemAddress,
            bool fetchIfMissing = true,
            bool excludeStaleResults = true,
            bool showMarketDetails = false,
            bool fetchEdsmVisitsAndComments = true )
        {
            if ( starSystemCache.TryGet( systemAddress, out var cachedSystem ) )
            {
                return cachedSystem;
            }

            var localDbSystems = await GetSqlStarSystemsAsync( new[] { systemAddress } ).ConfigureAwait( false );

            var localSystem = localDbSystems
                .Where( s => !( excludeStaleResults && IsStale( s ) ) )
                .OrderByDescending( s => s.lastVisitSeconds ?? 0 )
                .ThenByDescending( s => s.updatedat ?? 0 )
                .FirstOrDefault();

            if ( localSystem != null )
            {
                return localSystem;
            }

            if ( !fetchIfMissing || starSystemCache.IsUnavailable( systemAddress ) )
            {
                return null;
            }

            var fetchedSystem = await spanshService
                .GetStarSystemAsync( systemAddress, showMarketDetails, cts.Token )
                .ConfigureAwait( false );

            if ( fetchedSystem is null )
            {
                starSystemCache.MarkUnavailable( systemAddress );
                return null;
            }

            var fetchedSystems = new List<StarSystem> { fetchedSystem };

            if ( fetchEdsmVisitsAndComments )
            {
                fetchedSystems = ( await SyncFromStarMapServiceAsync( fetchedSystems ).ConfigureAwait( false ) ).ToList();
            }

            fetchedSystems = PreserveUnsyncedProperties( fetchedSystems, localDbSystems ).ToList();

            foreach ( var starSystem in fetchedSystems )
            {
                starSystem.lastupdated = DateTime.UtcNow;
            }

            await SaveStarSystemsAsync( fetchedSystems, cts.Token ).ConfigureAwait( false );

            return fetchedSystems.FirstOrDefault();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="systemName"></param>
        /// <param name="fetchIfMissing"></param>
        /// <param name="excludeStaleResults"></param>
        /// <param name="showMarketDetails"></param>
        /// <param name="fetchEdsmVisitsAndComments"></param>
        /// <returns></returns>
        public async Task<StarSystem> GetOrFetchStarSystemAsync ( string systemName, bool fetchIfMissing = true, bool excludeStaleResults = true, bool showMarketDetails = false, bool fetchEdsmVisitsAndComments = true )
        {
            if ( string.IsNullOrEmpty( systemName ) ) { return null; }

            // Fetch from cached systems
            if ( starSystemCache.TryGet( systemName, out var cachedSystem ) )
            { return cachedSystem; }

            // Fetch from the local database. If there is more than one result, return the most recent result (by visits and then by update time)
            var sqlStarSystems = await GetSqlStarSystemsAsync( [ systemName ], cts.Token ).ConfigureAwait(false);
            sqlStarSystems = sqlStarSystems
                .Where( s => !( excludeStaleResults && IsStale( s ) ) )
                .ToList();
            if ( sqlStarSystems.Count > 0 )
            {
                return sqlStarSystems
                    .OrderByDescending( s => s.lastVisitSeconds ?? 0 )
                    .ThenByDescending( s => s.updatedat ?? 0 )
                    .FirstOrDefault();
            }

            // Fetch from external data sources (when so instructed)
            var fetchedWaypoint = await GetOrFetchSystemWaypointAsync( systemName ).ConfigureAwait(false);
            if ( fetchedWaypoint is null )
            { return null; }

            var starSystems = await GetOrFetchStarSystemsAsync( [ fetchedWaypoint.systemAddress ], fetchIfMissing,
                excludeStaleResults, showMarketDetails, fetchEdsmVisitsAndComments ).ConfigureAwait(false);
            return starSystems?.FirstOrDefault();
        }

        private async Task<List<StarSystem>> GetOrFetchStarSystemsAsync (
            ulong[] systemAddresses,
            bool fetchIfMissing = true,
            bool excludeStaleResults = true,
            bool showMarketDetails = false,
            bool fetchEdsmVisitsAndComments = true )
        {
            if ( systemAddresses is null || systemAddresses.Length == 0 )
            {
                return new List<StarSystem>();
            }

            var distinctAddresses = systemAddresses
                .Where( s => s > 0 )
                .Distinct()
                .ToArray();

            var tasks = distinctAddresses
                .Select( systemAddress => GetOrFetchStarSystemAsync(
                    systemAddress,
                    fetchIfMissing,
                    excludeStaleResults,
                    showMarketDetails,
                    fetchEdsmVisitsAndComments
                ) ).ToArray();

            var results = ( await Task.WhenAll( tasks ).ConfigureAwait( false ) )
                .RemoveNulls()
                .ToList();

            var missingSystems = distinctAddresses
                .Where( systemAddress => results.All( s => s.systemAddress != systemAddress ) )
                .Where( systemAddress => !starSystemCache.IsUnavailable( systemAddress ) )
                .ToArray();

            if ( missingSystems.Length > 0 && fetchIfMissing )
            {
                Logging.Warn( "Unable to retrieve data on all requested star systems.", missingSystems );
            }

            return results;
        }

        public async Task<StarSystem> GetOrFetchQuickStarSystemAsync ( ulong systemAddress, bool fetchIfMissing = true )
        {
            if ( systemAddress <= 0 )
            { return null; }

            return ( await GetOrFetchQuickStarSystemsAsync( [ systemAddress ], fetchIfMissing ).ConfigureAwait( false ) )?.FirstOrDefault();
        }

        private async Task<List<StarSystem>> GetOrFetchQuickStarSystemsAsync ( ulong[] systemAddresses, bool fetchIfMissing = true )
        {
            var results = new List<StarSystem>();
            if ( systemAddresses is null || systemAddresses.Length == 0 )
            { return results; }

            ulong[] missingSystems () => systemAddresses.Where( k => results.All( s => s.systemAddress != k ) ).Distinct().ToArray();

            // Fetch from cached systems
            results.AddRange( starSystemCache.GetRange( missingSystems() ) );

            // Fetch from the local database
            results.AddRange( await GetSqlStarSystemsAsync( missingSystems() ).ConfigureAwait( false ) );

            // Fetch from external data providers (when so instructed)
            if ( missingSystems().Length > 0 && fetchIfMissing )
            {
                // Add the external data to our results
                results.AddRange( await spanshService.GetQuickStarSystemsAsync( missingSystems(), cts.Token ).ConfigureAwait( false ) );
            }

            if ( missingSystems().Length > 0 )
            {
                Logging.Warn( "Unable to retrieve data on all requested star systems.", missingSystems() );
            }

            return results;
        }

        public async Task<List<StarSystem>> GetOrFetchQuickStarSystemsAsync ( string[] systemNames, bool fetchIfMissing = true )
        {
            var waypointTasks = systemNames.AsParallel().Select( GetOrFetchSystemWaypointAsync );
            var waypoints = await Task.WhenAll(waypointTasks).ConfigureAwait(false);
            var systemAddresses = waypoints.Where( wp => wp != null ).Select( wp => wp.systemAddress ).ToArray();
            return await GetOrFetchQuickStarSystemsAsync( systemAddresses.ToArray(), fetchIfMissing ).ConfigureAwait( false );
        }

        public async Task<NavWaypoint> GetOrFetchSystemWaypointAsync ( string systemName )
        {
            if ( string.IsNullOrEmpty( systemName ) )
            { return null; }
            var wp = await GetOrFetchSystemWaypointsAsync( [ systemName ] ).ConfigureAwait(false);
            return wp.FirstOrDefault();
        }

        public async Task<List<NavWaypoint>> GetOrFetchSystemWaypointsAsync ( string[] systemNames )
        {
            var results = new List<NavWaypoint>();
            if ( systemNames is null || systemNames.Length == 0 )
            { return results; }

            string[] missingSystems () => systemNames.Where( k => results.All( s => s.systemName != k ) ).Distinct().ToArray();

            // Fetch from cached systems
            results.AddRange( starSystemCache.GetRange( missingSystems() ).Select( s => new NavWaypoint( s ) ) );

            // Fetch from Spansh
            var waypoints = missingSystems().AsParallel().Select( async systemName =>
            {
                var wp = await spanshService.GetWaypointsBySystemNameAsync( systemName.Trim(), cts.Token ).ConfigureAwait(false);
                return wp.FirstOrDefault( s => s.systemName.Equals( systemName, StringComparison.InvariantCultureIgnoreCase ) );
            } ).ToList();
            results.AddRange( await Task.WhenAll( waypoints ).ConfigureAwait( false ) );

            return results;
        }

        /// <summary>
        /// Find the station with the given system and station names from the Spansh Station Search API.
        /// </summary>
        /// <param name="fromSystemAddress"></param>
        /// <param name="fromMarketId"></param>
        /// <returns></returns>
        public async Task<NavWaypoint> GetOrFetchStationWaypointAsync ( ulong fromSystemAddress, long fromMarketId )
        {
            // Try to fetch from cached systems
            if ( starSystemCache.TryGet( fromSystemAddress, out var cachedStarSystem ) )
            {
                var cachedStation = cachedStarSystem.stations.FirstOrDefault( s => s.marketId == fromMarketId );
                if ( cachedStation != null )
                {
                    return new NavWaypoint( cachedStarSystem )
                    {
                        stationName = cachedStation.name,
                        marketID = cachedStation.marketId
                    };
                }
            }

            var system = await GetOrFetchQuickStarSystemAsync( fromSystemAddress ).ConfigureAwait(false);
            var station = system?.stations.FirstOrDefault( s => s.marketId == fromMarketId );
            if ( station != null )
            {
                return new NavWaypoint( system )
                {
                    stationName = station.name,
                    marketID = station.marketId
                };
            }

            return null;
        }

        /// <summary>
        /// Find the station with the given system and station names from the Spansh Station Search API.
        /// </summary>
        /// <param name="fromSystemName"></param>
        /// <param name="fromMarketId"></param>
        /// <returns></returns>
        public async Task<NavWaypoint> GetOrFetchStationWaypointAsync ( string fromSystemName, long fromMarketId )
        {
            // Try to fetch from cached systems
            if ( !string.IsNullOrEmpty( fromSystemName ) && starSystemCache.TryGet( fromSystemName, out var cachedStarSystem ) )
            {
                var cachedStation = cachedStarSystem.stations.FirstOrDefault( s => s.marketId == fromMarketId );
                if ( cachedStation != null )
                {
                    return new NavWaypoint( cachedStarSystem )
                    {
                        stationName = cachedStation.name,
                        marketID = cachedStation.marketId
                    };
                }
            }

            // Fetch from Spansh
            var wp = await GetOrFetchSystemWaypointAsync( fromSystemName ).ConfigureAwait(false);
            if ( wp?.systemAddress != null )
            {

                var system = await GetOrFetchQuickStarSystemAsync( wp.systemAddress ).ConfigureAwait(false);
                var station = system?.stations.FirstOrDefault( s => s.marketId == fromMarketId );
                return new NavWaypoint( system )
                {
                    stationName = station?.name,
                    marketID = station?.marketId
                };
            }

            return null;
        }

        #region StarSystemSqlLiteRepository

        private async Task<List<StarSystem>> GetSqlStarSystemsAsync ( ulong[] systemAddresses )
        {
            var dbStarSystems = await starSystemRepository.GetSqlStarSystemsAsync( systemAddresses, cts.Token ).ConfigureAwait(false);
            return DeserializeSqlStarSystems( dbStarSystems );
        }

        private async Task<List<StarSystem>> GetSqlStarSystemsAsync ( string[] systemNames, CancellationToken cancellationToken )
        {
            var dbStarSystems = await starSystemRepository.GetSqlStarSystemsAsync( systemNames, cancellationToken ).ConfigureAwait(false);
            return DeserializeSqlStarSystems( dbStarSystems );
        }

        private static bool IsStale ( StarSystem starSystem )
        {
            // Consider a star system to be stale if it hasn't been updated within the last hour
            // (for populated systems) or the last month (for unpopulated systems, which are
            // less likely to have changed)
            return starSystem.population > 0
                ? starSystem.lastupdated < DateTime.UtcNow.AddHours( -1 )
                : starSystem.lastupdated < DateTime.UtcNow.AddMonths( -1 );
        }

        private List<StarSystem> DeserializeSqlStarSystems ( List<DatabaseStarSystem> dbStarSystems )
        {
            var results = new List<StarSystem>();
            foreach ( var dbStarSystem in dbStarSystems )
            {
                if ( HasKeyFields( dbStarSystem ) )
                {
                    // Deserialize the result
                    var result = DeserializeStarSystem( dbStarSystem.systemAddress, dbStarSystem.systemJson );

                    // Exclude null results
                    if ( result is not null )
                    {
                        results.Add( result );
                    }
                }

            }

            return results;

            static bool HasKeyFields ( DatabaseStarSystem dbStarSystem )
            {
                return dbStarSystem.systemAddress > 0 && !string.IsNullOrEmpty( dbStarSystem.systemJson );
            }
        }

        internal static IList<StarSystem> PreserveUnsyncedProperties ( IList<StarSystem> updatedSystems, IList<StarSystem> databaseStarSystems )
        {
            foreach ( var updatedSystem in updatedSystems ?? new List<StarSystem>() )
            {
                foreach ( var oldStarSystem in databaseStarSystems ?? new List<StarSystem>() )
                {
                    if ( updatedSystem.systemAddress == oldStarSystem.systemAddress )
                    {
                        PreserveSystemProperties( updatedSystem, oldStarSystem );
                        PreserveBodyProperties( updatedSystem, oldStarSystem );
                        PreserveFactionProperties( updatedSystem, oldStarSystem );
                        // No station data needs to be carried over at this time.
                    }
                }
            }
            return updatedSystems ?? new List<StarSystem>();
        }

        private static void PreserveSystemProperties ( StarSystem updatedSystem, StarSystem oldStarSystem )
        {
            // Carry over StarSystem properties that we want to preserve
            updatedSystem.totalbodies = oldStarSystem.totalbodies;

            // Visits should sync from EDSM, but in case there is a problem with the connection we will also seed back in our old star system visit data
            foreach ( var visit in oldStarSystem.visitLog )
            {
                // The SortedSet<T> class does not accept duplicate elements so we can safely add timestamps which may be duplicates of visits already reported from EDSM.
                // If an item is already in the set, processing continues and no exception is thrown.
                updatedSystem.visitLog.Add( visit );
            }
        }

        private static void PreserveBodyProperties ( StarSystem updatedSystem, StarSystem oldStarSystem )
        {
            // Carry over Body properties that we want to preserve (e.g. exploration data)
            updatedSystem.PreserveBodyData( oldStarSystem.bodies.ToList(), updatedSystem.bodies );
        }

        private static void PreserveFactionProperties ( StarSystem updatedSystem, StarSystem oldStarSystem )
        {
            // Carry over Faction properties that we want to preserve (e.g. reputation data)
            foreach ( var oldFaction in oldStarSystem.factions )
            {
                foreach ( var updatedFaction in updatedSystem.factions )
                {
                    // Only preserve reputation data if the faction name matches and the updated faction does not include reputation data
                    // (to avoid overwriting reputation data obtained from the journal)
                    if ( updatedFaction.name == oldFaction.name && updatedFaction.myreputation is null )
                    {
                        updatedFaction.myreputation = oldFaction.myreputation;
                    }
                }
            }
        }

        private StarSystem DeserializeStarSystem ( ulong systemAddress, string data )
        {
            if ( systemAddress == 0 || data == string.Empty )
            { return null; }

            // Check our short term star system cache for a previously deserialized star system and return that if it is available.
            if ( starSystemCache.TryGet( systemAddress, out var cachedStarSystem ) )
            {
                return cachedStarSystem;
            }

            // Not found in memory, proceed with deserialization
            try
            {
                var result = JsonConvert.DeserializeObject<StarSystem>( data );
                if ( result == null )
                {
                    Logging.Info( "Failed to obtain system for address " + systemAddress + " from the SQLiteRepository" );
                }
                // Save the deserialized star system to our short term star system cache for reference
                if ( result != null )
                {
                    factionCache.AddOrUpdate( result.factions );
                    starSystemCache.AddOrUpdate( result );
                }

                return result;
            }
            catch ( Exception ex )
            {
                Logging.Warn( $"Problem reading data for star system address {systemAddress} from database.", ex );
            }
            return null;
        }

        public async Task SaveStarSystemAsync ( StarSystem starSystem )
        {
            if ( starSystem == null )
            { return; }
            await SaveStarSystemsAsync( new List<StarSystem> { starSystem }, cts.Token ).ConfigureAwait( false );
        }

        private async Task SaveStarSystemsAsync ( IList<StarSystem> starSystems, CancellationToken cancellationToken )
        {
            if ( !starSystems.Any() )
            { return; }

            // Update any faction and star systems in our short term faction and star system caches to minimize repeat deserialization
            foreach ( var starSystem in starSystems )
            {
                factionCache.AddOrUpdate( starSystem.factions );
                starSystemCache.AddOrUpdate( starSystem );
            }

            if ( unitTesting )
            { return; }

            await starSystemRepository.SaveStarSystemsAsync( starSystems, cancellationToken ).ConfigureAwait( false );
        }

        #endregion

        #region Spansh Endpoints

        public Task<NavWaypointCollection> FetchCarrierRouteAsync ( string currentSystem, string[] targetSystems, long usedCarrierCapacity,
            bool calculateTotalFuelRequired = true, string[] refuelDestinations = null, bool fromUIquery = false )
        {
            return spanshService.GetCarrierRouteAsync( currentSystem, targetSystems, usedCarrierCapacity,
                calculateTotalFuelRequired, refuelDestinations, fromUIquery );
        }

        public Task<NavWaypointCollection> FetchGalaxyRouteAsync ( string currentSystem, string targetSystem, Ship ship,
            int? cargoCarriedTons = null, bool isSupercharged = false, bool useSupercharge = true,
            bool useInjections = false, bool excludeSecondary = false, bool fromUIquery = false )
        {
            return spanshService.GetGalaxyRouteAsync( currentSystem, targetSystem, ship, cargoCarriedTons, isSupercharged,
                useSupercharge, useInjections, excludeSecondary, fromUIquery );
        }

        /// <summary>
        /// Find the nearest station with specific station services from the Spansh Station Search API.
        /// </summary>
        /// <param name="fromX"></param>
        /// <param name="fromY"></param>
        /// <param name="fromZ"></param>
        /// <param name="filters"></param>
        /// <returns></returns>
        public async Task<NavWaypoint> FetchStationWaypointAsync ( decimal fromX, decimal fromY, decimal fromZ, Dictionary<string, object> filters )
        {
            var data = await spanshService.DistanceOrderedQueryAsync( SpanshService.QueryGroup.stations, fromX, fromY, fromZ, filters, cts.Token ).ConfigureAwait(false);
            if ( data?[ "error" ] != null )
            {
                Logging.Warn( "Spansh API responded with: " + data[ "error" ] );
                return null;
            }
            return SpanshService.ParseQuickStationWaypoint( data?[ "results" ]?.FirstOrDefault() );
        }

        /// <summary>
        /// Find the nearest body with specific parameters from the Spansh Station Search API.
        /// </summary>
        /// <param name="fromX"></param>
        /// <param name="fromY"></param>
        /// <param name="fromZ"></param>
        /// <param name="filters"></param>
        /// <returns></returns>
        public async Task<NavWaypoint> FetchBodyWaypointAsync ( decimal fromX, decimal fromY, decimal fromZ, Dictionary<string, object> filters )
        {
            var data = await spanshService.DistanceOrderedQueryAsync( SpanshService.QueryGroup.bodies, fromX, fromY, fromZ, filters, cts.Token ).ConfigureAwait(false);
            if ( data?[ "error" ] != null )
            {
                Logging.Warn( "Spansh API responded with: " + data[ "error" ] );
                return null;
            }
            return ParseQuickBody( data?[ "results" ]?.FirstOrDefault() );

            static NavWaypoint ParseQuickBody ( JToken bodyData )
            {
                if ( bodyData is null )
                { return null; }

                var systemName = bodyData[ "system_name" ]?.ToString();
                var systemAddress = bodyData[ "system_id64" ]?.ToObject<ulong>() ?? 0;
                var systemX = bodyData[ "system_x" ]?.ToObject<decimal>() ?? 0;
                var systemY = bodyData[ "system_y" ]?.ToObject<decimal>() ?? 0;
                var systemZ = bodyData[ "system_z" ]?.ToObject<decimal>() ?? 0;

                return new NavWaypoint( systemName, systemAddress, systemX, systemY, systemZ );
            }
        }

        public async Task<Faction> FetchFactionByNameAsync ( string factionName, string presenceSystemName = null )
        {
            if ( string.IsNullOrEmpty( factionName ) )
            { return null; }

            // First try to fetch the faction from the cache
            if ( factionCache.TryGet( factionName, out var faction ) )
            {
                return faction;
            }

            // Next, try to fetch the faction from Spansh
            faction = await spanshService.GetFactionByNameAsync( factionName, cts.Token, presenceSystemName ).ConfigureAwait( false );

            // If we've successfully retrieved the faction then update our cache
            if ( faction != null )
            {
                factionCache.AddOrUpdate( faction );
            }

            return faction;
        }

        #endregion

        #region EDSM Endpoints

        [CanBeNull]
        public async Task<Traffic> GetSystemTrafficAsync ( string systemName, long? edsmId = null )
        {
            if ( string.IsNullOrEmpty( systemName ) )
            { return null; }
            return await edsmService.GetStarMapTrafficAsync( systemName, edsmId ).ConfigureAwait( false ) ?? new Traffic();
        }

        [CanBeNull]
        public async Task<Traffic> GetSystemDeathsAsync ( string systemName, long? edsmId = null )
        {
            if ( string.IsNullOrEmpty( systemName ) )
            { return null; }
            return await edsmService.GetStarMapDeathsAsync( systemName, edsmId ).ConfigureAwait( false ) ?? new Traffic();
        }

        [CanBeNull]
        public async Task<Traffic> GetSystemHostilityAsync ( string systemName, long? edsmId = null )
        {
            if ( string.IsNullOrEmpty( systemName ) )
            { return null; }
            return await edsmService.GetStarMapHostilityAsync( systemName, edsmId ).ConfigureAwait( false ) ?? new Traffic();
        }

        // EDSM Journal Synchronization

        public bool TryStartEdsmJournal ()
        {
            edsmService?.StartJournalSync();
            return edsmService != null;
        }

        public async Task StopEdsmJournalAsync ()
        {
            if ( edsmService != null )
            {
                await edsmService.StopJournalAsync().ConfigureAwait( false );
            }
        }

        public Task<List<string>> GetIgnoredEdsmEventsAsync ()
        {
            return edsmService?.getIgnoredEventsAsync();
        }

        public void EnqueueEdsmEvent ( IDictionary<string, object> eventObject )
        {
            edsmService.EnqueueEvent( eventObject );
        }

        public async Task SyncFromStarMapServiceAsync ( DateTime? lastSync = null )
        {
            try
            {
                Logging.Info( "Syncing all flight logs from EDSM" );

                var flightLogs = await edsmService.getStarMapLogAsync(lastSync).ConfigureAwait(false);
                if ( flightLogs == null || flightLogs.Count == 0 )
                {
                    Logging.Debug( "EDSM flight logs are already synchronized; no new flight logs since last sync." );
                    return;
                }

                var comments = await edsmService.getStarMapCommentsAsync().ConfigureAwait(false);

                // Process in batches
                var total = flightLogs.Count;
                for ( var i = 0; i < total; i += StarMapService.syncBatchSize )
                {
                    var batchSize = Math.Min(StarMapService.syncBatchSize, total - i);
                    var batch = flightLogs.GetRange(i, batchSize);
                    await SyncEdsmLogBatchAsync( batch, comments ).ConfigureAwait( false );
                }

                Logging.Info( "EDSM flight logs synchronized" );
            }
            catch ( EDSMException ex )
            {
                Logging.Debug( "EDSM error received: " + ex.Message, ex );
            }
            catch ( OperationCanceledException ex )
            {
                Logging.Debug( "EDSM update canceled by user: " + ex.Message );
            }
        }

        // EDSM flight log synchronization (named star systems)
        public async Task<IList<StarSystem>> SyncFromStarMapServiceAsync ( IList<StarSystem> starSystems )
        {
            if ( starSystems.Count > 0 && !unitTesting )
            {
                try
                {
                    Logging.Debug( $"Syncing flight logs from EDSM for {starSystems.Count} system(s)." );
                    var flightLogs = await edsmService.getStarMapLogAsync(null, starSystems.Select(s => s.systemAddress).ToArray()).ConfigureAwait(false);
                    var comments = await edsmService.getStarMapCommentsAsync().ConfigureAwait(false);

                    if ( flightLogs?.Count > 0 )
                    {
                        foreach ( var starSystem in starSystems )
                        {
                            if ( starSystem?.systemname != null )
                            {
                                Logging.Debug( "Syncing star system " + starSystem.systemname + " from EDSM." );
                                foreach ( var flightLog in flightLogs )
                                {
                                    if ( flightLog.systemId64 == starSystem.systemAddress )
                                    {
                                        starSystem.visitLog.Add( flightLog.date );
                                    }
                                }
                                var comment = comments.FirstOrDefault(s => s.Key == starSystem.systemname);
                                if ( !string.IsNullOrEmpty( comment.Value ) )
                                {
                                    starSystem.comment = comment.Value;
                                }
                            }
                        }
                    }
                    else
                    {
                        Logging.Warn( $"No flight logs received for {starSystems.Count} system(s).." );
                    }
                }
                catch ( EDSMException edsme )
                {
                    Logging.Debug( "EDSM error received: " + edsme.Message, edsme );
                }
                catch ( ThreadAbortException e )
                {
                    Logging.Debug( "EDSM update stopped by user: " + e.Message );
                }
            }
            return starSystems;
        }

        public async Task SyncEdsmLogBatchAsync ( List<StarMapResponseLogEntry> flightLogBatch, Dictionary<string, string> comments )
        {
            var syncedSystems = new List<StarSystem>();
            var uniqueAddresses = flightLogBatch
                .Select( f => f.systemId64 )
                .Distinct()
                .ToArray();
            var batchSystems = await GetOrFetchStarSystemsAsync( uniqueAddresses, excludeStaleResults: false, fetchEdsmVisitsAndComments: false ).ConfigureAwait(false);
            var lookup = batchSystems.ToDictionary(s => s.systemAddress);
            var groupedLogs = flightLogBatch.GroupBy(f => f.systemId64);

            foreach ( var group in groupedLogs )
            {
                var address = group.Key;
                var firstLog = group.First();
                var systemName = firstLog.system;

                // Get or create the StarSystem
                if ( !lookup.TryGetValue( address, out var starSystem ) )
                {
                    starSystem = new StarSystem
                    {
                        systemAddress = address,
                        systemname = systemName,
                        lastupdated = firstLog.date
                    };
                    batchSystems.Add( starSystem );
                    lookup[ address ] = starSystem;
                }

                // Backfill missing body system addresses
                if ( starSystem.systemAddress == 0 && address > 0 )
                {
                    starSystem.systemAddress = address;
                    foreach ( var body in starSystem.bodies )
                    {
                        body.systemAddress = address;
                    }
                    starSystem.AddOrUpdateBodies( starSystem.bodies );
                }

                // Merge comments
                if ( comments.TryGetValue( systemName, out var comment ) )
                {
                    starSystem.comment = comment;
                }

                // Append all visit dates at once
                foreach ( var d in group.Select( f => f.date ) )
                {
                    starSystem.visitLog.Add( d );
                }

                syncedSystems.Add( starSystem );
            }

            await saveFromStarMapServiceAsync( syncedSystems ).ConfigureAwait( false );
        }

        private async Task saveFromStarMapServiceAsync ( List<StarSystem> syncSystems )
        {
            await starSystemRepository.SaveStarSystemsAsync( syncSystems, cts.Token ).ConfigureAwait( false );
            var starMapConfiguration = ConfigService.Instance.edsmConfiguration;
            starMapConfiguration.lastFlightLogSync = DateTime.UtcNow;
            ConfigService.Instance.edsmConfiguration = starMapConfiguration;
        }

        #endregion
    }
}
