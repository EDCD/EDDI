using EddiConfigService;
using EddiConfigService.Configurations;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Utilities;
using Exception = System.Exception;
using OperationCanceledException = System.OperationCanceledException;

namespace EddiStarMapService
{
    public partial class StarMapService
    {
        // The timeout for journal events is lengthened to 30 seconds (as recommended by Anthor in private message)
        private const int JournalTimeoutMilliseconds = 30000;

        // Pause a short time to allow any initial events to build in the queue before our first EDSM responder event sync
        private const int startupDelayMilliSeconds = 1000 * 10; // 10 seconds

        // The minimum interval between EDSM responder event syncs
        private const int syncIntervalMilliSeconds = 5000; // 5 seconds

        private IEdsmHttpClient edsmJournalHttpClient { get; }

        public void StartJournalSync ()
        {
            if ( syncCancellationTS is null || syncCancellationTS.IsCancellationRequested )
            {
                syncCancellationTS = new CancellationTokenSource();
                Logging.Debug( "Enabling EDSM Responder event sync." );
                BackgroundJournalSyncAsync().SafeFireAndForget( ex => Logging.Error( ex.Message, ex ) );
            }
        }

        public async Task StopJournalAsync ()
        {
            if ( syncCancellationTS != null && !syncCancellationTS.IsCancellationRequested )
            {
                Logging.Debug( "Stopping EDSM Responder event sync." );
                syncCancellationTS.Cancel();
                queuedEvents.CompleteAdding();
                // Clean up by sending anything left in the queue.
                await SendEventsAsync( queuedEvents.ToList() ).ConfigureAwait(false);
            }
        }

        private async Task BackgroundJournalSyncAsync ()
        {
            try
            {
                // Pause a short time to allow any initial events to build in the queue before our first sync
                await Task.Delay( startupDelayMilliSeconds, syncCancellationTS.Token ).ConfigureAwait( false );
                
                // The `GetConsumingEnumerable` method blocks the thread while the underlying collection is empty
                // If we haven't extracted events to send to EDSM, this will wait / pause background sync until `queuedEvents` is no longer empty.
                var holdingQueue = new List<IDictionary<string, object>>();
                try
                {
                    Logging.Debug( "Sending queued events to EDSM: ", queuedEvents );
                    foreach ( var pendingEvent in queuedEvents.GetConsumingEnumerable(
                                 syncCancellationTS.Token ) )
                    {
                        holdingQueue.Add( pendingEvent );

                        // If we've reached the batch size or the queue is empty, send the batch
                        if ( queuedEvents.Count == 0 )
                        {
                            // Once we hit zero queued events, wait a couple more seconds for any concurrent events to register
                            await Task.Delay( 2000, syncCancellationTS.Token ).ConfigureAwait(false);
                            if ( queuedEvents.Count > 0 )
                            {
                                continue;
                            }

                            // No additional events registered, send any events we have in our holding queue
                            if ( holdingQueue.Count > 0 )
                            {
                                var sendingQueue = holdingQueue.ToList();
                                await SendEventsAsync( sendingQueue ).ConfigureAwait(false);
                                await Task.Delay( syncIntervalMilliSeconds, syncCancellationTS.Token ).ConfigureAwait(false);
                            }
                        }
                    }
                }
                catch ( OperationCanceledException )
                {
                    // Operation was cancelled. Return any events we've extracted back to the primary queue.
                    if ( !queuedEvents.IsAddingCompleted )
                    {
                        foreach ( var pendingEvent in holdingQueue )
                        {
                            queuedEvents.Add( pendingEvent );
                        }
                    }
                }
                catch ( Exception ex )
                {
                    Logging.Error( ex.Message, ex );
                }

                holdingQueue.Clear();
            }
            catch ( TaskCanceledException )
            {
                // Task cancelled. Nothing to do here.
            }
        }

        public void EnqueueEvent ( IDictionary<string, object> eventObject )
        {
            if ( eventObject is null || queuedEvents.IsAddingCompleted )
            { return; }
            if ( currentGameVersion != null && currentGameVersion < minGameVersion )
            { return; }

            queuedEvents.Add( eventObject );
        }

