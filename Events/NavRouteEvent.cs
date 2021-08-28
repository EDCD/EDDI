using EddiDataDefinitions;
using System;
using System.Collections.Generic;
using Utilities;

namespace EddiEvents
{
    public class NavRouteEvent : Event
    {
        public const string NAME = "Nav route";
        public const string DESCRIPTION = "Triggered when the navigation route is updated";
        public static NavRouteEvent SAMPLE = new NavRouteEvent(DateTime.UtcNow, new List<NavWaypoint>()
        {
            new NavWaypoint("Cemiess", 1522322606443, new List<decimal>() { 66.06250M, -105.34375M, 27.09375M }, "G"),
            new NavWaypoint("CT Tucanae", 358864556762, new List<decimal>() { 66.40625M, -123.37500M, 51.90625M }, "M"),
            new NavWaypoint("LHS 4031", 1733254091490, new List<decimal>() {67.56250M, -134.12500M, 66.84375M }, "K")
        });

        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static NavRouteEvent()
        {
            VARIABLES.Add("route", "The plotted route (this is a collection of NavWaypoint objects)");
            VARIABLES.Add("jumps", "The number of jumps in the plotted route");
            VARIABLES.Add("distance", "The total distance spanned by the plotted route, in light years");
            VARIABLES.Add("directdistance", "The direct line distance to the destination starsystem, in light years");
        }

        public List<NavWaypoint> route { get; }

        public int jumps => route.Count;

        public decimal? distance => CalculateTotalDistance();

        public decimal? directdistance => CalculateDirectDistance();

        public NavRouteEvent(DateTime timestamp, List<NavWaypoint> route) : base(timestamp, NAME)
        {
            this.route = route;
        }
        
        private decimal? CalculateTotalDistance()
        {
            decimal? dist = 0M;
            for (int i = 0; i < jumps - 1; i++)
            {
                var curr = route[i];
                var dest = route[i + 1];
                dist += Functions.DistanceFromCoordinates(curr.x, curr.y, curr.z, dest.x, dest.y, dest.z) ?? 0;
            }
            return dist;
        }

        private decimal? CalculateDirectDistance()
        {
            return Functions.DistanceFromCoordinates(route[0].x, route[0].y, route[0].z, route[jumps - 1].x, route[jumps - 1].y, route[jumps - 1].z) ?? 0;
        }
    }
}