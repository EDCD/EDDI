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
            VARIABLES.Add("systemname", "The system at which the commander is currently located");
            VARIABLES.Add("bodyname", "The nearest body to the commander");
            VARIABLES.Add("bodytype", "The type of the nearest body to the commander");
        }

        [JsonProperty("status")]
        public bool gliding { get; private set; }

        [JsonProperty("system")]
        public string systemname { get; private set; }

        public string bodytype => (bodyType ?? BodyType.None).localizedName;

        [JsonProperty("body")]
        public string bodyname { get; private set; }

        // Deprecated, maintained for compatibility with user scripts
        [JsonIgnore, Obsolete("Use systemname instead")]
        public string system => systemname;
        [JsonIgnore, Obsolete("Use bodyname instead")]
        public string body => bodyname;

        // Variables below are not intended to be user facing
        public long? systemAddress { get; private set; }
        public BodyType bodyType { get; private set; } = BodyType.None;

        public GlideEvent(DateTime timestamp, bool gliding, string systemName, long? systemAddress, string bodyName, BodyType bodyType) : base(timestamp, NAME)
        {
            this.gliding = gliding;
            this.systemname = systemName;
            this.systemAddress = systemAddress;
            this.bodyType = bodyType;
            this.bodyname = bodyName;
        }
    }
}
