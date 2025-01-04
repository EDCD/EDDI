using EddiBgsService;
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
using Utilities;

[assembly: InternalsVisibleTo( "Tests" )]
namespace EddiDataProviderService
{
    /// <summary>Access data services. Prefer our cache and local database wherever possible.</summary>
    public class DataProviderService
    {
        internal readonly BgsService bgsService;
        internal readonly StarMapService edsmService;
        internal readonly SpanshService spanshService;
        internal readonly StarSystemSqLiteRepository starSystemRepository;
        internal readonly StarSystemCache starSystemCache;

        public static bool unitTesting;

        public DataProviderService ( BgsService bgsService = null, StarMapService edsmService = null,
            SpanshService spanshService = null, StarSystemSqLiteRepository starSystemRepository = null )
        {
            starSystemCache = new StarSystemCache( 300 ); // Keep a cache of star systems for 5 minutes
            this.bgsService = bgsService ?? new BgsService();
            this.edsmService = edsmService ?? new StarMapService();
            this.spanshService = spanshService ?? new SpanshService();
            this.starSystemRepository = starSystemRepository ?? new StarSystemSqLiteRepository();
        }

        public List<string> GetTypeAheadSystems ( string systemName )
        {
            return spanshService.GetWaypointsBySystemName( systemName ).Select(s => s.systemName).ToList();
        }

        public List<StarSystem> GetOrCreateStarSystems ( Dictionary<ulong, string> requestedSystems, bool refreshIfOutdated = true, bool showMarketDetails = false )
        {
            var results = new List<StarSystem>();
            if ( !requestedSystems.Any() ) { return new List<StarSystem>(); }

            var missingSystems = requestedSystems.Where( k => results.All( s => s.systemAddress != k.Key ) )
                .ToDictionary( k => k.Key, v => v.Value );

            results = GetOrFetchStarSystems( missingSystems.Keys.ToArray(), true, refreshIfOutdated, showMarketDetails ) ?? new List<StarSystem>();

            // Create a new system object for each name that isn't in the database and couldn't be fetched from a server
            var createdStarSystems = missingSystems
                .Select( s => new StarSystem { systemname = s.Value, systemAddress = s.Key } )
                .ToList();
            results.AddRange( createdStarSystems );
            SaveStarSystems( createdStarSystems );
            return results;
        }

        public StarSystem GetOrFetchStarSystem ( ulong systemAddress, string systemName = null, bool fetchIfMissing = true, bool refreshIfOutdated = true, bool showMarketDetails = false )
        {
            if ( systemAddress <= 0 ) { return null; }

            return GetOrFetchStarSystems( new[] { systemAddress }, fetchIfMissing, refreshIfOutdated, showMarketDetails )?.FirstOrDefault();
        }

        public StarSystem GetOrFetchStarSystem ( string systemName, bool fetchIfMissing = true, bool refreshIfOutdated = true, bool showMarketDetails = false )
        {
            if ( string.IsNullOrEmpty( systemName ) ) { return null; }
            var system = GetOrFetchSystemWaypoint( systemName );
            if ( system is null ) { return null; }
            return GetOrFetchStarSystems( new[] { system.systemAddress }, fetchIfMissing, refreshIfOutdated, showMarketDetails )?.FirstOrDefault();
        }

