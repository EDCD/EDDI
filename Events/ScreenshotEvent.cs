using System;
using System.Collections.Generic;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class ScreenshotEvent (
        DateTime timestamp,
        string filename,
        int width,
        int height,
        string system,
        string body,
        decimal? longitude,
        decimal? latitude )
        : Event( timestamp, NAME )
    {
        public const string NAME = "Screenshot";
        public const string DESCRIPTION = "Triggered when you take a screenshot";
        public const string SAMPLE = @"{ ""timestamp"":""2018-02-03T23:46:07Z"", ""event"":""Screenshot"", ""Filename"":""\\ED_Pictures\\Screenshot_0003.bmp"", ""Width"":3440, ""Height"":1440, ""System"":""Wyrd"", ""Body"":""Wyrd A 2"", ""Latitude"":-63.855904, ""Longitude"":-81.981064, ""Heading"":50 }";

        [PublicAPI("The name of the file where the screenshot has been saved")]
        public string filename { get; private set; } = filename;

        [PublicAPI("The width in pixels of the screenshot")]
        public int width { get; private set; } = width;

        [PublicAPI("The height in pixels of the screenshot")]
        public int height { get; private set; } = height;

        [PublicAPI("The name of the system where the screenshot was taken")]
        public string system { get; private set; } = system;

        [PublicAPI("The name of the nearest body to where the screenshot was taken")]
        public string body { get; private set; } = body;

        [PublicAPI("The longitude where the screenshot was taken (if applicable)")]
        public decimal? longitude { get; private set; } = longitude;

        [PublicAPI("The latitude where the screenshot was taken (if applicable)")]
        public decimal? latitude { get; private set; } = latitude;

        public static bool Handle ( DateTime timestamp, string line, IDictionary<string, object> data, ref List<Event> events, bool fromLogLoad )
        {
            if ( fromLogLoad ) { return true; } // Skip handling this during log loading

            var filename = JsonParsing.getString(data, "Filename");
            var width = JsonParsing.getInt( data, "Width" );
            var height = JsonParsing.getInt( data, "Height" );
            var system = JsonParsing.getString(data, "System");
            var body = JsonParsing.getString(data, "Body");
            var latitude = JsonParsing.getOptionalDecimal(data, "Latitude");
            var longitude = JsonParsing.getOptionalDecimal(data, "Longitude");

            events.Add( new ScreenshotEvent( timestamp, filename, width, height, system, body, longitude, latitude ) { raw = line, fromLoad = false } );
            return true;
        }
    }
}
