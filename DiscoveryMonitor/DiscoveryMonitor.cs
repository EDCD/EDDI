﻿using EddiCore;
using EddiDataDefinitions;
using EddiDataProviderService;
using EddiEvents;
using EddiStatusService;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Controls;
using Utilities;

namespace EddiDiscoveryMonitor
{
    public class DiscoveryMonitor : IEddiMonitor
    {
        private class FSS_Signals
        {
            public ulong systemAddress; // For reference to double check
            public long bodyId;         // For reference to double check
            public int geoCount;     // The number of geological signals detected
            public int bioCount;     // The number of biological signals detected
            public bool status;         // Has this body had its bios predicted yet (false = FSSBodySignals event has occured but not Scan event)
        }

        // Dictionary of FSSBodySignals events
        //  - The Tuple is the SystemAddress and BodyId.
        //  - The bool value 
        //private List<Tuple<long, long>> FSS_Status;
        private Dictionary<Tuple<ulong, long>, FSS_Signals> _fss_Signals;

        private string _currentGenus;
        private long _currentBodyId;
        private StarSystem _currentSystem => EDDI.Instance?.CurrentStarSystem;
        private Body _currentBody(long bodyId) => _currentSystem?.BodyWithID( bodyId );
        
        
        //private IDictionary<string,Exobiology> currentBios => currentBody.surfaceSignals.bio.list;

        //[PublicAPI( "The current biological" )]
        //public Exobiology currentBio
        //{
        //    get
        //    {
        //        if ( currentBody.surfaceSignals.bio.list.ContainsKey( currentGenus ) )
        //        {
        //            return currentBody.surfaceSignals.bio.list[ currentGenus ];
        //        }
        //        return null;
        //    }
        //}

        public DiscoveryMonitor ()
        {
            StatusService.StatusUpdatedEvent += HandleStatus;
            //System.Diagnostics.Debug.WriteLine($"Initialized {MonitorName()}");

            _fss_Signals = new Dictionary<Tuple<ulong, long>, FSS_Signals>();
        }

        public string MonitorName()
        {
            return "Discovery Monitor";
        }

        public string LocalizedMonitorName()
        {
            return "Discovery Monitor";
        }

        public string MonitorDescription()
        {
            return "Monitor Elite: Dangerous' Discovery events for Organics (including exobiology), geology, phenomena, codex entries, etc.";
        }

        public bool IsRequired()
        {
            return true;
        }

        public bool NeedsStart()
        {
            return false;
        }

        public void Start()
        {
        }

        public void Stop()
        {
        }

        public void Reload()
        {
        }

        public UserControl ConfigurationTabItem ()
        {
            return new ConfigurationWindow();
        }

        private void HandleStatus ( object sender, EventArgs e )
        {
            var currentStatus = StatusService.Instance.CurrentStatus;
            if ( currentStatus != null )
            {
                UpdateScanDistance( currentStatus );
            }
        }

