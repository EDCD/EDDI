using EddiCompanionAppService;
using EddiCore.GameState;
using EddiDataDefinitions;
using EddiDataProviderService;
using EddiEvents;
using JetBrains.Annotations;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Utilities;

namespace EddiCore.EventHandling
{
    internal sealed class EddiEventProcessor : IDisposable
    {
        private readonly IEddiEventProcessorContext _context;
        private readonly EddiLocationStateService _locationStateService;
        private readonly EddiStationMarketEventHandler _stationMarketEventHandler;
        private string multicrewVehicleHolder;

        internal EddiEventProcessor ( IEddiEventProcessorContext context )
        {
            _context = context;
            _locationStateService = new EddiLocationStateService( context );
            _stationMarketEventHandler = new EddiStationMarketEventHandler( context, enqueueEvent );
        }

        private IEddiGameState GameState => _context.GameState;
        private IEddiGameStateMutator GameStateMutator => _context.GameStateMutator;
        private DataProviderService DataProvider => _context.DataProvider;
        private OrganicSamplingTracker OrganicSamplingTracker => _context.OrganicSamplingTracker;
        private StarSystem CurrentStarSystem { get => GameState.CurrentStarSystem; set => GameStateMutator.CurrentStarSystem = value; }
        private StarSystem LastStarSystem { get => GameState.LastStarSystem; set => GameStateMutator.LastStarSystem = value; }
        private StarSystem NextStarSystem { get => GameState.NextStarSystem; set => GameStateMutator.NextStarSystem = value; }
        private StarSystem DestinationStarSystem { get => GameState.DestinationStarSystem; set => GameStateMutator.DestinationStarSystem = value; }
        private Station CurrentStation { get => GameState.CurrentStation; set => GameStateMutator.CurrentStation = value; }
        private Body CurrentStellarBody { get => GameState.CurrentStellarBody; set => GameStateMutator.CurrentStellarBody = value; }
        private FleetCarrier FleetCarrier { get => GameState.FleetCarrier; set => GameStateMutator.FleetCarrier = value; }
        private FleetCarrier SquadronCarrier { get => GameState.SquadronCarrier; set => GameStateMutator.SquadronCarrier = value; }
        private string Environment { get => GameState.Environment; set => GameStateMutator.Environment = value; }
        private string Vehicle { get => GameState.Vehicle; set => GameStateMutator.Vehicle = value; }
        private bool inTelepresence { get => GameState.inTelepresence; set => GameStateMutator.inTelepresence = value; }
        private bool inHorizons { get => GameState.inHorizons; set => GameStateMutator.inHorizons = value; }
        private bool inOdyssey { get => GameState.inOdyssey; set => GameStateMutator.inOdyssey = value; }
        private bool gameIsBeta { get => GameState.gameIsBeta; set => GameStateMutator.gameIsBeta = value; }
        private System.Collections.Concurrent.ConcurrentDictionary<string, Event> lastEventOfType => _context.EventPipeline.LastEventOfType;

        private IEddiMonitor ObtainMonitor ( string invariantName, StringComparison stringComparison = StringComparison.InvariantCultureIgnoreCase )
        {
            return _context.ObtainMonitor( invariantName, stringComparison );
        }

        private void enqueueEvent ( Event @event )
        {
            _context.EventPipeline.Enqueue( @event );
        }

        private Task conditionallyRefreshStationProfileAsync (
            string expectedSystemName,
            long expectedLastMarketID,
            bool forceUpdate = false,
            JObject profileJson = null )
        {
            return _context.conditionallyRefreshStationProfileAsync(
                expectedSystemName,
                expectedLastMarketID,
                forceUpdate,
                profileJson );
        }

