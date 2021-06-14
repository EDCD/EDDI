using EddiDataDefinitions;
using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class GlideEvent : Event
    {
        public const string NAME = "Glide";
        public const string DESCRIPTION = "Triggered when your ship enters or exits glide";
        public const string SAMPLE = null;

        [PublicAPI("The glide status (either true or false, true if entering a glide)")]
        public bool gliding { get; private set; }

        [PublicAPI("The system at which the commander is currently located")]
        public string systemname { get; private set; }

        [PublicAPI("The type of the nearest body to the commander")]
        public string bodytype => (bodyType ?? BodyType.None).localizedName;

        [PublicAPI("The nearest body to the commander")]
        public string bodyname { get; private set; }

        // Deprecated, maintained for compatibility with user scripts

        [Obsolete("Use systemname instead")]
        public string system => systemname;

        [Obsolete("Use bodyname instead")]
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
