using EddiDataDefinitions;
using System;
using System.Collections.Generic;
using System.Linq;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class RouteDetailsEvent : Event
    {
        public const string NAME = "Route details";
        public const string DESCRIPTION = "Triggered when a route has been generated or updated";
        public const string SAMPLE = null;

        [PublicAPI("Type of route query")]
        public string routetype { get; private set; }

        [PublicAPI("The destination system")]
        public string system { get; private set; }

        [PublicAPI("The destination station, if applicable")]
        public string station { get; private set; }

        [PublicAPI("List of system names, if applicable")]
        public List<string> route => (Route ?? new NavWaypointCollection()).Waypoints.Select(r => r.systemName).ToList();

        [PublicAPI("Count of missions, systems, or expiry seconds, depending on route type")]
        public long count { get; private set; }

        [PublicAPI("The distance to the destination system")]
        public decimal distance => Route?.Waypoints.Count > 1 ? Route?.Waypoints[1].distance ?? 0 : 0;

        [PublicAPI("The remaining distance of the missions route, if applicable")]
        public decimal routedistance => Route?.RouteDistance ?? 0;

        [PublicAPI("The mission ID(s) associated with the destination system, if applicable")]
        public List<long> missionids { get; private set; }

        // Not intended to be user facing
        public NavWaypointCollection Route { get; private set; }

        public RouteDetailsEvent(DateTime timestamp, string routetype, string system, string station, NavWaypointCollection route, long count, List<long> missionids) : base(timestamp, NAME)
        {
            this.routetype = routetype;
            this.system = system;
            this.station = station;
            this.Route = route;
            this.count = count;
            this.missionids = missionids;
        }
    }
}
