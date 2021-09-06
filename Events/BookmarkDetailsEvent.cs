using EddiDataDefinitions;
using System;
using System.Collections.Generic;
using Utilities;

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
            VARIABLES.Add("request", "The bookmark request (location, query, update, set)");
            VARIABLES.Add("system", "The bookmarked system");
            VARIABLES.Add("body", "The bookmarked body, if applicable");
            VARIABLES.Add("poi", "The bookmarked 'point of interest', if applicable");
            VARIABLES.Add("isstation", "True if the point of interest is a station");
            VARIABLES.Add("latitude", "The latitude on the surface of a landable body");
            VARIABLES.Add("longitude", "The longitude on the surface of a landable body");
            VARIABLES.Add("landable", "True if the body is 'landable'");
        }

        [PublicAPI]
        public string request { get; private set; }

        [PublicAPI]
        public string system { get; private set; }

        [PublicAPI]
        public string body { get; private set; }

        [PublicAPI]
        public string poi { get; private set; }

        [PublicAPI]
        public bool isstation { get; private set; }

        [PublicAPI]
        public decimal? latitude { get; private set; }

        [PublicAPI]
        public decimal? longitude { get; private set; }

        [PublicAPI]
        public bool landable { get; private set; }

        public bool isset { get; private set; }

        public BookmarkDetailsEvent(DateTime timestamp, string request, NavBookmark bookmark) : base(timestamp, NAME)
        {
            this.request = request;
            this.system = bookmark.systemname;
            this.body = bookmark.bodyname;
            this.poi = bookmark.poi;
            this.isstation = bookmark.isstation;
            this.latitude = bookmark.latitude;
            this.longitude = bookmark.longitude;
            this.landable = bookmark.landable;
            this.isset = bookmark.isset;
        }
    }
}