        internal async Task<bool> ProcessEventAsync ( Event @event )
        {
            var passEvent = true;
            if ( @event is FileHeaderEvent fileHeaderEvent )
            {
                passEvent = eventFileHeader( fileHeaderEvent );
            }
            else if ( @event is LocationEvent locationEvent )
            {
                passEvent = await eventLocationAsync( locationEvent ).ConfigureAwait( false );
            }
            else if ( @event is DockedEvent dockedEvent )
            {
                passEvent = await eventDockedAsync( dockedEvent ).ConfigureAwait( false );
            }
            else if ( @event is UndockedEvent undockedEvent )
            {
                passEvent = eventUndocked( undockedEvent );
            }
            else if ( @event is DockingRequestedEvent dockingRequestedEvent )
            {
                passEvent = eventDockingRequested( dockingRequestedEvent );
            }
            else if ( @event is TouchdownEvent touchdownEvent )
            {
                passEvent = await eventTouchdownAsync( touchdownEvent ).ConfigureAwait( false );
            }
            else if ( @event is LiftoffEvent liftoffEvent )
            {
                passEvent = await eventLiftoffAsync( liftoffEvent ).ConfigureAwait( false );
            }
            else if ( @event is FSDEngagedEvent fsdEngagedEvent )
            {
                passEvent = await eventFSDEngagedAsync( fsdEngagedEvent ).ConfigureAwait( false );
            }
            else if ( @event is FSDTargetEvent fsdTargetEvent )
            {
                passEvent = await eventFSDTargetAsync( fsdTargetEvent ).ConfigureAwait( false );
            }
            else if ( @event is JumpedEvent jumpedEvent )
            {
                passEvent = await eventJumpedAsync( jumpedEvent ).ConfigureAwait( false );
            }
            else if ( @event is EnteredSupercruiseEvent enteredSupercruiseEvent )
            {
                passEvent = await eventEnteredSupercruiseAsync( enteredSupercruiseEvent ).ConfigureAwait( false );
            }
            else if ( @event is EnteredNormalSpaceEvent enteredNormalSpaceEvent )
            {
                passEvent = await eventEnteredNormalSpaceAsync( enteredNormalSpaceEvent ).ConfigureAwait( false );
            }
            else if ( @event is CommanderContinuedEvent commanderContinuedEvent )
            {
                passEvent = eventCommanderContinued( commanderContinuedEvent );
            }
            else if ( @event is CrewJoinedEvent crewJoinedEvent )
            {
                passEvent = eventCrewJoined( crewJoinedEvent );
            }
            else if ( @event is CrewLeftEvent )
            {
                passEvent = eventCrewLeft();
            }
            else if ( @event is EnteredCQCEvent )
            {
                passEvent = eventEnteredCQC();
            }
            else if ( @event is VesselDockedEvent vesselDockedEvent)
            {
                passEvent = eventVesselDocked( vesselDockedEvent );
            }
            else if ( @event is VesselLaunchedEvent vesselLaunchedEvent )
            {
                passEvent = eventVesselLaunched( vesselLaunchedEvent );
            }
            else if ( @event is StarScannedEvent starScannedEvent )
            {
                passEvent = await eventStarScannedAsync( starScannedEvent ).ConfigureAwait( false );
            }
            else if ( @event is BodyScannedEvent bodyScannedEvent )
            {
                passEvent = await eventBodyScannedAsync( bodyScannedEvent ).ConfigureAwait( false );
            }
            else if ( @event is BodyMappedEvent bodyMappedEvent )
            {
                passEvent = await eventBodyMappedAsync( bodyMappedEvent ).ConfigureAwait( false );
            }
            else if ( @event is RingHotspotsEvent ringHotspotsEvent )
            {
                passEvent = await eventRingHotspotsAsync( ringHotspotsEvent ).ConfigureAwait( false );
            }
            else if ( @event is VesselDestroyedEvent vesselDestroyedEvent )
            {
                passEvent = eventVesselDestroyed( vesselDestroyedEvent );
            }
            else if ( @event is NearSurfaceEvent nearSurfaceEvent )
            {
                passEvent = await eventNearSurfaceAsync( nearSurfaceEvent ).ConfigureAwait( false );
            }
            else if ( @event is FriendsEvent friendsEvent )
            {
                passEvent = eventFriends( friendsEvent );
            }
            else if ( @event is MarketEvent marketEvent )
            {
                passEvent = await _stationMarketEventHandler.HandleMarketAsync( marketEvent ).ConfigureAwait( false );
            }
            else if ( @event is OutfittingEvent outfittingEvent )
            {
                passEvent = await _stationMarketEventHandler.HandleOutfittingAsync( outfittingEvent ).ConfigureAwait( false );
            }
            else if ( @event is ShipyardEvent shipyardEvent )
            {
                passEvent = await _stationMarketEventHandler.HandleShipyardAsync( shipyardEvent ).ConfigureAwait( false );
            }
            else if ( @event is DiscoveryScanEvent discoveryScanEvent )
            {
                passEvent = await eventDiscoveryScanAsync( discoveryScanEvent ).ConfigureAwait( false );
            }
            else if ( @event is SystemScanComplete systemScanComplete )
            {
                passEvent = await eventSystemScanCompleteAsync( systemScanComplete ).ConfigureAwait( false );
            }
            else if ( @event is CarrierJumpEngagedEvent carrierJumpEngagedEvent )
            {
                passEvent = await eventCarrierJumpEngagedAsync( carrierJumpEngagedEvent ).ConfigureAwait( false );
            }
            else if ( @event is CarrierJumpedEvent carrierJumpedEvent )
            {
                passEvent = await eventCarrierJumpedAsync( carrierJumpedEvent ).ConfigureAwait( false );
            }
            else if ( @event is DisembarkEvent disembarkEvent )
            {
                passEvent = await eventDisembarkAsync( disembarkEvent ).ConfigureAwait(false);
            }
            else if ( @event is EmbarkEvent embarkEvent )
            {
                passEvent = eventEmbark( embarkEvent );
            }
            else if ( @event is UnderAttackEvent underAttackEvent )
            {
                passEvent = eventUnderAttack( underAttackEvent );
            }
            else if ( @event is SettlementApproachedEvent settlementApproachedEvent )
            {
                passEvent = eventSettlementApproached( settlementApproachedEvent );
            }
            else if ( @event is SignalDetectedEvent signalDetectedEvent )
            {
                passEvent = eventSignalDetected( signalDetectedEvent );
            }
            else if ( @event is DiedEvent )
            {
                passEvent = eventDied();
            }

            if ( OrganicSamplingTracker is { } organicSamplingTracker )
            {
                organicSamplingTracker.TrackLocationEvent( @event );
                if ( @event is ScanOrganicEvent scanOrganicEvent )
                {
                    await organicSamplingTracker.TrackScanOrganicAsync( scanOrganicEvent ).ConfigureAwait( false );
                }
            }

            return passEvent;
        }

        public void Dispose ()
        {
            _locationStateService.Dispose();
        }

        private bool eventDied ()
        {
            GameState.DeployedVessels.Clear();
            return true;
        }

        private async Task<bool> eventRingHotspotsAsync ( RingHotspotsEvent @event )
        {
            var ring = CurrentStarSystem?.bodies?
                .Where(b => b.rings is { } list && list.Count > 0 )
                .SelectMany(b => b.rings)
                .FirstOrDefault(r => r.name == @event.bodyname);
            if ( ring != null )
            {
                ring.mapped = @event.timestamp;
                ring.hotspots = @event.hotspots;
                await DataProvider.SaveStarSystemAsync( CurrentStarSystem ).ConfigureAwait(false);
            }

            return true;
        }

        internal bool eventSignalDetected ( SignalDetectedEvent @event )
        {
            if ( CurrentStarSystem != null &&
                 CurrentStarSystem.systemAddress == @event.systemAddress )
            {
                // If more signal detected events are have been received and are waiting to be processed, simply enqueue the current signal source. Otherwise, batch add the signals to the star system.
                if ( StarSystemSignalSourceManager.newSignalSources.TryGetValue( @event.systemAddress, out var newSignalSources ) )
                {
                    @event.unique =
                        !CurrentStarSystem.signalsources.Contains( @event.signalSource.localizedName ) &&
                        !newSignalSources.Select( s => s.localizedName ).Contains( @event.signalSource.localizedName );
                    newSignalSources.Add( @event.signalSource );
                }
                else
                {
                    @event.unique = true;
                    newSignalSources = [ @event.signalSource ];
                    StarSystemSignalSourceManager.newSignalSources.Add( @event.systemAddress, newSignalSources );
                }

                if ( !_context.EventPipeline.HasQueuedSignalDetectedEvents() )
                {
                    CurrentStarSystem.AddOrUpdateSignalSources( newSignalSources );
                    newSignalSources.Clear();
                }

                return true;
            }

            return false;
        }

