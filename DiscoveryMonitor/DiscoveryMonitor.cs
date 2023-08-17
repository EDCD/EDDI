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
        private class FSSBioSignals
        {
            public ulong systemAddress; // For reference to double check
            public long bodyId;         // For reference to double check
            public int signalCount;     // The number of biological signals detected (set bio.numTotal to this)
            public bool status;         // Has this body had its bios predicted yet (false = FSSBodySignals event has occured but not Scan event)
        }

        // Dictionary of FSSBodySignals events
        //  - The Tuple is the SystemAddress and BodyId.
        //  - The bool value 
        //private List<Tuple<long, long>> FSS_Status;
        private Dictionary<Tuple<ulong, long>, FSSBioSignals> _fss_BioSignals;

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

            _fss_BioSignals = new Dictionary<Tuple<ulong, long>, FSSBioSignals>();
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
                foreach ( SignalAmount sig in @event.surfacesignals )
                {
                    if ( sig.signalSource.edname == "SAA_SignalType_Biological" )
                    {
                        FSSBioSignals signals = new FSSBioSignals();

                        signals.systemAddress = (ulong)@event.systemAddress;
                        signals.bodyId = @event.bodyId;
                        signals.signalCount = sig.amount;
                        signals.status = false;

                        _fss_BioSignals.Add( new Tuple<ulong, long>( (ulong)@event.systemAddress, @event.bodyId ), signals );
                    }
                }
            }
            else if ( @event.detectionType == "SAA" )
            {
                // TODO:#2212........[Do we need to do anything here? Double check before removing this comment.]
            }
        }

        private void handleScanOrganicEvent ( ScanOrganicEvent @event )
        {
            _currentBodyId = @event.bodyId;
            _currentGenus = @event.genus;

            // TODO:#2212........[Remove]
            //Logging.Info( $"[handleScanOrganicEvent] --------------------------------------------" );
            //Thread.Sleep( 10 );

            if ( CheckSafe() )
            {
                // TODO:#2212........[Remove]
                //Logging.Info( $"[handleScanOrganicEvent] CheckSafe OK" );
                //Thread.Sleep( 10 );

                Body body = _currentBody(_currentBodyId);

                // If the biological doesn't exist, lets add it now
                if ( !body.surfaceSignals.bio.list.ContainsKey( @event.genus ) )
                {
                    // TODO:#2212........[Remove]
                    //Logging.Info( $"[handleScanOrganicEvent] Genus doesn't exist in list, adding {@event.genus}" );
                    //Thread.Sleep( 10 );
                    body.surfaceSignals.AddBio( @event.genus );
                }

                // If only the genus is present, then finish other data (and prune predictions)
                if ( body.surfaceSignals.bio.list[ @event.genus ].samples == 0 )
                {
                    // TODO:#2212........[Remove]
                    //Logging.Info( $"[handleScanOrganicEvent] Samples is zero, setting additional data from variant" );
                    //Thread.Sleep( 10 );
                    body.surfaceSignals.bio.list[ @event.genus ].SetData( @event.variant );
                }

                body.surfaceSignals.bio.list[ @event.genus ].Sample( @event.scanType,
                                                                     @event.variant,
                                                                     StatusService.Instance.CurrentStatus.latitude,
                                                                     StatusService.Instance.CurrentStatus.longitude );

                @event.bio = body.surfaceSignals.GetBio( @event.genus );

                // TODO:#2212........[Remove]
                //Logging.Info( $"[handleScanOrganicEvent] SetBio ---------------------------------------------" );
                //Thread.Sleep( 10 );
                //Logging.Info( $"[handleScanOrganicEvent] SetBio:    Genus = '{@event.bio.genus.name}'" );
                //Thread.Sleep( 10 );
                //Logging.Info( $"[handleScanOrganicEvent] SetBio:  Species = '{@event.bio.species.name}'" );
                //Thread.Sleep( 10 );
                //Logging.Info( $"[handleScanOrganicEvent] SetBio:  Variant = '{@event.bio.variant}'" );
                //Thread.Sleep( 10 );
                //Logging.Info( $"[handleScanOrganicEvent] SetBio:    Genus = '{@event.bio.genus.name}'" );
                //Thread.Sleep( 10 );
                //Logging.Info( $"[handleScanOrganicEvent] SetBio: Distance = '{@event.bio.genus.distance}'" );
                //Thread.Sleep( 10 );
                //Logging.Info( $"[handleScanOrganicEvent] SetBio ---------------------------------------------" );
                //Thread.Sleep( 10 );

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
            // Do predictions

            if ( _fss_BioSignals != null )
            {
                if ( _fss_BioSignals.ContainsKey( Tuple.Create<ulong, long>( (ulong)@event.systemAddress, (long)@event.bodyId ) ) )
                {
                    FSSBioSignals signal = _fss_BioSignals[ Tuple.Create<ulong, long>( (ulong)@event.systemAddress, (long)@event.bodyId ) ];

                    // Double check if system/body matches
                    if ( signal.systemAddress == @event.systemAddress && signal.bodyId == @event.bodyId )
                    {
                        
                        _currentBodyId = (long)@event.bodyId;
                        if ( CheckSafe( _currentBodyId ) )
                        {
                            Body body = _currentBody(_currentBodyId);

                            // Always update the reported total biologicals
                            body.surfaceSignals.bio.reportedTotal = signal.signalCount;
                            // TODO:#2212........[Remove]
                            //Logging.Info( $"[handleBodyScannedEvent] Signal Count is {signal.signalCount}, ({body.surfaceSignals.bio.reportedTotal})" );

                            if ( signal.status == false )
                            {
                                if ( signal.signalCount > 0 )
                                {
                                    List<string> bios = PredictBios( body );
                                    body.surfaceSignals.bio.list.Clear();

                                    foreach ( string genus in bios )
                                    {
                                        body.surfaceSignals.AddBio( genus );
                                        // TODO:#2212........[Remove]
                                        //Logging.Info( $"[handleBodyScannedEvent] AddBio {genus}" );
                                        //Thread.Sleep( 10 );
                                    }

                                    // This is used by SAASignalsFound to know if we can safely clear the list to create the actual bio list
                                    body.surfaceSignals.predicted = true;
                                    _fss_BioSignals[ Tuple.Create<ulong, long>( (ulong)@event.systemAddress, (long)@event.bodyId ) ].status = true;
                                    List<string> bioList = body.surfaceSignals.GetBios();

                                    // TODO:#2212........[Remove]
                                    //foreach ( string genus in bioList )
                                    //{
                                    //    Logging.Info( $"[handleBodyScannedEvent] GetBios {genus}" );
                                    //    Thread.Sleep( 10 );
                                    //}

                                    // TODO:#2212........[Do not enqueue if from @event.fromLoad?]
                                    // This doesn't have to be used but is provided just in case
                                    EDDI.Instance.enqueueEvent( new OrganicPredictionEvent( DateTime.UtcNow, body.surfaceSignals.GetBios() ) );
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

        public List<string> PredictBios ( Body body )
        {
            // Create temporary list of ALL species possible
            List<string> list = new List<string>();
            foreach ( string species in OrganicInfo.speciesData.Keys )
            {
                list.Add( species );
            }

            // Create an empty list for species that do not meet conditions
            List<string> purge = new List<string>();

            // Iterate though species
            foreach ( string species in list )
            {
                // TODO:#2212........[Remove]
                //Logging.Info( $"[Predictions] CHECKING SPECIES ==========> {species} <==========" );
                //Thread.Sleep( 10 );

                // Iterate through conditions
                OrganicInfo.SpeciesData data = OrganicInfo.speciesData[species];
                {
                    // Check if body meets max gravity requirements
                    // maxG: Maximum gravity
                    if ( data.maxG != null )
                    {
                        if ( body.gravity > data.maxG )
                        {
                            // TODO:#2212........[Remove]
                            //Logging.Info( $"[Predictions] PURGE ----------> {species} <----------" );
                            //Thread.Sleep( 10 );
                            purge.Add( species );
                            goto Skip_To_End;
                        }
                    }

                    // Check if body meets temperature (K) requirements
                    //  - data.kRange: 'None'=No K requirements; 'Min'=K must be greater than minK; 'Max'=K must be less than maxK; 'MinMax'=K must be between minK and maxK
                    //  - data.minK: Minimum temperature
                    //  - data.maxK: Maximum temperature
                    if ( data.kRange != "" && data.kRange != "None" )
                    {
                        if ( data.kRange == "Min" )
                        {
                            if ( body.temperature <= data.minK )
                            {
                                purge.Add( species );
                                // TODO:#2212........[Remove]
                                //Logging.Info( $"[Predictions] PURGE ----------> {species} <----------" );
                                //Thread.Sleep( 10 );
                                goto Skip_To_End;
                            }
                        }
                        else if ( data.kRange == "Max" )
                        {
                            if ( body.temperature >= data.maxK )
                            {
                                purge.Add( species );
                                // TODO:#2212........[Remove]
                                //Logging.Info( $"[Predictions] PURGE ----------> {species} <----------" );
                                //Thread.Sleep( 10 );
                                goto Skip_To_End;
                            }
                        }
                        else if ( data.kRange == "MinMax" )
                        {
                            if ( body.temperature < data.minK || body.temperature > data.maxK )
                            {
                                purge.Add( species );
                                // TODO:#2212........[Remove]
                                //Logging.Info( $"[Predictions] PURGE ----------> {species} <----------" );
                                //Thread.Sleep( 10 );
                                goto Skip_To_End;
                            }
                        }
                    }

                    // Check if body has appropriate parent star
                    //data.parentStar;
                    bool found = false;
                    if ( data.parentStar != null && data.parentStar != "" && data.parentStar != "None" )
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

                                    // TODO:#2212........[Remove]
                                    //Logging.Info( $"[Predictions] Parent Star: '{starClass}'" );
                                    //Thread.Sleep( 10 );

                                    string[] starParts = data.parentStar.Split(',');
                                    foreach ( string part in starParts )
                                    {
                                        if ( part == starClass )
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

                                            // TODO:#2212........[Remove]
                                            //Logging.Info( $"[Predictions] BaryCentre Parent Star: '{starClass}'" );
                                            //Thread.Sleep( 10 );

                                            string[] starParts = data.parentStar.Split(',');
                                            foreach ( string part in starParts )
                                            {
                                                if ( part == starClass )
                                                {
                                                    // TODO:#2212........[Remove]
                                                    //Logging.Info( $"[Predictions] Found Star Match: '{part}' == '{starClass}'" );
                                                    //Thread.Sleep( 10 );
                                                    found = true;
                                                    break;
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
                            //Logging.Info( $"[Predictions] PURGE ----------> {species} <----------" );
                            //Thread.Sleep( 10 );
                            goto Skip_To_End;
                        }
                    }

                    // Check if body has appropriate class
                    //data.planetClass;
                    found = false;
                    if ( data.planetClass != "" )
                    {
                        string[] classParts = data.planetClass.Split(',');
                        foreach ( string part in classParts )
                        {
                            if ( part == body.planetClass.edname )
                            {
                                found = true;
                            }
                        }

                        if ( !found )
                        {
                            purge.Add( species );
                            // TODO:#2212........[Remove]
                            //Logging.Info( $"[Predictions] PURGE ----------> {species} <----------" );
                            //Thread.Sleep( 10 );
                            //break;
                            goto Skip_To_End;
                        }
                    }

                    // Check if body has appropriate astmosphere
                    //data.atmosphereClass;
                    found = false;
                    if ( data.atmosphereClass != "" )
                    {
                        string[] atmosParts = data.atmosphereClass.Split(',');
                        foreach ( string part in atmosParts )
                        {
                            if ( part == body.atmosphereclass.edname )
                            {
                                found = true;
                            }
                        }

                        if ( !found )
                        {
                            purge.Add( species );
                            // TODO:#2212........[Remove]
                            //Logging.Info( $"[Predictions] PURGE ----------> {species} <----------" );
                            //Thread.Sleep( 10 );
                            goto Skip_To_End;
                        }
                    }

                    // Check if body has appropriate volcanism
                    //data.volcanism;
                    found = false;
                    if ( data.volcanism != "" )
                    {
                        if ( body.volcanism != null )
                        {
                            string[] volcanismParts = data.volcanism.Split(',');
                            foreach ( string part in volcanismParts )
                            {
                                if ( part == body.volcanism.invariantComposition )
                                {
                                    found = true;
                                }
                            }
                        }
                        else
                        {
                            found = false;
                        }

                        if ( !found )
                        {
                            purge.Add( species );
                            // TODO:#2212........[Remove]
                            //Logging.Info( $"[Predictions] PURGE ----------> {species} <----------" );
                            //Thread.Sleep( 10 );
                            goto Skip_To_End;
                        }
                    }
                }

                Skip_To_End:
                ;
            }

            // Remove species that don't meet conditions from temporary list
            foreach ( string species in purge )
            {
                list.Remove( species );
            }

            // Create a list of only the unique genus' found
            List<string> genus = new List<string>();
            foreach ( string species in list )
            {
                if ( !genus.Contains( OrganicInfo.speciesData[ species ].genus ) )
                {
                    genus.Add( OrganicInfo.speciesData[ species ].genus );
                }
            }

            body.surfaceSignals.predicted = true;

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