        private void ReEnqueueEvents ( IEnumerable<IDictionary<string, object>> eventData )
        {
            if ( eventData is null || queuedEvents.IsAddingCompleted )
            { return; }
            // Re-enqueue the data so we can send it again later (preserving order as best we can).
            var newQueue = eventData.ToList();
            while ( queuedEvents.TryTake( out var eventObject ) )
            {
                newQueue.Add( eventObject );
            }
            foreach ( var eventObject in newQueue )
            {
                queuedEvents.Add( eventObject );
            }
        }

        private async Task SendEventsAsync ( List<IDictionary<string, object>> queue )
        {
            if ( currentGameVersion is null ) { return; } // Wait until we have a game version before sending events
            var starMapConfiguration = ConfigService.Instance.edsmConfiguration;
            await SendEventBatchAsync( queue, starMapConfiguration ).ConfigureAwait(false);
        }

        private async Task SendEventBatchAsync ( List<IDictionary<string, object>> eventData, StarMapConfiguration starMapConfiguration )
        {
            // The EDSM responder has a `gameIsBeta` flag that it checks prior to sending data via this method.  
            if ( !TrySetEdsmCredentials() ) { return; }

            // Filter any stale data
            eventData = eventData
                .Where( e => JsonParsing.getDateTime( "timestamp", e ) > starMapConfiguration.lastJournalSync )
                .ToList();
            if ( eventData.Count == 0 ) { return; }

            var url = $"{baseUrl}api-journal-v1";
            var maxRetries = 3;
            var delay = syncIntervalMilliSeconds; // Initial delay in milliseconds
            for ( var retry = 0; retry < maxRetries; retry++ )
            {
                using ( var httpContent = new MultipartFormDataContent() )
                {
                    httpContent.Add( new StringContent( commanderName ), "commanderName" );
                    httpContent.Add( new StringContent( apiKey ), "apiKey" );
                    httpContent.Add( new StringContent( Constants.EDDI_NAME ), "fromSoftware" );
                    httpContent.Add( new StringContent( Constants.EDDI_VERSION.ToString() ), "fromSoftwareVersion" );
                    httpContent.Add( new StringContent( gameVersion ), "fromGameVersion" );
                    httpContent.Add( new StringContent( gameBuild ), "fromGameBuild" );
                    httpContent.Add( new StringContent( JsonConvert.SerializeObject( eventData ), Encoding.UTF8, "application/json" ), "message" );

                    try
                    {
                        Logging.Debug( "Sending message to EDSM: " + url, httpContent );
                        var responseJson = await edsmJournalHttpClient.PostAsync( url, httpContent ).ConfigureAwait(false);
                        var response = responseJson is null
                            ? null
                            : JsonConvert.DeserializeObject<StarMapLogResponse>( responseJson );

                        if ( response is null )
                        {
                            ReEnqueueEvents( eventData );
                        }
                        else if ( response.msgnum >= 100 && response.msgnum <= 104 )
                        {
                            // 100 -  Everything went fine! 
                            // 101 -  The journal message was already processed in our database. 
                            // 102 -  The journal message was already in a newer version in our database. 
                            // 103 -  Duplicate event request (cached data already reported from another software client). 
                            // 104 -  Commander is in a crew session without being the captain. As such we do not register any logs. 
                            starMapConfiguration.lastJournalSync = eventData
                                .Select( e => JsonParsing.getDateTime( "timestamp", e ) )
                                .Max();
                            ConfigService.Instance.edsmConfiguration = starMapConfiguration;
                        }

                        if ( response?.msgnum != 100 )
                        {
                            if ( !string.IsNullOrEmpty( response?.msg ) )
                            {
                                Logging.Warn( "EDSM responded with: " + response.msg );
                            }
                            else
                            {
                                Logging.Warn( "EDSM responded with: " + JsonConvert.SerializeObject( response ) );
                            }
                        }
                    }
                    catch ( WebException wex )
                    {
                        Logging.Warn( $"Attempt {retry + 1} failed: {wex.Message}", wex );
                        if ( retry == ( maxRetries - 1 ) )
                        {
                            Logging.Warn( "Failed to send events to EDSM", wex );
                        }
                    }

                    await Task.Delay( delay ).ConfigureAwait(false);
                    delay *= 2; // Exponential backoff
                }
            }
        }

        public async Task<List<string>> getIgnoredEventsAsync ()
        {
            var url = $"{baseUrl}api-journal-v1/discard";
            var responseJson = await edsmHttpClient.GetAsync(url).ConfigureAwait(false);
            var response = responseJson is null ? null : JsonConvert.DeserializeObject<List<string>>( responseJson );
            return response;
        }
    }
}