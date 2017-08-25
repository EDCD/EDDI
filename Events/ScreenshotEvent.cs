using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EddiEvents
{
    public class ScreenshotEvent : Event
    {
        public const string NAME = "Screenshot";
        public const string DESCRIPTION = "Triggered when you take a screenshot";
        public const string SAMPLE = "{ \"timestamp\":\"2016-09-20T12:43:18Z\", \"event\":\"Screenshot\", \"Filename\":\"\\ED_Pictures\\Screenshot_0000.bmp\", \"Width\":1920, \"Height\":1080, \"System\":\"Harm\", \"Body\":\"Gentil Hub\" }";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static ScreenshotEvent()
        {
            VARIABLES.Add("filename", "The name of the file where the screenshot has been saved");
            VARIABLES.Add("width", "The width in pixels of the screenshot");
            VARIABLES.Add("height", "The height in pixels of the screenshot");
            VARIABLES.Add("system", "The name of the system where the screenshot was taken");
            VARIABLES.Add("body", "The name of the nearest body to where the screenshot was taken");
            VARIABLES.Add("longitude", "The longitude where the screenshot was taken (if applicable)");
            VARIABLES.Add("latitude", "The latitude where the screenshot was taken (if applicable)");
        }

        public string filename { get; private set; }
        public int width { get; private set; }
        public int height { get; private set; }
        public string system { get; private set; }
        public string body { get; private set; }
        
        [JsonProperty("longitude")]
        public decimal? longitude { get; private set; }

        [JsonProperty("latitude")]
        public decimal? latitude { get; private set; }     

        public ScreenshotEvent(DateTime timestamp, string filename, int width, int height, string system, string body, decimal? longitude, decimal? latitude) : base(timestamp, NAME)
        {
            this.filename = filename;
            this.width = width;
            this.height = height;
            this.system = system;
            this.body = body;
            this.longitude = longitude;
            this.latitude = latitude;
        }
    }
}