        private bool eventSettlementApproached(SettlementApproachedEvent settlementApproachedEvent)
        {
            if (CurrentStarSystem?.systemAddress == settlementApproachedEvent.systemAddress
                && settlementApproachedEvent.marketId != null )
            {
                var station = CurrentStarSystem?.stations.FirstOrDefault(s => s.marketId == settlementApproachedEvent.marketId);
                if (station is null)
                {
                    // This station is unknown to us, might not be in our data source or we might not have connectivity.  Use a placeholder
                    station = new Station
                    {
                        name = settlementApproachedEvent.name,
                        marketId = settlementApproachedEvent.marketId,
                        systemname = CurrentStarSystem?.systemname,
                        systemAddress = settlementApproachedEvent.systemAddress
                    };
                    CurrentStarSystem?.AddOrUpdateStation( station );
                }
                station.Faction = settlementApproachedEvent.controllingFaction;
                station.stationServices = settlementApproachedEvent.stationServices;
                station.economyShares = settlementApproachedEvent.economyShares;
            }
            return true;
        }

        private bool eventUnderAttack(UnderAttackEvent underAttackEvent)
        {
            // Suppress repetitious `Under attack` events when loading or
            // when the target has already been reported as under attack within the last 10 seconds.
            var passEvent = !(underAttackEvent.fromLoad || (
                lastEventOfType.TryGetValue( underAttackEvent.type, out var ev ) && ev is UnderAttackEvent lastEvent
                && lastEvent.target == underAttackEvent.target
                && ( underAttackEvent.timestamp - lastEvent.timestamp ).TotalSeconds < 10
            ));
            return passEvent;
        }

        private async Task<bool> eventDisembarkAsync(DisembarkEvent @event) 
        {
            Vehicle = Constants.VEHICLE_LEGS;
            Logging.Info($"Disembarked to {Vehicle}");

            if ( @event.onplanet != true ) { return true; }
            await updateCurrentStellarBodyAsync( @event.bodyname, @event.bodyId, @event.systemname, @event.systemAddress )
                .ConfigureAwait( false );
            var body = CurrentStarSystem?.BodyWithID( @event.bodyId );
            if ( body != null )
            {
                if ( body.alreadyfirstfootfalled == false && body.footfalledDateTime is null )
                {
                    // This is a first footfall event
                    @event.firstfootfall = true;
                }
                body.footfalledDateTime ??= @event.timestamp;
                await DataProvider.SaveStarSystemAsync( CurrentStarSystem ).ConfigureAwait( false );
            }

            return true;
        }

        private bool eventEmbark(EmbarkEvent @event) 
        {
            if (@event.tomulticrew)
            {
                Vehicle = Constants.VEHICLE_MULTICREW;
            }
            if (@event.toship)
            {
                Vehicle = Constants.VEHICLE_SHIP;
            }
            if (@event.tosrv)
            {
                Vehicle = Constants.VEHICLE_SRV;
            }
            if (@event.totransport)
            {
                Vehicle = Constants.VEHICLE_TAXI;
            }

            @event.deployedVessels = GameState.DeployedVessels.Values.RemoveNulls().ToList();

            Logging.Info($"Embarked to {Vehicle}");

            return true;
        }

        private async Task<bool> eventCarrierJumpEngagedAsync( CarrierJumpEngagedEvent @event )
        {
            // Update our current environment, vehicle, and station information if we are still docked at the carrier
            if (Environment == Constants.ENVIRONMENT_DOCKED && @event.carrierID == CurrentStation?.marketId)
            {
                // We are in witch space and in the ship.
                @event.docked = true;
                Environment = Constants.ENVIRONMENT_WITCH_SPACE;
                Vehicle = Constants.VEHICLE_SHIP;

                // Make sure we have at least basic information about the destination star system
                NextStarSystem = await DataProvider.GetOrCreateStarSystemAsync( @event.systemAddress, @event.systemname ).ConfigureAwait(false);

                // Remove the carrier from its prior location in the origin system so that we can re-save it with a new location
                CurrentStarSystem?.RemoveStation( @event.carrierID );

                // Set the destination system as the current star system
                await updateCurrentSystemAsync( @event.systemname, @event.systemAddress ).ConfigureAwait(false);

                // Update our station information
                CurrentStation = CurrentStarSystem?.stations.FirstOrDefault(s => s.marketId == @event.carrierID) ?? new Station();
                CurrentStation.marketId = @event.carrierID;
                CurrentStation.systemname = @event.systemname;
                CurrentStation.systemAddress = @event.systemAddress;

                // Add the carrier to the destination system
                CurrentStarSystem?.AddOrUpdateStation(CurrentStation);
            }
            else if (!string.IsNullOrEmpty(@event.originSystemName))
            {
                // Remove the carrier from its prior location in the origin system so that we can re-save it with a new location
                var originStarSystem = await DataProvider.GetOrFetchStarSystemAsync(@event.originSystemAddress ).ConfigureAwait(false);
                var carrier = originStarSystem?.stations.FirstOrDefault(s => s.marketId == @event.carrierID);
                originStarSystem?.RemoveStation( @event.carrierID );
                // Save the carrier to the updated star system
                if ( carrier != null)
                {
                    carrier.systemname = @event.systemname;
                    carrier.systemAddress = @event.systemAddress;
                    if (@event.systemAddress == CurrentStarSystem?.systemAddress)
                    {
                        CurrentStarSystem?.AddOrUpdateStation( carrier );
                        await DataProvider.SaveStarSystemAsync( originStarSystem ).ConfigureAwait( false );
                    }
                    else
                    {
                        var updatedStarSystem = await DataProvider.GetOrCreateStarSystemAsync( @event.systemAddress, @event.systemname ).ConfigureAwait(false);
                        updatedStarSystem.AddOrUpdateStation( carrier);
                        await DataProvider.SaveStarSystemAsync( updatedStarSystem ).ConfigureAwait( false );
                    }
                }
            }

            return true;
        }