        /// <summary>
        /// Update the currently active bio scan distance (if any)
        /// </summary>
        private void UpdateScanDistance( Status status )
        {
            if ( CheckSafe() )
            {
                Body body = _currentBody(_currentBodyId);

                if ( body.surfaceSignals.bio.list.ContainsKey( _currentGenus ) ) {
                    int samples = body.surfaceSignals.bio.list[ _currentGenus ].samples;
                    if ( samples > 0 && samples < 3)
                    {
                        if ( status.latitude != null && status.longitude != null )
                        {
                            // Is the current location status not equal to the last status (0=no change), and if the distance is less than (1) or greater than (2) the sample range.
                            int status1 = 0;
                            int status2 = 0;

                            Exobiology.Coordinates coords1;
                            Exobiology.Coordinates coords2;

                            if ( samples >= 1 )
                            {
                                coords1 = body.surfaceSignals.bio.list[ _currentGenus ].coords[ 0 ];
                                coords1.lastStatus = coords1.status;
                                decimal? distance1 = Utilities.Functions.SurfaceDistanceKm(body.radius*1000, status.latitude, status.longitude, coords1.latitude, coords1.longitude);

                                if ( distance1 != null )
                                {
                                    // convert Km to m
                                    distance1 *= (decimal)1000.0;

                                    if ( distance1 <= body.surfaceSignals.bio.list[ _currentGenus ].genus.distance )
                                    {
                                        // Was previously outside sample range, alert that we have violated the radius
                                        if ( coords1.lastStatus == Exobiology.Status.OutsideSampleRange )
                                        {
                                            status1 = 1;
                                            coords1.status = Exobiology.Status.InsideSampleRange;
                                        }
                                    }
                                    else if ( distance1 > body.surfaceSignals.bio.list[ _currentGenus ].genus.distance )
                                    {
                                        // Was previously inside sample range, alert that we have traveled past the sample radius
                                        if ( coords1.lastStatus == Exobiology.Status.InsideSampleRange )
                                        {
                                            status1 = 2;
                                            coords1.status = Exobiology.Status.OutsideSampleRange;
                                        }
                                    }
                                }
                            }

                            if ( samples >= 2 )
                            {
                                coords2 = body.surfaceSignals.bio.list[ _currentGenus ].coords[ 1 ];
                                coords2.lastStatus = coords2.status;
                                decimal? distance2 = Utilities.Functions.SurfaceDistanceKm(body.radius*1000, status.latitude, status.longitude, coords2.latitude, coords2.longitude);


                                if ( distance2 != null )
                                {
                                    // convert Km to m
                                    distance2 *= (decimal)1000.0;
                                    if ( distance2 <= body.surfaceSignals.bio.list[ _currentGenus ].genus.distance )
                                    {
                                        // Was previously outside sample range, alert that we have violated the radius
                                        if ( coords2.lastStatus == Exobiology.Status.OutsideSampleRange )
                                        {
                                            status2 = 1;
                                            coords2.status = Exobiology.Status.InsideSampleRange;
                                        }
                                    }
                                    else if ( distance2 > body.surfaceSignals.bio.list[ _currentGenus ].genus.distance )
                                    {
                                        // Was previously inside sample range, alert that we have traveled past the sample radius
                                        if ( coords2.lastStatus == Exobiology.Status.InsideSampleRange )
                                        {
                                            status2 = 2;
                                            coords2.status = Exobiology.Status.OutsideSampleRange;
                                        }
                                    }
                                }
                            }

                            if ( status1 > 0 || status2 > 0 )
                            {
                                try
                                {
                                    // 2212: Save/Update Body data
                                    // Only update when there is a status change, otherwise we don't care
                                    EDDI.Instance?.CurrentStarSystem.AddOrUpdateBody( body );
                                    StarSystemSqLiteRepository.Instance.SaveStarSystem( _currentSystem );

                                    EDDI.Instance.enqueueEvent( new ScanOrganicDistanceEvent( DateTime.UtcNow, body.surfaceSignals.bio.list[ _currentGenus ].genus.distance, status1, status2 ) );
                                }
                                catch ( Exception e )
                                {
                                    Logging.Error( $"Exobiology: Failed to Enqueue 'ScanOrganicDistanceEvent' [{e}]" );
                                }
                            }
                            
                        }
                    }
                }
            }

        }

        public void PreHandle(Event @event)
        {
            //if ( !@event.fromLoad )
            //{
                if ( @event is CodexEntryEvent )            { handleCodexEntryEvent( (CodexEntryEvent)@event ); }
                else if ( @event is SurfaceSignalsEvent )   { handleSurfaceSignalsEvent( (SurfaceSignalsEvent)@event ); }
                else if ( @event is ScanOrganicEvent )      { handleScanOrganicEvent( (ScanOrganicEvent)@event ); }
                else if ( @event is BodyScannedEvent )      { handleBodyScannedEvent( (BodyScannedEvent)@event ); }
            //}
        }

        private void handleCodexEntryEvent ( CodexEntryEvent @event )
        {            
            // Not sure if we have anything to do here with this yet
        }

        /// <summary>
        /// Triggered when a planet is scanned (FSS) and mapped (SAA).
        /// For FSS, predict genus that will be present
        /// </summary>
        private void handleSurfaceSignalsEvent ( SurfaceSignalsEvent @event )
        {
            if ( @event.detectionType == "FSS" )
            {
                FSS_Signals signals = new FSS_Signals();

                signals.systemAddress = (ulong)@event.systemAddress;
                signals.bodyId = @event.bodyId;
                bool addSignal = false;

                foreach ( SignalAmount sig in @event.surfacesignals )
                {
                    if ( sig.signalSource.edname == "SAA_SignalType_Biological" )
                    {
                        signals.bioCount = sig.amount;
                        signals.status = false;
                        addSignal = true;
                    }
                    else if ( sig.signalSource.edname == "SAA_SignalType_Geological" )
                    {
                        signals.geoCount = sig.amount;
                        addSignal = true;
                    }
                }

                if ( addSignal )
                {
                    Tuple<ulong, long> myTuple = new Tuple<ulong, long>( (ulong)@event.systemAddress, @event.bodyId );
                    if ( !_fss_Signals.ContainsKey( myTuple ) )
                    {
                        _fss_Signals.Add( myTuple, signals );
                    }
                    //else
                    //{
                    //    _fss_Signals[ myTuple ] = signals;
                    //}
                }
            }
            else if ( @event.detectionType == "SAA" )
            {
                // TODO:#2212........[Do we need to do anything here?]
            }
        }

