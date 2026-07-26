using EddiConfigService;
using EddiDataDefinitions;
using EddiStarMapService;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Utilities;

namespace EddiDataProviderService
{
    internal class EdsmDataProvider
    {
        private readonly StarMapService edsmService;
        private readonly StarSystemSqLiteRepository starSystemRepository;
        private readonly CancellationTokenSource cts;
        private readonly bool unitTesting;

        internal EdsmDataProvider (
            StarMapService edsmService,
            StarSystemSqLiteRepository starSystemRepository,
            CancellationTokenSource cts,
            bool unitTesting )
        {
            this.edsmService = edsmService;
            this.starSystemRepository = starSystemRepository;
            this.cts = cts;
            this.unitTesting = unitTesting;
        }

        [CanBeNull]
        internal async Task<Traffic> GetSystemTrafficAsync ( string systemName, long? edsmId = null )
        {
            if ( string.IsNullOrEmpty( systemName ) )
            { return null; }
            return await edsmService.GetStarMapTrafficAsync( systemName, edsmId ).ConfigureAwait( false ) ?? new Traffic();
        }

        [CanBeNull]
        internal async Task<Traffic> GetSystemDeathsAsync ( string systemName, long? edsmId = null )
        {
            if ( string.IsNullOrEmpty( systemName ) )
            { return null; }
            return await edsmService.GetStarMapDeathsAsync( systemName, edsmId ).ConfigureAwait( false ) ?? new Traffic();
        }

        [CanBeNull]
        internal async Task<Traffic> GetSystemHostilityAsync ( string systemName, long? edsmId = null )
        {
            if ( string.IsNullOrEmpty( systemName ) )
            { return null; }
            return await edsmService.GetStarMapHostilityAsync( systemName, edsmId ).ConfigureAwait( false ) ?? new Traffic();
        }

        internal bool TryStartEdsmJournal ()
        {
            edsmService?.StartJournalSync();
            return edsmService != null;
        }

        internal async Task StopEdsmJournalAsync ()
        {
            if ( edsmService != null )
            {
                await edsmService.StopJournalAsync().ConfigureAwait( false );
            }
        }

        internal Task<List<string>> GetIgnoredEdsmEventsAsync ()
        {
            return edsmService?.getIgnoredEventsAsync();
        }

        internal void EnqueueEdsmEvent ( IDictionary<string, object> eventObject )
        {
            edsmService.EnqueueEvent( eventObject );
        }

        internal async Task SyncFromStarMapServiceAsync (
            DateTime? lastSync,
            Func<ulong[], Task<List<StarSystem>>> resolveStarSystemsAsync )
        {
            try
            {
                Logging.Info( "Syncing all flight logs from EDSM" );

                var flightLogs = await edsmService.getStarMapLogAsync( lastSync ).ConfigureAwait( false );
                if ( flightLogs == null || flightLogs.Count == 0 )
                {
                    Logging.Debug( "EDSM flight logs are already synchronized; no new flight logs since last sync." );
                    return;
                }

                var comments = await edsmService.getStarMapCommentsAsync().ConfigureAwait( false );

                var total = flightLogs.Count;
                for ( var i = 0; i < total; i += StarMapService.syncBatchSize )
                {
                    var batchSize = Math.Min( StarMapService.syncBatchSize, total - i );
                    var batch = flightLogs.GetRange( i, batchSize );
                    await SyncEdsmLogBatchAsync( batch, comments, resolveStarSystemsAsync ).ConfigureAwait( false );
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

        internal async Task<IList<StarSystem>> SyncFromStarMapServiceAsync ( IList<StarSystem> starSystems )
        {
            if ( starSystems.Count > 0 && !unitTesting )
            {
                try
                {
                    Logging.Debug( $"Syncing flight logs from EDSM for {starSystems.Count} system(s)." );
                    var flightLogs = await edsmService.getStarMapLogAsync( null, starSystems.Select( s => s.systemAddress ).ToArray() ).ConfigureAwait( false );
                    var comments = await edsmService.getStarMapCommentsAsync().ConfigureAwait( false );

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
                                var comment = comments.FirstOrDefault( s => s.Key == starSystem.systemname );
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

        internal async Task SyncEdsmLogBatchAsync (
            List<StarMapResponseLogEntry> flightLogBatch,
            Dictionary<string, string> comments,
            Func<ulong[], Task<List<StarSystem>>> resolveStarSystemsAsync )
        {
            var syncedSystems = new List<StarSystem>();
            var uniqueAddresses = flightLogBatch
                .Select( f => f.systemId64 )
                .Distinct()
                .ToArray();
            var batchSystems = await resolveStarSystemsAsync( uniqueAddresses ).ConfigureAwait( false );
            var lookup = batchSystems.ToDictionary( s => s.systemAddress );
            var groupedLogs = flightLogBatch.GroupBy( f => f.systemId64 );

            foreach ( var group in groupedLogs )
            {
                var address = group.Key;
                var firstLog = group.First();
                var systemName = firstLog.system;

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

                if ( starSystem.systemAddress == 0 && address > 0 )
                {
                    starSystem.systemAddress = address;
                    foreach ( var body in starSystem.bodies )
                    {
                        body.systemAddress = address;
                    }
                    starSystem.AddOrUpdateBodies( starSystem.bodies );
                }

                if ( comments.TryGetValue( systemName, out var comment ) )
                {
                    starSystem.comment = comment;
                }

                foreach ( var d in group.Select( f => f.date ) )
                {
                    starSystem.visitLog.Add( d );
                }

                syncedSystems.Add( starSystem );
            }

            await SaveFromStarMapServiceAsync( syncedSystems ).ConfigureAwait( false );
        }

        private async Task SaveFromStarMapServiceAsync ( List<StarSystem> syncSystems )
        {
            await starSystemRepository.SaveStarSystemsAsync( syncSystems, cts.Token ).ConfigureAwait( false );
            var starMapConfiguration = ConfigService.Instance.edsmConfiguration;
            starMapConfiguration.lastFlightLogSync = DateTime.UtcNow;
            ConfigService.Instance.edsmConfiguration = starMapConfiguration;
        }
    }
}