        private async Task<bool> eventCarrierJumpedAsync( CarrierJumpedEvent @event )
        {
            Logging.Info( "Carrier jumped to: " + @event.systemname );
            
            if ( @event.docked || @event.onFoot )
            {
                // We are either docked and in a ship or on foot and in normal space.
                Environment = @event.docked ? Constants.ENVIRONMENT_DOCKED : Constants.ENVIRONMENT_NORMAL_SPACE;
                Vehicle = @event.docked ? Constants.VEHICLE_SHIP : Constants.VEHICLE_LEGS;

                // Remove the carrier from its prior location (of any) so that we can re-save it with a new location
                // If we haven't already updated our current star system, the carrier should be in `CurrentStarSystem`. If we have, it should be in `LastStarSystem`.

                // There's a journal bug here where carrier market information is missing if we are on foot but present if we are docked
                // so we fall back to our saved FleetCarrier object information if event information is missing.
                var carrierID = @event.carrierID ?? ( @event.carrierType == StationModel.FleetCarrier
                    ? FleetCarrier?.carrierID
                    : @event.carrierType == StationModel.SquadronCarrier
                        ? SquadronCarrier?.carrierID
                        : null );
                if ( carrierID != @event.carrierID )
                {
                    @event.carrierID = carrierID;
                }
                
                var carrierCallsign = @event.carriername ?? FleetCarrier?.callsign;

                // Remove the carrier from the current star system or last star system
                CurrentStation = CurrentStarSystem?.stations.FirstOrDefault( s => 
                    s.marketId == carrierID || s.name == carrierCallsign );
                if ( CurrentStation != null )
                {
                    CurrentStarSystem?.RemoveStation( carrierID ?? 0 );
                }
                else if ( LastStarSystem != null )
                {
                    CurrentStation = LastStarSystem.stations.FirstOrDefault( s =>
                        s.marketId == carrierID || s.name == carrierCallsign );
                    if ( CurrentStation != null )
                    {
                        LastStarSystem.RemoveStation( carrierID ?? 0 );
                        await DataProvider.SaveStarSystemAsync( LastStarSystem ).ConfigureAwait(false);
                    }
                }

                // If the carrier is not found in the current or last star system but a fleet carrier object is present,
                // we can generate current station information from the FleetCarrier object
                CurrentStation ??= FleetCarrier?.Market?.UpdateStation( @event.timestamp, new Station() );

                // Update current station properties
                if ( CurrentStation != null )
                {
                    CurrentStation.systemname = @event.systemname;
                    CurrentStation.systemAddress = @event.systemAddress;
                    CurrentStation.name = carrierCallsign;
                    CurrentStation.marketId = carrierID;
                    CurrentStation.Faction = @event.carrierFaction;
                    CurrentStation.Model = @event.carrierType;
                    CurrentStation.economyShares = @event.carrierEconomies;
                    CurrentStation.stationServices = @event.carrierServices;
                } 

                // Update our current star system and carrier location
                await updateCurrentSystemAsync( @event.systemname, @event.systemAddress ).ConfigureAwait(false);

                // Update our system properties
                if ( CurrentStarSystem is null ) { return false; }

                await ApplyCurrentSystemSnapshotAsync( CurrentStarSystem, @event.systemname, @event.systemAddress, @event.x, @event.y, @event.z, @event.controllingsystemfaction, @event.factions, @event.conflicts, @event.systemEconomy, @event.systemEconomy2, @event.securityLevel, @event.population, @event.Power, @event.NearbyPowers, @event.PowerState, @event.powerAcquisitionProgress, @event.powerControlProgress, @event.powerReinforcementControlPoints, @event.powerUnderminingControlPoints, @event.ThargoidWar, @event.timestamp, true ).ConfigureAwait(false);

                if ( CurrentStation != null )
                {
                    // Add our carrier to the new current star system
                    CurrentStarSystem.AddOrUpdateStation( CurrentStation );
                }

                // Kick off the profile refresh if the companion API is available
                if (CompanionAppService.Instance.CurrentState == CompanionAppService.State.Authorized && carrierID != null)
                {
                    // Refresh station data
                    if (@event.fromLoad) { return true; } // Don't fire this event when loading pre-existing logs

                    conditionallyRefreshStationProfileAsync( @event.systemname, @event.carrierID ?? 0 )
                        .SafeFireAndForget( ex => Logging.Error( ex.Message, ex ) );
                }
            }
            else
            {
                // We shouldn't be here - `the CarrierJump` event is only supposed to be written when docked with a fleet carrier as it jumps.
                Environment = Constants.ENVIRONMENT_NORMAL_SPACE;
                Logging.Error("Whoops! CarrierJump event recorded when not docked.", @event);
                throw new NotImplementedException();
            }

            return true;
        }

        internal async Task<bool> eventSystemScanCompleteAsync(SystemScanComplete @event)
        {
            // There is a bug in the player journal output (as of player journal v.25) that can cause the `SystemScanComplete` event to fire multiple times 
            // in rapid succession when performing a system scan of a star system with only stars and no other bodies.
            if (CurrentStarSystem != null)
            {
                if (CurrentStarSystem.systemScanCompleted)
                {
                    // We will suppress repetitions of the event within the same star system.
                    return false;
                }
                CurrentStarSystem.systemScanCompleted = true;
                // Update any bodies that aren't yet recorded as scanned (these were likely scanned while EDDI was not running)
                var bodiesToUpdate = new List<Body>();
                foreach (var body in CurrentStarSystem.bodies.Where(b => b.scannedDateTime is null))
                {
                    body.scannedDateTime = @event.timestamp;
                    bodiesToUpdate.Add(body);
                }
                if ( bodiesToUpdate.Count > 0 ) { CurrentStarSystem.AddOrUpdateBodies(bodiesToUpdate); }
                // Save the updated star system data
                await DataProvider.SaveStarSystemAsync(CurrentStarSystem).ConfigureAwait(false);
            }
            return true;
        }

        private async Task<bool> eventDiscoveryScanAsync(DiscoveryScanEvent @event)
        {
            if (CurrentStarSystem != null)
            {
                CurrentStarSystem.totalbodies = @event.totalbodies;

                if (@event.progress == 100 && CurrentStarSystem.scannedbodies < @event.totalbodies) // Fully scanned system, make sure that all bodies are marked as scanned
                {
                    // Update any bodies that aren't yet recorded as scanned (these were likely scanned while EDDI was not running)
                    var bodiesToUpdate = new List<Body>();
                    foreach (var body in CurrentStarSystem.bodies.Where(b => b.scannedDateTime is null))
                    {
                        body.scannedDateTime = @event.timestamp;
                        bodiesToUpdate.Add(body);
                    }
                    if ( bodiesToUpdate.Count > 0 ) { CurrentStarSystem.AddOrUpdateBodies(bodiesToUpdate); }
                }

                await DataProvider.SaveStarSystemAsync(CurrentStarSystem).ConfigureAwait(false);
            }
            return true;
        }

        internal bool eventFriends ( FriendsEvent @event )
        {
            var passEvent = false;
            var friend = new Friend
            {
                name = @event.name,
                status = @event.status
            };

            // Does this friend exist in our friends list?
            var commanderMonitorVariables = ObtainMonitor( "Commander Monitor" ).GetVariableValues();
            if ( commanderMonitorVariables.TryGetValue( "cmdr", out Commander Cmdr ) )
            {
                var index = Cmdr.friends.FindIndex( f => f.name == @event.name );
                if ( index >= 0 )
                {
                    if ( Cmdr.friends[ index ].status != @event.status )
                    {
                        // This is a known friend with a revised status: replace in situ (this is more efficient than removing and re-adding).
                        Cmdr.friends[ index ] = friend;
                        passEvent = true;
                    }
                }
                else
                {
                    // This is a new friend, add them to the list
                    Cmdr.friends.Add( friend );
                }
            }

            return passEvent;
        }

