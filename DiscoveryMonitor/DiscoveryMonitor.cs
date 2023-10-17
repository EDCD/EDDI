using EddiConfigService;
using EddiConfigService.Configurations;
using EddiCore;
using EddiDataDefinitions;
using EddiDataProviderService;
using EddiEvents;
using EddiStatusService;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Controls;
using Utilities;

[assembly: InternalsVisibleTo( "Tests" )]
namespace EddiDiscoveryMonitor
{
    public class DiscoveryMonitor : IEddiMonitor
    {
        internal class FssSignal
        {
            public ulong systemAddress;         // For reference to double check
            public long bodyId;                 // For reference to double check
            public int geoCount;                // The number of geological signals detected
            public int bioCount;                // The number of biological signals detected
        }
        internal readonly HashSet<FssSignal> fssSignalsLibrary = new HashSet<FssSignal>();

        internal DiscoveryMonitorConfiguration configuration;
        internal Exobiology _currentOrganic;
        internal long _currentBodyId;
        internal StarSystem _currentSystem => EDDI.Instance?.CurrentStarSystem;
        private Body _currentBody ( long bodyId ) => _currentSystem?.BodyWithID( bodyId );

        public DiscoveryMonitor ()
        {
            StatusService.StatusUpdatedEvent += HandleStatus;
            configuration = ConfigService.Instance.discoveryMonitorConfiguration;
        }

        public string MonitorName ()
        {
            return "Discovery Monitor";
        }

        public string LocalizedMonitorName ()
        {
            return Properties.DiscoveryMonitor.monitorName;
        }

        public string MonitorDescription ()
        {
            return Properties.DiscoveryMonitor.monitorDescription;
        }

        public bool IsRequired ()
        {
            return true;
        }

        public bool NeedsStart ()
        {
            return false;
        }

        public void Start ()
        { }

        public void Stop ()
        { }

        public void Reload ()
        {
            configuration = ConfigService.Instance.discoveryMonitorConfiguration;
        }

        public UserControl ConfigurationTabItem ()
        {
            return new ConfigurationWindow();
        }

        private void HandleStatus ( object sender, EventArgs e )
        {
            try
            {
                if ( sender is Status status )
                {
                    if ( TryCheckScanDistance( status, out var bio ) )
                    {
                        EDDI.Instance.enqueueEvent( new ScanOrganicDistanceEvent( DateTime.UtcNow, bio ) );
                    }
                }
            }
            catch ( Exception exception )
            {
                Logging.Error( "Failed to handle status", exception );
                throw;
            }
        }

        /// <summary>
        /// Check the currently active bio scan distance (if any). Return true if it's time to post a `ScanOrganicDistance` event.
        /// </summary>
        internal bool TryCheckScanDistance ( Status status, out Exobiology bioResult )
        {
            bioResult = null;
            if ( !CheckSafe() || status.latitude is null || status.longitude is null ) { return false; }

            var body = _currentBody(_currentBodyId);
            if ( body.surfaceSignals.TryGetBio( _currentOrganic, out var bio ) && bio.samples > 0 )
            {
                // If the bio has been fully sampled, ignore it.
                if( bio.scanState == Exobiology.State.SampleAnalysed) {
                    return false;
                }

                var distanceFromSamplesKm = new SortedSet<decimal>();
                foreach ( var coords in bio.sampleCoords )
                {
                    var distance = Functions.SurfaceDistanceKm( body.radius * 1000, status.latitude, status.longitude, coords.Item1, coords.Item2 );
                    if ( distance != null )
                    {
                        distanceFromSamplesKm.Add( (decimal)distance );
                    }
                }

                var maxDistanceKm = distanceFromSamplesKm.LastOrDefault();
                var minDistanceKm = distanceFromSamplesKm.FirstOrDefault();
                //var distanceM = maxDistanceKm * 1000;
                var distanceM = minDistanceKm * 1000;

                if ( distanceM <= bio.genus.minimumDistanceMeters )
                {
                    // Was previously outside sample range, alert that we have violated the radius
                    if ( !bio.nearPriorSample )
                    {
                        bio.nearPriorSample = true;
                        bioResult = bio;
                        return true;
                    }
                }
                else if ( distanceM > bio.genus.minimumDistanceMeters )
                {
                    // Was previously inside sample range, alert that we have traveled past the sample radius
                    if ( bio.nearPriorSample )
                    {
                        bio.nearPriorSample = false;
                        bioResult = bio;
                        return true;
                    }
                }
            }
            return false;
        }