        private void handleScanOrganicEvent ( ScanOrganicEvent @event )
        {
            _currentBodyId = @event.bodyId;
            _currentGenus = @event.genus;

            // TODO:#2212........[Remove]
            Logging.Info( $"[handleScanOrganicEvent] --------------------------------------------" );
            Thread.Sleep( 10 );

            if ( CheckSafe() )
            {
                // TODO:#2212........[Remove]
                Logging.Info( $"[handleScanOrganicEvent] CheckSafe OK" );
                Thread.Sleep( 10 );

                Body body = _currentBody(_currentBodyId);

                // If the biological doesn't exist, lets add it now
                if ( !body.surfaceSignals.bio.list.ContainsKey( @event.genus ) )
                {
                    // TODO:#2212........[Remove]
                    Logging.Info( $"[handleScanOrganicEvent] Genus doesn't exist in list, adding {@event.genus}" );
                    Thread.Sleep( 10 );
                    body.surfaceSignals.AddBio( @event.genus );
                }

                // If only the genus is present, then finish other data (and prune predictions)
                if ( body.surfaceSignals.bio.list[ @event.genus ].samples == 0 )
                {
                    // TODO:#2212........[Remove]
                    Logging.Info( $"[handleScanOrganicEvent] Samples is zero, setting additional data from variant" );
                    Thread.Sleep( 10 );
                    body.surfaceSignals.bio.list[ @event.genus ].SetData( @event.variant );
                }

                body.surfaceSignals.bio.list[ @event.genus ].Sample( @event.scanType,
                                                                     @event.variant,
                                                                     StatusService.Instance.CurrentStatus.latitude,
                                                                     StatusService.Instance.CurrentStatus.longitude );

                @event.bio = body.surfaceSignals.GetBio( @event.genus );

                // TODO:#2212........[Remove]
                Logging.Info( $"[handleScanOrganicEvent] SetBio ---------------------------------------------" );
                Thread.Sleep( 10 );
                Logging.Info( $"[handleScanOrganicEvent] SetBio:    Genus = '{@event.bio.genus.localizedName}'" );
                Thread.Sleep( 10 );
                Logging.Info( $"[handleScanOrganicEvent] SetBio:  Species = '{@event.bio.species.localizedName}'" );
                Thread.Sleep( 10 );
                Logging.Info( $"[handleScanOrganicEvent] SetBio:  Variant = '{@event.bio.variant.localizedName}'" );
                Thread.Sleep( 10 );
                Logging.Info( $"[handleScanOrganicEvent] SetBio:    Genus = '{@event.bio.genus.localizedName}'" );
                Thread.Sleep( 10 );
                Logging.Info( $"[handleScanOrganicEvent] SetBio: Distance = '{@event.bio.genus.distance}'" );
                Thread.Sleep( 10 );
                Logging.Info( $"[handleScanOrganicEvent] SetBio ---------------------------------------------" );
                Thread.Sleep( 10 );

                // These are updated when the above Sample() function is called, se we send them back to the event
                // Otherwise we would probably have to enqueue a new event (maybe not a bad idea?)
                @event.numTotal = body.surfaceSignals.bio.numTotal;
                @event.numComplete = body.surfaceSignals.bio.numComplete;
                @event.numRemaining = body.surfaceSignals.bio.numRemaining;
                @event.listRemaining = body.surfaceSignals.bio.listRemaining;

                // 2212: Save/Update Body data
                EDDI.Instance?.CurrentStarSystem.AddOrUpdateBody( body );
                StarSystemSqLiteRepository.Instance.SaveStarSystem( _currentSystem );
            }
        }

