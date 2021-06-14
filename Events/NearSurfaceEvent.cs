using EddiDataDefinitions;
using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class NearSurfaceEvent : Event
    {
        public const string NAME = "Near surface";
        public const string DESCRIPTION = "Triggered when you enter or depart the gravity well around a surface";
        public const string SAMPLE = null;

        [PublicAPI("A boolean value. True if you are entering the gravity well and and false if you are leaving")]
        public bool approaching_surface { get; private set; }

        [PublicAPI("The name of the starsystem")]
        public string systemname { get; private set; }

        [PublicAPI("The name of the body")]
        public string bodyname { get; private set; }

        [PublicAPI("The short name of the body, less the system name")]
        public string shortname => Body.GetShortName(bodyname, systemname);

        // Deprecated, maintained for compatibility with user scripts

        [Obsolete("Use systemname instead")]
        public string system => systemname;
        
        [Obsolete("Use bodyname instead")]
        public string body => bodyname;

        // Variables below are not intended to be user facing

        public long systemAddress { get; private set; }

        public long? bodyId { get; private set; }

        public NearSurfaceEvent(DateTime timestamp, bool approachingSurface, string systemName, long systemAddress, string bodyName, long? bodyId) : base(timestamp, NAME)
        {
            this.approaching_surface = approachingSurface;
            this.systemname = systemName;
            this.systemAddress = systemAddress;
            this.bodyname = bodyName;
            this.bodyId = bodyId;
        }
    }
}