        public void PreHandle ( Event @event )
        {
            Logging.Debug($"Handling {@event.type} event.", @event);

            if ( @event is CodexEntryEvent entryEvent )
            {
                handleCodexEntryEvent( entryEvent );
            }
            else if ( @event is SurfaceSignalsEvent signalsEvent )
            {
                handleSurfaceSignalsEvent( signalsEvent );
            }
            else if ( @event is ScanOrganicEvent organicEvent )
            {
                handleScanOrganicEvent( organicEvent );
            }
            else if ( @event is BodyScannedEvent bodyScannedEvent )
            {
                handleBodyScannedEvent( bodyScannedEvent );
            }
            else if ( @event is StarScannedEvent starScannedEvent )
            {
                handleStarScannedEvent( starScannedEvent );
            }
        }

        private void handleCodexEntryEvent ( CodexEntryEvent @event )
        {
            // Not sure if we have anything to do here with this yet
        }

        /// <summary>
        /// Triggered when a planet is scanned (FSS) and mapped (SAA).
        /// For FSS, store information so that we can predict the genus that will be present
        /// </summary>
        private void handleSurfaceSignalsEvent ( SurfaceSignalsEvent @event )
        {
            var log = "";
            if ( @event.detectionType == "FSS" )
            {
                if ( !fssSignalsLibrary.Any( s => s.systemAddress == @event.systemAddress && s.bodyId == @event.bodyId ) &&
                     TryGetFssSurfaceSignals( @event, ref log, out var signals ) )
                {
                    fssSignalsLibrary.Add( signals );
                }
            }
            else if ( @event.detectionType == "SAA" && _currentSystem != null )
            {
                if ( TrySetSaaSurfaceSignals( @event, ref log, out var body ) )
                {
                    // Save/Update Body data
                    body.surfaceSignals.lastUpdated = @event.timestamp;
                    //_currentSystem.AddOrUpdateBody( body );
                    //StarSystemSqLiteRepository.Instance.SaveStarSystem( _currentSystem );
                    EDDI.Instance?.CurrentStarSystem.AddOrUpdateBody( body );
                    StarSystemSqLiteRepository.Instance.SaveStarSystem(EDDI.Instance.CurrentStarSystem);
                }
            }
            if ( configuration.enableLogging )
            {
                Logging.Debug( log );
            }
            catch ( Exception e )
            {
                Logging.Error("Failed to handle SurfaceSignalsEvent", e);
            }

        }

        private static bool TryGetFssSurfaceSignals ( SurfaceSignalsEvent @event, ref string log, out FssSignal signal )
        {
            if ( @event.systemAddress is null )
            { signal = null; return false; }

            log += "[FSSBodySignals]: ";
            signal = new FssSignal { systemAddress = (ulong)@event.systemAddress, bodyId = @event.bodyId };
            var addSignal = false;

            foreach ( var sig in @event.surfacesignals )
            {
                if ( sig.signalSource == SignalSource.Biological )
                {
                    log += $"Bios: {sig.amount}. ";
                    signal.bioCount = sig.amount;
                    addSignal = true;
                }
                else if ( sig.signalSource == SignalSource.Geological )
                {
                    log += $"Geos: {sig.amount}. ";
                    signal.geoCount = sig.amount;
                    addSignal = true;
                }
            }

            Logging.Debug( log );

            signal = addSignal ? signal : null;
            return addSignal;
        }

