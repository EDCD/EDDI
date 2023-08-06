using EddiCore;
using EddiDataDefinitions;
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
        //--------------
        //    Notes
        //--------------
        // StarSystem.AddOrUpdateBodies()
        // StarSystem.PreserveBodyData
        // StarSystemSqLiteRepository.Instance.SaveStarSystem(currentSystem);
        // EDDI.Instance.enqueueEvent( new ShipFsdEvent( DateTime.UtcNow, "charging complete" ) { fromLoad = @event.fromLoad } );
        // EddiStatusService.StatusService.Instance.CurrentStatus.latitude
        // EddiStatusService.StatusService.Instance.CurrentStatus.longitude

        private string currentGenus;
        private long currentBodyId;
        private StarSystem currentSystem => EDDI.Instance?.CurrentStarSystem;
        private Body currentBody => currentSystem?.BodyWithID( currentBodyId );
        private IDictionary<string,Exobiology> currentBios => currentBody.surfaceSignals.bioList;

        [PublicAPI("The current biological")]
        public Exobiology currentBio => currentBody.surfaceSignals.bioList[ currentGenus ];

        public DiscoveryMonitor ()
        {
            StatusService.StatusUpdatedEvent += HandleStatus;
            //Logging.Info($"Initialized {MonitorName()}");
        }

        public string MonitorName()
        {
            return "Discovery monitor";
        }

        public string LocalizedMonitorName()
        {
            return "Discovery monitor";
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
            // TODO:#2212........[Update biological scan distances and enqueue event]
            //EDDI.Instance.enqueueEvent( new ScanOrganicDistanceEvent( DateTime.UtcNow, "charging complete" ) { fromLoad = @event.fromLoad } );
            if ( CheckSafe() )
            {
                if ( currentBio != null ) {
                    int samples = currentBio.samples;
                    if ( samples > 0 && samples < 3)
                    {
                        if ( status.latitude != null && status.longitude != null )
                        {
                            //currentBio.
                            //System.Windows.Forms.MessageBox.Show( $"UpdateScanDistance:\n\tlatitude={status.latitude}\n\tlongitude={status.longitude}" );

                            //Exobiology.Coordinates coords0 = new Exobiology.Coordinates();
                            //coords0.latitude = status.latitude;
                            //coords0.longitude = status.longitude;

                            // Is the current location status not equal to the last status (0=no change), and if the distance is less than (1) or greater than (2) the sample range.
                            int status1 = 0;
                            int status2 = 0;

                            Exobiology.Coordinates coords1;
                            Exobiology.Coordinates coords2;

                            if ( samples >= 1 )
                            {
                                coords1 = currentBio.coords[ 0 ];
                                coords1.lastStatus = coords1.status;
                                decimal? distance1 = Utilities.Functions.SurfaceDistanceKm(currentBody.radius*1000, status.latitude, status.longitude, coords1.latitude, coords1.longitude);

                                if ( distance1 != null )
                                {
                                    // convert Km to m
                                    //decimal distance0 = (decimal)distance1;
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


                                    if ( distance1 <= currentBio.genus.distance )
                                    {
                                        // Was previously outside sample range, alert that we have violated the radius
                                        if ( coords1.lastStatus == Exobiology.Status.OutsideSampleRange )
                                        {
                                            status1 = 1;
                                            coords1.status = Exobiology.Status.InsideSampleRange;
                                        }
                                    }
                                    else if ( distance1 > currentBio.genus.distance )
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
                                coords2 = currentBio.coords[ 1 ];
                                coords2.lastStatus = coords2.status;
                                decimal? distance2 = Utilities.Functions.SurfaceDistanceKm(currentBody.radius*1000, status.latitude, status.longitude, coords2.latitude, coords2.longitude);

                                //new Thread( () => System.Windows.MessageBox.Show( $"Distance Update, Samples >=2.\n\nDistance2 = {distance2}" ) ).Start();

                                if ( distance2 != null )
                                {
                                    // convert Km to m
                                    distance2 *= (decimal)1000.0;
                                    if ( distance2 <= currentBio.genus.distance )
                                    {
                                        // Was previously outside sample range, alert that we have violated the radius
                                        if ( coords2.lastStatus == Exobiology.Status.OutsideSampleRange )
                                        {
                                            status2 = 1;
                                            coords2.status = Exobiology.Status.InsideSampleRange;
                                        }
                                    }
                                    else if ( distance2 > currentBio.genus.distance )
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
                                    EDDI.Instance.enqueueEvent( new ScanOrganicDistanceEvent( DateTime.UtcNow, currentBio.genus.distance, status1, status2 ) );
                                }
                                catch
                                {
                                    new Thread( () => System.Windows.MessageBox.Show( "Failed to Enqueue 'ScanOrganicDistanceEvent'" ) ).Start();
                                }
                            }
                            
                        }
                    }
                }
            }

        }

        /// <summary>
        /// https://stackoverflow.com/questions/639695/how-to-convert-latitude-or-longitude-to-meters
        /// </summary>
        //private long CalculatePlanetaryDistance ( double radius, decimal? lat1, decimal? lon1, decimal? lat2, decimal? lon2 )
        //{
        //    // TODO:#2212........[Replace call with Utilities.Functions.SurfaceDistanceKm]
        //    //radius = 6378.137; // Radius of earth in KM

        //    double dLat = (double)lat2 * Math.PI / 180 - (double)lat1 * Math.PI / 180;
        //    double dLon = (double)lon2 * Math.PI / 180 - (double)lon1 * Math.PI / 180;

        //    double a = Math.Sin(dLat/2) * Math.Sin(dLat/2) +
        //               Math.Cos((double)lat1 * Math.PI / 180) * Math.Cos((double)lat2 * Math.PI / 180) *
        //               Math.Sin(dLon/2) * Math.Sin(dLon/2);

        //    double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1-a));
        //    double d = radius * c;
        //    return (long)(d * 1000); // meters
        //}

        public void PreHandle(Event @event)
        {
            // TODO:#2212........[handle fromLoad events]
            if ( !@event.fromLoad )
            {
                if ( @event is CodexEntryEvent )            { handleCodexEntryEvent( (CodexEntryEvent)@event ); }
                else if ( @event is SurfaceSignalsEvent )   { handleSurfaceSignalsEvent( (SurfaceSignalsEvent)@event ); }
                else if ( @event is ScanOrganicEvent )      { handleScanOrganicEvent( (ScanOrganicEvent)@event ); }
            }
        }

        private void handleCodexEntryEvent ( CodexEntryEvent @event )
        {            
            // Not sure if we have anything to do here with this yet
        }

        /// <summary>
        ///  When a planet is mapped add the biological data to the planet.
        /// </summary>
        private void handleSurfaceSignalsEvent ( SurfaceSignalsEvent @event )
        {
            //Logging.Info( $"\t{MonitorName()}: Handling Event: {@event.ToString()}\n" );

            currentBodyId = @event.bodyId;

            if ( CheckSafe( @event.bodyId) )
            {
                @event.body.surfaceSignals.Predict( @event.body );

                // TODO:#2212........[Save/Update Body data?]
                //currentSystem.PreserveBodyData
                //currentSystem.AddOrUpdateBody
            }
        }

        private void handleScanOrganicEvent ( ScanOrganicEvent @event )
        {
            // TODO:#2212........[Handle fromLoad events, they may already exist in database but lets make sure]

            currentBodyId = @event.bodyId;

            if ( CheckSafe() )
            {
                currentGenus = @event.genus;

                // TESTING
                @event.currentSystem = currentSystem.systemname;
                @event.currentBody = currentBody.shortname;

                // If the biological doesn't exist, lets add it now
                if ( !currentBody.surfaceSignals.bioList.ContainsKey( @event.genus ) )
                {
                    currentBody.surfaceSignals.AddBio( @event.genus );
                }

                // If only the genus is present, then finish other data (and prune predictions)
                if ( currentBio.samples == 0 )
                {
                    // TODO:#2212........[Prune Predictions]

                    currentBody.surfaceSignals.bioList[ @event.genus ].SetData( @event.variant );
                }

                //try
                //{
                //new Thread( () => System.Windows.Forms.MessageBox.Show( $"Sample:\n\tscanType = {@event.scanType}\n" +
                //                                        $"\tvariant = {@event.variant}\n" +
                //                                        $"\tlatitude = {StatusService.Instance.CurrentStatus.latitude}\n" +
                //                                        $"\tlongitude = {StatusService.Instance.CurrentStatus.longitude}" ) ).Start();
                
                // TODO:#2212........[Edge case where lat/lon don't exist yet just after starting EDDI? Needs more testing to be sure.]

                currentBody.surfaceSignals.bioList[ @event.genus ].Sample( @event.scanType,
                                                                               @event.variant,
                                                                               StatusService.Instance.CurrentStatus.latitude,
                                                                               StatusService.Instance.CurrentStatus.longitude );

                //}
                //catch {
                //    System.Windows.Forms.MessageBox.Show( $"Failed to update sample:\n\tscanType = {@event.scanType}\n" +
                //                                            $"\tvariant = {@event.variant}\n" +
                //                                            $"\tlatitude = {EddiStatusService.StatusService.Instance.CurrentStatus.latitude}\n" +
                //                                            $"\tlongitude = {EddiStatusService.StatusService.Instance.CurrentStatus.longitude}" );
                //}

                // TODO:#2212........[Return bio status to Event]
                @event.bio = currentBio;
            }
        }

        /// <summary>
        /// Check if the current system and body exist
        /// </summary>
        private bool CheckSafe ()
        {
            if ( currentSystem != null )
            {
                if ( currentBody != null )
                {
                    return true;
                }
            }

            return false;
        }

        private bool CheckSafe ( long bodyId )
        {
            if ( EDDI.Instance?.CurrentStarSystem != null )
            {
                if ( EDDI.Instance?.CurrentStarSystem.BodyWithID( bodyId ) != null )
                {
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
