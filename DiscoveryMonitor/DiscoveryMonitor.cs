using EddiCore;
using EddiDataDefinitions;
using EddiEvents;
using EddiStatusService;
using MathNet.Numerics.RootFinding;
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
        //--------------
        //    Notes
        //--------------
        // StarSystem.AddOrUpdateBodies()
        // StarSystem.PreserveBodyData
        // StarSystemSqLiteRepository.Instance.SaveStarSystem(currentSystem);
        //
        //
        //

        private StarSystem currentSystem => EDDI.Instance?.CurrentStarSystem;

        private long currentBodyId;
        private Body body => currentSystem?.BodyWithID( currentBodyId );

        public Body currentBody => currentSystem?.BodyWithID( currentBodyId );

        public Exobiology currentBios => currentBody.bio;

        //OrganicItem currentOrganicData;

        public DiscoveryMonitor()
        {
            //StatusService.StatusUpdatedEvent += HandleStatus;
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
            //Logging.Info( $"{MonitorName()}: Started" );
        }

        public void Stop()
        {
        }

        public void Reload()
        {
        }

        public UserControl ConfigurationTabItem()
        {
            return null;
        }

        //private void HandleStatus ( object sender, EventArgs e )
        //{
        //    var currentStatus = StatusService.Instance.CurrentStatus;
        //    if ( currentStatus != null )
        //    {
        //        UpdateScanDistance(currentStatus);
        //    }
        //}

        /// <summary>
        /// Update the currently active bio scan distance (if any)
        /// </summary>
        private void UpdateScanDistance( Status status )
        {
            // TODO:#2212........[Update biological scan distances and enqueue event]
            //EDDI.Instance.enqueueEvent( new ScanOrganicDistanceEvent( DateTime.UtcNow, "charging complete" ) { fromLoad = @event.fromLoad } );
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
            Logging.Info( $"\t{MonitorName()}: Handling Event: {@event.ToString()}\n" );
            // TODO:#2212........[Get codex entry information]
        }

        /// <summary>
        ///  When a planet is mapped add the biological data to the planet.
        /// </summary>
        private void handleSurfaceSignalsEvent ( SurfaceSignalsEvent @event )
        {
            Logging.Info( $"\t{MonitorName()}: Handling Event: {@event.ToString()}\n" );

            // TODO:#2212........Make biological predictions

            if ( CheckSafe( @event.bodyId) )
            {
                //currentBody.bio.GetBio(
                //currentBody
                //currentSystem.PreserveBodyData
                //currentSystem.AddOrUpdateBody
            }
        }

        private void handleScanOrganicEvent ( ScanOrganicEvent @event )
        {
            // TODO:#2212........[Update organic data and scan status]
            // TODO:#2212........[Handle fromLoad events, they may already exist in database but lets make sure]
            //Logging.Info( $"\t{MonitorName()}: Handling Event: {@event.ToString()}\n" );

            //StarSystem currentSystemTest = EDDI.Instance?.CurrentStarSystem;
            //Logging.Info( $"\t\t\n" );

            //@event.currentSystem = "test system";
            //@event.currentBody = "test body";

            //if ( CheckSafe( @event.bodyId ) )
            //{
            //    Logging.Info( $"\t\tCheckSafe is True\n" );
            //    if ( EddiStatusService.StatusService.Instance.CurrentStatus.latitude != null && EddiStatusService.StatusService.Instance.CurrentStatus.longitude != null )
            //    {
            //        Logging.Info( $"\t\tLatitude and Longitude are Not Null\n" );
            //        body( @event.bodyId ).bio.Scan( @event.localisedGenus,
            //                                        @event.localisedSpecies,
            //                                        @event.localisedVariant,
            //                                        EddiStatusService.StatusService.Instance.CurrentStatus.latitude,
            //                                        EddiStatusService.StatusService.Instance.CurrentStatus.longitude );

            //        currentOrganicData = OrganicInfo.GetData( @event.localisedGenus, @event.localisedSpecies );

            //        @event.data_new = currentOrganicData;
            //    }

            //}

            //EDDI.Instance.enqueueEvent( new ShipFsdEvent( DateTime.UtcNow, "charging complete" ) { fromLoad = @event.fromLoad } );
        }

        /// <summary>
        /// Check if the current system and body exist
        /// </summary>
        private bool CheckSafe (long bodyId)
        {
            if ( currentSystem != null )
            {
                currentBodyId = bodyId;
                if ( currentBody != null )
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
