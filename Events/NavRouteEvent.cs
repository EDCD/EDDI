using EddiDataDefinitions;
using System;
using System.Collections.Generic;

namespace EddiEvents
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

        public List<NavRouteInfo> navRoute { get; private set; }

        public NavRouteEvent(DateTime timestamp, List<NavRouteInfo> navRoute) : base(timestamp, NAME)
        {
            this.navRoute = navRoute;
        }
    }
}