using EddiDataDefinitions;
using EddiEvents;
using System;
using System.Collections.Generic;

namespace EddiNavigationMonitor
{
    public class RouteEvent : Event
    {
        public const string NAME = "Route";
        public const string DESCRIPTION = "Triggered when the navigation route is updated";
        public const string SAMPLE = null;

        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static RouteEvent()
        {
        }

        public List<RouteInfo> route { get; private set; }

        public RouteEvent(DateTime timestamp, List<RouteInfo> route) : base(timestamp, NAME)
        {
            this.route = route;
        }
    }
}