        private void handleBodyScannedEvent ( BodyScannedEvent @event )
        {
            // Transfer biologicals from FSS to body.
            if ( _fss_Signals != null )
            {
                if ( _fss_Signals.ContainsKey( Tuple.Create<ulong, long>( (ulong)@event.systemAddress, (long)@event.bodyId ) ) )
                {
                    FSS_Signals signal = _fss_Signals[ Tuple.Create<ulong, long>( (ulong)@event.systemAddress, (long)@event.bodyId ) ];

                    // Double check if system/body matches
                    if ( signal.systemAddress == @event.systemAddress && signal.bodyId == @event.bodyId )
                    {
                        
                        _currentBodyId = (long)@event.bodyId;
                        if ( CheckSafe( _currentBodyId ) )
                        {
                            Body body = _currentBody(_currentBodyId);

                            // Always update the reported totals
                            body.surfaceSignals.bio.reportedTotal = signal.bioCount;
                            body.surfaceSignals.geo.reportedTotal = signal.geoCount;

                            // TODO:#2212........[Remove]
                            Logging.Info( $"[handleBodyScannedEvent]\r\n" +
                                          $"\tBio Count is {signal.bioCount} ({body.surfaceSignals.bio.reportedTotal})\r\n" +
                                          $"\tGeo Count is {signal.geoCount} ({body.surfaceSignals.geo.reportedTotal})" );

                            if ( signal.status == false )
                            {
                                if ( signal.bioCount > 0 )
                                {
                                    //List<string> bios = PredictBios( body );
                                    List<string> bios = PredictBySpecies( body );
                                    body.surfaceSignals.bio.list.Clear();

                                    // TODO:#2212........[Remove]
                                    string log = "[handleBodyScannedEvent]:";
                                    foreach ( string genus in bios )
                                    {
                                        log = log + $"\r\n\tAddBio {genus}";
                                        body.surfaceSignals.AddBio( genus );
                                        // TODO:#2212........[Remove]
                                        //Logging.Info( $"[handleBodyScannedEvent] AddBio {genus}" );
                                        //Thread.Sleep( 10 );
                                    }
                                    Logging.Info( log );
                                    Thread.Sleep( 10 );

                                    // This is used by SAASignalsFound to know if we can safely clear the list to create the actual bio list
                                    body.surfaceSignals.predicted = true;
                                    _fss_Signals[ Tuple.Create<ulong, long>( (ulong)@event.systemAddress, (long)@event.bodyId ) ].status = true;
                                    List<string> bioList = body.surfaceSignals.GetBios();

                                    // TODO:#2212........[Remove]
                                    log = "[handleBodyScannedEvent]:";
                                    foreach ( string genus in bioList )
                                    {
                                        log = log + $"\r\n\tGetBios {genus}";
                                    }
                                    Logging.Info( log );
                                    Thread.Sleep( 10 );

                                    // TODO:#2212........[Do not enqueue if from @event.fromLoad?]
                                    // This doesn't have to be used but is provided just in case
                                    EDDI.Instance.enqueueEvent( new OrganicPredictionEvent( DateTime.UtcNow, body, body.surfaceSignals.GetBios() ) );
                                }
                            }

                            // 2212: Save/Update Body data
                            EDDI.Instance?.CurrentStarSystem.AddOrUpdateBody( body );
                            StarSystemSqLiteRepository.Instance.SaveStarSystem( _currentSystem );
                        }
                    }
                }
            }
        }

