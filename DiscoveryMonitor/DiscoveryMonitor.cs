using EddiCore;
using EddiDataDefinitions;
using EddiDataProviderService;
using EddiEvents;
using EddiStatusService;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Windows.Controls;
using Utilities;
using System.Threading;

namespace EddiDiscoveryMonitor
{
    public class DiscoveryMonitor : IEddiMonitor
    {

        private class FSSBioSignals
        {
            public long systemAddress;
            public long bodyId;
            public int signalCount;     // The number of biological signals detected
            public bool status;         // Has this body had its bios predicted yet
        }

        // Dictionary of FSSBodySignals events
        //  - The Tuple is the SystemAddress and BodyId.
        //  - The bool value 
        //private List<Tuple<long, long>> FSS_Status;
        private Dictionary<Tuple<long, long>, FSSBioSignals> _fss_BioSignals;

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

                                    //new Thread( () => System.Windows.MessageBox.Show( $"Distance Update, Samples >=1.\n\n" +
                                    //                                                  $"\tCurrent Latitude = {status.latitude}\n" +
                                    //                                                  $"\tCurrent Longitude = {status.longitude}\n\n" +
                                    //                                                  $"\tSample Latitude = {coords1.latitude}\n" +
                                    //                                                  $"\tSample Longitude = {coords1.longitude}\n\n" +
                                    //                                                  $"\tDistance0 = {distance0}\n" +
                                    //                                                  $"\tDistance1 = {distance1}\n\n" +
                                    //                                                  $"\tCurrent Radius = {currentBody.radius}\n" +
                                    //                                                  $"\tSample Distance = {currentBio.genus.distance}" ) ).Start();


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
                                catch ( System.Exception e )
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
                // EDDI.Instance.enqueueEvent( new OrganicPredictionEvent( DateTime.UtcNow, body.surfaceSignals.GetBios() ) );

                foreach ( SignalAmount sig in @event.surfacesignals )
                {
                    if ( sig.signalSource.edname == "SAA_SignalType_Biological" )
                    {
                        // TODO:#2212........[Temporarily store bio numbers, wait for Scan event before predicting]
                        //_fss_BioSignals
                    }
                }
            }
            else if ( @event.detectionType == "SAA" )
            {
                // TODO:#2212........[Do we need to do anything here? Double check before removing this comment.]

                //currentBodyId = @event.bodyId;

                //////System.Diagnostics.Debug.WriteLine( $" - Surface Signals Event: {@event.bodyname},'{@event.detectionType}'" );

                //if ( CheckSafe( @event.bodyId ) )
                //{
                //    Body body = currentBody(currentBodyId);

                //    Logging.Info( $"[handleSurfaceSignalsEvent] numTotal = {body.surfaceSignals.bio.numTotal}" );
                //    Thread.Sleep( 10 );
                //    Logging.Info( $"[handleSurfaceSignalsEvent] numComplete = {body.surfaceSignals.bio.numComplete}" );
                //    Thread.Sleep( 10 );
                //    Logging.Info( $"[handleSurfaceSignalsEvent] numRemaining = {body.surfaceSignals.bio.numRemaining}" );
                //    Thread.Sleep( 10 );



                //    //    //System.Diagnostics.Debug.WriteLine( $" - Safe" );
                //    if ( @event.detectionType == "FSS" )
                //    {
                //        if ( @event.biosignals != null )
                //        {
                //            foreach ( string genus in @event.biosignals )
                //            {
                //                //Logging.Info( $" - Adding Predicted Bio: {genus}" );
                //                body.surfaceSignals.AddBio( genus, true );
                //            }

                //            // 2212: Save/Update Body data
                //            EDDI.Instance?.CurrentStarSystem.AddOrUpdateBody( body );
                //            StarSystemSqLiteRepository.Instance.SaveStarSystem( currentSystem );
                //        }

                //        try
                //        {
                //            //            //EDDI.Instance.enqueueEvent( new OrganicPredictionEvent( DateTime.UtcNow, @event.body ) );
                //            EDDI.Instance.enqueueEvent( new OrganicPredictionEvent( DateTime.UtcNow, body.surfaceSignals.GetBios() ) );
                //            //            EDDI.Instance.enqueueEvent( new OrganicPredictionEvent( DateTime.UtcNow, list ) );
                //        }
                //        catch ( System.Exception e )
                //        {
                //            Logging.Error( $"Surface Signals Event: Failed to Enqueue 'OrganicPredictionEvent' [{e}]" );
                //        }
                //    }
                //}
            }
        }

        private void handleScanOrganicEvent ( ScanOrganicEvent @event )
        {
            _currentBodyId = @event.bodyId;
            _currentGenus = @event.genus;

            Logging.Debug( $"[handleScanOrganicEvent] --------------------------------------------" );
            Thread.Sleep( 10 );

            if ( CheckSafe() )
            {
                Logging.Debug( $"[handleScanOrganicEvent] CheckSafe OK" );
                Thread.Sleep( 10 );

                Body body = _currentBody(_currentBodyId);

                // TESTING
                //@event.currentSystem = currentSystem.systemname;
                //@event.currentBody = body.shortname;

                // If the biological doesn't exist, lets add it now
                if ( !body.surfaceSignals.bio.list.ContainsKey( @event.genus ) )
                {
                    Logging.Debug( $"[handleScanOrganicEvent] Genus doesn't exist in list, adding {@event.genus}" );
                    Thread.Sleep( 10 );
                    body.surfaceSignals.AddBio( @event.genus );
                }

                // If only the genus is present, then finish other data (and prune predictions)
                if ( body.surfaceSignals.bio.list[ @event.genus ].samples == 0 )
                {
                    // TODO:#2212........[Prune Predictions]
                    // Set prediction to false
                    // Check if number of bios is >= number of bios reported by journal

                    Logging.Debug( $"[handleScanOrganicEvent] Samples is zero, setting additional data from variant" );
                    Thread.Sleep( 10 );
                    body.surfaceSignals.bio.list[ @event.genus ].SetData( @event.variant );
                }

                // TODO:#2212........[Possible edge case where lat/lon don't exist yet just after starting EDDI? Needs more testing to be sure.]
                body.surfaceSignals.bio.list[ @event.genus ].Sample( @event.scanType,
                                                                     @event.variant,
                                                                     StatusService.Instance.CurrentStatus.latitude,
                                                                     StatusService.Instance.CurrentStatus.longitude );

                @event.bio = body.surfaceSignals.GetBio( @event.genus );

                Logging.Debug( $"[handleScanOrganicEvent] SetBio ---------------------------------------------" );
                Thread.Sleep( 10 );
                Logging.Debug( $"[handleScanOrganicEvent] SetBio:    Genus = '{@event.bio.genus.name}'" );
                Thread.Sleep( 10 );
                Logging.Debug( $"[handleScanOrganicEvent] SetBio:  Species = '{@event.bio.species.name}'" );
                Thread.Sleep( 10 );
                Logging.Debug( $"[handleScanOrganicEvent] SetBio:  Variant = '{@event.bio.variant}'" );
                Thread.Sleep( 10 );
                Logging.Debug( $"[handleScanOrganicEvent] SetBio:    Genus = '{@event.bio.genus.name}'" );
                Thread.Sleep( 10 );
                Logging.Debug( $"[handleScanOrganicEvent] SetBio: Distance = '{@event.bio.genus.distance}'" );
                Thread.Sleep( 10 );
                Logging.Debug( $"[handleScanOrganicEvent] SetBio ---------------------------------------------" );
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