        public List<StarSystem> GetOrFetchStarSystems ( ulong[] systemAddresses, bool fetchIfMissing = true, bool refreshIfOutdated = true, bool showMarketDetails = false )
        {
            var results = new List<StarSystem>();
            if ( systemAddresses is null || !systemAddresses.Any() ) { return results; }

            ulong[] missingSystems () => systemAddresses.Where( k => results.All( s => s.systemAddress != k ) ).Distinct().ToArray();

            // Fetch from cached systems
            results.AddRange( starSystemCache.GetRange( missingSystems() ) );
            
            // Fetch from the local database
            results.AddRange( GetSqlStarSystems( systemAddresses.ToArray(), out var dbStarSystems, refreshIfOutdated ) );

            // Fetch from external data providers (when so instructed)
            if ( missingSystems().Any() && fetchIfMissing )
            {
                var fetchedSystems = FetchSystemsData( missingSystems(), showMarketDetails );
                if ( fetchedSystems?.Count > 0 )
                {
                    // Synchronize EDSM visits and comments
                    fetchedSystems = syncFromStarMapService( fetchedSystems );

                    // Update properties that aren't synced from the server and that we want to preserve
                    fetchedSystems = PreserveUnsyncedProperties( fetchedSystems, dbStarSystems );

                    // Update the `lastupdated` timestamps for the systems we have updated
                    foreach ( var starSystem in fetchedSystems ) { starSystem.lastupdated = DateTime.UtcNow; }

                    // Add the external data to our results
                    results.AddRange( fetchedSystems );
                    
                    // Save changes to our star systems
                    starSystemRepository.SaveStarSystems( fetchedSystems );
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
            var results = new List<StarSystem>();
            dbStarSystems = starSystemRepository.GetSqlStarSystems( systemAddresses );

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
            if ( updatedSystems is null ) { return new List<StarSystem>(); }
            foreach ( var updatedSystem in updatedSystems )
            {
                foreach ( var databaseStarSystem in databaseStarSystems )
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
            return updatedSystems;
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

        internal static void PreserveBodyProperties ( StarSystem updatedSystem, IDictionary<string, object> oldStarSystem )
        {
            // Carry over Body properties that we want to preserve (e.g. exploration data)
            oldStarSystem.TryGetValue( "bodies", out object bodiesVal );
            try
            {
                if ( bodiesVal != null )
                {
                    var oldBodiesString = JsonConvert.SerializeObject(bodiesVal);
                    Logging.Debug( $"Reading old body properties from {updatedSystem.systemname} from database", oldBodiesString );
                    var oldBodies = JsonConvert.DeserializeObject<List<Body>>(oldBodiesString);
                    updatedSystem.PreserveBodyData( oldBodies, updatedSystem.bodies );
                }
            }
            catch ( Exception e ) when ( e is JsonReaderException || e is JsonWriterException || e is JsonException )
            {
                Logging.Error( $"Failed to read exploration data for bodies in {updatedSystem.systemname} from database.", e );
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
            starSystemRepository.SaveStarSystems( new List<StarSystem> { starSystem } );
        }

        public void SaveStarSystems ( List<StarSystem> starSystems )
        {
            if ( !starSystems.Any() || unitTesting ) { return; }

            // Update any star systems in our short term star system cache to minimize repeat deserialization
            foreach ( var starSystem in starSystems )
            {
                starSystemCache.Remove( starSystem.systemAddress );
                starSystemCache.AddOrUpdate( starSystem );
            }

            starSystemRepository.SaveStarSystems( starSystems );
        }

        public void LeaveStarSystem ( StarSystem system )
        {
            if ( system?.systemAddress <= 0 ) { return; }
            SaveStarSystem( system );
        }

        #endregion

        #region EliteBGS Endpoints

        [CanBeNull]
        public Faction FetchFactionByName ( string factionName, string presenceSystemName = null )
        {
            // While it is possible to obtain faction data from Spansh, Spansh does not have a dedicated endpoint for faction data.
            // In benchmarking conducted Dec. 2024 it was both slower and less comprehensive than EliteBGS.
            return bgsService.GetFactionByName( factionName, presenceSystemName );
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
            return spanshService.ParseQuickStation( data?[ "results" ]?.FirstOrDefault() );
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

        #endregion

        #region EDSM Endpoints

        public Traffic GetSystemTraffic(string systemName, long? edsmId = null)
        {
            if (string.IsNullOrEmpty(systemName)) { return null; }
            return edsmService.GetStarMapTraffic(systemName, edsmId) ?? new Traffic();
        }

        public Traffic GetSystemDeaths(string systemName, long? edsmId = null)
        {
            if (string.IsNullOrEmpty(systemName)) { return null; }
            return edsmService.GetStarMapDeaths(systemName, edsmId) ?? new Traffic();
        }

        public Traffic GetSystemHostility(string systemName, long? edsmId = null)
        {
            if (string.IsNullOrEmpty(systemName)) { return null; }
            return edsmService.GetStarMapHostility(systemName, edsmId) ?? new Traffic();
        }

        // EDSM flight log synchronization
        public void syncFromStarMapService(DateTime? lastSync = null)
        {
            if (edsmService != null)
            {
                try
                {
                    Logging.Info( "Syncing all flight logs from EDSM" );
                    var flightLogs = edsmService.getStarMapLog(lastSync);
                    if (flightLogs?.Count > 0)
                    {
                        var comments = edsmService.getStarMapComments();
                        int total = flightLogs.Count;
                        int i = 0;

                        while (i < total)
                        {
                            int batchSize = Math.Min(total, StarMapService.syncBatchSize);
                            List<StarMapResponseLogEntry> flightLogBatch = flightLogs.Skip(i).Take(batchSize).ToList();
                            syncEdsmLogBatch(flightLogBatch, comments);
                            i += batchSize;
                        }
                        Logging.Info( "EDSM flight logs synchronized" );
                    }
                    else
                    {
                        Logging.Debug( "EDSM flight logs are already synchronized, no new flight logs since last sync." );
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
        }

        // EDSM flight log synchronization (named star systems)
        public List<StarSystem> syncFromStarMapService(List<StarSystem> starSystems)
        {
            if (edsmService != null && edsmService.EdsmCredentialsSet() && starSystems.Count > 0)
            {
                try
                {
                    Logging.Debug( $"Syncing flight logs from EDSM for {starSystems.Count} system(s)." );
                    List<StarMapResponseLogEntry> flightLogs = edsmService.getStarMapLog(null, starSystems.Select(s => s.systemAddress).ToArray());
                    Dictionary<string, string> comments = edsmService.getStarMapComments();

                    if (flightLogs?.Count > 0)
                    {
                        foreach (StarSystem starSystem in starSystems)
                        {
                            if (starSystem?.systemname != null)
                            {
                                Logging.Debug("Syncing star system " + starSystem.systemname + " from EDSM.");
                                foreach (StarMapResponseLogEntry flightLog in flightLogs)
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

        public void syncEdsmLogBatch(List<StarMapResponseLogEntry> flightLogBatch, Dictionary<string, string> comments)
        {
            var syncedSystems = new List<StarSystem>();
            var flightLogSystems = flightLogBatch.ToDictionary(k => k.systemId64, v => v.system);
            var batchSystems = GetOrCreateStarSystems(flightLogSystems, false );
            foreach (var starSystem in batchSystems)
            {
                if (starSystem != null)
                {
                    foreach (var flightLog in flightLogBatch.Where(log => log.system == starSystem.systemname))
                    {
                        // Fill missing SystemAddresses
                        if ( starSystem.systemAddress == 0 )
                        {
                            if ( flightLog.systemId64 > 0 )
                            {
                                starSystem.systemAddress = flightLog.systemId64;
                                var bodies = starSystem.bodies.Where(b => b.systemAddress is null).ToList();
                                bodies.AsParallel().ForAll( b =>
                                {
                                    b.systemAddress = flightLog.systemId64;
                                } );
                                foreach ( var body in starSystem.bodies )
                                {
                                    body.systemAddress = flightLog.systemId64;
                                }
                                starSystem.AddOrUpdateBodies( bodies );
                            }
                            else
                            {
                                // Skip flight log entries that are missing a SystemAddress property
                                // (and consequently may not be stored as unique items in our database)
                                continue;
                            }
                        }

                        // Update Comments
                        if ( comments.TryGetValue( flightLog.system, out var comment ) )
                        {
                            starSystem.comment = comment;
                        }

                        // Update Visit Log
                        starSystem.visitLog.Add(flightLog.date);

                        syncedSystems.Add( starSystem );
                    }
                }
            }
            saveFromStarMapService(syncedSystems);
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