        /// <summary>
        /// This currently works but gives incorrect predictions
        /// Prediction data needs adjustment to use this
        /// </summary>
        public List<string> PredictByVariants ( Body body )
        {
            String log = "";
            bool enableLog = true;

            // Create a list to store predicted variants
            List<string> listPredicted = new List<string>();

            // Iterate though species
            foreach ( string variant in OrganicVariant.VARIANTS.Keys )
            {
                // TODO:#2212........[Remove]
                log += $"[Predictions] CHECKING VARIANT {variant}: ";

                // Get conditions for current variant
                OrganicVariant check = OrganicVariant.LookupByVariant( variant );
                if ( check != null )
                {

                    // Check if body meets max gravity requirements
                    // maxG: Maximum gravity
                    if ( data.maxG != null )
                    {
                        if ( check.maxG != 0 && check.minG != 0 )
                        {
                            if ( body.gravity < check.minG )
                            {
                            // TODO:#2212........[Remove]
                                if ( enableLog ) { log += $"\tPURGE (gravity: {body.gravity} < {check.minG})\r\n"; }
                                goto Skip_To_Purge;
                        }
                            else if ( body.gravity > check.maxG )
                            {
                                // TODO:#2212........[Remove]
                                if ( enableLog ) { log += $"\tPURGE (gravity: {body.gravity} > {check.maxG})\r\n"; }
                                goto Skip_To_Purge;
                    }
                        }
                    }

                    // Check if body meets temperature (K) requirements
                    //  - data.kRange: 'None'=No K requirements; 'Min'=K must be greater than minK; 'Max'=K must be less than maxK; 'MinMax'=K must be between minK and maxK
                    //  - data.minK: Minimum temperature
                    //  - data.maxK: Maximum temperature
                    if ( data.kRange != "" && data.kRange != "None" )
                    {
                        if ( check.maxK != 0 && check.minK != 0 )
                        {
                            if ( body.temperature < check.minK )
                            {
                                purge.Add( species );
                                // TODO:#2212........[Remove]
                                if ( enableLog ) { log += $"\tPURGE (temperature: {body.temperature} < {check.minK})\r\n"; }
                                goto Skip_To_Purge;
                            }
                            else if ( body.temperature > check.maxK )
                            {
                                // TODO:#2212........[Remove]
                                if ( enableLog ) { log += $"\tPURGE (temperature: {body.temperature} > {check.maxK})\r\n"; }
                                goto Skip_To_Purge;
                        }
                        }
                    }

                    // Check if body has appropriate class
                        {
                        bool found = false;
                        //if ( enableLog ) { log += $"\tplanetClass.Count = {check.planetClass.Count}\r\n"; }
                        if ( check.planetClass.Count > 0 )
                            {
                            foreach ( string planetClass in check.planetClass )
                            {
                                if ( planetClass == body.planetClass.edname )
                                {
                                    found = true;
                                    break;  // If found then we don't care about the rest
                                }
                            }

                            if ( !found )
                            {
                                // TODO:#2212........[Remove]
                                if ( enableLog ) { log += $"\tPURGE (planet class: {body.planetClass.edname} != [{string.Join( ",", check.planetClass )}])\r\n"; }
                                goto Skip_To_Purge;
                            }
                        }
                    }

                    // Check if body has appropriate astmosphere
                        {
                        bool found = false;
                        //if ( enableLog ) { log += $"\tatmosphereClass.Count = {check.atmosphereClass.Count}\r\n"; }
                        if ( check.atmosphereClass.Count > 0 )
                            {
                            foreach ( string atmosphereClass in check.atmosphereClass )
                            {
                                if ( atmosphereClass == body.atmosphereclass.edname )
                                {
                                    found = true;
                                    break;  // If found then we don't care about the rest
                                }
                            }

                            if ( !found )
                            {
                                // TODO:#2212........[Remove]
                                if ( enableLog ) { log += $"\tPURGE (atmosphere class: {body.atmosphereclass.edname} != [{string.Join( ",", check.atmosphereClass )}])\r\n"; }
                                goto Skip_To_Purge;
                            }
                        }
                    }

                    // Check if body has appropriate volcanism
                    {
                        bool found = false;
                       // if ( enableLog ) { log += $"\tvolcanism.Count = {check.volcanism.Count}\r\n"; }
                        if ( check.volcanism.Count > 0 )
                        {
                            foreach ( string volcanism in check.volcanism )
                            {
                                string amount = null;
                                string composition = "";
                                string type = "";

                                string[] parts = volcanism.Split(',');
                                if ( parts.Length > 0 )
                                {
                                    if ( parts.Length == 2 )
                                    {
                                        // amount 'null' is normal
                                        composition = parts[0];
                                        type = parts[1];
                                    }
                                    else if ( parts.Length == 3 )
                                    {
                                        amount = parts[0];
                                        composition = parts[1];
                                        type = parts[2];
                                    }
                                }

                                // Check if amount, composition and type matc hthe current body
                                if ( amount == body.volcanism.invariantAmount && composition == body.volcanism.invariantComposition && type == body.volcanism.invariantType )
                                {
                                    found = true;
                                    break;  // If found then we don't care about the rest
                                }
                            }

                            if ( !found )
                            {
                                // TODO:#2212........[Remove]
                                if ( enableLog ) { log += $"\tPURGE (volcanism: {body.volcanism.invariantAmount} {body.volcanism.invariantComposition} {body.volcanism.invariantType} != [{string.Join(",", check.volcanism )}])\r\n"; }
                                goto Skip_To_Purge;
                            }
                        }
                    }

                    // Check if body has appropriate parent star
                    {
                    bool found = false;
                        string foundClass = "";
                        //if ( enableLog ) { log += $"\tstarClass.Count = {check.starClass.Count}\r\n"; }
                        if ( check.starClass.Count > 0 )
                    {
                        // TODO:#2212........[Remove]
                        //Logging.Info( $"[Predictions] Parent Star Required = '{data.parentStar}'" );
                        //Thread.Sleep( 10 );

                        bool foundParent = false;
                        foreach ( IDictionary<string, object> parent in body.parents )
                        {
                            foreach ( string key in parent.Keys )
                            {
                                if ( key == "Star" )
                                {
                                    foundParent = true;
                                    long starId = (long)parent[ key ];

                                    Body starBody = _currentSystem.BodyWithID( starId );
                                    string starClass = starBody.stellarclass;
                                        foundClass = starClass;

                                    // TODO:#2212........[Remove]
                                    //Logging.Info( $"[Predictions] Parent Star: '{starClass}'" );
                                    //Thread.Sleep( 10 );

                                        //string[] starParts = data.parentStar.Split(',');
                                        //foreach ( string part in starParts )
                                        //{
                                        //    if ( part == starClass )
                                        //    {
                                        //        // TODO:#2212........[Remove]
                                        //        //Logging.Info( $"[Predictions] Found Star Match: '{part}' == '{starClass}'" );
                                        //        //Thread.Sleep( 10 );
                                        //        found = true;
                                        //        //break;
                                        //        goto ExitParentStarLoop;
                                        //    }
                                        //}

                                        foreach ( string checkClass in check.starClass )
                                    {
                                            if ( checkClass == starClass )
                                        {
                                            // TODO:#2212........[Remove]
                                            //Logging.Info( $"[Predictions] Found Star Match: '{part}' == '{starClass}'" );
                                            //Thread.Sleep( 10 );
                                            found = true;
                                            //break;
                                            goto ExitParentStarLoop;
                                        }
                                    }

                                }
                                else if ( key == "Null" )
                                {
                                    long baryId = (long)parent[ key ];
                                    List<long> barys = _currentSystem.baryCentre.GetBaryCentres( baryId );

                                    foreach ( long bodyId in barys )
                                    {
                                        // TODO:#2212........[Remove]
                                        //Logging.Info( $"[Predictions] BaryCentre: '{bodyId}' -> '{_currentSystem.BodyWithID( bodyId ).bodyType.edname}'" );
                                        //Thread.Sleep( 10 );
                                        if ( _currentSystem.BodyWithID( bodyId ).bodyType.edname == "Star" )
                                        {
                                            long starId = bodyId;

                                            Body starBody = _currentSystem.BodyWithID( starId );
                                            string starClass = starBody.stellarclass;
                                                foundClass = starClass;

                                            // TODO:#2212........[Remove]
                                            //Logging.Info( $"[Predictions] BaryCentre Parent Star: '{starClass}'" );
                                            //Thread.Sleep( 10 );

                                                //string[] starParts = data.parentStar.Split(',');
                                                //foreach ( string part in starParts )
                                                //{
                                                //    if ( part == starClass )
                                                //    {
                                                //        // TODO:#2212........[Remove]
                                                //        //Logging.Info( $"[Predictions] Found Star Match: '{part}' == '{starClass}'" );
                                                //        //Thread.Sleep( 10 );
                                                //        found = true;
                                                //        break;
                                                //    }
                                                //}

                                                foreach ( string checkClass in check.starClass )
                                            {
                                                    if ( checkClass == starClass )
                                                {
                                                    // TODO:#2212........[Remove]
                                                    //Logging.Info( $"[Predictions] Found Star Match: '{part}' == '{starClass}'" );
                                                    //Thread.Sleep( 10 );
                                                    found = true;
                                                        goto ExitParentStarLoop;
                                                }
                                            }
                                        }

                                        if ( found )
                                        {
                                            goto ExitParentStarLoop;
                                        }
                                    }
                                }
                                if ( foundParent )
                                {
                                    goto ExitParentStarLoop;
                                }
                            }
                        }

                        ExitParentStarLoop:
                        ;

                        if ( !found )
                        {
                            purge.Add( species );
                            // TODO:#2212........[Remove]
                                if ( enableLog )
                                { log = log + $"\tPURGE (parent star: {foundClass} != {string.Join( ",", check.starClass )})\r\n"; }
                                goto Skip_To_Purge;
                            }
                        }
                    }

                    log += $"OK\r\n";
                    listPredicted.Add( variant );
                            goto Skip_To_End;
                        }

            Skip_To_Purge:
                ;

            Skip_To_End:
                ;

                Logging.Info( log );
                Thread.Sleep( 10 );
            }

            //// Remove species that don't meet conditions from temporary list
            ////log = "[Predictions] Purged Species:";
            //foreach ( string species in purge )
            //{
            //    //log = log + $"\r\n\t{species}";
            //    list.Remove( species );
            //}
            ////Logging.Info( log );
            ////Thread.Sleep( 10 );

            // Create a list of only the unique genus' found
            log = "[Predictions] Genus List:";
            List<string> genus = new List<string>();
            foreach ( string variant in listPredicted )
            {
                if ( !genus.Contains( OrganicVariant.LookupByVariant( variant ).genus ) )
                {
                    log += $"\r\n\t{OrganicVariant.LookupByVariant( variant ).genus}";
                    genus.Add( OrganicVariant.LookupByVariant( variant ).genus );
                }
            }
            Logging.Info( log );
            Thread.Sleep( 10 );

            return genus;
        }

