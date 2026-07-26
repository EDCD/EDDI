using EddiDataDefinitions;
using EddiSpanshService;
using JetBrains.Annotations;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Utilities;

namespace EddiDataProviderService
{
    internal class StarSystemDataProvider
    {
        private readonly SpanshService spanshService;
        private readonly StarSystemSqLiteRepository starSystemRepository;
        private readonly StarSystemCache starSystemCache;
        private readonly FactionDataProvider factionDataProvider;
        private readonly EdsmDataProvider edsmDataProvider;
        private readonly CancellationTokenSource cts;
        private readonly bool unitTesting;
        private readonly ConcurrentDictionary<ulong, SemaphoreSlim> starSystemResolutionLocks = new();

        internal StarSystemDataProvider (
            SpanshService spanshService,
            StarSystemSqLiteRepository starSystemRepository,
            StarSystemCache starSystemCache,
            FactionDataProvider factionDataProvider,
            EdsmDataProvider edsmDataProvider,
            CancellationTokenSource cts,
            bool unitTesting )
        {
            this.spanshService = spanshService;
            this.starSystemRepository = starSystemRepository;
            this.starSystemCache = starSystemCache;
            this.factionDataProvider = factionDataProvider;
            this.edsmDataProvider = edsmDataProvider;
            this.cts = cts;
            this.unitTesting = unitTesting;
        }