        private bool TrySetSaaSurfaceSignals ( SurfaceSignalsEvent @event, ref string log, out Body body )
        {
            body = _currentSystem?.BodyWithID( @event.bodyId );
            if ( body == null )
            {
                Logging.Debug("Body is null, aborting.");
                return false;
            }

            // Set the number of detected surface signals for each signal type
            foreach ( var signal in @event.surfacesignals )
            {
                if ( signal.signalSource == SignalSource.Biological )
                {
                    log += $"Bios: {signal.amount}. ";
                    body.surfaceSignals.reportedBiologicalCount = signal.amount;
                }
                else if ( signal.signalSource == SignalSource.Geological )
                {
                    log += $"Geos: {signal.amount}. ";
                    body.surfaceSignals.reportedGeologicalCount = signal.amount;
                }
            }

            // If the current list was predicted then erase and recreate with actual values
            // If the number of bios in the list does not match the reported number of bios then clear
            if ( body.surfaceSignals.predicted ||
                body.surfaceSignals.biosignals.Count != body.surfaceSignals.reportedBiologicalCount )
            {
                log += $"\r\n\tClearing predictions from bio list.";
                body.surfaceSignals.biosignals.Clear();

                // Only update the list if it was predicted or doesn't match the reported count (this should catch an empty list).
                body.surfaceSignals.biosignals = @event.bioSignals;
            }

            Logging.Debug( log );

            // TODO: Instead of a complete overwrite or just ignore add a function to update data only.

            //if ( @event.bioSignals != null )
            //{
                // TODO: Compare our predicted and actual bio signals.

                    if ( unpredictedBiologicals.Any() )
                    {
                        log = "Unpredicted biologicals found";
                        log += $"\tStar System:  {body.systemname}\r\n";
                        log += $"\tBody Name:    {body.bodyname}\r\n";
                        log += $"\tGravity:      {body.gravity}\r\n";
                        log += $"\tTemperature:  {body.temperature} K\r\n";
                        log += $"\tPlanet Class: {(body.planetClass ?? PlanetClass.None).edname}\r\n";
                        log += $"\tAtmosphere:   {(body.atmosphereclass ?? AtmosphereClass.None).edname}\r\n";
                        log += $"\tVolcanism:    {body.volcanism?.edComposition ?? "None"}\r\n";
                        if ( _currentSystem?.TryGetParentStar( body.bodyId, out var parentStar ) ?? false )
                        {
                            log += $"\tParent star class: {parentStar.stellarclass}\r\n";
                        }
                        Logging.Error( log, unpredictedBiologicals);
                    }

                    if ( missingBiologicals.Any() )
                    {
                        log = "Predicted biologicals not found";
                        log += $"\tStar System:  {body.systemname}\r\n";
                        log += $"\tBody Name:    {body.bodyname}\r\n";
                        log += $"\tGravity:      {body.gravity}\r\n";
                        log += $"\tTemperature:  {body.temperature} K\r\n";
                        log += $"\tPlanet Class: {( body.planetClass ?? PlanetClass.None ).edname}\r\n";
                        log += $"\tAtmosphere:   {( body.atmosphereclass ?? AtmosphereClass.None ).edname}\r\n";
                        log += $"\tVolcanism:    {body.volcanism?.edComposition ?? "None"}\r\n";
                        if ( _currentSystem?.TryGetParentStar( body.bodyId, out var parentStar ) ?? false )
                        {
                            log += $"\tParent star class: {parentStar.stellarclass}\r\n";
                        }
                        Logging.Debug( log, missingBiologicals );
                    }
                }

                // Update from predicted to actual bio signals
                //body.surfaceSignals.biosignals = @event.bioSignals;
            //}

            return true;
        }

