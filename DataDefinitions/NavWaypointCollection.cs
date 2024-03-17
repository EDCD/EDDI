using JetBrains.Annotations;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Utilities;

namespace EddiDataDefinitions
{
    public sealed class NavWaypointCollection: INotifyPropertyChanged
    {
        private decimal _routeDistance;
        private bool _guidanceEnabled;
        private bool _fillVisitedGaps;
        private int? _routeFuelTotal;

        public decimal RouteDistance
        {
            get => _routeDistance;
            private set { _routeDistance = value; OnPropertyChanged();}
        }

        public bool GuidanceEnabled
        {
            get => _guidanceEnabled;
            set { _guidanceEnabled = value; OnPropertyChanged();}
        }

        /// <summary>
        /// False when plotting missions and destinations may be visited out of order
        /// </summary>
        public bool FillVisitedGaps
        {
            get => _fillVisitedGaps;
            set { _fillVisitedGaps = value; OnPropertyChanged();}
        }

        public int? RouteFuelTotal
        {
            get => _routeFuelTotal;
            set { _routeFuelTotal = value; OnPropertyChanged(); }
        }

        public ObservableCollection<NavWaypoint> Waypoints { get; } = new ObservableCollection<NavWaypoint>();

        public List<NavWaypoint> UnvisitedWaypoints => Waypoints.Where(wp => !wp.visited).ToList();

        public NavWaypoint NextWaypoint
        {
            get => nextWaypoint ?? ( currentX is null || currentY is null || currentZ is null
                ? null
                : FillVisitedGaps
                    ? UnvisitedWaypoints.OrderBy( wp =>
                        Functions.StellarDistanceLy( currentX, currentY, currentZ, wp.x, wp.y, wp.z ) ).FirstOrDefault()
                    : UnvisitedWaypoints.FirstOrDefault() );
            set => nextWaypoint = value;
        }
        private NavWaypoint nextWaypoint;

        public decimal? NextWaypointDistance => 
            Functions.StellarDistanceLy( currentX, currentY, currentZ, NextWaypoint?.x, NextWaypoint?.y, NextWaypoint?.z );

        private decimal? currentX;
        private decimal? currentY;
        private decimal? currentZ;

        [JsonConstructor]
        public NavWaypointCollection (IEnumerable<NavWaypoint> items = null, bool fillVisitedGaps = false)
        {
            if ( items != null )
            {
                foreach ( var item in items )
                {
                    Waypoints.Add( item );
                }
                CalculateFuelUsed();
                CalculateRouteDistances();
            }

            FillVisitedGaps = fillVisitedGaps;
            Waypoints.CollectionChanged += NavWaypointList_CollectionChanged;
        }

        public NavWaypointCollection ( decimal x, decimal y, decimal z )
        {
            currentX = x;
            currentY = y;
            currentZ = z;
            Waypoints.CollectionChanged += NavWaypointList_CollectionChanged;
        }

        private void NavWaypointList_CollectionChanged(object sender,
            System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            void childPropertyChangedHandler(object childSender, PropertyChangedEventArgs child_e)
            {
                OnPropertyChanged(nameof(Waypoints));
            }
            if (e.NewItems != null)
            {
                foreach (NavWaypoint item in e.NewItems)
                {
                    item.PropertyChanged += childPropertyChangedHandler;
                }
            }
            if (e.OldItems != null)
            {
                foreach (NavWaypoint item in e.OldItems)
                {
                    item.PropertyChanged -= childPropertyChangedHandler;
                }
            }
            CalculateFuelUsed();
            CalculateRouteDistances();
        }

        public void AddRange(IEnumerable<NavWaypoint> items)
        {
            foreach (var item in items)
            {
                Waypoints.Add(item);
            }
            CalculateFuelUsed();
            CalculateRouteDistances();
        }

        private void CalculateFuelUsed()
        {
            if (Waypoints.Any(w => w.fuelUsed > 0))
            {
                RouteFuelTotal = 0;
                if (Waypoints.Count > 0)
                {
                    // Calculate fuel for each hop and total fuel used
                    Waypoints.First().fuelUsedTotal = 0;
                    for (int i = 0; i < Waypoints.Count - 1; i++)
                    {
                        Waypoints[i + 1].fuelUsedTotal = Waypoints[i].fuelUsedTotal + Waypoints[i + 1].fuelUsed;
                    }
                    RouteFuelTotal = Waypoints.Last().fuelUsedTotal;

                    // Calculate remaining fuel needs
                    foreach (var waypoint in Waypoints)
                    {
                        waypoint.fuelNeeded = RouteFuelTotal - waypoint.fuelUsedTotal;
                    }
                }
            }
        }

        private void CalculateRouteDistances()
        {
            RouteDistance = 0M;
            if (Waypoints.Count > 0)
            {
                // Calculate distance of each hop and total distance traveled
                Waypoints.First().distanceTraveled = 0;
                for (int i = 0; i < Waypoints.Count - 1; i++)
                {
                    Waypoints[i + 1].distance = Functions.StellarDistanceLy(
                        Waypoints[i].x, Waypoints[i].y, Waypoints[i].z,
                        Waypoints[i + 1].x, Waypoints[i + 1].y, Waypoints[i + 1].z) ?? 0;
                    Waypoints[i + 1].distanceTraveled = Waypoints[i].distanceTraveled + Waypoints[i + 1].distance;
                }
                RouteDistance = Waypoints.Last().distanceTraveled;
                
                Waypoints.Last().distanceTraveled = RouteDistance;
                
                // Calculate distance remaining and set index value
                int j = 0;
                foreach (var waypoint in Waypoints)
                {
                    waypoint.distanceRemaining = RouteDistance - waypoint.distanceTraveled;
                    waypoint.index = j++;
                }
            }
        }

        // Update coordinates
        public void UpdateLocationData ( 
            ulong currSystemAddress,
            decimal? currX = null, 
            decimal? currY = null, 
            decimal? currZ = null )
        {
            this.currentX = currX;
            this.currentY = currY;
            this.currentZ = currZ;

            UpdateVisitedStatus( currSystemAddress );
        }

        private void UpdateVisitedStatus(ulong visitedSystemAddress)
        {
            if (FillVisitedGaps)
            {
                var waypoint = Waypoints.FirstOrDefault(n => n.systemAddress == visitedSystemAddress);
                for (var i = 0; i <= waypoint?.index; i++)
                {
                    Waypoints[i].visited = true;
                }
            }
            else
            {
                foreach (var waypoint in Waypoints)
                {
                    if (!waypoint.visited && waypoint.systemAddress == visitedSystemAddress)
                    {
                        waypoint.visited = true;
                    }
                }
            }
        }

        public void PopulateMissionIds(List<Mission> missions)
        {
            if (missions is null) { return; }
            foreach (var waypoint in Waypoints)
            {
                var missionIds = missions
                    .Where(m => m.destinationsystem == waypoint.systemName ||
                                (m.destinationsystems != null
                                 && m.destinationsystems.Any(w => w.systemName == waypoint.systemName)))
                    .Select(m => m.missionid).ToList();
                waypoint.missionids = missionIds;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
