using EddiCore.GameState;
using EddiDataDefinitions;
using EddiDataProviderService;
using EddiEvents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Utilities;

namespace EddiCore.EventHandling
{
    internal sealed class EddiStationMarketEventHandler
    {
        private readonly IEddiEventProcessorContext _context;
        private readonly Action<Event> _enqueueEvent;

        internal EddiStationMarketEventHandler (
            IEddiEventProcessorContext context,
            Action<Event> enqueueEvent )
        {
            _context = context;
            _enqueueEvent = enqueueEvent;
        }

        private IEddiGameState GameState => _context.GameState;
        private IEddiGameStateMutator GameStateMutator => _context.GameStateMutator;
        private DataProviderService DataProvider => _context.DataProvider;
        private StarSystem CurrentStarSystem => GameState.CurrentStarSystem;
        private Station CurrentStation { get => GameState.CurrentStation; set => GameStateMutator.CurrentStation = value; }

        internal Task<bool> HandleMarketAsync ( MarketEvent @event )
        {
            return HandleStationDataAsync(
                @event,
                @event.info?.Items,
                item => item.ToCommodityMarketQuote(),
                ( station, items ) =>
                {
                    station.commodities = items;
                    station.commoditiesupdatedat = Dates.fromDateTimeToSeconds( @event.timestamp );
                },
                station => station.marketUpdatedThisVisit,
                ( station, updated ) => station.marketUpdatedThisVisit = updated,
                "market",
                true );
        }

        internal Task<bool> HandleOutfittingAsync ( OutfittingEvent @event )
        {
            return HandleStationDataAsync(
                @event,
                @event.info?.Items,
                Module.FromOutfittingInfo,
                ( station, modules ) =>
                {
                    station.outfitting = modules;
                    station.outfittingupdatedat = Dates.fromDateTimeToSeconds( @event.timestamp );
                },
                station => station.outfittingUpdatedThisVisit,
                ( station, updated ) => station.outfittingUpdatedThisVisit = updated,
                "outfitting" );
        }

        internal Task<bool> HandleShipyardAsync ( ShipyardEvent @event )
        {
            return HandleStationDataAsync(
                @event,
                @event.info?.PriceList,
                Ship.FromShipyardInfo,
                ( station, ships ) =>
                {
                    station.shipyard = ships;
                    station.shipyardupdatedat = Dates.fromDateTimeToSeconds( @event.timestamp );
                },
                station => station.shipyardUpdatedThisVisit,
                ( station, updated ) => station.shipyardUpdatedThisVisit = updated,
                "shipyard" );
        }

        private async Task<bool> HandleStationDataAsync<TInput, TOutput> (
            Event @event,
            IReadOnlyCollection<TInput> sourceItems,
            Func<TInput, TOutput> convert,
            Action<Station, List<TOutput>> updateStation,
            Func<Station, bool> wasUpdatedThisVisit,
            Action<Station, bool> setUpdatedThisVisit,
            string updateType,
            bool allowNullItems = false )
            where TOutput : class
        {
            if ( @event.fromLoad ) { return false; }

            var stationEvent = GetStationMarketEvent( @event );
            if ( stationEvent.system != CurrentStarSystem?.systemname ) { return false; }

            var convertedItems = sourceItems?
                .Select( convert )
                .ToList();

            if ( convertedItems is null || ( !allowNullItems && convertedItems.Any( item => item is null ) ) )
            {
                return false;
            }

            if ( CurrentStation?.marketId != null && CurrentStation.marketId == stationEvent.marketId )
            {
                updateStation( CurrentStation, convertedItems );
                Logging.Debug( "Star system information updated from remote server; updating local copy" );
                await DataProvider.SaveStarSystemAsync( CurrentStarSystem ).ConfigureAwait( false );

                if ( !wasUpdatedThisVisit( CurrentStation ) )
                {
                    setUpdatedThisVisit( CurrentStation, true );
                    _enqueueEvent( new MarketInformationUpdatedEvent(
                        @event.timestamp,
                        stationEvent.marketId,
                        stationEvent.station,
                        stationEvent.system,
                        [ updateType ] )
                    {
                        raw = @event.raw
                    } );
                }

                return true;
            }

            var station = CurrentStarSystem?.stations.FirstOrDefault( s => s.marketId == stationEvent.marketId );
            if ( station != null )
            {
                updateStation( station, convertedItems );
                await DataProvider.SaveStarSystemAsync( CurrentStarSystem ).ConfigureAwait( false );
            }
            return false;
        }

        private static StationMarketEventData GetStationMarketEvent ( Event @event )
        {
            return @event switch
            {
                MarketEvent marketEvent => new StationMarketEventData( marketEvent.marketId, marketEvent.station, marketEvent.system ),
                OutfittingEvent outfittingEvent => new StationMarketEventData( outfittingEvent.marketId, outfittingEvent.station, outfittingEvent.system ),
                ShipyardEvent shipyardEvent => new StationMarketEventData( shipyardEvent.marketId, shipyardEvent.station, shipyardEvent.system ),
                _ => throw new ArgumentException( $"Unsupported station market event type {@event.GetType().Name}.", nameof( @event ) )
            };
        }

        private sealed record StationMarketEventData ( long marketId, string station, string system );
    }
}
