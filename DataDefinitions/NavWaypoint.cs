using System.Collections.Generic;
using Newtonsoft.Json;
using System.Linq;

namespace EddiDataDefinitions
{
    public class NavWaypoint
    {
        public string systemName { get; set; }
        public ulong? systemAddress { get; set; }
        public decimal? x { get; set; }
        public decimal? y { get; set; }
        public decimal? z { get; set; }

        public decimal distance { get; set; }
        public decimal distanceTraveled { get; set; }
        public decimal distanceRemaining { get; set; }

        public int index { get; set; }

        public string stationName { get; set; }

        // NavRoute only
        public string stellarclass { get; set; }

        // NavRoute and Spansh Neutron and Galaxy plotters only
        public bool hasNeutronStar { get; set; }

        // NavRoute and Spansh Galaxy plotter only
        public bool? isScoopable { get; set; }

        // Spansh Galaxy plotter only
        public bool? refuelRecommended { get; set; }

        // Spansh Carrier plotter only
        public bool? hasIcyRing { get; set; }
        public bool? hasPristineMining { get; set; }
        public bool? isDesiredDestination { get; set; } // If this is one of the destinations prescribed by the commander for the route
        public int? fuelUsed { get; set; } // The amount of tritium used to jump to this location

        // Info seeded from other monitors
        public bool isMissionSystem => missionids?.Any() ?? false;

        public List<long> missionids { get; set; } = new List<long>();

        // Whether we've already visited the system during our current route
        public bool visited { get; set; }

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
            var closeStars = starSystem.bodies.Where(b => b.bodyType == BodyType.FromEDName("Star") && b.distance < 100).ToList();
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
    }
}