        /// <summary>
        /// This currently works and provides fairly accurate predictions
        /// </summary>
        public List<string> PredictBySpecies ( Body body )
        {
            String log = "";
            bool enableLog = true;

            // Create temporary list of ALL species possible
            List<string> listPredicted = new List<string>();

            // Iterate though species
            foreach ( string species in OrganicSpecies.SPECIES.Keys )
            {
                // TODO:#2212........[Remove]
                if ( enableLog ) { log += $"[Predictions] CHECKING SPECIES {species}: "; }

                // Iterate through conditions
                // Get conditions for current variant
                OrganicSpecies check = OrganicSpecies.Lookup( species );
                if ( check != null )
                {
                    // Check if body meets max gravity requirements
                    {
                        // maxG: Maximum gravity
                        if ( check.maxG != null )
                        {
                            if ( body.gravity > check.maxG )
                            {
                                // TODO:#2212........[Remove]
                                log += $"PURGE (gravity: {body.gravity} > {check.maxG})\r\n";
                                goto Skip_To_Purge;
                            }
                        }
                    }

                    // Check if body meets temperature (K) requirements
                    {
                        //  - data.kRange: 'None'=No K requirements; 'Min'=K must be greater than minK; 'Max'=K must be less than maxK; 'MinMax'=K must be between minK and maxK
                        //  - data.minK: Minimum temperature
                        //  - data.maxK: Maximum temperature
                        if ( check.kRange != "" && check.kRange != "None" )
                        {
                            if ( check.kRange == "<k" )
                            {
                                if ( body.temperature <= check.minK )
                                {
                                    // TODO:#2212........[Remove]
                                    log += $"PURGE (temp: {body.temperature} <= {check.minK})\r\n";
                                    goto Skip_To_Purge;
                                }
                            }
                            else if ( check.kRange == "k>" )
                            {
                                if ( body.temperature >= check.maxK )
                                {
                                    // TODO:#2212........[Remove]
                                    log += $"PURGE (temp: {body.temperature} >= {check.maxK})\r\n";
                                    goto Skip_To_Purge;
                                }
                            }
                            else if ( check.kRange == "<k<" )
                            {
                                if ( body.temperature <= check.minK || body.temperature >= check.maxK )
                                {
                                    // TODO:#2212........[Remove]
                                    log += $"PURGE (temp: {body.temperature} < {check.minK} || {body.temperature} > {check.maxK})\r\n";
                                    goto Skip_To_Purge;
                                }
                            }
                        }
                    }

                    // Check if body has appropriate class
                    {
                        bool found = false;
                        //if ( enableLog ) { log += $"\tplanetClass.Count = {check.planetClass.Count}\r\n"; }
                        if ( check.planetClass.Count > 0 )
                    {
                            foreach ( string planetClass in check.planetClass )
                        {
                                if ( planetClass == body.planetClass.edname )
                            {
                                found = true;
                                    break;  // If found then we don't care about the rest
                            }
                        }

                        if ( !found )
                        {
                            purge.Add( species );
                            // TODO:#2212........[Remove]
                                if ( enableLog )
                                { log += $"\tPURGE (planet class: {body.planetClass.edname} != [{string.Join( ",", check.planetClass )}])\r\n"; }
                                goto Skip_To_Purge;
                            }
                        }
                    }

                    // Check if body has appropriate astmosphere
                    {
                        bool found = false;
                        //if ( enableLog ) { log += $"\tatmosphereClass.Count = {check.atmosphereClass.Count}\r\n"; }
                        if ( check.atmosphereClass.Count > 0 )
                    {
                            foreach ( string atmosphereClass in check.atmosphereClass )
                        {
                                if ( ( atmosphereClass == "Any" && body.atmosphereclass.edname != "None") ||
                                     ( atmosphereClass == body.atmosphereclass.edname ) )
                            {
                                found = true;
                                    break;  // If found then we don't care about the rest
                            }
                        }

                        if ( !found )
                        {
                            purge.Add( species );
                            // TODO:#2212........[Remove]
                                if ( enableLog )
                                { log += $"\tPURGE (atmosphere class: {body.atmosphereclass.edname} != [{string.Join( ",", check.atmosphereClass )}])\r\n"; }
                                goto Skip_To_Purge;
                            }
                        }
                    }

                    // Check if body has appropriate volcanism
                    {
                        bool found = false;
                        //if ( enableLog ) { log += $"\tvolcanism.Count = {check.volcanism.Count}\r\n"; }
                        if ( check.volcanism.Count > 0 )
                        {
                            foreach ( string composition in check.volcanism )
                            {
                                // Check if amount, composition and type matc hthe current body
                                //if ( amount == body.volcanism.invariantAmount && composition == body.volcanism.invariantComposition && type == body.volcanism.invariantType )
                                if ( body.volcanism != null )
                                {
                                    if ( composition == body.volcanism.invariantComposition )
                                    {
                                        found = true;
                                        break;  // If found then we don't care about the rest
                                    }
                                }
                            }

                            if ( !found )
                            {
                                // TODO:#2212........[Remove]
                                if ( enableLog )
                    {
                        if ( body.volcanism != null )
                        {
                                        log += $"\tPURGE (volcanism: {body.volcanism.invariantComposition} != [{string.Join( ",", check.volcanism )}])\r\n";
                                    }
                                    else
                                    {
                                        log += $"\tPURGE (volcanism: null != [{string.Join( ",", check.volcanism )}])\r\n";
                                    }
                                }
                                goto Skip_To_Purge;
                            }
                        }
                    }

                    // Check if body has appropriate parent star
                    {
                        bool found = false;
                        string foundClass = "";
                        //if ( enableLog ) { log += $"\tstarClass.Count = {check.starClass.Count}\r\n"; }
                        if ( check.starClass.Count > 0 )
                        {
                            // TODO:#2212........[Remove]
                            //Logging.Info( $"[Predictions] Parent Star Required = '{data.parentStar}'" );
                            //Thread.Sleep( 10 );

                            bool foundParent = false;
                            foreach ( IDictionary<string, object> parent in body.parents )
                            {
                                foreach ( string key in parent.Keys )
                                {
                                    if ( key == "Star" )
                                    {
                                        foundParent = true;
                                        long starId = (long)parent[ key ];

                                        Body starBody = _currentSystem.BodyWithID( starId );
                                        string starClass = starBody.stellarclass;
                                        foundClass = starClass;

                                        // TODO:#2212........[Remove]
                                        //Logging.Info( $"[Predictions] Parent Star: '{starClass}'" );
                                        //Thread.Sleep( 10 );

                                        //string[] starParts = data.parentStar.Split(',');
                                        //foreach ( string part in starParts )
                                        //{
                                        //    if ( part == starClass )
                                        //    {
                                        //        // TODO:#2212........[Remove]
                                        //        //Logging.Info( $"[Predictions] Found Star Match: '{part}' == '{starClass}'" );
                                        //        //Thread.Sleep( 10 );
                                        //        found = true;
                                        //        //break;
                                        //        goto ExitParentStarLoop;
                                        //    }
                                        //}

                                        foreach ( string checkClass in check.starClass )
                                        {
                                            if ( checkClass == starClass )
                                            {
                                                // TODO:#2212........[Remove]
                                                //Logging.Info( $"[Predictions] Found Star Match: '{part}' == '{starClass}'" );
                                                //Thread.Sleep( 10 );
                                                found = true;
                                                goto ExitParentStarLoop;
                                            }
                                        }

                                    }
                                    else if ( key == "Null" )
                                    {
                                        long baryId = (long)parent[ key ];
                                        List<long> barys = _currentSystem.baryCentre.GetBaryCentres( baryId );

                                        foreach ( long bodyId in barys )
                                        {
                                            // TODO:#2212........[Remove]
                                            //Logging.Info( $"[Predictions] BaryCentre: '{bodyId}' -> '{_currentSystem.BodyWithID( bodyId ).bodyType.edname}'" );
                                            //Thread.Sleep( 10 );
                                            if ( _currentSystem.BodyWithID( bodyId ).bodyType.edname == "Star" )
                                            {
                                                long starId = bodyId;

                                                Body starBody = _currentSystem.BodyWithID( starId );
                                                string starClass = starBody.stellarclass;
                                                foundClass = starClass;

                                                // TODO:#2212........[Remove]
                                                //Logging.Info( $"[Predictions] BaryCentre Parent Star: '{starClass}'" );
                                                //Thread.Sleep( 10 );

                                                //string[] starParts = data.parentStar.Split(',');
                                                //foreach ( string part in starParts )
                                                //{
                                                //    if ( part == starClass )
                                                //    {
                                                //        // TODO:#2212........[Remove]
                                                //        //Logging.Info( $"[Predictions] Found Star Match: '{part}' == '{starClass}'" );
                                                //        //Thread.Sleep( 10 );
                                                //        found = true;
                                                //        break;
                                                //    }
                                                //}

                                                foreach ( string checkClass in check.starClass )
                            {
                                                    if ( checkClass == starClass )
                                {
                                                        // TODO:#2212........[Remove]
                                                        //Logging.Info( $"[Predictions] Found Star Match: '{part}' == '{starClass}'" );
                                                        //Thread.Sleep( 10 );
                                    found = true;
                                                        goto ExitParentStarLoop;
                                                    }
                                                }
                                            }

                                            if ( found )
                                            {
                                                goto ExitParentStarLoop;
                                }
                            }
                        }
                                    if ( foundParent )
                        {
                                        goto ExitParentStarLoop;
                                    }
                                }
                        }

                        ExitParentStarLoop:
                            ;

                        if ( !found )
                        {
                            purge.Add( species );
                            // TODO:#2212........[Remove]
                                if ( enableLog )
                                { log = log + $"\tPURGE (parent star: {foundClass} != {string.Join( ",", check.starClass )})\r\n"; }
                                goto Skip_To_Purge;
                            }
                        }
                    }
                }

                log += $"OK\r\n";
                listPredicted.Add( species );
                goto Skip_To_End;

            Skip_To_Purge:
                ;

                Skip_To_End:
                ;
            }

                Logging.Info( "\r\n"+log );
                Thread.Sleep( 10 );
            }

            // Create a list of only the unique genus' found
            log = "[Predictions] Genus List:";
            List<string> genus = new List<string>();
            foreach ( string species in listPredicted )
            {
                if ( !genus.Contains( OrganicSpecies.Lookup( species ).genus ) )
                {
                    log += $"\r\n\t{OrganicSpecies.Lookup( species ).genus}";
                    genus.Add( OrganicSpecies.Lookup( species ).genus );
                }
            }
            Logging.Info( log );
            Thread.Sleep( 10 );

            return genus;
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

        public void PostHandle(Event @event)
        {
        }

        public void HandleProfile(JObject profile)
        {
        }

        //public IDictionary<string, Tuple<Type, object>> GetVariables()
        //{
        //    lock ( StatusService.Instance.statusLock )
        //    {
        //        return new Dictionary<string, Tuple<Type, object>>
        //        {
        //            { "status", new Tuple<Type, object>(typeof(Status), StatusService.Instance.CurrentStatus ) },
        //            { "lastStatus", new Tuple < Type, object >(typeof(Status), StatusService.Instance.LastStatus) }
        //        };
        //    }
        //}

        public IDictionary<string, Tuple<Type, object>> GetVariables ()
        {
            return null;
        }
    }
}
