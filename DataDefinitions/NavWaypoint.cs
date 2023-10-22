using JetBrains.Annotations;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace EddiDataDefinitions
{
    public class NavWaypoint : INotifyPropertyChanged
    {
        private bool _visited;
        private List<long> _missionids = new List<long>();
        private bool? _isScoopable;
        private bool _hasNeutronStar;

        public string systemName { get; set; }
        public ulong systemAddress { get; set; }
        public decimal? x { get; set; }
        public decimal? y { get; set; }
        public decimal? z { get; set; }

        /// <summary>
        /// Distance to this waypoint (from the last waypoint)
        /// </summary>
        public decimal distance { get; set; }

        /// <summary>
        /// Total distance travelled
        /// </summary>
        public decimal distanceTraveled { get; set; }

        /// <summary>
        /// Total distance remaining
        /// </summary>
        public decimal distanceRemaining { get; set; }

        public int index { get; set; }

        public string stationName { get; set; }

        // NavRoute only
        public string stellarclass { get; set; }

        // NavRoute and Spansh Galaxy plotter only
        public bool hasNeutronStar
        {
            get => _hasNeutronStar;
            set { _hasNeutronStar = value; OnPropertyChanged(); }
        }

        // NavRoute and Spansh Galaxy plotter only
        public bool? isScoopable
        {
            get => _isScoopable;
            set { _isScoopable = value; OnPropertyChanged(); }
        }

        // Spansh Galaxy plotter only
        public bool? refuelRecommended { get; set; } = false;

        // Spansh Carrier plotter only
        public bool? hasIcyRing { get; set; } = false;
        public bool? hasPristineMining { get; set; } = false;
        public bool? isDesiredDestination { get; set; } = false; // If this is one of the destinations prescribed by the commander for the route
        public int? fuelUsed { get; set; } // The amount of tritium used to jump to this location on this hop
        public int? fuelUsedTotal { get; set; } // The amount of tritium used to jump to this location from the beginning of the route
        public int? fuelNeeded { get; set; } // The amount of tritium needed for remaining hops in the route

        // Info seeded from other monitors
        public bool isMissionSystem => missionids?.Any() ?? false;

        public List<long> missionids
        {
            get => _missionids;
            set { _missionids = value; OnPropertyChanged();}
        }

        // Whether we've already visited the system during our current route
        public bool visited
        {
            get => _visited;
            set { _visited = value; OnPropertyChanged(); }
        }

        // Default constructor
        [JsonConstructor]
        public NavWaypoint(string systemName, decimal x, decimal y, decimal z)
        {
            this.systemName = systemName;
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public NavWaypoint(StarSystem starSystem)
        {
            this.systemName = starSystem.systemname;
            this.systemAddress = starSystem.systemAddress;
            this.x = starSystem.x;
            this.y = starSystem.y;
            this.z = starSystem.z;
            var closeStars = starSystem.bodies.Where(b => b.bodyType == BodyType.Star && b.distance < 100).ToList();
            stellarclass = closeStars.FirstOrDefault(b => b.mainstar ?? false)?.stellarclass;
            isScoopable = closeStars.Any(b => !string.IsNullOrEmpty(b.stellarclass) && "KGBFOAM".Contains(b.stellarclass));
            hasNeutronStar = closeStars.Any(b => !string.IsNullOrEmpty(b.stellarclass) && "N".Contains(b.stellarclass));
        }

        // From `NavRouteInfoItem`
        public NavWaypoint(NavRouteInfoItem navRouteItem)
        {
            systemName = navRouteItem.systemname;
            systemAddress = navRouteItem.systemAddress;
            x = navRouteItem.x;
            y = navRouteItem.y;
            z = navRouteItem.z;
            stellarclass = navRouteItem.stellarclass;
            isScoopable = !string.IsNullOrEmpty(navRouteItem.stellarclass) && "KGBFOAM".Contains(navRouteItem.stellarclass);
            hasNeutronStar = !string.IsNullOrEmpty(navRouteItem.stellarclass) && "N".Contains(navRouteItem.stellarclass);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
