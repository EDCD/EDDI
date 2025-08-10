using EddiConfigService;
using EddiDataDefinitions;
using EddiSpanshService;
using EddiStarMapService;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
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
        internal readonly StarMapService edsmService;
        internal readonly SpanshService spanshService;
        internal readonly StarSystemSqLiteRepository starSystemRepository;

        internal readonly FactionCache factionCache;
        internal readonly StarSystemCache starSystemCache;

        public static bool unitTesting;

        public DataProviderService ( StarMapService edsmService = null,
            SpanshService spanshService = null, StarSystemSqLiteRepository starSystemRepository = null )
        {
            factionCache = new FactionCache( 3600 ); // Keep a cache of factions for 1 hour
            starSystemCache = new StarSystemCache( 300 ); // Keep a cache of star systems for 5 minutes
            this.edsmService = edsmService ?? new StarMapService();
            this.spanshService = spanshService ?? new SpanshService();
            this.starSystemRepository = starSystemRepository ?? new StarSystemSqLiteRepository(unitTesting);
        }

        public List<NavWaypoint> GetTypeAheadSystems ( string systemName )
        {
            return spanshService.GetWaypointsBySystemName( systemName ).ToList();
        }

        [NotNull]
        public StarSystem GetOrCreateStarSystem ( ulong systemAddress, string systemName, bool fetchIfMissing = true, bool refreshIfOutdated = true, bool showMarketDetails = false )
        {
            return GetOrCreateStarSystems( new Dictionary<ulong, string> { { systemAddress, systemName } }, fetchIfMissing, refreshIfOutdated, showMarketDetails ).First();
        }

        [ItemNotNull]
        public List<StarSystem> GetOrCreateStarSystems ( Dictionary<ulong, string> requestedSystems,
            bool fetchIfMissing = true, bool refreshIfOutdated = true, bool showMarketDetails = false, bool fetchEdsmVisitsAndComments = true )
        {
            var results = new List<StarSystem>();
            if ( !requestedSystems.Any() ) { return results; }

            Dictionary<ulong, string> missingSystems () => requestedSystems
                .Where( k => results.All( s => s.systemAddress != k.Key ) )
                .ToDictionary( k => k.Key, v => v.Value );

            results = GetOrFetchStarSystems( missingSystems().Keys.ToArray(), fetchIfMissing, refreshIfOutdated, showMarketDetails, fetchEdsmVisitsAndComments ) ?? new List<StarSystem>();

            // Create a new system object for each name that isn't in the database and couldn't be fetched from a server
            var createdStarSystems = missingSystems()
                .Select( s => new StarSystem { systemname = s.Value, systemAddress = s.Key } )
                .ToList();
            results.AddRange( createdStarSystems );
            SaveStarSystems( createdStarSystems );
            return results;
        }

        public StarSystem GetOrFetchStarSystem ( ulong systemAddress, bool fetchIfMissing = true, bool refreshIfOutdated = true, bool showMarketDetails = false, bool fetchEdsmVisitsAndComments = true )
        {
            if ( systemAddress <= 0 ) { return null; }

            return GetOrFetchStarSystems( new[] { systemAddress }, fetchIfMissing, refreshIfOutdated, showMarketDetails, fetchEdsmVisitsAndComments )?.FirstOrDefault();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="systemName"></param>
        /// <param name="fetchIfMissing"></param>
        /// <param name="refreshIfOutdated"></param>
        /// <param name="showMarketDetails"></param>
        /// <param name="fetchEdsmVisitsAndComments"></param>
        /// <returns></returns>
        public StarSystem GetOrFetchStarSystem ( string systemName, bool fetchIfMissing = true, bool refreshIfOutdated = true, bool showMarketDetails = false, bool fetchEdsmVisitsAndComments = true )
        {
            if ( string.IsNullOrEmpty( systemName ) ) { return null; }

            // Fetch from cached systems
            if ( starSystemCache.TryGet( systemName, out var cachedSystem ) ) { return cachedSystem; }

            // Fetch from the local database. If there is more than one result, return the most recent result (by visits and then by update time)
            var sqlStarSystems = GetSqlStarSystems( new[] { systemName }, out _, refreshIfOutdated );
            if ( sqlStarSystems.Any() )
            {
                return sqlStarSystems
                    .OrderByDescending( s => s.lastVisitSeconds ?? 0 )
                    .ThenByDescending( s => s.updatedat ?? 0 )
                    .FirstOrDefault();
            }

            // Fetch from external data sources (when so instructed)
            var fetchedWaypoint = GetOrFetchSystemWaypoint( systemName );
            if ( fetchedWaypoint is null ) { return null; }
            return GetOrFetchStarSystems( new[] { fetchedWaypoint.systemAddress }, fetchIfMissing, refreshIfOutdated, showMarketDetails, fetchEdsmVisitsAndComments )?.FirstOrDefault();
        }

        public List<StarSystem> GetOrFetchStarSystems ( ulong[] systemAddresses, bool fetchIfMissing = true, bool refreshIfOutdated = true, bool showMarketDetails = false, bool fetchEdsmVisitsAndComments = true )
        {
            var results = new List<StarSystem>();
            if ( systemAddresses is null || !systemAddresses.Any() ) { return results; }

            ulong[] missingSystems () => systemAddresses.Where( k => results.All( s => s.systemAddress != k ) ).Distinct().ToArray();

            // Fetch from cached systems
            results.AddRange( starSystemCache.GetRange( missingSystems() ) );
            
            // Fetch from the local database
            results.AddRange( GetSqlStarSystems( missingSystems(), out var dbStarSystems, refreshIfOutdated ) );

            // Fetch from external data providers (when so instructed)
            if ( missingSystems().Any() && fetchIfMissing )
            {
                var fetchedSystems = FetchSystemsData( missingSystems(), showMarketDetails ) ?? new List<StarSystem>();
                if ( fetchedSystems?.Count > 0 )
                {
                    if ( fetchEdsmVisitsAndComments )
                    {
                        // Synchronize EDSM visits and comments
                        fetchedSystems = SyncFromStarMapServiceAsync( fetchedSystems ).GetAwaiter().GetResult();                        
                    }

                    // Update properties that aren't synced from the server and that we want to preserve
                    fetchedSystems = PreserveUnsyncedProperties( fetchedSystems, dbStarSystems );

                    // Update the `lastupdated` timestamps for the systems we have updated
                    foreach ( var starSystem in fetchedSystems ) { starSystem.lastupdated = DateTime.UtcNow; }

                    // Add the external data to our results
                    results.AddRange( fetchedSystems );
                    
                    // Save changes to our star systems
                    SaveStarSystems( fetchedSystems );
                }

                if ( missingSystems().Any() )
                {
                    Logging.Warn( "Unable to retrieve data on all requested star systems.", missingSystems() );
                }
            }

            return results;
        }

        public StarSystem GetOrFetchQuickStarSystem ( ulong systemAddress, bool fetchIfMissing = true )
        {
            if ( systemAddress <= 0 ) { return null; }

            return GetOrFetchQuickStarSystems( new[] { systemAddress }, fetchIfMissing )?.FirstOrDefault();
        }

        public List<StarSystem> GetOrFetchQuickStarSystems ( ulong[] systemAddresses, bool fetchIfMissing = true )
        {
            var results = new List<StarSystem>();
            if ( systemAddresses is null || !systemAddresses.Any() ) { return results; }

            ulong[] missingSystems () => systemAddresses.Where( k => results.All( s => s.systemAddress != k ) ).Distinct().ToArray();

            // Fetch from cached systems
            results.AddRange( starSystemCache.GetRange( missingSystems() ) );

            // Fetch from the local database
            results.AddRange( GetSqlStarSystems( missingSystems(), out _, false ) );

            // Fetch from external data providers (when so instructed)
            if ( missingSystems().Any() && fetchIfMissing )
            {
                // Add the external data to our results
                results.AddRange( spanshService.GetQuickStarSystems( missingSystems() ) );
            }

            if ( missingSystems().Any() )
            {
                Logging.Warn( "Unable to retrieve data on all requested star systems.", missingSystems() );
            }

            return results;
        }

        public List<StarSystem> GetOrFetchQuickStarSystems ( string[] systemNames, bool fetchIfMissing = true )
        {
            var systemAddresses = systemNames.AsParallel().Select( GetOrFetchSystemWaypoint )
                .Where( wp => wp != null ).Select( wp => wp.systemAddress ).ToArray();
            return GetOrFetchQuickStarSystems( systemAddresses.ToArray(), fetchIfMissing );
        }

        public NavWaypoint GetOrFetchSystemWaypoint ( string systemName )
        {
            if ( string.IsNullOrEmpty(systemName) ) { return null; }
            return GetOrFetchSystemWaypoints( new[] { systemName } ).FirstOrDefault();
        }

        public List<NavWaypoint> GetOrFetchSystemWaypoints ( string[] systemNames )
        {
            var results = new List<NavWaypoint>();
            if ( systemNames is null || !systemNames.Any() ) { return results; }

            string[] missingSystems () => systemNames.Where( k => results.All( s => s.systemName != k ) ).Distinct().ToArray();

            // Fetch from cached systems
            results.AddRange( starSystemCache.GetRange( missingSystems() ).Select( s => new NavWaypoint( s ) ) );

            // Fetch from Spansh
            var waypoints = missingSystems().AsParallel().Select( systemName =>
                spanshService.GetWaypointsBySystemName( systemName.Trim() ).FirstOrDefault( s =>
                    s.systemName.Equals( systemName, StringComparison.InvariantCultureIgnoreCase ) ) ).ToList();
            results.AddRange( waypoints );

            return results;
        }

        /// <summary>
        /// Find the station with the given system and station names from the Spansh Station Search API.
        /// </summary>
        /// <param name="fromSystemAddress"></param>
        /// <param name="fromMarketId"></param>
        /// <returns></returns>
        public NavWaypoint GetOrFetchStationWaypoint ( ulong fromSystemAddress, long fromMarketId )
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

            var system = GetOrFetchQuickStarSystem( fromSystemAddress );
            var station = system?.stations.FirstOrDefault( s => s.marketId == fromMarketId );
            if ( station != null )
            {
                return new NavWaypoint( system )
                {
                    stationName = station?.name,
                    marketID = station?.marketId
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
        public NavWaypoint GetOrFetchStationWaypoint ( string fromSystemName, long fromMarketId )
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
            var systemAddress = GetOrFetchSystemWaypoint( fromSystemName )?.systemAddress;
            if ( systemAddress != null )
            {

                var system = GetOrFetchQuickStarSystem( (ulong)systemAddress );
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

        internal List<StarSystem> GetSqlStarSystems ( ulong[] systemAddresses, out List<DatabaseStarSystem> dbStarSystems, bool refreshIfOutdated = true )
        {
            dbStarSystems = starSystemRepository.GetSqlStarSystems( systemAddresses );
            return DeserializeSqlStarSystems( dbStarSystems, refreshIfOutdated );
        }

        internal List<StarSystem> GetSqlStarSystems ( string[] systemNames, out List<DatabaseStarSystem> dbStarSystems, bool refreshIfOutdated = true )
        {
            dbStarSystems = starSystemRepository.GetSqlStarSystems( systemNames );
            return DeserializeSqlStarSystems( dbStarSystems, refreshIfOutdated );
        }

        private List<StarSystem> DeserializeSqlStarSystems ( List<DatabaseStarSystem> dbStarSystems, bool refreshIfOutdated )
        {
            var results = new List<StarSystem>();
            foreach ( var dbStarSystem in dbStarSystems )
            {
                if ( refreshIfOutdated && dbStarSystem.lastUpdated < DateTime.UtcNow.AddHours( -1 ) )
                {
                    // When specified, exclude stale data to force a refresh from another source
                    continue;
                }

                // Deserialize the result
                var result = DeserializeStarSystem(dbStarSystem.systemAddress, dbStarSystem.systemJson);

                // Exclude null results and results with missing coordinates (forcing a refresh from another source)
                if ( result?.x != null && result.y != null && result.z != null )
                {
                    results.Add( result );
                }
            }

            return results;
        }

        internal static List<StarSystem> PreserveUnsyncedProperties ( List<StarSystem> updatedSystems, List<DatabaseStarSystem> databaseStarSystems )
        {
            foreach ( var updatedSystem in updatedSystems ?? new List<StarSystem>() )
            {
                foreach ( var databaseStarSystem in databaseStarSystems ?? new List<DatabaseStarSystem>() )
                {
                    if ( updatedSystem.systemAddress == databaseStarSystem.systemAddress )
                    {
                        var oldStarSystem = Deserializtion.DeserializeData(databaseStarSystem.systemJson);

                        if ( oldStarSystem != null )
                        {
                            PreserveSystemProperties( updatedSystem, oldStarSystem );
                            PreserveBodyProperties( updatedSystem, oldStarSystem );
                            PreserveFactionProperties( updatedSystem, oldStarSystem );
                            // No station data needs to be carried over at this time.
                        }
                    }
                }
            }
            return updatedSystems ?? new List<StarSystem>();
        }

        internal static void PreserveSystemProperties ( StarSystem updatedSystem, IDictionary<string, object> oldStarSystem )
        {
            // Carry over StarSystem properties that we want to preserve
            updatedSystem.totalbodies = JsonParsing.getOptionalInt( oldStarSystem, "discoverableBodies" ) ?? 0;
            if ( oldStarSystem.TryGetValue( "visitLog", out object visitLogObj ) )
            {
                // Visits should sync from EDSM, but in case there is a problem with the connection we will also seed back in our old star system visit data
                if ( visitLogObj is List<object> oldVisitLog )
                {
                    foreach ( var obj in oldVisitLog )
                    {
                        if ( obj is DateTime visit )
                        {
                            // The SortedSet<T> class does not accept duplicate elements so we can safely add timestamps which may be duplicates of visits already reported from EDSM.
                            // If an item is already in the set, processing continues and no exception is thrown.
                            updatedSystem.visitLog.Add( visit );
                        }
                    }
                }
            }
        }

        internal static void PreserveBodyProperties ( StarSystem updatedSystem,
            IDictionary<string, object> oldStarSystem )
        {
            // Carry over Body properties that we want to preserve (e.g. exploration data)
            oldStarSystem.TryGetValue( "bodies", out object bodiesVal );
            if ( bodiesVal is List<object> bodyVals )
            {
                updatedSystem.PreserveBodyData( bodyVals, updatedSystem.bodies );
            }
        }

        internal static void PreserveFactionProperties ( StarSystem updatedSystem, IDictionary<string, object> oldStarSystem )
        {
            // Carry over Faction properties that we want to preserve (e.g. reputation data)
            oldStarSystem.TryGetValue( "factions", out object factionsVal );
            try
            {
                if ( factionsVal != null )
                {
                    var oldFactionsString = JsonConvert.SerializeObject(factionsVal);
                    Logging.Debug( $"Reading old faction properties from {updatedSystem.systemname} from database", oldFactionsString );
                    var oldFactions = JsonConvert.DeserializeObject<List<Faction>>(oldFactionsString);
                    if ( oldFactions?.Count > 0 )
                    {
                        foreach ( var updatedFaction in updatedSystem.factions )
                        {
                            foreach ( var oldFaction in oldFactions )
                            {
                                if ( updatedFaction.name == oldFaction.name )
                                {
                                    updatedFaction.myreputation = oldFaction.myreputation;
                                }
                            }
                        }
                    }
                }
            }
            catch ( Exception e ) when ( e is JsonReaderException || e is JsonWriterException || e is JsonException )
            {
                Logging.Error( "Failed to read commander faction reputation data for " + updatedSystem.systemname + " from database.", e );
            }
        }

        internal StarSystem DeserializeStarSystem ( ulong systemAddress, string data )
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
                    factionCache.AddOrUpdate(result.factions);
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

        public void SaveStarSystem ( StarSystem starSystem )
        {
            if ( starSystem == null ) { return; }
            SaveStarSystems( new List<StarSystem> { starSystem } );
        }

        public void SaveStarSystems ( List<StarSystem> starSystems )
        {
            if ( !starSystems.Any() ) { return; }

            // Update any faction and star systems in our short term faction and star system caches to minimize repeat deserialization
            foreach ( var starSystem in starSystems )
            {
                factionCache.AddOrUpdate( starSystem.factions );
                starSystemCache.AddOrUpdate( starSystem );
            }

            if ( unitTesting ) { return; }

            starSystemRepository.SaveStarSystems( starSystems );
        }

        public void LeaveStarSystem ( StarSystem system )
        {
            if ( system?.systemAddress <= 0 ) { return; }
            SaveStarSystem( system );
        }

        #endregion

        #region Spansh Endpoints

        public NavWaypointCollection FetchCarrierRoute ( string currentSystem, string[] targetSystems, long usedCarrierCapacity,
            bool calculateTotalFuelRequired = true, string[] refuelDestinations = null, bool fromUIquery = false )
        {
            return spanshService.GetCarrierRoute( currentSystem, targetSystems, usedCarrierCapacity,
                calculateTotalFuelRequired, refuelDestinations, fromUIquery );
        }

        public NavWaypointCollection FetchGalaxyRoute ( string currentSystem, string targetSystem, Ship ship,
            int? cargoCarriedTons = null, bool isSupercharged = false, bool useSupercharge = true,
            bool useInjections = false, bool excludeSecondary = false, bool fromUIquery = false )
        {
            return spanshService.GetGalaxyRoute( currentSystem, targetSystem, ship, cargoCarriedTons, isSupercharged,
                useSupercharge, useInjections, excludeSecondary, fromUIquery );
        }

        internal List<StarSystem> FetchSystemsData ( ulong[] systemAddresses, bool showMarketDetails = false )
        {
            if ( systemAddresses == null || systemAddresses.Length == 0 ) { return new List<StarSystem>(); }
            return spanshService.GetStarSystems( systemAddresses, showMarketDetails ).ToList();
        }

        /// <summary>
        /// Find the nearest station with specific station services from the Spansh Station Search API.
        /// </summary>
        /// <param name="fromX"></param>
        /// <param name="fromY"></param>
        /// <param name="fromZ"></param>
        /// <param name="filters"></param>
        /// <returns></returns>
        public NavWaypoint FetchStationWaypoint ( decimal fromX, decimal fromY, decimal fromZ, Dictionary<string, object> filters )
        {
            var data = spanshService.DistanceOrderedQuery( SpanshService.QueryGroup.stations, fromX, fromY, fromZ, filters );
            if ( data?[ "error" ] != null )
            {
                Logging.Warn( "Spansh API responded with: " + data[ "error" ] );
                return null;
            }
            return spanshService.ParseQuickStationWaypoint( data?[ "results" ]?.FirstOrDefault() );
        }

        /// <summary>
        /// Find the nearest body with specific parameters from the Spansh Station Search API.
        /// </summary>
        /// <param name="fromX"></param>
        /// <param name="fromY"></param>
        /// <param name="fromZ"></param>
        /// <param name="filters"></param>
        /// <returns></returns>
        public NavWaypoint FetchBodyWaypoint ( decimal fromX, decimal fromY, decimal fromZ, Dictionary<string, object> filters )
        {
            var data = spanshService.DistanceOrderedQuery( SpanshService.QueryGroup.bodies, fromX, fromY, fromZ, filters );
            if ( data?[ "error" ] != null )
            {
                Logging.Warn( "Spansh API responded with: " + data[ "error" ] );
                return null;
            }
            return ParseQuickBody( data?[ "results" ]?.FirstOrDefault() );

            NavWaypoint ParseQuickBody ( JToken bodyData )
            {
                if ( bodyData is null ) { return null; }

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
            if ( string.IsNullOrEmpty( factionName ) ) { return null; }

            // First try to fetch the faction from the cache
            if ( factionCache.TryGet( factionName, out var faction ) )
            {
                return faction;
            }

            // Next, try to fetch the faction from Spansh
            faction = await spanshService.GetFactionByNameAsync( factionName, presenceSystemName );

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
            if (string.IsNullOrEmpty(systemName)) { return null; }
            return await edsmService.GetStarMapTrafficAsync(systemName, edsmId).ConfigureAwait(false) ?? new Traffic();
        }

        [CanBeNull]
        public async Task<Traffic> GetSystemDeathsAsync ( string systemName, long? edsmId = null )
        {
            if (string.IsNullOrEmpty(systemName)) { return null; }
            return await edsmService.GetStarMapDeathsAsync(systemName, edsmId).ConfigureAwait( false ) ?? new Traffic();
        }

        [CanBeNull]
        public async Task<Traffic> GetSystemHostilityAsync( string systemName, long? edsmId = null )
        {
            if (string.IsNullOrEmpty(systemName)) { return null; }
            return await edsmService.GetStarMapHostilityAsync(systemName, edsmId).ConfigureAwait( false ) ?? new Traffic();
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
                    SyncEdsmLogBatch( batch, comments );
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
        public async Task<List<StarSystem>> SyncFromStarMapServiceAsync(List<StarSystem> starSystems)
        {
            if (starSystems.Count > 0)
            {
                try
                {
                    Logging.Debug( $"Syncing flight logs from EDSM for {starSystems.Count} system(s)." );
                    var flightLogs = await edsmService.getStarMapLogAsync(null, starSystems.Select(s => s.systemAddress).ToArray()).ConfigureAwait(false);
                    var comments = await edsmService.getStarMapCommentsAsync().ConfigureAwait(false);

                    if (flightLogs?.Count > 0)
                    {
                        foreach (var starSystem in starSystems)
                        {
                            if (starSystem?.systemname != null)
                            {
                                Logging.Debug("Syncing star system " + starSystem.systemname + " from EDSM.");
                                foreach (var flightLog in flightLogs)
                                {
                                    if (flightLog.systemId64 == starSystem.systemAddress)
                                    {
                                        starSystem.visitLog.Add(flightLog.date);
                                    }
                                }
                                var comment = comments.FirstOrDefault(s => s.Key == starSystem.systemname);
                                if (!string.IsNullOrEmpty(comment.Value))
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
                catch (EDSMException edsme)
                {
                    Logging.Debug("EDSM error received: " + edsme.Message, edsme);
                }
                catch (ThreadAbortException e)
                {
                    Logging.Debug("EDSM update stopped by user: " + e.Message);
                }
            }
            return starSystems;
        }

        public void SyncEdsmLogBatch ( List<StarMapResponseLogEntry> flightLogBatch, Dictionary<string, string> comments )
        {
            var syncedSystems = new List<StarSystem>();
            var uniqueAddresses = flightLogBatch
                .Select( f => f.systemId64 )
                .Distinct()
                .ToArray();
            var batchSystems = GetOrFetchStarSystems( uniqueAddresses, refreshIfOutdated: false, fetchEdsmVisitsAndComments: false );
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

            saveFromStarMapService( syncedSystems );
        }

        private void saveFromStarMapService(List<StarSystem> syncSystems)
        {
            starSystemRepository.SaveStarSystems(syncSystems);
            var starMapConfiguration = ConfigService.Instance.edsmConfiguration;
            starMapConfiguration.lastFlightLogSync = DateTime.UtcNow;
            ConfigService.Instance.edsmConfiguration = starMapConfiguration;
        }

        #endregion
    }
}
