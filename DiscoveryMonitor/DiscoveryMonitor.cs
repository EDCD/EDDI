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
using System.Windows.Controls;
using Utilities;

namespace EddiDiscoveryMonitor
{
    public class DiscoveryMonitor : IEddiMonitor
    {
        private class FssSignal
        {
            public ulong systemAddress;         // For reference to double check
            public long bodyId;                 // For reference to double check
            public int geoCount;                // The number of geological signals detected
            public int bioCount;                // The number of biological signals detected
            public bool exobiologyPredicted;    // Has this body had its bios predicted yet (false = FSSBodySignals event has occured but not Scan event)
        }
        private HashSet<FssSignal> fssSignalsLibrary = new HashSet<FssSignal>();

        private DiscoveryMonitorConfiguration configuration;
        private OrganicGenus _currentGenus;
        private long _currentBodyId;
        private StarSystem _currentSystem => EDDI.Instance?.CurrentStarSystem;
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
            return "Discovery Monitor";
        }

        public string MonitorDescription ()
        {
            return "Monitor Elite: Dangerous' Discovery events for Organics (including exobiology), geology, phenomena, codex entries, etc.";
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
            if ( sender is Status status )
            {
                UpdateScanDistance( status );
            }
        }

        /// <summary>
        /// Update the currently active bio scan distance (if any)
        /// </summary>
        private void UpdateScanDistance ( Status status )
        {
            if ( !CheckSafe() || status.latitude is null || status.longitude is null )
            { return; }

            var body = _currentBody(_currentBodyId);
            if ( body.surfaceSignals.TryGetBio( _currentGenus, out var bio ) && bio.samples > 0 )
            {
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
                var distanceM = maxDistanceKm * 1000;

                if ( distanceM <= bio.genus.minimumDistanceMeters )
                {
                    // Was previously outside sample range, alert that we have violated the radius
                    if ( !bio.nearPriorSample )
                    {
                        bio.nearPriorSample = true;
                        EDDI.Instance.enqueueEvent( new ScanOrganicDistanceEvent( DateTime.UtcNow, bio ) );
                    }
                }
                else if ( distanceM > bio.genus.minimumDistanceMeters )
                {
                    // Was previously inside sample range, alert that we have traveled past the sample radius
                    if ( bio.nearPriorSample )
                    {
                        bio.nearPriorSample = false;
                        EDDI.Instance.enqueueEvent( new ScanOrganicDistanceEvent( DateTime.UtcNow, bio ) );
                    }
                }
            }
        }

