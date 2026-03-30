using EddiCore;
using EddiDataDefinitions;
using EddiEvents;
using JetBrains.Annotations;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Controls;
using Utilities;

[assembly: InternalsVisibleTo( "Tests" )]
namespace EddiLocationMonitor
{
    [UsedImplicitly]
    public class LocationMonitor : IEddiMonitor
    {
        private readonly OrganicSampling exobiology = new();
        private ulong? systemAddress;
        private int? bodyId;
        private decimal? latitude;
        private decimal? longitude;

        public string MonitorName () => "Location monitor";

        public string LocalizedMonitorName () => Properties.Resources.MonitorName;

        public string MonitorDescription () => Properties.Resources.MonitorDescription;

        public bool IsRequired () => true;

        public bool NeedsStart () => false;

        public void Start () { }

        public void Stop () { }

        public void Reload () { }

        public Task PreHandleAsync ( Event @event )
        {
            if ( @event is CarrierJumpedEvent carrierJumpedEvent )
            {
                handleCarrierJumpedEvent( carrierJumpedEvent );
            }
            else if ( @event is DisembarkEvent disembarkEvent )
            {
                handleDisembarkEvent( disembarkEvent );
            }
            else if ( @event is EmbarkEvent embarkEvent )
            {
                handleEmbarkEvent( embarkEvent );
            }
            else if ( @event is JumpedEvent jumpedEvent )
            {
                handleJumpedEvent( jumpedEvent );
            }
            else if ( @event is LocationEvent locationEvent )
            {
                handleLocationEvent( locationEvent );
            }
            else if ( @event is ScanOrganicEvent scanOrganicEvent )
            {
                handleScanOrganicEventAsync(scanOrganicEvent)
                    .SafeFireAndForget(e => Logging.Error(e.Message, e));
            }
            return Task.CompletedTask;
        }

        private void handleCarrierJumpedEvent ( CarrierJumpedEvent @event )
        {
            systemAddress = @event.systemAddress;
            bodyId = null;
        }

        private void handleDisembarkEvent ( DisembarkEvent @event )
        {
            systemAddress = @event.systemAddress;
            bodyId = @event.bodyId;
        }

        private void handleEmbarkEvent ( EmbarkEvent @event )
        {
            systemAddress = @event.systemAddress;
            bodyId = @event.bodyId;
        }

        private void handleJumpedEvent ( JumpedEvent @event )
        {
            systemAddress = @event.systemAddress;
            bodyId = null;
        }

        private void handleLocationEvent ( LocationEvent @event )
        {
            systemAddress = @event.systemAddress;
            bodyId = @event.bodyId;
        }

        private async Task handleScanOrganicEventAsync ( ScanOrganicEvent @event )
        {
            if ( @event.fromLoad ) { return; }

            if ( @event.scanStage < 4 && latitude != null && longitude != null )
            {
                var system = await EDDI.Instance.DataProvider
                    .GetOrFetchStarSystemAsync( @event.systemAddress ).ConfigureAwait( false );
                var body = system?.bodies?.Find( b => b.bodyId == @event.bodyId );
                @event.organic.firstFootfallRegistered = body?.alreadyfirstfootfalled == false;
                exobiology.LogSample( @event.organic, @event.systemAddress, @event.bodyId, (decimal)latitude, (decimal)longitude );
            }
            else if ( @event.scanStage == 4 )
            {
                exobiology.Reset();
            }
        }

        public Task PostHandleAsync ( Event @event ) => Task.CompletedTask;

        public Task HandleProfileAsync ( JObject profile ) => Task.CompletedTask;

        public Task HandleStatusAsync ( Status status )
        {
            if ( ( status?.near_surface ?? false ) &&
                 systemAddress != null && bodyId != null )
            {
                if ( exobiology.organic != null )
                {
                    var isNearPriorSample = exobiology.IsNearby( (ulong)systemAddress, (int)bodyId,
                        status.planetradius, status.latitude, status.longitude );

                    // Generate an event when we transition from being near a prior sample to not being near one, or vice versa
                    if ( ( !isNearPriorSample && exobiology.wasNearPriorSample ) ||
                         ( isNearPriorSample && !exobiology.wasNearPriorSample ) )
                    {
                        EDDI.Instance.enqueueEvent( new ScanOrganicDistanceEvent( DateTime.UtcNow, exobiology.organic,
                            isNearPriorSample ) );
                    }

                    exobiology.wasNearPriorSample = isNearPriorSample;
                }

                latitude = status.latitude;
                longitude = status.longitude;
            }
            
            return Task.CompletedTask;
        }

        public IDictionary<string, Tuple<Type, object>> GetVariables () => null;

        public UserControl ConfigurationTabItem () => null;
    }
}
