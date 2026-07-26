using EddiDataDefinitions;
using EddiSpanshService;
using EddiStarMapService;
using JetBrains.Annotations;
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
        internal readonly SpanshService spanshService;
        internal readonly StarSystemSqLiteRepository starSystemRepository;

        private readonly CancellationTokenSource cts = new();
        private readonly EdsmDataProvider edsmDataProvider;
        private readonly FactionDataProvider factionDataProvider;
        private readonly StarSystemDataProvider starSystemDataProvider;

        private DataProviderService ( StarMapService edsmService = null,
            SpanshService spanshService = null, StarSystemSqLiteRepository starSystemRepository = null, bool unitTesting = false )
        {
            var factionCache = new FactionCache( 3600 ); // Keep a cache of faction details for 1 hour
            var starSystemCache = new StarSystemCache( 300 ); // Keep a cache of star systems for 5 minutes

            this.spanshService = spanshService;
            this.starSystemRepository = starSystemRepository;
            IsUnitTesting = unitTesting;

            factionDataProvider = new FactionDataProvider( starSystemRepository, factionCache, cts );
            edsmDataProvider = new EdsmDataProvider( edsmService, starSystemRepository, cts, unitTesting );
            starSystemDataProvider = new StarSystemDataProvider(
                spanshService,
                starSystemRepository,
                starSystemCache,
                factionDataProvider,
                edsmDataProvider,
                cts,
                unitTesting );
        }

        public bool IsUnitTesting { get; }

        public CancellationToken CancellationToken => cts.Token;

        public void CancelPendingRequests()
        {
            cts.Cancel();
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
            // Prefer type ahead system names for star systems we've visited.
            var localSystemNames = await starSystemRepository.GetStarSystemNamesAsync( input, cancellationToken ).ConfigureAwait( false );
            localSystemNames = localSystemNames
                .OrderBy( k => k.LevenshteinDistance( input, StringComparison.CurrentCultureIgnoreCase ) )
                .ToList();
            if ( localSystemNames.Count >= 10 )
            {
                return localSystemNames.Take( 10 ).ToList();
            }

            // Insufficient local results, use Spansh for a more comprehensive search.
            return await spanshService.GetTypeAheadSystemNamesAsync( input, cancellationToken ).ConfigureAwait( false );
        }

        [NotNull]
        public Task<StarSystem> GetOrCreateStarSystemAsync (
            ulong systemAddress,
            string systemName,
            bool fetchIfMissing = true,
            bool refreshIfOutdated = true,
            bool showMarketDetails = false )
        {
            return starSystemDataProvider.GetOrCreateStarSystemAsync(
                systemAddress,
                systemName,
                fetchIfMissing,
                refreshIfOutdated,
                showMarketDetails );
        }

        public Task<StarSystem> GetOrFetchStarSystemAsync (
            ulong systemAddress,
            bool fetchIfMissing = true,
            bool excludeStaleResults = true,
            bool showMarketDetails = false,
            bool fetchEdsmVisitsAndComments = true )
        {
            return starSystemDataProvider.GetOrFetchStarSystemAsync(
                systemAddress,
                fetchIfMissing,
                excludeStaleResults,
                showMarketDetails,
                fetchEdsmVisitsAndComments );
        }

        public Task<StarSystem> GetOrFetchStarSystemAsync (
            string systemName,
            bool fetchIfMissing = true,
            bool excludeStaleResults = true,
            bool showMarketDetails = false,
            bool fetchEdsmVisitsAndComments = true )
        {
            return starSystemDataProvider.GetOrFetchStarSystemAsync(
                systemName,
                fetchIfMissing,
                excludeStaleResults,
                showMarketDetails,
                fetchEdsmVisitsAndComments );
        }

        public Task<StarSystem> GetOrFetchQuickStarSystemAsync ( ulong systemAddress, bool fetchIfMissing = true )
        {
            return starSystemDataProvider.GetOrFetchQuickStarSystemAsync( systemAddress, fetchIfMissing );
        }

        public Task<List<StarSystem>> GetOrFetchQuickStarSystemsAsync ( string[] systemNames, bool fetchIfMissing = true )
        {
            return starSystemDataProvider.GetOrFetchQuickStarSystemsAsync( systemNames, fetchIfMissing );
        }

        public Task<NavWaypoint> GetOrFetchSystemWaypointAsync ( string systemName )
        {
            return starSystemDataProvider.GetOrFetchSystemWaypointAsync( systemName );
        }

        public Task<List<NavWaypoint>> GetOrFetchSystemWaypointsAsync ( string[] systemNames )
        {
            return starSystemDataProvider.GetOrFetchSystemWaypointsAsync( systemNames );
        }

        public Task<NavWaypoint> GetOrFetchStationWaypointAsync ( ulong fromSystemAddress, long fromMarketId )
        {
            return starSystemDataProvider.GetOrFetchStationWaypointAsync( fromSystemAddress, fromMarketId );
        }

        public Task<NavWaypoint> GetOrFetchStationWaypointAsync ( string fromSystemName, long fromMarketId )
        {
            return starSystemDataProvider.GetOrFetchStationWaypointAsync( fromSystemName, fromMarketId );
        }

        public Task SaveStarSystemAsync ( StarSystem starSystem )
        {
            return starSystemDataProvider.SaveStarSystemAsync( starSystem );
        }

        internal static IList<StarSystem> PreserveUnsyncedProperties (
            IList<StarSystem> updatedSystems,
            IList<StarSystem> databaseStarSystems )
        {
            return StarSystemDataProvider.PreserveUnsyncedProperties( updatedSystems, databaseStarSystems );
        }

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

        public Task<Faction> FetchFactionByNameAsync ( string factionName, string presenceSystemName = null )
        {
            return factionDataProvider.FetchFactionByNameAsync( factionName, spanshService, cts.Token, presenceSystemName );
        }

        #endregion

        #region EDSM Endpoints

        [CanBeNull]
        public Task<Traffic> GetSystemTrafficAsync ( string systemName, long? edsmId = null )
        {
            return edsmDataProvider.GetSystemTrafficAsync( systemName, edsmId );
        }

        [CanBeNull]
        public Task<Traffic> GetSystemDeathsAsync ( string systemName, long? edsmId = null )
        {
            return edsmDataProvider.GetSystemDeathsAsync( systemName, edsmId );
        }

        [CanBeNull]
        public Task<Traffic> GetSystemHostilityAsync ( string systemName, long? edsmId = null )
        {
            return edsmDataProvider.GetSystemHostilityAsync( systemName, edsmId );
        }

        public bool TryStartEdsmJournal ()
        {
            return edsmDataProvider.TryStartEdsmJournal();
        }

        public Task StopEdsmJournalAsync ()
        {
            return edsmDataProvider.StopEdsmJournalAsync();
        }

        public Task<List<string>> GetIgnoredEdsmEventsAsync ()
        {
            return edsmDataProvider.GetIgnoredEdsmEventsAsync();
        }

        public void EnqueueEdsmEvent ( IDictionary<string, object> eventObject )
        {
            edsmDataProvider.EnqueueEdsmEvent( eventObject );
        }

        public Task SyncFromStarMapServiceAsync ( DateTime? lastSync = null )
        {
            return edsmDataProvider.SyncFromStarMapServiceAsync( lastSync, starSystemDataProvider.GetOrFetchStarSystemsForEdsmSyncAsync );
        }

        public Task<IList<StarSystem>> SyncFromStarMapServiceAsync ( IList<StarSystem> starSystems )
        {
            return edsmDataProvider.SyncFromStarMapServiceAsync( starSystems );
        }

        public Task SyncEdsmLogBatchAsync ( List<StarMapResponseLogEntry> flightLogBatch, Dictionary<string, string> comments )
        {
            return edsmDataProvider.SyncEdsmLogBatchAsync(
                flightLogBatch,
                comments,
                starSystemDataProvider.GetOrFetchStarSystemsForEdsmSyncAsync );
        }

        #endregion
    }
}
