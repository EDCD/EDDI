using EddiDataDefinitions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace EddiConfigService
{
    /// <summary>Storage for configuration of navigation details</summary>
    [JsonObject(MemberSerialization.OptOut), RelativePath(@"\navigationmonitor.json")]
    public class NavigationMonitorConfiguration : Config
    {
        public DateTime updatedat { get; set; }

        // Search parameters
        public int? maxSearchDistanceFromStarLs { get; set; } = 10000;

        public bool prioritizeOrbitalStations { get; set; } = true;

        // Search data
        public string searchQuery { get; set; }

        public dynamic[] searchQueryArgs { get; set; }

        // Ship touchdown data
        public decimal? tdLat { get; set; }

        public decimal? tdLong { get; set; }

        public string tdPOI { get; set; }

        // Saved bookmarks
        public ObservableCollection<NavBookmark> bookmarks { get; set; } = new ObservableCollection<NavBookmark>();

        // Current in-game route
        public NavWaypointCollection navRouteList { get; set; } = new NavWaypointCollection();

        // Plotted route
        public NavWaypointCollection plottedRouteList { get; set; } = new NavWaypointCollection();
    }
}
