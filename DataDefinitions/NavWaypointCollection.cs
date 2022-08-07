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
        private int? _fuelUsed;

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

        public bool FillVisitedGaps
        {
            get => _fillVisitedGaps;
            set { _fillVisitedGaps = value; OnPropertyChanged();}
        }

        public int? FuelUsed
        {
            get => _fuelUsed;
            set { _fuelUsed = value; OnPropertyChanged(); }
        }

        public ObservableCollection<NavWaypoint> Waypoints { get; } = new ObservableCollection<NavWaypoint>();

        [JsonConstructor]
        public NavWaypointCollection()
        {
            Waypoints.CollectionChanged += NavWaypointList_CollectionChanged;
        }

        public NavWaypointCollection(bool fillVisitedGaps = false)
        {
            FillVisitedGaps = fillVisitedGaps;
            Waypoints.CollectionChanged += NavWaypointList_CollectionChanged;
        }

        public NavWaypointCollection(IEnumerable<NavWaypoint> items, bool fillVisitedGaps = false)
        {
            foreach (var item in items)
            {
                Waypoints.Add(item);
            }

            CalculateFuelUsed();
            CalculateRouteDistances();

            FillVisitedGaps = fillVisitedGaps;
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
                FuelUsed = 0;
                if (Waypoints.Count > 0)
                {
                    // Calculate fuel for each hop and total fuel used
                    Waypoints.First().fuelUsed = 0;
                    for (int i = 0; i < Waypoints.Count - 1; i++)
                    {
                        Waypoints[i + 1].fuelUsedTotal = Waypoints[i].fuelUsedTotal + Waypoints[i + 1].fuelUsed;
                    }
                    FuelUsed = Waypoints.Last().fuelUsed;

                    // Calculate remaining fuel needs
                    foreach (var waypoint in Waypoints)
                    {
                        waypoint.fuelNeeded = FuelUsed - waypoint.fuelUsedTotal;
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

        public void UpdateVisitedStatus(ulong visitedSystemAddress)
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
