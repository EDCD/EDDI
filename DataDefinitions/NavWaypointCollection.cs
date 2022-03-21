using Newtonsoft.Json;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Utilities;

namespace EddiDataDefinitions
{
    public sealed class NavWaypointCollection
    {
        public decimal RouteDistance { get; private set; }
        public bool GuidanceEnabled { get; set; }
        public bool FillVisitedGaps { get; set; }
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
            CalculateRouteDistances();
            FillVisitedGaps = fillVisitedGaps;
            Waypoints.CollectionChanged += NavWaypointList_CollectionChanged;
        }

        private void NavWaypointList_CollectionChanged(object sender,
            System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            CalculateRouteDistances();
        }

        public void AddRange(IEnumerable<NavWaypoint> items)
        {
            foreach (var item in items)
            {
                Waypoints.Add(item);
            }
            CalculateRouteDistances();
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
                for (var i = 0; i < waypoint?.index; i++)
                {
                    Waypoints[i++].visited = true;
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
    }
}