        internal async Task<bool> eventLocationAsync( LocationEvent theEvent )
        {
            Logging.Info("Location StarSystem: " + theEvent.systemname);

            // Set our vehicle
            if (theEvent.taxi)
            {
                Vehicle = Constants.VEHICLE_TAXI;
            }
            else if (theEvent.multicrew)
            {
                Vehicle = Constants.VEHICLE_MULTICREW;
            }
            else if (theEvent.inSRV)
            {
                Vehicle = Constants.VEHICLE_SRV;
            }
            else if (theEvent.onFoot)
            {
                Vehicle = Constants.VEHICLE_LEGS;
            }
            // If none of these are true we may either be in our ship or in a fighter.
            Logging.Info($"Vehicle mode is {Vehicle}");

            await updateCurrentSystemAsync( theEvent.systemname, theEvent.systemAddress ).ConfigureAwait(false);
            if ( CurrentStarSystem is null ) { return false; }

            await ApplyCurrentSystemSnapshotAsync( CurrentStarSystem, theEvent.systemname, theEvent.systemAddress, theEvent.x, theEvent.y, theEvent.z, theEvent.controllingsystemfaction, theEvent.factions, theEvent.conflicts, theEvent.Economy, theEvent.Economy2, theEvent.securityLevel, theEvent.population, theEvent.Power, theEvent.NearbyPowers, theEvent.PowerState, theEvent.powerAcquisitionProgress, theEvent.powerControlProgress, theEvent.powerReinforcementControlPoints, theEvent.powerUnderminingControlPoints, theEvent.ThargoidWar, theEvent.timestamp, true ).ConfigureAwait(false);

            if ( theEvent.docked )
            {
                // Update the station
                Logging.Debug( "Now at station " + theEvent.station );
                var station = CurrentStarSystem.stations.Find( s => s.marketId == theEvent.marketId );
                if ( station == null )
                {
                    // This station is unknown to us, might not be in our data source or we might not have connectivity.  Use a placeholder
                    station = new Station { name = theEvent.station, marketId = theEvent.marketId, systemname = theEvent.systemname, systemAddress = theEvent.systemAddress };
                    CurrentStarSystem.AddOrUpdateStation( station );
                }

                // We are docked
                Environment = Constants.ENVIRONMENT_DOCKED;

                // If we're not in a taxi or multicrew then we're in our own ship.
                if ( !theEvent.taxi && !theEvent.multicrew ) { Vehicle = Constants.VEHICLE_SHIP; }

                // Update station properties known from this event
                station.systemAddress = theEvent.systemAddress;
                station.Faction = theEvent.controllingstationfaction;
                station.Model = theEvent.stationModel;
                station.distancefromstar = theEvent.distancefromstar;

                CurrentStation = station;

                // Kick off the profile refresh if the companion API is available
                if ( CompanionAppService.Instance.CurrentState == CompanionAppService.State.Authorized && theEvent.marketId != null )
                {
                    // Refresh station data
                    if ( theEvent.fromLoad ) { return true; } // Don't fire this event when loading pre-existing logs

                    conditionallyRefreshStationProfileAsync( theEvent.systemname, theEvent.marketId ?? 0 )
                        .SafeFireAndForget( ex => Logging.Error( ex.Message, ex ) );
                }
            }
            else if ( theEvent.latitude != null && theEvent.longitude != null )
            {
                Environment = Constants.ENVIRONMENT_LANDED;
            }
            else
            {
                Environment = Constants.ENVIRONMENT_NORMAL_SPACE;
            }

            if ( theEvent.bodyType == BodyType.Planet )
            {
                theEvent.bodyType = CurrentStarSystem.bodies.FirstOrDefault( b => b.bodyId != null && b.bodyId == theEvent.bodyId )?.bodyType ?? theEvent.bodyType;
            }
            if ( theEvent.bodyname != null && ( theEvent.bodyType == BodyType.Moon ||
                                                theEvent.bodyType == BodyType.Planet ) )
            {
                // Update the body 
                Logging.Debug( "Now at body " + theEvent.bodyname );
                await updateCurrentStellarBodyAsync( theEvent.bodyname, theEvent.bodyId, theEvent.systemname, theEvent.systemAddress ).ConfigureAwait(false);
            }

            // Update to most recent information
            CurrentStarSystem.updatedat = Dates.fromDateTimeToSeconds( theEvent.timestamp );
            await DataProvider.SaveStarSystemAsync( CurrentStarSystem ).ConfigureAwait(false);

            return true;
        }

        private bool eventDockingRequested(DockingRequestedEvent theEvent)
        {
            var passEvent = !string.IsNullOrEmpty(theEvent.station);
            var station = CurrentStarSystem?.stations.Find(s => s.name == theEvent.station);
            if (station is null && CurrentStarSystem != null)
            {
                // This station is unknown to us, might not be in our data source or we might not have connectivity.  Use a placeholder
                station = new Station
                {
                    name = theEvent.station,
                    marketId = theEvent.marketId,
                    systemname = CurrentStarSystem.systemname,
                    systemAddress = CurrentStarSystem.systemAddress
                };
                CurrentStarSystem?.AddOrUpdateStation( station );
            }

            if ( station != null )
            {
                station.Model = theEvent.stationDefinition;
                station.landingPads = theEvent.landingPads;
            }

            return passEvent;
        }

