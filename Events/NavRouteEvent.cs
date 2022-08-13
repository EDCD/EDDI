using EddiDataDefinitions;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using Utilities;

namespace EddiEvents
{
    public class NavRouteEvent : Event
    {
        public const string NAME = "Nav route";
        public const string DESCRIPTION = "Triggered when the navigation route is updated";
        public static NavRouteEvent SAMPLE = new NavRouteEvent(DateTime.UtcNow, new List<NavRouteInfoItem>()
        {
            new NavRouteInfoItem("Cemiess", 1522322606443, new List<decimal>() { 66.06250M, -105.34375M, 27.09375M }, "G"),
            new NavRouteInfoItem("CT Tucanae", 358864556762, new List<decimal>() { 66.40625M, -123.37500M, 51.90625M }, "M"),
            new NavRouteInfoItem("LHS 4031", 1733254091490, new List<decimal>() {67.56250M, -134.12500M, 66.84375M }, "K")
        });

        [PublicAPI("The plotted route (this is a collection of NavWaypoint objects)")]
        public List<NavRouteInfoItem> route { get; }

        // The route includes the originating star system - we need this to calculate 
        // distances but it should not be included in the jump count.
        [PublicAPI("The number of jumps in the plotted route")]
        public int jumps => route.Count - 1;

        [PublicAPI("The total distance spanned by the plotted route, in light years")]
        public decimal? distance => CalculateTotalDistance();

        [PublicAPI("The direct line distance to the destination starsystem, in light years")]
        public decimal? directdistance => CalculateDirectDistance();

        public NavRouteEvent(DateTime timestamp, List<NavRouteInfoItem> route) : base(timestamp, NAME)
        {
            this.route = route;
        }
        
        private decimal? CalculateTotalDistance()
        {
            decimal? dist = 0M;
            for (int i = 0; i < jumps; i++)
            {
                var curr = route[i];
                var dest = route[i + 1];
                dist += Functions.StellarDistanceLy(curr.x, curr.y, curr.z, dest.x, dest.y, dest.z) ?? 0;
            }
            return dist;
        }

        private decimal? CalculateDirectDistance()
        {
            if (route.Count > 1)
            {
                return Functions.StellarDistanceLy(route[0].x, route[0].y, route[0].z, route[jumps].x, route[jumps].y, route[jumps].z) ?? 0;
            }
            else
            {
                return 0;
            }
        }
    }
}