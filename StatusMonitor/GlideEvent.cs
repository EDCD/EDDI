using EddiDataDefinitions;
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
            VARIABLES.Add("gliding", "The glide status (either true or false, true if entering a glide)");
            VARIABLES.Add("system", "The system at which the commander is currently located");
            VARIABLES.Add("body", "The nearest body to the commander");
            VARIABLES.Add("bodytype", "The type of the nearest body to the commander");
        }

        [JsonProperty("status")]
        public bool gliding { get; private set; }

        [JsonProperty("system")]
        public string system { get; private set; }

        public string bodytype => (bodyType ?? BodyType.None).localizedName;

        [JsonProperty("body")]
        public string body { get; private set; }

        // Variables below are not intended to be user facing
        public long? systemAddress { get; private set; }
        public BodyType bodyType { get; private set; } = BodyType.None;

        public GlideEvent(DateTime timestamp, bool gliding, string system, long? systemAddress, string body, BodyType bodyType) : base(timestamp, NAME)
        {
            this.gliding = gliding;
            this.system = system;
            this.systemAddress = systemAddress;
            this.bodyType = bodyType;
            this.body = body;
        }
    }
}