        [NotNull]
        internal async Task<StarSystem> GetOrCreateStarSystemAsync (
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

        internal async Task<StarSystem> GetOrFetchStarSystemAsync (
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

        internal async Task<StarSystem> GetOrFetchStarSystemAsync (
            string systemName,
            bool fetchIfMissing = true,
            bool excludeStaleResults = true,
            bool showMarketDetails = false,
            bool fetchEdsmVisitsAndComments = true )
        {
            if ( string.IsNullOrEmpty( systemName ) ) { return null; }

            if ( starSystemCache.TryGet( systemName, out var cachedSystem ) )
            {
                factionDataProvider.ApplyFactionCache( cachedSystem );
                return cachedSystem;
            }

            var sqlStarSystems = await GetSqlStarSystemsAsync( [ systemName ], cts.Token ).ConfigureAwait( false );
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

            if ( !fetchIfMissing )
            {
                return null;
            }

            var fetchedWaypoint = await GetOrFetchSystemWaypointAsync( systemName ).ConfigureAwait( false );
            if ( fetchedWaypoint is null )
            { return null; }

            var starSystems = await GetOrFetchStarSystemsAsync( [ fetchedWaypoint.systemAddress ], fetchIfMissing,
                excludeStaleResults, showMarketDetails, fetchEdsmVisitsAndComments ).ConfigureAwait( false );
            return starSystems?.FirstOrDefault();
        }

        internal async Task<StarSystem> GetOrFetchQuickStarSystemAsync ( ulong systemAddress, bool fetchIfMissing = true )
        {
            if ( systemAddress <= 0 )
            { return null; }

            return ( await GetOrFetchQuickStarSystemsAsync( [ systemAddress ], fetchIfMissing ).ConfigureAwait( false ) )?.FirstOrDefault();
        }

        internal async Task<List<StarSystem>> GetOrFetchQuickStarSystemsAsync ( string[] systemNames, bool fetchIfMissing = true )
        {
            var waypointTasks = systemNames.AsParallel().Select( GetOrFetchSystemWaypointAsync );
            var waypoints = await Task.WhenAll( waypointTasks ).ConfigureAwait( false );
            var systemAddresses = waypoints.Where( wp => wp != null ).Select( wp => wp.systemAddress ).ToArray();
            return await GetOrFetchQuickStarSystemsAsync( systemAddresses.ToArray(), fetchIfMissing ).ConfigureAwait( false );
        }

        internal async Task<NavWaypoint> GetOrFetchSystemWaypointAsync ( string systemName )
        {
            if ( string.IsNullOrEmpty( systemName ) )
            { return null; }
            var wp = await GetOrFetchSystemWaypointsAsync( [ systemName ] ).ConfigureAwait( false );
            return wp.FirstOrDefault();
        }

        internal async Task<List<NavWaypoint>> GetOrFetchSystemWaypointsAsync ( string[] systemNames )
        {
            var results = new List<NavWaypoint>();
            if ( systemNames is null || systemNames.Length == 0 )
            { return results; }

            string[] missingSystems () => systemNames.Where( k => results.All( s => s.systemName != k ) ).Distinct().ToArray();

            results.AddRange( starSystemCache.GetRange( missingSystems() ).Select( s => new NavWaypoint( s ) ) );

            var waypoints = missingSystems().AsParallel().Select( async systemName =>
            {
                var wp = await spanshService.GetWaypointsBySystemNameAsync( systemName.Trim(), cts.Token ).ConfigureAwait( false );
                return wp.FirstOrDefault( s => s.systemName.Equals( systemName, StringComparison.InvariantCultureIgnoreCase ) );
            } ).ToList();
            results.AddRange( await Task.WhenAll( waypoints ).ConfigureAwait( false ) );

            return results;
        }

        internal async Task<NavWaypoint> GetOrFetchStationWaypointAsync ( ulong fromSystemAddress, long fromMarketId )
        {
            if ( starSystemCache.TryGet( fromSystemAddress, out var cachedStarSystem ) )
            {
                factionDataProvider.ApplyFactionCache( cachedStarSystem );
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

            var system = await GetOrFetchQuickStarSystemAsync( fromSystemAddress ).ConfigureAwait( false );
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

        internal async Task<NavWaypoint> GetOrFetchStationWaypointAsync ( string fromSystemName, long fromMarketId )
        {
            if ( !string.IsNullOrEmpty( fromSystemName ) && starSystemCache.TryGet( fromSystemName, out var cachedStarSystem ) )
            {
                factionDataProvider.ApplyFactionCache( cachedStarSystem );
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

            var wp = await GetOrFetchSystemWaypointAsync( fromSystemName ).ConfigureAwait( false );
            if ( wp?.systemAddress != null )
            {
                var system = await GetOrFetchQuickStarSystemAsync( wp.systemAddress ).ConfigureAwait( false );
                var station = system?.stations.FirstOrDefault( s => s.marketId == fromMarketId );
                return new NavWaypoint( system )
                {
                    stationName = station?.name,
                    marketID = station?.marketId
                };
            }

            return null;
        }

        internal Task<List<StarSystem>> GetOrFetchStarSystemsForEdsmSyncAsync ( ulong[] systemAddresses )
        {
            return GetOrFetchStarSystemsAsync(
                systemAddresses,
                excludeStaleResults: false,
                fetchEdsmVisitsAndComments: false );
        }

        internal async Task SaveStarSystemAsync ( StarSystem starSystem )
        {
            if ( starSystem == null )
            { return; }
            await SaveStarSystemsAsync( new List<StarSystem> { starSystem }, cts.Token ).ConfigureAwait( false );
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

        private async Task<StarSystem> GetOrFetchStarSystemCoreAsync (
            ulong systemAddress,
            bool fetchIfMissing = true,
            bool excludeStaleResults = true,
            bool showMarketDetails = false,
            bool fetchEdsmVisitsAndComments = true )
        {
            if ( starSystemCache.TryGet( systemAddress, out var cachedSystem ) )
            {
                factionDataProvider.ApplyFactionCache( cachedSystem );
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
                fetchedSystems = ( await edsmDataProvider.SyncFromStarMapServiceAsync( fetchedSystems ).ConfigureAwait( false ) ).ToList();
            }

            fetchedSystems = PreserveUnsyncedProperties( fetchedSystems, localDbSystems ).ToList();
            await factionDataProvider.HydrateFactionDataAsync( fetchedSystems ).ConfigureAwait( false );

            foreach ( var starSystem in fetchedSystems )
            {
                starSystem.lastupdated = DateTime.UtcNow;
            }

            await SaveStarSystemsAsync( fetchedSystems, cts.Token ).ConfigureAwait( false );

            return fetchedSystems.FirstOrDefault();
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

        private async Task<List<StarSystem>> GetOrFetchQuickStarSystemsAsync ( ulong[] systemAddresses, bool fetchIfMissing = true )
        {
            var results = new List<StarSystem>();
            if ( systemAddresses is null || systemAddresses.Length == 0 )
            { return results; }

            ulong[] missingSystems () => systemAddresses.Where( k => results.All( s => s.systemAddress != k ) ).Distinct().ToArray();

            results.AddRange( starSystemCache.GetRange( missingSystems() ) );
            results.AddRange( await GetSqlStarSystemsAsync( missingSystems() ).ConfigureAwait( false ) );

            if ( missingSystems().Length > 0 && fetchIfMissing )
            {
                results.AddRange( await spanshService.GetQuickStarSystemsAsync( missingSystems(), cts.Token ).ConfigureAwait( false ) );
            }

            await factionDataProvider.HydrateFactionDataAsync( results ).ConfigureAwait( false );
            foreach ( var result in results )
            {
                factionDataProvider.ApplyFactionCache( result );
            }

            if ( missingSystems().Length > 0 )
            {
                Logging.Warn( "Unable to retrieve data on all requested star systems.", missingSystems() );
            }

            return results;
        }

        private async Task<List<StarSystem>> GetSqlStarSystemsAsync ( ulong[] systemAddresses )
        {
            var dbStarSystems = await starSystemRepository.GetSqlStarSystemsAsync( systemAddresses, cts.Token ).ConfigureAwait( false );
            return await DeserializeSqlStarSystemsAsync( dbStarSystems ).ConfigureAwait( false );
        }

        private async Task<List<StarSystem>> GetSqlStarSystemsAsync ( string[] systemNames, CancellationToken cancellationToken )
        {
            var dbStarSystems = await starSystemRepository.GetSqlStarSystemsAsync( systemNames, cancellationToken ).ConfigureAwait( false );
            return await DeserializeSqlStarSystemsAsync( dbStarSystems ).ConfigureAwait( false );
        }

        private static bool IsStale ( StarSystem starSystem )
        {
            return starSystem.population > 0
                ? starSystem.lastupdated < DateTime.UtcNow.AddHours( -1 )
                : starSystem.lastupdated < DateTime.UtcNow.AddMonths( -1 );
        }

        private async Task<List<StarSystem>> DeserializeSqlStarSystemsAsync ( List<DatabaseStarSystem> dbStarSystems )
        {
            var results = new List<StarSystem>();
            foreach ( var dbStarSystem in dbStarSystems )
            {
                if ( HasKeyFields( dbStarSystem ) )
                {
                    var result = DeserializeStarSystem( dbStarSystem.systemAddress, dbStarSystem.systemJson );

                    if ( result is not null )
                    {
                        results.Add( result );
                    }
                }
            }

            await factionDataProvider.HydrateFactionDataAsync( results ).ConfigureAwait( false );
            foreach ( var result in results )
            {
                factionDataProvider.ApplyFactionCache( result );
                factionDataProvider.CacheFactionData( result.factions );
                starSystemCache.AddOrUpdate( result );
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
                    }
                }
            }
            return updatedSystems ?? new List<StarSystem>();
        }

        private static void PreserveSystemProperties ( StarSystem updatedSystem, StarSystem oldStarSystem )
        {
            updatedSystem.totalbodies = oldStarSystem.totalbodies;

            foreach ( var visit in oldStarSystem.visitLog )
            {
                updatedSystem.visitLog.Add( visit );
            }
        }

        private static void PreserveBodyProperties ( StarSystem updatedSystem, StarSystem oldStarSystem )
        {
            updatedSystem.PreserveBodyData( oldStarSystem.bodies.ToList(), updatedSystem.bodies );
        }

        private static void PreserveFactionProperties ( StarSystem updatedSystem, StarSystem oldStarSystem )
        {
            foreach ( var oldFaction in oldStarSystem.factions )
            {
                foreach ( var updatedFaction in updatedSystem.factions )
                {
                    if ( updatedFaction.name == oldFaction.name &&
                         updatedFaction.myreputation is null &&
                         oldFaction.myreputation != null )
                    {
                        updatedFaction.myreputation = oldFaction.myreputation;
                        updatedFaction.updatedAt = oldFaction.updatedAt;
                    }
                }
            }
        }

        private StarSystem DeserializeStarSystem ( ulong systemAddress, string data )
        {
            if ( systemAddress == 0 || data == string.Empty )
            { return null; }

            if ( starSystemCache.TryGet( systemAddress, out var cachedStarSystem ) )
            {
                return cachedStarSystem;
            }

            try
            {
                var result = JsonConvert.DeserializeObject<StarSystem>( data );
                if ( result == null )
                {
                    Logging.Info( "Failed to obtain system for address " + systemAddress + " from the SQLiteRepository" );
                }
                if ( result != null )
                {
                    factionDataProvider.ApplyFactionCache( result );
                    factionDataProvider.CacheFactionData( result.factions );
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

        private async Task SaveStarSystemsAsync ( IList<StarSystem> starSystems, CancellationToken cancellationToken )
        {
            if ( !starSystems.Any() )
            { return; }

            foreach ( var starSystem in starSystems )
            {
                factionDataProvider.NormalizeFactionReputationUpdatedAt( starSystem );
                factionDataProvider.ApplyFactionCache( starSystem );
                factionDataProvider.CacheFactionData( starSystem.factions );
                starSystemCache.AddOrUpdate( starSystem );
            }

            if ( unitTesting )
            { return; }

            await starSystemRepository.SaveStarSystemsAsync( starSystems, cancellationToken ).ConfigureAwait( false );
        }
    }
}
