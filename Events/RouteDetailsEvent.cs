using EddiDataDefinitions;
using System;
using System.Collections.Generic;
using System.Linq;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class RouteDetailsEvent (
        DateTime timestamp,
        string routetype,
        string systemName,
        ulong? systemAddress,
        string stationName,
        long? marketId,
        NavWaypointCollection route,
        long count,
        List<ulong> missionIds )
        : Event( timestamp, NAME )
    {
        public const string NAME = "Route details";
        public const string DESCRIPTION = "Triggered when a route has been generated or updated";
        public const string SAMPLE = null;

        [PublicAPI("Type of route query")]
        public string routetype { get; private set; } = routetype;

        [PublicAPI("The next star system waypoint in the route")]
        public string system { get; private set; } = systemName;

        [PublicAPI( "The numeric system address of the next star system waypoint in the route" )]
        public ulong? systemAddress { get; private set; } = systemAddress;

        [PublicAPI("The station returned from the search, if applicable")]
        public string station { get; private set; } = stationName;

        [PublicAPI( "The numeric ID of the station returned from the search, if applicable" )]
        public long? marketId { get; private set; } = marketId;

        [PublicAPI("A list of the system names which are visited during the route, if applicable")]
        public List<string> route => (Route ?? new NavWaypointCollection()).Waypoints.Select(r => r.systemName).ToList();

        [PublicAPI("A count of missions, systems, or mission expiry seconds, depending on the route type")]
        public long count { get; private set; } = count;

        [PublicAPI("The distance to the next star system waypoint in ly")]
        public decimal distance => Route?.UnvisitedWaypoints.Count > 0 ? Route?.NextWaypointDistance ?? 0 : 0;

        [PublicAPI("The total distance of the route in ly, if applicable")]
        public decimal routedistance => Route?.RouteDistance ?? 0;

        [PublicAPI("The mission ID(s) associated with the next waypoint star system, if applicable")]
        public List<ulong> missionids { get; private set; } = missionIds ?? [ ];

        [PublicAPI("Required total tritium required to complete the route, if applicable")]
        public int? tritiumused => routetype == "carrier" ? Route.RouteFuelTotal : null;

        // Not intended to be user facing
        public NavWaypointCollection Route { get; private set; } = route;
    }
}