        private void handleScanOrganicEvent ( ScanOrganicEvent @event )
        {
            try
            {
                if ( CheckSafe( @event.bodyId ) )
                {
                    _currentBodyId = @event.bodyId;
                    var body = _currentBody(_currentBodyId);
                    var log = "";

                    // Retrieve and/or add the organic
                    if ( body.surfaceSignals.TryGetBio( @event.variant, @event.species, @event.genus, out var bio ) )
                    {
                        log += "Fetched biological\r\n";
                    }
                    else
                    {
                        log += "Adding biological\r\n";
                        bio = bio ?? body.surfaceSignals.AddBio( @event.variant, @event.species, @event.genus );
                    }

                    if ( bio == null )
                    {
                        Logging.Debug( log );
                        return;
                    }

                    _currentOrganic = bio;

                    if ( bio.scanState == Exobiology.State.Predicted )
                    {
                        log += $"Presence of predicted organic {bio.species} is confirmed\r\n";
                        bio.scanState = Exobiology.State.Confirmed;
                    }

                    if ( bio.variant is null )
                    {
                        log += "Setting additional data from variant details\r\n";
                        bio.SetVariantData( @event.variant );
                    }

                    bio.Sample( @event.scanType,
                        @event.variant,
                        StatusService.Instance.CurrentStatus.latitude,
                        StatusService.Instance.CurrentStatus.longitude );

                    // These are updated when the above Sample() function is called, se we send them back to the event
                    // Otherwise we would probably have to enqueue a new event (maybe not a bad idea?)
                    @event.bio = bio;
                    @event.remainingBios = body.surfaceSignals.bioSignalsRemaining.Except( new[] { bio } ).ToList();

                    Logging.Debug( log, @event );

                if ( bio.scanState == Exobiology.State.SampleComplete )
                {
                    // The `Analyse` journal event normally takes place about 5 seconds after completing the sample
                    // but can be delayed if the commander holsters their scanner before the analysis cycle is completed.
                    Task.Run( async () =>
                    {
                        int timeMs = 15000; // If after 15 seconds the event hasn't generated then
                                            // we'll generate our own event and update our own internal tracking
                                            // (regardless of whether the scanner is holstered).
                        await Task.Delay( timeMs );
                        if ( bio.scanState < Exobiology.State.SampleAnalysed )
                        {
                            EDDI.Instance.enqueueEvent(
                                new ScanOrganicEvent( 
                                    @event.timestamp.AddMilliseconds( timeMs ), 
                                    @event.systemAddress,
                                    @event.bodyId, "Analyse", 
                                    @event.genus, 
                                    @event.species, 
                                    @event.variant )
                                {
                                    fromLoad = @event.fromLoad
                                } );
                        }
                    } ).ConfigureAwait( false );
                }

                // Save/Update Body data
                body.surfaceSignals.lastUpdated = @event.timestamp;
                //_currentSystem.AddOrUpdateBody( body );
                //StarSystemSqLiteRepository.Instance.SaveStarSystem( _currentSystem );
                EDDI.Instance?.CurrentStarSystem.AddOrUpdateBody( body );
                StarSystemSqLiteRepository.Instance.SaveStarSystem(EDDI.Instance.CurrentStarSystem);
            }
        }

        private void handleBodyScannedEvent ( BodyScannedEvent @event )
        {
            try
            {
                if ( @event.bodyId is null || !CheckSafe( (long)@event.bodyId ) ) { return; }

                if ( @event.systemAddress == _currentSystem.systemAddress )
                {
                    // Predict biologicals for a scanned body
                    var body = _currentBody( (long)@event.bodyId );
                    var signal = fssSignalsLibrary.FirstOrDefault( s =>
                        s.systemAddress == body.systemAddress && s.bodyId == body.bodyId );

                    if ( signal != null &&
                         !body.surfaceSignals.bioSignals.Any() &&
                         TryPredictBios( signal, ref body ) )
                    {
                        EDDI.Instance.enqueueEvent( new OrganicPredictionEvent( DateTime.UtcNow, body,
                            body.surfaceSignals.bioSignals ) );

                    // Save/Update Body data
                    body.surfaceSignals.lastUpdated = @event.timestamp;
                    //_currentSystem.AddOrUpdateBody( body );
                    //StarSystemSqLiteRepository.Instance.SaveStarSystem( _currentSystem );
                    EDDI.Instance?.CurrentStarSystem.AddOrUpdateBody( body );
                    StarSystemSqLiteRepository.Instance.SaveStarSystem(EDDI.Instance.CurrentStarSystem);
                }
            }
        }

        private void handleStarScannedEvent ( StarScannedEvent @event )
        {
            try
            {
                if ( @event.bodyId is null ) { return; }

                if ( _currentSystem != null && @event.systemAddress == _currentSystem.systemAddress )
                {
                    // Predict biologicals for previously scanned bodies when a star is scanned
                    var childBodyIDs = _currentSystem.GetChildBodyIDs( (long)@event.bodyId );
                    foreach ( var childBodyID in _currentSystem.bodies
                                 .Where( b => b.bodyId != null && childBodyIDs.Contains( (long)b.bodyId ) )
                                 .Select( b => b.bodyId ) )
                    {
                        var body = _currentBody( (long)childBodyID );
                        var signal = fssSignalsLibrary.FirstOrDefault( s =>
                            s.systemAddress == body.systemAddress && s.bodyId == body.bodyId );

                        if ( signal != null &&
                             !body.surfaceSignals.bioSignals.Any() &&
                             TryPredictBios( signal, ref body ) )
                        {
                            EDDI.Instance.enqueueEvent( new OrganicPredictionEvent( DateTime.UtcNow, body,
                                body.surfaceSignals.bioSignals ) );

                        // Save/Update Body data
                        body.surfaceSignals.lastUpdated = @event.timestamp;
                        //_currentSystem.AddOrUpdateBody( body );
                        //StarSystemSqLiteRepository.Instance.SaveStarSystem( _currentSystem );
                        EDDI.Instance?.CurrentStarSystem.AddOrUpdateBody( body );
                        StarSystemSqLiteRepository.Instance.SaveStarSystem(EDDI.Instance.CurrentStarSystem);
                    }
                }
            }
        }

        private bool TryPredictBios(FssSignal signal, ref Body body)
        {
            var log = "";
            var hasPredictedBios = false;

            // TODO: This shouldn't be here, has nothing to do with bio predictions
            if ( signal?.geoCount > 0 && body != null)
            {
                body.surfaceSignals.reportedGeologicalCount = signal.geoCount;
            }

            if ( signal?.bioCount > 0 && body != null)
            {
                // Always update the reported totals
                body.surfaceSignals.reportedBiologicalCount = signal.bioCount;
                log += $"[handleBodyScannedEvent:FSS backlog <{body.systemAddress},{body.bodyId}>\r\n" +
                       $"\tBio Count is {signal.bioCount} ({body.surfaceSignals.reportedBiologicalCount})\r\n" +
                       $"\tGeo Count is {signal.geoCount} ({body.surfaceSignals.reportedGeologicalCount})\r\n";
                
                // Predict possible biological genuses
                HashSet<OrganicGenus> bios;
                log += "Predicting organics (by species):\r\n";
                bios = new ExobiologyPredictions( _currentSystem, body, parentStar, configuration ).PredictBySpecies();
                foreach ( var genus in bios )
                {
                    log += $"\tAdding predicted bio {genus.invariantName}\r\n";
                    body.surfaceSignals.AddBioFromGenus( genus, true );
                }
                hasPredictedBios = true;
            }

            Logging.Debug( log );

            return hasPredictedBios;
        }

        /// <summary>
        /// Check if the current system and body exist
        /// </summary>
        private bool CheckSafe ()
        {
            if ( _currentOrganic != null )
            {
                if ( _currentSystem != null )
                {
                    if ( _currentBody( _currentBodyId ) != null )
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private bool CheckSafe ( long bodyId )
        {
            if ( _currentSystem != null )
            {
                if ( _currentBody( bodyId ) != null )
                {
                    _currentBodyId = bodyId;
                    return true;
                }
            }

            return false;
        }

        public void PostHandle ( Event @event )
        { }

        public void HandleProfile ( JObject profile )
        { }

        public IDictionary<string, Tuple<Type, object>> GetVariables ()
        {
            //return null;

            return new Dictionary<string, Tuple<Type, object>>
            {
                [ "bio_settings" ] = new Tuple<Type, object>( typeof( DiscoveryMonitorConfiguration.Exobiology ), configuration.exobiology ),
                [ "codex_settings" ] = new Tuple<Type, object>( typeof( DiscoveryMonitorConfiguration.Codex ), configuration.codex )
            };
        }
    }
}
