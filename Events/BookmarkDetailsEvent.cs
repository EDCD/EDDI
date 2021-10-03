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

        [PublicAPI("The bookmark request (location, query, update, set)")]
        public string request { get; private set; }

        [PublicAPI("The bookmarked system")]
        public string system { get; private set; }

        [PublicAPI("The bookmarked body, if applicable")]
        public string body { get; private set; }

        [PublicAPI("The bookmarked 'point of interest', if applicable")]
        public string poi { get; private set; }

        [PublicAPI("True if the point of interest is a station")]
        public bool isstation { get; private set; }

        [PublicAPI("The latitude on the surface of a landable body")]
        public decimal? latitude { get; private set; }

        [PublicAPI("The longitude on the surface of a landable body")]
        public decimal? longitude { get; private set; }

        [PublicAPI("True if the body is 'landable'")]
        public bool landable { get; private set; }

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
        }
    }
}