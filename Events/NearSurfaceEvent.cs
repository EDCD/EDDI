using EddiDataDefinitions;
using System;
using System.Collections.Generic;
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

        [PublicAPI( "The numeric system address of the star system" )]
        public ulong systemAddress { get; private set; }

        [PublicAPI("The name of the body")]
        public string bodyname { get; private set; }

        [PublicAPI( "The numeric ID of the body" )]
        public long? bodyId { get; private set; }

        [PublicAPI("The short name of the body, less the system name")]
        public string shortname => Body.GetShortName(bodyname, systemname);

        // Deprecated, maintained for compatibility with user scripts

        [Obsolete("Use systemname instead")]
        public string system => systemname;
        
        [Obsolete("Use bodyname instead")]
        public string body => bodyname;

        public NearSurfaceEvent(DateTime timestamp, bool approachingSurface, string systemName, ulong systemAddress, string bodyName, long? bodyId) : base(timestamp, NAME)
        {
            this.approaching_surface = approachingSurface;
            this.systemname = systemName;
            this.systemAddress = systemAddress;
            this.bodyname = bodyName;
            this.bodyId = bodyId;
        }

        public static bool Handle ( DateTime timestamp, string edType, string line, IDictionary<string, object> data, ref List<Event> events, bool fromLogLoad )
        {
            var approachingSurface = edType == "ApproachBody";
            var system = JsonParsing.getString(data, "StarSystem");
            var systemAddress = JsonParsing.getULong(data, "SystemAddress");
            var body = JsonParsing.getString(data, "Body");
            var bodyId = JsonParsing.getOptionalLong(data, "BodyID");
            events.Add( new NearSurfaceEvent( timestamp, approachingSurface, system, systemAddress, body, bodyId ) { raw = line, fromLoad = fromLogLoad } );
            return true;
        }
    }
}
