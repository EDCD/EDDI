using System;
using System.Collections.Generic;

namespace EddiEvents
{
    public class BookmarkDetailsEvent : Event
    {
        public const string NAME = "Bookmark details";
        public const string DESCRIPTION = "Triggered when a bookmark has been added or updated";
        public const string SAMPLE = null;

        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static BookmarkDetailsEvent()
        {
            VARIABLES.Add("request", "The bookmark request (location, search, select)");
            VARIABLES.Add("system", "The bookmarked system");
            VARIABLES.Add("body", "The bookmarked body, if applicable");
            VARIABLES.Add("poi", "The bookmarked 'point of interest', if applicable");
            VARIABLES.Add("isstation", "True if the point of interest is a station");
            VARIABLES.Add("latitude", "The latitude on the surface of a landable body");
            VARIABLES.Add("longitude", "The longitude on the surface of a landable body");
            VARIABLES.Add("landable", "True if the body is 'landable'");
        }

        public string request { get; private set; }

        public string system { get; private set; }

        public string body { get; private set; }

        public string poi { get; private set; }

        public bool isstation { get; private set; }

        public decimal? latitude { get; private set; }

        public decimal? longitude { get; private set; }

        public bool landable { get; private set; }

        public BookmarkDetailsEvent(DateTime timestamp, string request, string system, string body, string poi, bool isstation, decimal? latitude, decimal? longitude, bool landable) : base(timestamp, NAME)
        {
            this.request = request;
            this.system = system;
            this.body = body;
            this.poi = poi;
            this.isstation = isstation;
            this.latitude = latitude;
            this.longitude = longitude;
            this.landable = landable;
        }
    }
}