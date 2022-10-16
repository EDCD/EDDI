using EddiDataDefinitions;
using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class NearBookmarkEvent : Event
    {
        public const string NAME = "Near bookmark";
        public const string DESCRIPTION = "Triggered when entering or departing the nearby radius of a bookmark";
        public const string SAMPLE = null;

        [PublicAPI("A boolean value. True if you are entering the nearby radius of the bookmark and and false if you are leaving")]
        public bool near { get; private set; }

        [PublicAPI("The bookmarked system name")]
        public string systemname => bookmark?.systemname;

        [PublicAPI("The bookmarked body name, if applicable")]
        public string bodyname => bookmark?.bodyname;

        [PublicAPI("The bookmarked body short name, if applicable")]
        public string bodyshortname => bookmark?.bodyshortname;

        [PublicAPI("The bookmarked 'point of interest', if applicable")]
        public string poi => bookmark?.poi;

        [PublicAPI("The bookmarked comment, if applicable")]
        public string comment => bookmark?.comment;

        [PublicAPI("True if the point of interest is a station")]
        public bool isstation => bookmark?.isstation ?? false;

        [PublicAPI("The latitude on the surface of a landable body")]
        public decimal? latitude => bookmark?.latitude;

        [PublicAPI("The longitude on the surface of a landable body")]
        public decimal? longitude => bookmark?.longitude;

        [PublicAPI("True if the body is 'landable'")]
        public bool landable => bookmark?.landable ?? false;

        // Variables below are not intended to be user facing
        private NavBookmark bookmark;

        public NearBookmarkEvent(DateTime timestamp, bool approachingBookmark, NavBookmark bookmark) : base(timestamp, NAME)
        {
            this.near = approachingBookmark;
            this.bookmark = bookmark;
        }
    }
}
