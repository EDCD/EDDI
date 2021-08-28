using System;
using System.Collections.Generic;

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
            VARIABLES.Add("status", "The status of the guidance system ('engaged', disengaged', 'update')");
            VARIABLES.Add("heading", "The required heading to the POI");
            VARIABLES.Add("headingError", "The error in actual heading to required POI heading");
            VARIABLES.Add("slope", "The required slope to the POI");
            VARIABLES.Add("slopeError", "The error in actual slope to required POI slope");
            VARIABLES.Add("distance", "The distance to the POI");
        }

        public string status { get; private set; }

        public decimal? heading { get; private set; }

        public decimal? headingError { get; private set; }

        public decimal? slope { get; private set; }

        public decimal? slopeError { get; private set; }

        public decimal? distance { get; private set; }

        public GuidanceSystemEvent(DateTime timestamp, string status, decimal? heading, decimal? headingError, decimal? slope, decimal? slopeError, decimal? distance) : base(timestamp, NAME)
        {
            this.status = status;
            this.heading = heading;
            this.headingError = headingError;
            this.slope = slope;
            this.slopeError = slopeError;
            this.distance = distance;
        }
    }
}