        public void PreHandle ( Event @event )
        {
            if ( @event is CodexEntryEvent entryEvent )
            { handleCodexEntryEvent( entryEvent ); }
            else if ( @event is SurfaceSignalsEvent signalsEvent )
            { handleSurfaceSignalsEvent( signalsEvent ); }
            else if ( @event is ScanOrganicEvent organicEvent )
            { handleScanOrganicEvent( organicEvent ); }
            else if ( @event is BodyScannedEvent scannedEvent )
            { handleBodyScannedEvent( scannedEvent ); }
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
                    _currentSystem.AddOrUpdateBody( body );
                    StarSystemSqLiteRepository.Instance.SaveStarSystem( _currentSystem );
                }
            }
            if ( configuration.enableLogging )
            {
                Logging.Debug( log );
            }
        }

        private bool TryGetFssSurfaceSignals ( SurfaceSignalsEvent @event, ref string log, out FssSignal signal )
        {
            if ( @event.systemAddress is null )
            { signal = null; return false; }

            log += "[FSSBodySignals]:\r\n";
            signal = new FssSignal { systemAddress = (ulong)@event.systemAddress, bodyId = @event.bodyId };
            var addSignal = false;

            foreach ( var sig in @event.surfacesignals )
            {
                if ( sig.signalSource.edname == "SAA_SignalType_Biological" )
                {
                    log += $"\tDetect bios: {sig.amount}\r\n";
                    signal.bioCount = sig.amount;
                    signal.exobiologyPredicted = false;
                    addSignal = true;
                }
                else if ( sig.signalSource.edname == "SAA_SignalType_Geological" )
                {
                    log += $"\tDetect geos: {sig.amount}\r\n";
                    signal.geoCount = sig.amount;
                    addSignal = true;
                }
            }

            signal = addSignal ? signal : null;
            return addSignal;
        }

        private bool TrySetSaaSurfaceSignals ( SurfaceSignalsEvent @event, ref string log, out Body body )
        {
            body = _currentSystem?.BodyWithID( @event.bodyId );
            if ( body == null )
            { return false; }

            // Set the number of detected signals for both Bio and Geo
            foreach ( var signal in @event.surfacesignals )
            {
                // Save the number of biologicals to update SurfaceSignals
                if ( signal.edname == "SAA_SignalType_Biological" )
                {
                    body.surfaceSignals.reportedBiologicalCount = signal.amount;
                }

                if ( signal.edname == "SAA_SignalType_Geological" )
                {
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
            }

            Logging.Debug( log );

            if ( @event.bioSignals != null )
            {
                // The bio list is no longer a prediction, do not update it again.
                body.surfaceSignals.predicted = false;

                // TODO: Compare our predicted and actual bio signals.

                // Update from predicted to actual bio signals
                body.surfaceSignals.biosignals = @event.bioSignals;
            }

            return true;
        }

        private void handleScanOrganicEvent ( ScanOrganicEvent @event )
        {
            string log = "";

            _currentBodyId = @event.bodyId;
            _currentGenus = @event.genus;

            log += $"[handleScanOrganicEvent] --------------------------------------------\r\n";

            if ( CheckSafe() )
            {
                log += $"[handleScanOrganicEvent] CheckSafe OK\r\n";

                Body body = _currentBody(_currentBodyId);

                // If the biological doesn't exist, lets add it now
                if ( !body.surfaceSignals.TryGetBio( @event.genus, out var bio ) )
                {
                    log += $"[handleScanOrganicEvent] Genus doesn't exist in list, adding {@event.genus}\r\n";
                    body.surfaceSignals.AddBioFromGenus( @event.genus );
                }
                // If only the genus is present, then finish other data (and prune predictions)
                else if ( bio.samples == 0 )
                {
                    log += $"[handleScanOrganicEvent] Samples is zero, setting additional data from variant\r\n";
                    bio.SetVariantData( @event.variant );
                }

                if ( configuration.enableLogging )
                {
                    Logging.Debug( log );
                }

                bio.Sample( @event.scanType,
                            @event.variant,
                            StatusService.Instance.CurrentStatus.latitude,
                            StatusService.Instance.CurrentStatus.longitude );

                // These are updated when the above Sample() function is called, se we send them back to the event
                // Otherwise we would probably have to enqueue a new event (maybe not a bad idea?)
                @event.bio = bio;
                @event.numTotal = body.surfaceSignals.biosignals.Count;
                @event.listRemaining = body.surfaceSignals.biosignalsremaining().Select( b => b.genus.localizedName ).ToList();

                if ( configuration.enableLogging )
                {
                    log = $"[handleScanOrganicEvent] SetBio ---------------------------------------------\r\n";
                    log += $"[handleScanOrganicEvent] SetBio:    Genus = '{@event.bio.genus.invariantName}'\r\n";
                    log += $"[handleScanOrganicEvent] SetBio:  Species = '{@event.bio.species.invariantName}'\r\n";
                    log += $"[handleScanOrganicEvent] SetBio:  Variant = '{@event.bio.variant.invariantName}'\r\n";
                    log += $"[handleScanOrganicEvent] SetBio:    Genus = '{@event.bio.genus.invariantName}'\r\n";
                    log += $"[handleScanOrganicEvent] SetBio: Distance = '{@event.bio.genus.minimumDistanceMeters}'\r\n";
                    log += $"[handleScanOrganicEvent] SetBio ---------------------------------------------\r\n";
                    Logging.Info( log );
                }

                // TODO: 2212: Save/Update Body data
                body.surfaceSignals.lastUpdated = @event.timestamp;
                _currentSystem.AddOrUpdateBody( body );
                StarSystemSqLiteRepository.Instance.SaveStarSystem( _currentSystem );
            }
        }

        private void handleBodyScannedEvent ( BodyScannedEvent @event )
        {
            string log = "";

            // Transfer biologicals from FSS to body.
            if ( fssSignalsLibrary != null && @event.systemAddress != null && @event.bodyId != null )
            {
                FssSignal signal = fssSignalsLibrary.First(s => s.systemAddress == (ulong)@event.systemAddress && s.bodyId == (long)@event.bodyId );

                bool saveBody = false;

                _currentBodyId = (long)@event.bodyId;
                if ( CheckSafe( _currentBodyId ) )
                {
                    Body body = _currentBody(_currentBodyId);

                    // Always update the reported totals
                    body.surfaceSignals.reportedBiologicalCount = signal.bioCount;
                    body.surfaceSignals.reportedGeologicalCount = signal.geoCount;

                    log += $"[handleBodyScannedEvent:FSS backlog <{@event.systemAddress},{@event.bodyId}>\r\n" +
                           $"\tBio Count is {signal.bioCount} ({body.surfaceSignals.reportedBiologicalCount})\r\n" +
                           $"\tGeo Count is {signal.geoCount} ({body.surfaceSignals.reportedGeologicalCount})\r\n";

                    if ( signal.exobiologyPredicted == false )
                    {
                        if ( signal.bioCount > 0 )
                        {
                            log += "[handleBodyScannedEvent] FSS status is false:\r\n";
                            HashSet<OrganicGenus> bios;
                            if ( configuration.enableVariantPredictions )
                            {
                                log += "[handleBodyScannedEvent] Predicting by variants:\r\n";
                                bios = new ExobiologyPredictions(_currentSystem, body, configuration).PredictByVariants();
                            }
                            else
                            {
                                log += "[handleBodyScannedEvent] Predicting by species:\r\n";
                                bios = new ExobiologyPredictions( _currentSystem, body, configuration ).PredictBySpecies();
                            }

                            log += $"\r\n\tClearing current bio list";
                            body.surfaceSignals.biosignals.Clear();

                            foreach ( var genus in bios )
                            {
                                log += $"\r\n\tAddBio {genus}";
                                body.surfaceSignals.AddBioFromGenus( genus, true );
                            }
                            if ( configuration.enableLogging )
                            {
                                Logging.Debug( log );
                            }

                            // This is used by SAASignalsFound to know if we can safely clear the list to create the actual bio list
                            body.surfaceSignals.predicted = true;
                            signal.exobiologyPredicted = true;

                            if ( configuration.enableLogging )
                            {
                                log += "\r\n[handleBodyScannedEvent]:";
                                var bioList = body.surfaceSignals.biosignals.Select(s => s.genus.invariantName).ToList();
                                foreach ( var invariantGenus in bioList )
                                {
                                    log += $"\r\n\tGetBios {invariantGenus}";
                                }
                            }

                            // This doesn't have to be used but is provided just in case
                            EDDI.Instance.enqueueEvent( new OrganicPredictionEvent( DateTime.UtcNow, body, body.surfaceSignals.biosignals ) );

                            saveBody = true;
                        }
                    }
                    else
                    {
                        log += "\r\n[handleBodyScannedEvent] FSS status is true (already added)]:";
                    }

                    if ( configuration.enableLogging )
                    {
                        Logging.Debug( log );
                    }

                    if ( saveBody )
                    {
                        // TODO:#2212: Save/Update Body data
                        body.surfaceSignals.lastUpdated = @event.timestamp;
                        _currentSystem.AddOrUpdateBody( body );
                        StarSystemSqLiteRepository.Instance.SaveStarSystem( _currentSystem );
                    }
                }
            }
        }

        /// <summary>
        /// Check if the current system and body exist
        /// </summary>
        private bool CheckSafe ()
        {
            if ( _currentGenus != null )
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
        {
        }

        public void HandleProfile ( JObject profile )
        {
        }

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
