using System;
using System.Collections.Generic;
using EddiDataDefinitions;

namespace EddiEvents
{
    public class GuidanceSystemEvent : Event
    {
        public const string NAME = "Guidance system";
        public const string DESCRIPTION = "Triggered when the guidance system parameters have been updated";
        public const string SAMPLE = null;

        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static GuidanceSystemEvent()
        {
            VARIABLES.Add("poi", "The POI name of the destination, if set");
            VARIABLES.Add("shortbodyname", "The short body name of the destination");
            VARIABLES.Add("status", "The status of the guidance system ('engaged', disengaged', 'update', 'complete')");
            VARIABLES.Add("heading", "The required heading to the POI in degrees");
            VARIABLES.Add("headingerror", "The error in actual heading to required POI heading");
            VARIABLES.Add("slope", "The required slope to the POI in degrees");
            VARIABLES.Add("slopeerror", "The error in actual slope to required POI slope");
            VARIABLES.Add("distance", "The distance to the POI in kilometers");
        }

        public string poi { get; private set; }

        public string shortbodyname { get; private set; }

        public string status { get; private set; }

        public decimal? heading { get; private set; }

        public decimal? headingerror { get; private set; }

        public decimal? slope { get; private set; }

        public decimal? slopeerror { get; private set; }

        public decimal? distance { get; private set; }

        public GuidanceSystemEvent(DateTime timestamp, GuidanceStatus status, NavBookmark bookmark = null, decimal? heading = null, decimal? headingError = null, decimal? slope = null, decimal? slopeError = null, decimal? distance = null) : base(timestamp, NAME)
        {
            this.poi = bookmark?.poi;
            this.shortbodyname = bookmark?.bodyshortname;
            this.status = status.ToString();
            this.heading = heading;
            this.headingerror = headingError;
            this.slope = slope;
            this.slopeerror = slopeError;
            this.distance = distance;
        }
    }

    public enum GuidanceStatus
    {
        complete,
        disengaged,
        engaged,
        update
    }
}