        private async Task<bool> eventDockedAsync ( DockedEvent @event )
        {
            await updateCurrentSystemAsync( @event.system, @event.systemAddress ).ConfigureAwait(false);

            if ( CurrentStarSystem == null ) { return false; }

            var station = CurrentStarSystem.stations.Find( s => s.marketId == @event.Station.marketId );
            if ( Environment == Constants.ENVIRONMENT_DOCKED && CurrentStation?.marketId == station?.marketId )
            {
                // We are already at this station
                Logging.Debug( $"Already at station {@event.station} ({@event.marketId})." );
                return false;
            }

            // Update the station
            Logging.Debug( $"Now at station {@event.station} ({@event.marketId})." );
            if ( station == null && @event.Station != null )
            {
                // This station is unknown to us
                station = @event.Station;
            }

            Environment = Constants.ENVIRONMENT_DOCKED;
            CurrentStarSystem.AddOrUpdateStation( station );
            CurrentStation = station;

            // Kick off the profile refresh if the companion API is available
            if ( CompanionAppService.Instance.CurrentState == CompanionAppService.State.Authorized && @event.marketId != null )
            {
                // Refresh station data
                if ( @event.fromLoad )
                {
                    return false;
                } // Don't fire this event when loading pre-existing logs or if we were already at this station

                conditionallyRefreshStationProfileAsync( @event.system, @event.marketId ?? 0 )
                    .SafeFireAndForget( ex => Logging.Error( ex.Message, ex ) );
            }

            return true;
        }

        private bool eventUndocked ( UndockedEvent @event )
        {
            Environment = Constants.ENVIRONMENT_NORMAL_SPACE;
            var station = CurrentStarSystem?.stations.Find( s => s.marketId == @event.marketId );
            if ( station != null )
            {
                station.marketUpdatedThisVisit = false;
                station.outfittingUpdatedThisVisit = false;
                station.shipyardUpdatedThisVisit = false;
            }

            CurrentStation = null;

            // Kick off the profile refresh if the companion API is available
            if ( CompanionAppService.Instance.CurrentState == CompanionAppService.State.Authorized &&
                 @event.marketId != null && CurrentStarSystem != null )
            {
                // Refresh station data
                // Don't fire this event when loading pre-existing logs or if we were already at this station
                if ( @event.fromLoad )
                {
                    return false;
                }

                conditionallyRefreshStationProfileAsync( CurrentStarSystem.systemname, @event.marketId ?? 0 )
                    .SafeFireAndForget( ex => Logging.Error( ex.Message, ex ) );
            }

            return true;
        }

        private async Task<bool> eventTouchdownAsync( TouchdownEvent @event )
        {
            await updateCurrentStellarBodyAsync( @event.bodyname, @event.bodyId, @event.systemname, @event.systemAddress ).ConfigureAwait(false);

            if (@event.taxi is true)
            {
                Vehicle = Constants.VEHICLE_TAXI;
            }
            else if (@event.multicrew is true)
            {
                Vehicle = Constants.VEHICLE_MULTICREW;
            }

            // Only pass on this event if our longitude and lattitude are set
            // (if not then this is probably being written prior to a `Location` event))
            if (@event.latitude != null && @event.longitude != null)
            {
                Environment = Constants.ENVIRONMENT_LANDED;
                if (@event.taxi is true)
                {
                    Vehicle = Constants.VEHICLE_TAXI;
                }
                else if (@event.multicrew is true)
                {
                    Vehicle = Constants.VEHICLE_MULTICREW;
                }
                else if (@event.playercontrolled)
                {
                    Vehicle = Constants.VEHICLE_SHIP;
                }
                else
                {
                    Vehicle = Constants.VEHICLE_SRV;
                }

                var body = CurrentStarSystem?.BodyWithID( @event.bodyId );
                if ( body != null && ( !body.alreadyfirstfootfalled ?? false ) && body.footfalledDateTime is null )
                {
                    // We can be the first to set foot on this world
                    @event.canfirstfootfall = true;
                }

                return true;
            }
            
            Logging.Info($"Touchdown in {Vehicle}");
            return false;
        }

        private async Task<bool> eventLiftoffAsync( LiftoffEvent theEvent )
        {
            await updateCurrentStellarBodyAsync( theEvent.bodyname, theEvent.bodyId, theEvent.systemname, theEvent.systemAddress ).ConfigureAwait(false);

            Environment = Constants.ENVIRONMENT_NORMAL_SPACE;

            if (theEvent.taxi is true)
            {
                Vehicle = Constants.VEHICLE_TAXI;
            }
            else if (theEvent.multicrew is true)
            {
                Vehicle = Constants.VEHICLE_MULTICREW;
            }

            Logging.Info($"Liftoff in {Vehicle}");
            return true;
        }

        private async Task ApplyCurrentSystemSnapshotAsync (
            StarSystem system,
            string systemName,
            ulong systemAddress,
            decimal? x,
            decimal? y,
            decimal? z,
            Faction controllingFaction,
            List<Faction> factions,
            List<Conflict> conflicts,
            Economy economy,
            Economy economy2,
            SecurityLevel security,
            long? population,
            Power power,
            List<Power> nearbyPowers,
            PowerplayState powerState,
            List<PowerAcquisitionProgress> acquisitionProgress,
            decimal controlProgress,
            int reinforcementPoints,
            int underminingPoints,
            ThargoidWar thargoidWar,
            DateTime timestamp,
            bool addVisit )
        {
            if ( system is null )
            { return; }

            system.systemname = systemName;
            system.systemAddress = systemAddress;
            system.x = x;
            system.y = y;
            system.z = z;

            if ( factions != null )
            {
                system.factions = factions;
                system.conflicts = conflicts;

                // Make the controlling faction reference the same object as the factions list.
                system.Faction = factions.FirstOrDefault( f => f.name == controllingFaction?.name )
                                 ?? controllingFaction;
            }
            else
            {
                system.Faction = controllingFaction;
                system.conflicts = conflicts;
            }

            system.ThargoidWar = thargoidWar;
            system.Economies = [ economy, economy2 ];
            system.securityLevel = security ?? SecurityLevel.None;

            if ( population != null )
            {
                system.population = population;
            }

            system.Power = power;
            system.NearbyPowers = nearbyPowers;
            system.powerState = powerState ?? system.powerState;
            system.powerAcquisitionProgress = acquisitionProgress;
            system.powerControlProgress = controlProgress;
            system.powerReinforcementControlPoints = reinforcementPoints;
            system.powerUnderminingControlPoints = underminingPoints;

            if ( addVisit )
            {
                system.visitLog.Add( timestamp );
            }

            system.updatedat = Dates.fromDateTimeToSeconds( timestamp );

            await DataProvider.SaveStarSystemAsync( system ).ConfigureAwait( false );
        }

        internal async Task updateCurrentSystemAsync([NotNull] string systemName, ulong systemAddress )
        {
            await _locationStateService.UpdateCurrentSystemAsync( systemName, systemAddress ).ConfigureAwait(false);
        }

        private async Task updateCurrentStellarBodyAsync(string bodyName, int? bodyId, string systemName, ulong systemAddress )
        {
            await _locationStateService.UpdateCurrentStellarBodyAsync( bodyName, bodyId, systemName, systemAddress ).ConfigureAwait(false);
        }

