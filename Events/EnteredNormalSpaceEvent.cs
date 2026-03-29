using EddiDataDefinitions;
using System;
using System.Collections.Generic;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class EnteredNormalSpaceEvent (
        DateTime timestamp,
        string systemName,
        ulong systemAddress,
        string bodyName,
        int? bodyId,
        BodyType bodyType,
        bool? taxi,
        bool? multicrew )
        : Event( timestamp, NAME )
    {
        public const string NAME = "Entered normal space";
        public const string DESCRIPTION = "Triggered when your ship enters normal space";
        public const string SAMPLE = "{\"timestamp\":\"2016-06-10T14:32:03Z\",\"event\":\"SupercruiseExit\",\"StarSystem\":\"Shinrarta Dezhra\",\"SystemAddress\": 3932277478106,\"Body\":\"Jameson Memorial\",\"BodyType\":\"Station\"}";

        [PublicAPI("The system at which the commander has entered normal space")]
        public string systemname { get; private set; } = systemName;

        [PublicAPI( "The numeric system address of the star system at which the commander has entered normal space" )]
        public ulong systemAddress { get; private set; } = systemAddress;

        [PublicAPI("The localized type of the nearest body to the commander when entering normal space")]
        public string bodytype => (bodyType ?? BodyType.None).localizedName;

        [PublicAPI("The invariant type of the nearest body to the commander when entering normal space")]
        public string bodytype_invariant => (bodyType ?? BodyType.None).invariantName;

        [PublicAPI("The nearest body to the commander when entering normal space (if any)")]
        public string bodyname { get; private set; } = bodyName;

        [PublicAPI( "The numeric ID of the nearest body to the commander when entering normal space (if any)" )]
        public int? bodyId { get; private set; } = bodyId;

        [PublicAPI("True if the ship is an transport (e.g. taxi or dropship)")]
        public bool? taxi { get; private set; } = taxi;

        [PublicAPI("True if the ship is belongs to another player")]
        public bool? multicrew { get; private set; } = multicrew;

        // Deprecated, maintained for compatibility with user scripts

        [Obsolete("Use systemname instead")]
        public string system => systemname;

        [Obsolete("Use bodyname instead")]
        public string body => bodyname;

        // Variables below are not intended to be user facing

        public BodyType bodyType { get; set; } = bodyType;

        public static bool Handle ( DateTime timestamp, string line, IDictionary<string, object> data, ref List<Event> events, bool fromLogLoad )
        {
            if ( fromLogLoad ) { return true; } // Skip handling this during log loading

            var system = JsonParsing.getString(data, "StarSystem");
            var systemAddress = JsonParsing.getULong(data, "SystemAddress");
            var body = JsonParsing.getString(data, "Body");
            var bodyId = JsonParsing.getOptionalInt(data, "BodyID");
            var bodyType = BodyType.FromEDName(JsonParsing.getString(data, "BodyType")) ?? BodyType.None;
            var taxi = JsonParsing.getOptionalBool(data, "Taxi");
            var multicrew = JsonParsing.getOptionalBool(data, "Multicrew");
            events.Add( new EnteredNormalSpaceEvent( timestamp, system, systemAddress, body, bodyId, bodyType, taxi, multicrew ) { raw = line, fromLoad = false } );
            return true;
        }
    }
}
