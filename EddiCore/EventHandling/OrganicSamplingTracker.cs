using EddiDataDefinitions;
using EddiDataProviderService;
using EddiEvents;
using System;
using System.Threading.Tasks;

namespace EddiCore.EventHandling
{
    internal sealed class OrganicSamplingTracker
    {
        private readonly DataProviderService _dataProvider;
        private readonly Action<Event> _enqueueEvent;
        private readonly OrganicSampling _exobiology = new();
        private ulong? _systemAddress;
        private int? _bodyId;
        private decimal? _latitude;
        private decimal? _longitude;

        internal OrganicSamplingTracker ( DataProviderService dataProvider, Action<Event> enqueueEvent )
        {
            _dataProvider = dataProvider;
            _enqueueEvent = enqueueEvent;
        }

        internal void TrackLocationEvent ( Event @event )
        {
            switch ( @event )
            {
                case CarrierJumpedEvent carrierJumpedEvent:
                    _systemAddress = carrierJumpedEvent.systemAddress;
                    _bodyId = null;
                    break;
                case DisembarkEvent disembarkEvent:
                    _systemAddress = disembarkEvent.systemAddress;
                    _bodyId = disembarkEvent.bodyId;
                    break;
                case EmbarkEvent embarkEvent:
                    _systemAddress = embarkEvent.systemAddress;
                    _bodyId = embarkEvent.bodyId;
                    break;
                case JumpedEvent jumpedEvent:
                    _systemAddress = jumpedEvent.systemAddress;
                    _bodyId = null;
                    break;
                case LocationEvent locationEvent:
                    _systemAddress = locationEvent.systemAddress;
                    _bodyId = locationEvent.bodyId;
                    break;
            }
        }

        internal async Task TrackScanOrganicAsync ( ScanOrganicEvent @event )
        {
            if ( @event.fromLoad ) { return; }

            if ( _latitude != null && _longitude != null )
            {
                var system = await _dataProvider
                    .GetOrFetchStarSystemAsync( @event.systemAddress ).ConfigureAwait( false );
                var body = system?.bodies?.Find( b => b.bodyId == @event.bodyId );
                @event.organic.firstFootfallRegistered = body?.alreadyfirstfootfalled == false;
                if ( @event.scanStage < 4 )
                {
                    _exobiology.LogSample( @event.organic, @event.systemAddress, @event.bodyId, (decimal)_latitude, (decimal)_longitude );
                }
            }

            if ( @event.scanStage == 4 )
            {
                _exobiology.Reset();
            }
        }

        internal Task HandleStatusAsync ( Status status )
        {
            if ( ( status?.near_surface ?? false ) &&
                 _systemAddress != null && _bodyId != null )
            {
                if ( _exobiology.organic != null )
                {
                    var isNearPriorSample = _exobiology.IsNearby(
                        (ulong)_systemAddress,
                        (int)_bodyId,
                        status.planetradius,
                        status.latitude,
                        status.longitude );

                    if ( ( !isNearPriorSample && _exobiology.wasNearPriorSample ) ||
                         ( isNearPriorSample && !_exobiology.wasNearPriorSample ) )
                    {
                        _enqueueEvent( new ScanOrganicDistanceEvent( DateTime.UtcNow, _exobiology.organic, isNearPriorSample ) );
                    }

                    _exobiology.wasNearPriorSample = isNearPriorSample;
                }

                _latitude = status.latitude;
                _longitude = status.longitude;
            }

            return Task.CompletedTask;
        }
    }
}