        internal async Task<bool> eventFSDEngagedAsync( FSDEngagedEvent @event )
        {
            // Keep track of our environment
            Environment = @event.target == "Supercruise" 
                ? Constants.ENVIRONMENT_SUPERCRUISE 
                : Constants.ENVIRONMENT_WITCH_SPACE;

            // Set the destination system as the next star system
            if ( @event.systemAddress != null && @event.systemAddress != NextStarSystem?.systemAddress )
            {
                NextStarSystem = await DataProvider
                    .GetOrCreateStarSystemAsync( (ulong)@event.systemAddress, @event.systemname )
                    .ConfigureAwait( false );
            }

            // Remove information about the current station and stellar body 
            CurrentStation = null;
            CurrentStellarBody = null;

            return true;
        }

        private async Task<bool> eventFSDTargetAsync( FSDTargetEvent @event )
        {
            // Set and prepare data about the next star system
            NextStarSystem = await DataProvider.GetOrCreateStarSystemAsync( @event.systemAddress, @event.system ).ConfigureAwait(false);
            if (NextStarSystem != null && !NextStarSystem.bodies.Any(b => b.mainstar ?? false))
            {
                // This system is unknown to us, might not be recorded, or we might not have connectivity.  Use a placeholder main star
                var mainStar = new Body
                {
                    bodyType = BodyType.Star,
                    systemname = NextStarSystem.systemname,
                    systemAddress = NextStarSystem.systemAddress,
                    distance = 0M,
                    stellarclass = @event.starclass
                };
                NextStarSystem.AddOrUpdateBody(mainStar);
                await DataProvider.SaveStarSystemAsync(NextStarSystem).ConfigureAwait(false);
            }
            return true;
        }

        private bool eventFileHeader(FileHeaderEvent @event)
        {
            // Test whether we're in beta by checking the filename, version described by the header,
            // and certain version / build combinations. Test the most common situations first.
            gameIsBeta = @event.filename.Contains("Alpha") ||
                         @event.filename.Contains("Beta") ||
                         @event.version.Contains("Beta") ||
                         @event.version.Contains("Alpha") ||
                         (
                             @event.version.Contains("2.2") &&
                             (
                                 @event.build.Contains("r121645/r0") ||
                                 @event.build.Contains("r129516/r0")
                             )
                         );
            CompanionAppService.Instance.gameIsBeta = gameIsBeta;
            if (gameIsBeta)
            {
                Logging.Info("Beta game version detected");
            }

            GameStateMutator.SetGameVersionDetails( @event.version, @event.build );

            return true;
        }

        internal async Task<bool> eventJumpedAsync( JumpedEvent theEvent )
        {
            bool passEvent;

            if ( theEvent.taxi is true )
            {
                Vehicle = Constants.VEHICLE_TAXI;
            }
            else if ( theEvent.multicrew is true )
            {
                Vehicle = Constants.VEHICLE_MULTICREW;
            }
            else
            {
                Vehicle = Constants.VEHICLE_SHIP;
            }

            if ( CurrentStarSystem?.systemAddress > 0 && CurrentStarSystem.systemAddress == theEvent.systemAddress )
            {
                // Thargoid Hyperdiction
                Logging.Info( $"Jump Interrupted: Hyperdicted in {theEvent.system}" );

                // After hyperdiction we are in normal space rather than supercruise
                Environment = Constants.ENVIRONMENT_NORMAL_SPACE;

                // Generate a hyperdiction event
                enqueueEvent(new HyperdictedEvent( theEvent.timestamp, theEvent.fuelused, theEvent.fuelremaining, theEvent.boostused, theEvent.taxi, theEvent.multicrew, theEvent.ThargoidWar ) { raw = null, fromLoad = theEvent.fromLoad } );

                passEvent = false;
            }
            else
            {
                // Normal FSD jump
                Logging.Info( "Jumped to " + theEvent.system );

                // After jump has completed we are always in supercruise
                Environment = Constants.ENVIRONMENT_SUPERCRUISE;

                passEvent = true;
            }

            await updateCurrentSystemAsync( theEvent.system, theEvent.systemAddress ).ConfigureAwait(false);
            if ( CurrentStarSystem is null ) { return false; }
            await ApplyCurrentSystemSnapshotAsync( CurrentStarSystem, theEvent.system, theEvent.systemAddress, theEvent.x, theEvent.y, theEvent.z, theEvent.controllingfaction, theEvent.factions, theEvent.conflicts, theEvent.Economy, theEvent.Economy2, theEvent.securityLevel, theEvent.population, theEvent.Power, theEvent.NearbyPowers, theEvent.PowerState, theEvent.powerAcquisitionProgress, theEvent.powerControlProgress, theEvent.powerReinforcementControlPoints, theEvent.powerUnderminingControlPoints, theEvent.ThargoidWar, theEvent.timestamp, true ).ConfigureAwait( false );

            return passEvent;
        }

        private async Task<bool> eventEnteredSupercruiseAsync( EnteredSupercruiseEvent theEvent )
        {
            Environment = Constants.ENVIRONMENT_SUPERCRUISE;
            await updateCurrentSystemAsync( theEvent.system, theEvent.systemAddress ).ConfigureAwait(false);

            if (theEvent.taxi is true)
            {
                Vehicle = Constants.VEHICLE_TAXI;
            }
            else if (theEvent.multicrew is true)
            {
                Vehicle = Constants.VEHICLE_MULTICREW;
            }
            else
            {
                Vehicle = Constants.VEHICLE_SHIP;
            }

            GameState.DeployedVessels.Clear();

            return true;
        }

        private async Task<bool> eventEnteredNormalSpaceAsync( EnteredNormalSpaceEvent theEvent )
        {
            Environment = Constants.ENVIRONMENT_NORMAL_SPACE;

            if (theEvent.bodyname != null && (theEvent.bodyType == BodyType.Moon || theEvent.bodyType == BodyType.Planet) )
            {
                await updateCurrentStellarBodyAsync( theEvent.bodyname, theEvent.bodyId, theEvent.systemname, theEvent.systemAddress).ConfigureAwait(false);
            }
            else
            {
                await updateCurrentSystemAsync( theEvent.systemname, theEvent.systemAddress ).ConfigureAwait( false );
            }

            if ( theEvent.bodyType == BodyType.Planet )
            {
                theEvent.bodyType = CurrentStarSystem?.bodies
                                        .FirstOrDefault( b => b.bodyId != null && b.bodyId == theEvent.bodyId )
                                        ?.bodyType ??
                                    theEvent.bodyType;
            }

            if (theEvent.taxi is true)
            {
                Vehicle = Constants.VEHICLE_TAXI;
            }
            else if (theEvent.multicrew is true)
            {
                Vehicle = Constants.VEHICLE_MULTICREW;
            }
            else
            {
                Vehicle = Constants.VEHICLE_SHIP;
            }

            return true;
        }

