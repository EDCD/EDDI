using JetBrains.Annotations;
using Newtonsoft.Json;

namespace EddiDataDefinitions
{
    public class NavPOIBookmark : NavBookmark
    {
        // Galactic POI URL
        [JsonIgnore] public string url { get; set; }

        // Galactic POI Description (markdown format)
        [JsonIgnore] public string descriptionMarkdown { get; set; }

        // Galactic POI Description state
        [JsonIgnore] public bool descriptionMarkdownHasValue => !string.IsNullOrEmpty(descriptionMarkdown);

        // Drop down visibility
        [JsonIgnore, UsedImplicitly] public new bool hasRowDetails => descriptionMarkdownHasValue || landable;

        // Default Constructor
        public NavPOIBookmark() { }

        [JsonConstructor]
        public NavPOIBookmark(string systemname, ulong? systemAddress, decimal? x, decimal? y, decimal? z, string bodyname, string poi, bool isstation, decimal? latitude, decimal? longitude, bool nearby)
        {
            this.systemname = systemname;
            this.systemAddress = systemAddress;
            this.x = x;
            this.y = y;
            this.z = z;
            this.bodyname = bodyname;
            this.poi = poi;
            this.isstation = isstation;
            this.latitude = latitude;
            this.longitude = longitude;
            this.nearby = nearby;
        }
    }
}
