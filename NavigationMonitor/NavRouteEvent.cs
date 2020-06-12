using EddiDataDefinitions;
using EddiEvents;
using System;
using System.Collections.Generic;

namespace EddiNavigationMonitor
{
    public class NavRouteEvent : Event
    {
        public const string NAME = "Nav route";
        public const string DESCRIPTION = "Triggered when the navigation route is updated";
        public const string SAMPLE = null;

        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static NavRouteEvent()
        {
        }

        public List<RouteInfo> navRoute { get; private set; }

        public NavRouteEvent(DateTime timestamp, List<RouteInfo> route) : base(timestamp, NAME)
        {
            this.navRoute = route;
        }
    }
}