        private bool eventCrewJoined(CrewJoinedEvent @event)
        {
            inTelepresence = @event.telepresence ?? false;
            multicrewVehicleHolder = Vehicle;
            Vehicle = Constants.VEHICLE_MULTICREW;
            Logging.Info("Entering multicrew session");
            return true;
        }

        private bool eventCrewLeft()
        {
            inTelepresence = false;
            Vehicle = multicrewVehicleHolder;
            Logging.Info($"Leaving multicrew session to vehicle {Vehicle}");
            return true;
        }

        private bool eventCommanderContinued(CommanderContinuedEvent theEvent)
        {
            // Set Vehicle state for commander from ship model
            if (theEvent.shipEDModel.Contains("Suit"))
            {
                Vehicle = Constants.VEHICLE_LEGS;
            }
            else if (theEvent.shipEDModel == "TestBuggy" || theEvent.shipEDModel.Contains("SRV") || theEvent.shipEDModel.Contains( "Lander" ) )
            {
                Vehicle = Constants.VEHICLE_SRV;
            }
            else
            {
                Vehicle = Constants.VEHICLE_SHIP;
            }
            Logging.Debug($"Commander Continued: vehicle is {Vehicle}");

            // Set Environment state for the ship if 'startlanded' is present in the event
            if (theEvent.startlanded ?? false)
            {
                Environment = Constants.ENVIRONMENT_LANDED;
            }
            else if (theEvent.startlanded != null)
            {
                Environment = Constants.ENVIRONMENT_NORMAL_SPACE;
            }

            // If we see this it means that we aren't in Telepresence
            inTelepresence = false;

            // Identify active game version
            inHorizons = theEvent.horizons;
            inOdyssey = theEvent.odyssey;
            GameStateMutator.SetGameVersionDetails( theEvent.gameversion, theEvent.gamebuild );

            return true;
        }

        private bool eventEnteredCQC()
        {
            // In CQC we don't want to report anything, so set our Telepresence flag
            inTelepresence = true;
            return true;
        }

        private bool eventVesselDocked(VesselDockedEvent @event)
        {
            // We are back in the ship
            Vehicle = Constants.VEHICLE_SHIP;

            GameState.DeployedVessels.Remove( @event.id );
            @event.deployedVessels = GameState.DeployedVessels.Values.RemoveNulls().ToList();

            return true;
        }

        private bool eventVesselLaunched(VesselLaunchedEvent @event)
        {
            Vehicle = @event.playercontrolled 
                ? ( @event.vesselDefinition?.vesselGroup == VesselGroup.Piloted 
                    ? Constants.VEHICLE_SRV 
                    : Constants.VEHICLE_FIGHTER ) // We are in a vessel (either a piloted SRV or telepresence fighter). 
                : Constants.VEHICLE_SHIP; // We are (still) in the ship

            GameState.DeployedVessels.Add( @event.id, @event.vesselDefinition );

            return true;
        }

        private bool eventVesselDestroyed(VesselDestroyedEvent @event)
        {
            if ( @event.vesselDefinition.vesselGroup == VesselGroup.Telepresence )
            {
                // We are back in the ship
                Vehicle = Constants.VEHICLE_SHIP;
            }

            GameState.DeployedVessels.Remove( @event.id );
            return true;
        }

        private async Task<bool> eventNearSurfaceAsync( NearSurfaceEvent theEvent )
        {
            await _locationStateService.SetNearSurfaceBodyAsync( theEvent ).ConfigureAwait(false);
            return true;
        }

        private async Task<bool> eventStarScannedAsync(StarScannedEvent theEvent)
        {
            // We just scanned a star.  We can only proceed if we know our current star system
            if (CurrentStarSystem == null) { return false; }

            // Clear any temporary / placeholder stars (e.g. from FSDTarget events)
            if ( theEvent.star.mainstar ?? false )
            {
                CurrentStarSystem.ClearTemporaryStars();
            }

            // We use an un-named temporary star at distance 0M during the FSD Target event.
            // Try to match and replace that temporary star if it exists. Otherwise, match by body name.
            var star = CurrentStarSystem.bodies?
                .Where(s => s.bodyType == BodyType.Star).ToList()
                .Find(s => 
                    (string.IsNullOrEmpty(s.bodyname) && s.distance == 0M && s.distance == theEvent.distance) || 
                    s.bodyname == theEvent.bodyname);
            if (star?.scannedDateTime is null)
            {
                CurrentStarSystem.AddOrUpdateBody(theEvent.star);
                await DataProvider.SaveStarSystemAsync(CurrentStarSystem).ConfigureAwait(false);
                return true;
            }
            return false;
        }

        internal async Task<bool> eventBodyScannedAsync(BodyScannedEvent theEvent)
        {
            // We just scanned a body.  We can only proceed if we know our current star system
            if (CurrentStarSystem == null) { return false; }

            // Suppress repetitious `Body scanned` events generated within 10 seconds after mapping.
            var systemBody = CurrentStarSystem.bodies.FirstOrDefault( s => s.bodyname == theEvent.bodyname );
            if ( systemBody?.scannedDateTime != null && systemBody.mappedDateTime < ( theEvent.timestamp + TimeSpan.FromSeconds( 10 ) ) )
            {
                return false;
            }

            CurrentStarSystem.AddOrUpdateBody(theEvent.body);

            Logging.Debug("Saving data for scanned body " + theEvent.bodyname);
            await DataProvider.SaveStarSystemAsync(CurrentStarSystem).ConfigureAwait(false);

            return true;
        }

        internal async Task<bool> eventBodyMappedAsync(BodyMappedEvent theEvent)
        {
            if (CurrentStarSystem != null && theEvent.systemAddress == CurrentStarSystem.systemAddress)
            {
                // We've already updated the body (via the journal monitor) if the CurrentStarSystem isn't null
                // Here, we just need to save the data.
                await DataProvider.SaveStarSystemAsync(CurrentStarSystem).ConfigureAwait(false);
            }
            return true;
        }
    }
}
