using EddiDataDefinitions;
using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class EnteredNormalSpaceEvent : Event
    {
        public const string NAME = "Entered normal space";
        public const string DESCRIPTION = "Triggered when your ship enters normal space";
        public const string SAMPLE = "{\"timestamp\":\"2016-06-10T14:32:03Z\",\"event\":\"SupercruiseExit\",\"StarSystem\":\"Shinrarta Dezhra\",\"SystemAddress\": 3932277478106,\"Body\":\"Jameson Memorial\",\"BodyType\":\"Station\"}";

        [PublicAPI("The system at which the commander has entered normal space")]
        public string systemname { get; private set; }

        [PublicAPI("The localized type of the nearest body to the commander when entering normal space")]
        public string bodytype => (bodyType ?? BodyType.None).localizedName;

        [PublicAPI("The invariant type of the nearest body to the commander when entering normal space")]
        public string bodytype_invariant => (bodyType ?? BodyType.None).localizedName;

        [PublicAPI("The nearest body to the commander when entering normal space")]
        public string bodyname { get; private set; }

        [PublicAPI("True if the ship is an transport (e.g. taxi or dropship)")]
        public bool? taxi { get; private set; }

        [PublicAPI("True if the ship is belongs to another player")]
        public bool? multicrew { get; private set; }

        // Deprecated, maintained for compatibility with user scripts

        [Obsolete("Use systemname instead")]
        public string system => systemname;

        [Obsolete("Use bodyname instead")]
        public string body => bodyname;

        // Variables below are not intended to be user facing

        public long systemAddress { get; private set; }

        public BodyType bodyType { get; private set; } = BodyType.None;

        public long? bodyId { get; private set; }

        public EnteredNormalSpaceEvent(DateTime timestamp, string systemName, long systemAddress, string bodyName, long? bodyId, BodyType bodyType, bool? taxi, bool? multicrew) : base(timestamp, NAME)
        {
            this.systemname = systemName;
            this.systemAddress = systemAddress;
            this.bodyType = bodyType;
            this.bodyname = bodyName;
            this.bodyId = bodyId;
            this.taxi = taxi;
            this.multicrew = multicrew;
        }
    }
}
