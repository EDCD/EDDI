using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace EddiEvents
{
    public class GlideEvent : Event
    {
        public const string NAME = "Glide";
        public const string DESCRIPTION = "Triggered when your ship enters or exits glide";
        public const string SAMPLE = null;
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static GlideEvent()
        {
            VARIABLES.Add("status", "The glide status (either \"started\" or \"stopped\")");
        }

        [JsonProperty("status")]
        public string status { get; private set; }

        public GlideEvent(DateTime timestamp, string status) : base(timestamp, NAME)
        {
            this.status = status;
        }
    }
}
